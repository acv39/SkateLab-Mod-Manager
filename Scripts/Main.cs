using Godot;
using System;
using System.IO;
using System.IO.Compression;


//Thanks To Chad Jippity for helping with lots.

public partial class Main : Control
{
	string GameFileExecutableBinary = "SkateLab-Win64-Shipping.exe"; // change this to the game executable in the "binaries" section of your unreal engine game
	string ModManagerFolderName = "SL-MM"; // change this to the folder name of your choice for the mod manager mods folder (which is placed in the "paks" Folder)
	public static string SteamID = "2983940"; // Steam Game ID, so it launches with steam features. If you arent using steam modify the "_on_start_game_pressed" function in FilesScene.cs
	// to OS.
	public Label notification;
	public override void _Ready()
	{
		base._Ready();
		notification = GetNode<Label>("Notification");
		Global.LoadData();
		_on_file_dialog_file_selected(Global.GameDirectory);
		TextEdit GameDir = GetNode<TextEdit>("Game Dir");
		GameDir.Text = Global.GameDirectory;
		
	}

	void _on_online_pressed()
	{
		if(Global.GameDirectoryIsSet == true)
		{
			GetTree().ChangeSceneToFile("res://Other/Online.tscn");
		}
		else
		{
			GetNode<AcceptDialog>("AcceptDialog").Popup();
		}
	}
	
	void _on_settings_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Settings.tscn");
	}
	
	void _on_file_dialog_file_selected(string filepath) // filepath is the selected file
	{
		GetNode<TextEdit>("Game Dir").Text = filepath;
		if(GetGameBinary(filepath))// if the file is the correct executable.
		{
			string exePath = Global.GameDirectory;
// Get the "Win64" folder
			string win64Dir = Path.GetDirectoryName(exePath);
			GD.Print($"Win64 Dir: {win64Dir}");
// Go up twice: Win64 -> Binaries -> Game root
			string gameRoot = Path.GetFullPath(Path.Combine(win64Dir, "..", ".."));
// Build mod paths
			string contentDir = Path.Combine(gameRoot, "Content", "Paks");
			string modsDir = Path.Combine(contentDir, ModManagerFolderName);
// Assign globals
			Global.ModsFolderDirectory = modsDir;

			GD.Print($"Game root: {gameRoot}");
			GD.Print($"Mods dir: {Global.ModsFolderDirectory}");
			int result = CreateModFolder(contentDir);
			switch (result)
			{
				case 0:
					GD.Print("Mods folder already exists.");
					break;
				case 1:
					GD.Print("Mods folder created.");
					break;
				case 3:
					GD.Print($"Could not open directory ' {contentDir} '");
					break;
				default:
					GD.Print("Unknown error.");
					break;
			} // new error handling, more reliablility and debugging
			GD.Print($"ModManagerStorageDirBeforeUsing: {Global.ModManagerStorageDirectory}");
			using (DirAccess dirAccess = DirAccess.Open(win64Dir))
			{
				if (!dirAccess.DirExists(ModManagerFolderName + "-DATA"))
				{
					GD.Print("Data Folder Not Found, Creating It!");
					dirAccess.MakeDir(ModManagerFolderName + "-DATA"); // relative to Global.ModManagerStorageDirectory
					GD.Print(dirAccess.GetCurrentDir(),ModManagerFolderName,"-DATA");
				}
				else
				{
					GD.Print("Data Folder Found");
				}

				Global.ModManagerStorageDirectory = Path.Combine(win64Dir,ModManagerFolderName +"-DATA") ;
				Global.GameDirectoryIsSet = true;
				Global.SaveData();
			}
		}else{
			GD.Print("Not A Proper Game Path");
			notification.Text = "Not The Correct Game Path";
		}
	}

	bool GetGameBinary(string filepath)
	{
		if (Path.GetFileName(@filepath) == GameFileExecutableBinary) // if the file is the correct executable.
		{
			notification.Text = "Game Directory Set";
			GD.Print("Path Exists 1");
			Global.GameDirectory = @filepath;
			GD.Print(filepath);
			return true;
		}
		else
		{
			return false; // only if its done borked
		}
	}

	int CreateModFolder(string openPath)
	{
		using (DirAccess dirAccess = DirAccess.Open(openPath))
		{
			if (dirAccess == null)
			{
				GD.Print($"Failed to open path: {openPath}");
				return 1; // couldn't open directory
			}
			// Case 1: ModManager folder already exists
			if (dirAccess.DirExists(ModManagerFolderName))
			{
				GD.Print("Mod Manager Folder Found");
				Global.ModsFolderDirectory = Path.Combine(openPath, ModManagerFolderName) + Path.DirectorySeparatorChar;
				return 0; // found
			}

			// Case 2: We're in the paks folder and can create it
			GD.Print("Paks Folder Found, Creating Mods Folder");
			dirAccess.MakeDir(ModManagerFolderName);
			Global.ModsFolderDirectory = Path.Combine(openPath, ModManagerFolderName) + Path.DirectorySeparatorChar;
			GD.Print("Created Mods Folder");
			return 2; // created
		}
		// Shouldn't hit this because of using block, but just in case
		return 3; // unknown error
	}
	
	void _on_game_dir_pressed()
	{
		FileDialog fileDialog = GetNode<FileDialog>("FileDialog");
		fileDialog.AddFilter("*.exe", GameFileExecutableBinary);
		fileDialog.Popup();
	}

	void _on_files_pressed()
	{
		if(Global.GameDirectoryIsSet == true){
			GetTree().ChangeSceneToFile("res://Other/files.tscn");
		}else{
			GetNode<AcceptDialog>("AcceptDialog").Popup();
		}

	}

	void on_home_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Main.tscn");
	}

	void _on_mod_creator_pressed()
	{
		if(Global.GameDirectoryIsSet == true)
		{
			GetTree().ChangeSceneToFile("res://Other/ModCreator.tscn");
		}else
		{
			GetNode<AcceptDialog>("AcceptDialog").Popup();
		}
		
	}
}


