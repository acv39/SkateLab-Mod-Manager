using Godot;
using System;
using System.IO;
using System.IO.Compression;


public partial class Main : Control
{
	string GameFileExecutableBinary = "SkateLab-Win64-Shipping.exe"; // change this to the game executable in the "binaries" section of your unreal engine game
	string ModManagerFolderName = "SL-MM"; // change this to the folder name of your choice for the mod manager mods folder (which is placed in the "paks" Folder)
	public static string SteamID = "2983940"; // Steam Game ID, so it launches with steam features. If you arent using steam modify the "_on_start_game_pressed" function in FilesScene.cs
	// to OS.
	public override void _Ready()
	{
		base._Ready();
		Global.LoadData();
		_on_file_dialog_file_selected(Global.GameDirectory);
		TextEdit GameDir = GetNode<TextEdit>("Game Dir");
		GameDir.Text = Global.GameDirectory;
		
	}
	
	void _on_settings_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Settings.tscn");
	}
	
	void _on_file_dialog_file_selected(string filepath) // filepath is the selected file
	{
		GetNode<TextEdit>("Game Dir").Text = filepath;
		Label notification = GetNode<Label>("Notification");
		if(Path.GetFileName(@filepath) == GameFileExecutableBinary){ // if the file is the correct executable.
			notification.Text = "Game Directory Set";
			GD.Print("Path Exists 1");
			Global.GameDirectory = @filepath;
			GD.Print(filepath);
			Global.ModManagerStorageDirectory = Global.GameDirectory.TrimSuffix(GameFileExecutableBinary);
			GD.Print($"Mod Manager Storage Dir: {Global.ModManagerStorageDirectory}");
			string pathfix = Path.Combine("Binaries", "Win64");
			Global.ModsFolderDirectory = Global.ModManagerStorageDirectory.Replace(pathfix,"");
			GD.Print($"Mods Folder Dir w/ cotentpaks: {Global.ModsFolderDirectory}Content/Paks");
			using(DirAccess dirAccess = DirAccess.Open(Global.ModsFolderDirectory))
			{ 
				if(dirAccess.DirExists("Content/Paks/"+ModManagerFolderName) == true)
				{ // if the mods folder is made TODO: Object Reference not set to an instance
					GD.Print("Mod Manager Folder Found");
					Global.ModsFolderDirectory = Global.ModsFolderDirectory+"Content/Paks/"+ModManagerFolderName;
				}
				else if(dirAccess.DirExists("Content/Paks/") == true){ // else if paks folder exist make mods folder
					GD.Print("Paks Folder Found, Creating Mods Folder");
					dirAccess.MakeDir(Global.ModsFolderDirectory+"Content/Paks/"+ModManagerFolderName);
					Global.ModsFolderDirectory = Global.ModsFolderDirectory+"Content/Paks/"+ModManagerFolderName+"/";
					GD.Print("Created Mods Folder");
				}else
				{
					GD.Print("Folder Not Found.");
				}
			}
			using(DirAccess dirAccess = DirAccess.Open(Global.ModManagerStorageDirectory)){ // Open diraccess for creation of Mod Manager data folder, disposed of after code executed.
				if (dirAccess.DirExists(ModManagerFolderName+"-DATA") == false){
					GD.Print("Data Folder Not Found, Creating It!");
					dirAccess.MakeDir(Global.ModManagerStorageDirectory+ModManagerFolderName+"-DATA");
					Global.ModManagerStorageDirectory = Global.ModManagerStorageDirectory+ModManagerFolderName+"-DATA/";
					Global.GameDirectoryIsSet = true;
					Global.SaveData();
				}else{
					GD.Print("data folder found");
					Global.ModManagerStorageDirectory = Global.ModManagerStorageDirectory+ModManagerFolderName+"-DATA/";
					Global.GameDirectoryIsSet = true;
					Global.SaveData();
				}
			}
		}else{
			GD.Print("Not A Proper Game Path");
			notification.Text = "Not The Correct Game Path";
		}
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


