using Godot;
using System;

public partial class Settings : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<CheckButton>("CheckButton").ToggleMode = Global.AutoImport;
	}

	void _on_check_button_toggled(bool toggled_on)
	{
		Global.AutoImport = toggled_on;
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
}
