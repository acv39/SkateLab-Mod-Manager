using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using FileAccess = System.IO.FileAccess;

public partial class ModCreator : Control
{
	private string PakPath;
	private string PreviewPath;
	private string ModTitle;
	private string ModDescription;
	private int ModCategoryIdx = 10;
	private bool ModPreviewType = true;
	private bool Pak = false; // what type of file is created after packing mods cooked files
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		GetTree().GetRoot().FilesDropped += _on_files_dropped;
	}
	
	void _on_online_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Online.tscn");
	}
	
	void CreateZip(string pakpath)
	{
		GD.Print($"PakPath: {pakpath}");
		GD.Print($"PreviewPath: {PreviewPath}");
		GD.Print($"ModTitle: {ModTitle}");
		GD.Print($"ModDescription: {ModDescription}");
		if (!string.IsNullOrEmpty(pakpath) && !string.IsNullOrEmpty(PreviewPath) && !string.IsNullOrEmpty(ModTitle) && !string.IsNullOrEmpty(ModDescription))
		{
			GD.Print("Its Working");
			string tempDir = Path.Combine(Global.ModManagerStorageDirectory, "temp");
			GD.Print("String Created 1");
			Directory.CreateDirectory(tempDir);
			GD.Print("Created Dir No Issues LOL");
			string modInfoPath = Path.Combine(tempDir, "mod_info.txt");
			string modInfoContent = $"{ModTitle},{ModDescription},{ModCategoryIdx},{ModPreviewType}";
			File.WriteAllText(modInfoPath, $"{ModTitle},{ModDescription},{ModCategoryIdx},{ModPreviewType}");
			string zipFilePath = Path.Combine(tempDir, $"{ModTitle}.zip");
			using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
			{
				using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
				{
					archive.CreateEntryFromFile(modInfoPath, "mod_info.txt");
					archive.CreateEntryFromFile(pakpath, Path.GetFileName(pakpath));
					archive.CreateEntryFromFile(PreviewPath, Path.GetFileName(PreviewPath));
				}
			}
			GetNode<AcceptDialog>("Mod_Created").SetText("Mod Zip Stored In: " + tempDir);
			GetNode<AcceptDialog>("Mod_Created").Popup();
		}
	}
	
	void _on_create_mod_pressed()
	{
		if (Pak)
		{
			//RunUnrealPak(PakPath); // Pak Standalone
		}
		else
		{
			//string newPak = RunUnrealPak(PakPath); // Zip file export for Packer
			//GD.PrintErr(newPak);
			CreateZip(PakPath);
		}
	}
	
	void _on_pak_or_zip_toggled(bool state)
	{
		if (state) //true
		{
			GetNode<CheckButton>("PakOrZip").Text = ".Pak";
		}
		else
		{
			GetNode<CheckButton>("PakOrZip").Text = ".Zip";
		}
		Pak = state;
	}
	
	void _on_files_dropped(string[] path)
	{
		if (path[0].GetExtension() == ".pak")
		{
			PakPath = path[0];
		}
	}
	
	void _on_mod_explorer_button_pressed()
	{
		GetNode<FileDialog>("Pak").Popup();
	}

	void _on_mod_preview_img_button_pressed()
	{
		GetNode<FileDialog>("Preview IMG").Popup();
	}

	void _on_pak_dir_selected(string dir)
	{
		GetNode<TextEdit>("ModDir").Text = dir;
		PakPath = dir;
	}
	
	void _on_pak_file_selected(string path)
	{
		GetNode<TextEdit>("ModDir").Text = path;
		PakPath = path;
	}

	void _on_preview_img_file_selected(string path)
	{
		GetNode<TextEdit>("ModDir2").Text = path;
		PreviewPath = path;
	}

	void _on_title_text_changed()
	{
		ModTitle = GetNode<TextEdit>("Title").GetText();
	}

	void _on_mod_description_text_changed()
	{
		ModDescription = GetNode<TextEdit>("Mod Description").GetText();
	}

	void _on_option_button_item_selected(int index)
	{
		ModCategoryIdx = index;
	}

	void _on_home_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Main.tscn");
	}

	void _on_files_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/files.tscn");
	}

	void _on_settings_pressed()
	{
		GetTree().ChangeSceneToFile("res://Other/Settings.tscn");
	}
	
}
