using Godot;
using System;

public partial class SideBar : Control
{
	
	void _on_mod_creator_pressed()
	{
		if(Global.GameDirectoryIsSet)
		{
			GetTree().ChangeSceneToFile("res://Other/ModCreator.tscn");
		}
		else
		{
			GetNode<AcceptDialog>("AcceptDialog").Popup();
		}
		
	}
	

	void _on_files_pressed()
	{
		if(Global.GameDirectoryIsSet)
		{
			GetTree().ChangeSceneToFile("res://Other/files.tscn");
		}
		else
		{
			GetNode<AcceptDialog>("AcceptDialog").Popup();
		}
		
	}
	
	void _on_home_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Main.tscn");
	}

	void _on_settings_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Settings.tscn");
	}
    void _on_print_all_global_pressed()
    {
	    GD.Print("GameDirectory: ", Global.GameDirectory);
	    GD.Print("ModDirectory: ", Global.ModDirectory);
	    GD.Print("ModManagerStorageDirectory: ", Global.ModManagerStorageDirectory);
	    GD.Print("ModsFolderDirectory: ", Global.ModsFolderDirectory);
	    GD.Print("GameDirectoryIsSet: ", Global.GameDirectoryIsSet);
	    GD.Print("ModSettings: ", Global.ModSettings);
	    GD.Print("LoadedMods: ", Global.LoadedMods);
	    GD.Print("CreatedNodes: ", Global.CreatedNodes);
	    GD.Print("CreatedOnlineNodes: ", Global.CreatedOnlineNodes);
	    GD.Print("SelectedMod: ", Global.SelectedMod);
    }
        
}
