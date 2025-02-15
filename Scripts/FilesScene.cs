using System;
using Godot;
using Godot.Collections;
using System.IO;
using System.IO.Compression;



public partial class FilesScene : Control
{
	VBoxContainer ModsList;

	public override void _Ready()
	{
		base._Ready();
		GetTree().GetRoot().FilesDropped += _on_files_dropped;
		ModsList = GetNode<VBoxContainer>("ModsListControl/Mods/ModsList");
		CreateNodes();
		GetNode<TextEdit>("Mod Dir").Text = Global.ModDirectory.Join("");
	}

	void _on_mod_explorer_button_pressed()
	{
		GetNode<FileDialog>("FileDialogmod").Popup();
	}
	
	void _on_mod_creator_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/ModCreator.tscn");
	}
	
	void _on_settings_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Settings.tscn");
	}
	
	void _on_file_dialogmod_files_selected(string[] path) // multiple files
	{
		Global.ModDirectory = path;
		GetNode<TextEdit>("Mod Dir").Text = path.Join("");
		if (Global.AutoImport == true)
		{
			_on_import_pressed();
		}
		else
		{
			GD.Print("its Disabled");
		}
	}

	void _on_files_dropped(string[] files)
	{
		Global.ModDirectory = files;
		GetNode<TextEdit>("Mod Dir").Text = files.ToString();
		if (Global.AutoImport == true)
		{
			_on_import_pressed();
		}
		else
		{
			GD.Print("its Disabled");
		}
	}

	void _on_import_pressed()
	{

		foreach (var file in Global.ModDirectory)
		{
			if (file.GetExtension() == "pak")
			{
				GD.Print("Pak Mod");
				Pak(file);
				CreateNodes();
				GetTree().ReloadCurrentScene();
			}
			else if (file.GetExtension() == "zip")
			{
				GD.Print("Extracting A Zip File Mod");
				ZipExtract(file);
				CreateNodes();
				GetTree().ReloadCurrentScene();
			}
			else
			{
				GD.Print("Not A Valid File Type");
			}
		}
	}

	void _on_home_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Main.tscn");
	}

	void CreateNodes()
	{
		PackedScene ModNode = GD.Load<PackedScene>("res://Other/mod.tscn"); // load the mod node for instancing
		foreach (var (LoadedMods, ModData) in Global.LoadedMods)
		{
			var ModDataDict = (Dictionary)Global.LoadedMods[LoadedMods];
			var ModCategoryDict = ModDataDict["ModCategory"].AsInt32();
			var ModCategory = Global.ModTypes[ModCategoryDict]; // It works but throws error if out of range, can fix later
			var ModNodeInstance = ModNode.Instantiate();
			ModNodeInstance.Name = ModDataDict["ModName"].ToString();
			if (ModsList.GetChildCount() < Global.LoadedMods.Count)
			{
				ModsList.AddChild(ModNodeInstance);
				Global.CreatedNodes[ModNodeInstance.GetName()] = GetNode(GetPathTo(ModNodeInstance));
				Label ModNameLabel = ModNodeInstance.FindChild("ModName") as Label;
				Label ModCategoryLabel = ModNodeInstance.FindChild("ModCategory") as Label;
				CheckButton ModToggle = ModNodeInstance.FindChild("ModToggle") as CheckButton;
				Area2D ClickFixButton = ModNodeInstance.FindChild("ClickFix") as Area2D;
				ModNameLabel.Text = ModDataDict["ModName"].ToString();
				ModCategoryLabel.Text = ModCategory.ToString();
				ModToggle.Toggled += (pressed) => { ModDataDict["Toggled"] = pressed; };
				ModToggle.ButtonPressed = (bool)ModDataDict["Toggled"];
				ClickFixButton.InputEvent += (viewport, @event, idx) =>
				{
					if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && @event.IsPressed() == true)
					{
						Global.SelectedMod = ModDataDict;
						if (ModDataDict["ModPreview2D"].AsBool() == true)
						{
							GD.Print("2d shit");
							GetNode<SubViewportContainer>("Preview/Control/3DPreviewContainer").Hide();
							string previewPath = Path.GetFullPath((string)ModDataDict["ModPreviewPath"]).GetBaseDir();
							GD.Print($"Img Preview Path: {previewPath}");
							if (Directory.Exists(previewPath))
							{
								Image image = new Image();
								image.Load((string)ModDataDict["ModPreviewPath"]);
								ImageTexture imageTexture = new ImageTexture();
								imageTexture.SetImage(image);
								GetNode<TextureRect>("Preview/Control/TextureRect").Texture = imageTexture;
							}
							else
							{
								GD.PrintErr("Image File Not Found");
							}
						}
						else if (ModDataDict["ModPreview2D"].AsBool() == false)
						{
							GD.Print("3d Shit");
							GetNode<SubViewportContainer>("Preview/Control/3DPreviewContainer").Show();
							Image image = new Image();
							image.Load((string)ModDataDict["ModPreviewPath"]);
							ImageTexture imageTexture = new ImageTexture();
							imageTexture.SetImage(image);
							MeshInstance3D mesh = GetNode<MeshInstance3D>(
								"Preview/Control/3DPreviewContainer/SubViewport/3D Preview/SkateLab SDK/Armature/Skeleton3D/Deck");
							StandardMaterial3D material = mesh.GetSurfaceOverrideMaterial(0) as StandardMaterial3D;
							material.AlbedoTexture = imageTexture;
							mesh.SetSurfaceOverrideMaterial(0, material);
						}
						GetNode<RichTextLabel>("Preview/Control/RichTextLabel").Text = (string)ModDataDict["ModDescription"];
					}
				};
			}
		}
	}

	static void Pak(string pakpath)
	{
		string Data = Global.ModManagerStorageDirectory;
		string NewFolder = pakpath.GetFile();
		NewFolder = NewFolder.TrimSuffix("_P.pak");
		using (DirAccess dirAccess = DirAccess.Open(Data))
		{
			string WriteTo = Data + NewFolder + "/";
			dirAccess.MakeDir(Data + NewFolder);
			DirAccess.CopyAbsolute(pakpath, WriteTo + NewFolder + "_P.pak");
			Global.StoreData(NewFolder, NewFolder, null, "Pak Import", WriteTo + NewFolder + "_P.pak", "Misc", true);
		}
		Global.SaveData();
	}

	void ZipExtract(string zippath)
	{
		Node autoload = GetNode("/root");
		string pakpath = Global.ModManagerStorageDirectory;
		string ModDescription = null;
		string ModSettingsPath = null;
		string ModImagePath = null;
		string ModCategory = "9";
		string ModName = "Set A Mod Name";
		string NewFolder = Path.GetFileNameWithoutExtension(zippath);
		string FolderName = null;
		bool ModPreview2D = true;
		string extractPath = Global.ModManagerStorageDirectory + NewFolder;
		Directory.CreateDirectory(extractPath); // make sure the directory exists
		ZipFile.ExtractToDirectory(zippath, extractPath, true); // Overwrite existing files if they exist. 
		foreach (string filePath in Directory.GetFiles(extractPath))
		{
			string fileExtension = Path.GetExtension(filePath).ToLower();
			if (fileExtension == ".pak")
			{
				GD.Print("Pak Found" + filePath);
				pakpath = filePath;
			}
			else if (fileExtension == ".txt")
			{
				GD.Print("Settings File Found" + filePath);
				ModSettingsPath = filePath;
			}
			else if (fileExtension == ".png")
			{
				GD.Print("Preview Found" + filePath);
				ModImagePath = filePath;
			}
			else
			{
				GD.Print("File Not Needed");
			}
		}

		if (ModSettingsPath != null)
		{
			string fileContents;
			string[] modSettings;
			using (Godot.FileAccess fileAccess =
			       Godot.FileAccess.Open(ModSettingsPath, Godot.FileAccess.ModeFlags.Read))
			{
				fileContents = fileAccess.GetAsText();
			}

			modSettings = fileContents.Split(','); // turn mod settings into an array.
			ModName = modSettings[0];
			ModDescription = modSettings[1];
			ModCategory = modSettings[2];
			ModPreview2D = Convert.ToBoolean(modSettings[3]);
		}

		Global.StoreData(NewFolder, ModName, ModImagePath, ModDescription, pakpath, ModCategory, ModPreview2D);
		Global.SaveData();
	}
	
	void _on_load_mods_pressed()
	{
		GD.Print($"Mod Folder Directory: {Global.ModsFolderDirectory}");
		string modPath = Global.ModsFolderDirectory;
		GetNode<Window>("Window").Popup();
		foreach (var (key,_value) in Global.LoadedMods) // key is mods
		{
			if (((Dictionary)Global.LoadedMods[key])["Toggled"].AsBool() && ((Dictionary)Global.LoadedMods[key])["ModLoadedInGameFiles"].AsBool() == false) // some bullshit way to convert my gdscript to c# lol
			{
				GD.Print(key);
				var ModFilePath = ((Dictionary)Global.LoadedMods[key])["ModPath"].AsString();
				string ModFile = ModFilePath.GetFile();
				string WriteTo = modPath;
				GD.Print($"1 ModFilePath,WriteTo,ModFile: {ModFilePath},{WriteTo},{ModFile}");
				DirAccess.CopyAbsolute(ModFilePath, WriteTo + ModFile);
				((Dictionary)Global.LoadedMods[key])["ModLoadedInGameFiles"] = true;
			}

			if (((Dictionary)Global.LoadedMods[key])["Toggled"].AsBool() == false && ((Dictionary)Global.LoadedMods[key])["ModLoadedInGameFiles"].AsBool()) //if its checkbox is toggled false and if its loaded in game files
			{
				var ModFilePath = ((Dictionary)Global.LoadedMods[key])["ModPath"].AsString();
				string ModFile = ModFilePath.GetFile();
				string WriteTo = modPath;
				GD.Print($"2 ModFilePath,WriteTo,ModFile: {ModFilePath},{WriteTo},{ModFile}");
				if (Directory.Exists(WriteTo))
				{
					GD.Print("OS Trashed1");
					OS.MoveToTrash(WriteTo+ModFile);
					GD.Print("OS Trashed2");
				}
				else
				{
					GD.PrintErr($"{WriteTo}{ModFile}: File Was Deleted At Some Point.");
				}
				((Dictionary)Global.LoadedMods[key])["ModLoadedInGameFiles"] = false;
			}
		}
		GetNode<Window>("Window").Hide();
		Global.SaveData();
	}

	void _on_remove_mod_pressed()
	{
		GetNode<ConfirmationDialog>("DeleteModPopup").PopupCentered();
	}

	void _on_delete_mod_popup_confirmed()
	{
		if (Global.SelectedMod != null)
		{
			GetNode<Window>("DeleteModPopup").PopupCentered();
			var Selected = Global.SelectedMod;
			string ModsFolder = Global.ModsFolderDirectory + "/";
			string DataFolder = Global.ModManagerStorageDirectory;
			string selectedpath = Selected["ModPath"].AsString();
			string selectedfile = selectedpath.GetFile();
			OS.MoveToTrash(ModsFolder + selectedfile); // deletes it from paks mod folder
			OS.MoveToTrash(DataFolder + Selected["FolderName"]); // deletes it from mm data folder
			Global.LoadedMods.Remove(Selected["FolderName"]);
			Global.SelectedMod = null;
			Global.SaveData();
			GetNode<Window>("DeleteModPopup").Hide();
			GetTree().ReloadCurrentScene();
		}
	}
	
	// !!
	
	void _on_start_game_pressed()
	{
		OS.ShellOpen("steam://launch/" + Main.SteamID); // comment this out if not using steam.
		//OS.Execute(Global.GameDirectory); uncomment this if using non steam, or change the code above for epic games/etc launchers.
	}

	
}
