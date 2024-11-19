using Godot;
using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class Global : Node
{
	public static string GameDirectory; // game dir
	public static string[] ModDirectory; // mod dirs
	public static string ModManagerStorageDirectory; //mod manager storage dir
	public static string ModsFolderDirectory; // modsfolderdir
	public static bool GameDirectoryIsSet = false; // if the game dir is set or not
	public static string[] ModSettings; // mods settings from text file
	public static Dictionary LoadedMods = new Dictionary(); // loaded mods. 
	public static Dictionary CreatedNodes = new Dictionary(); // currently created nodes
	public static Dictionary SelectedMod = new Dictionary(); // currently selected mod
	public static Resource FilesScene = GD.Load("res://Other/files.tscn"); // fix for laggy scene change as its always loaded
	public static Array ModTypes = new Array{ "Map", "Deck", "Griptape", "Trucks", "Wheels", "Shoes", "Obj Dropper", "Audio", "Blueprint", "Misc" };
	public static bool AutoImport = true;
	
	
	public static void StoreData(string FolderName,string ModName,string ModImagePath,string ModDescription,string ModPath,string ModCategory,bool ModPreview2D)
    { 
      
	    if(LoadedMods.ContainsKey(FolderName)){
            LoadedMods[FolderName] = new Dictionary{
            {"ModName",ModName},
            {"ModPreviewPath",ModImagePath},
            {"ModDescription", ModDescription},
			{"ModPath",ModPath},
            {"ModCategory",ModCategory},
            {"ModLoadedInGameFiles",false},
            {"Toggled",true},
            {"FolderName",FolderName},
            {"ModPreview2D",ModPreview2D}};
        }else{
            LoadedMods.Add(FolderName,new Dictionary{
            {"ModName",ModName},
            {"ModPreviewPath",ModImagePath},
            {"ModDescription", ModDescription},
			{"ModPath",ModPath},
            {"ModCategory",ModCategory},
            {"ModLoadedInGameFiles",false},
            {"Toggled",true},
            {"FolderName",FolderName},
            {"ModPreview2D",ModPreview2D}
			});
        }
        GD.Print(LoadedMods);
    }

	public const string SaveFilePath = "user://SkateLab-Mod-Manager-CS.dat";
	public static void SaveData()
	{
		var SaveData = new Dictionary
		{
			{"LoadedMods",Global.LoadedMods},
			{"AutoImport", Global.AutoImport},
			{"GameDirectory",Global.GameDirectory},
		};
		using(var savefile = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write))
		{
			if (savefile == null)
			{
				GD.Print("Error Creating Save File", FileAccess.GetOpenError());
			}
			var JsonString = Json.Stringify(SaveData);
			savefile.StoreLine(JsonString);
		}

		
	}
	
	public static void LoadData()
	{
		if (!FileAccess.FileExists(SaveFilePath))
		{
			GD.Print("Save File Not Found");
			return;
		}

		using (var SaveFile = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read))
		{
			var JsonString = SaveFile.GetAsText();
			var json = new Json();
			var parseResults = json.Parse(JsonString);
			if (parseResults != Error.Ok)
			{
				GD.Print("Json Parse Error" + json.GetErrorMessage() + "On Line" + json.GetErrorLine());
			}

			var SaveData = json.GetData().AsGodotDictionary();
			GD.Print(SaveData);
			foreach (var (key, value) in SaveData)
			{
				if ((string)key == "LoadedMods")
				{
					Global.LoadedMods = (Dictionary)value;
					GD.Print("Set 1");
					GD.Print(Global.LoadedMods);
				}
				if ((string)key == "AutoImport")
				{
					Global.AutoImport = (bool)value;
					GD.Print("Set 2");
					GD.Print(AutoImport);
				}
				if ((string)key == "GameDirectory")
				{
					Global.GameDirectory = (string)value;
					GD.Print("Set 3");
					GD.Print(GameDirectory);
					Global.GameDirectoryIsSet = true;
				}
				
			}
		}
	}
}