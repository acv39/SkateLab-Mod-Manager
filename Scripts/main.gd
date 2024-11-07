extends Control
@onready var modslist: VBoxContainer = $ModsListControl/Mods/ModsList

func _ready(): #gets called every scene change btw
	get_tree().get_root().files_dropped.connect(_on_files_dropped)
	print(autoload.scene)
	if autoload.scene == true:
		load_data()
		_on_file_dialog_file_selected(autoload.gamedir)
		$"Game Dir".text = autoload.gamedir
	else: 
		createnodes()
		$"Mod Dir".text = autoload.moddir
#button events
func _on_game_dir_pressed() -> void: #get game dir button
	$FileDialog.add_filter("*.exe ; SkateLab-Win64-Shipping.exe")
	$FileDialog.popup()

func _on_mod_files_explorer_pressed() -> void: #get mod dir button
	$FileDialogmod.add_filter("*.pak, *.zip ; Mod Directory")
	$FileDialogmod.popup()

func _on_file_dialogmod_file_selected(path: String) -> void: #filedialog shit for mod path
	autoload.moddir = path
	$"Mod Dir".text = autoload.moddir

func _on_file_dialog_file_selected(path: String) -> void:
	$"Game Dir".text = path
	if path.get_file() == "SkateLab-Win64-Shipping.exe":
		autoload.gamedir = path #set path to game executable
		autoload.modmanagerstoragedir = autoload.gamedir.trim_suffix("SkateLab-Win64-Shipping.exe")
		autoload.modsfolderdir = autoload.gamedir.trim_suffix("Binaries\\Win64\\SkateLab-Win64-Shipping.exe") #step 1/2 getting mods folder
		print(autoload.modsfolderdir)
		print(autoload.modsfolderdir+"Content\\Paks\\") 
		var diraccess = DirAccess.open(autoload.modsfolderdir)
		if diraccess.dir_exists("Content\\Paks\\SL-MM") == true: #finding paks folder, then creating a folder for mods from slmm
			print("mod manager folder found")
			autoload.modsfolderdir = autoload.modsfolderdir+"Content\\Paks\\SL-MM\\"
		elif diraccess.dir_exists("Content\\Paks"):
			autoload.modsfolderdir = autoload.modsfolderdir+"Content\\Paks\\"
			print("paks folder found, creating a folder")
			diraccess.make_dir(autoload.modsfolderdir+"SL-MM")
			autoload.modsfolderdir = autoload.modsfolderdir+"Content\\Paks\\SL-MM\\"
		else:
			print("paks folder not found")
		diraccess = DirAccess.open(autoload.modmanagerstoragedir)
		if diraccess.dir_exists("SL-MM-DATA"):
			print("data folder found")
			autoload.modmanagerstoragedir = autoload.modmanagerstoragedir+"SL-MM-DATA\\"
		else:
			print("data folder not found, creating it")
			diraccess.make_dir(autoload.modmanagerstoragedir+"SL-MM-DATA")
			autoload.modmanagerstoragedir = autoload.modmanagerstoragedir+"SL-MM-DATA\\"
		autoload.isgamedirset = true
		save_data()
	else:
		$Notification.text = "Not Proper Game Path"
		print("Not Proper Game Path")
		
func _on_files_dropped(files):
	if autoload.scene == false:
		autoload.moddir = files[0]
		$"Mod Dir".text = autoload.moddir
		print(autoload.moddir)
	else:
		print("not mod scene, this to prevent crash")	

func _on_import_pressed() -> void:
	if autoload.moddir.get_extension() == "pak":
		pak(autoload.moddir)
		createnodes()
		get_tree().reload_current_scene()
	elif autoload.moddir.get_extension() == "zip":
		unzip(autoload.moddir)
		createnodes()
		get_tree().reload_current_scene()
	else:
		print("Not A Valid File Type")
	
#change scenes
func _on_files_pressed() -> void:
	if autoload.isgamedirset == true:
		autoload.scene = false
		get_tree().change_scene_to_file("res://Other/files.tscn")
	else:
		$AcceptDialog.popup()
func _on_home_pressed() -> void:
	save_data()
	autoload.scene = true
	get_tree().change_scene_to_file("res://Other/Main.tscn")

func pak(pakpath:String):
	var data :String = autoload.modmanagerstoragedir
	var newfolder = pakpath.get_file()
	newfolder = newfolder.trim_suffix("_P.pak")
	var diraccess = DirAccess.open(data)
	var writeto = data+newfolder+"\\"
	diraccess.make_dir(data+newfolder)
	DirAccess.copy_absolute(pakpath,writeto+newfolder+"_P.pak")
	storevalue(newfolder,newfolder,null,"Pak Import, No Description",writeto+newfolder+"_P.pak","Misc",true)
	save_data()

func createnodes():
	for loadedmods in autoload.loadmods:
		var moddata = autoload.loadmods[loadedmods]
		var modnodeinstance = autoload.ModNode.instantiate()
		modnodeinstance.name = str(moddata["ModName"])
		if modslist.get_child_count() < autoload.loadmods.size():
			modslist.add_child(modnodeinstance)
			autoload.creatednodes[modnodeinstance.get_name()] = get_node(get_path_to(modnodeinstance))
			var modnamelabel = modnodeinstance.find_child("ModName")
			var modcategorylabel = modnodeinstance.find_child("ModCategory")
			var modtoggle = modnodeinstance.find_child("ModToggle")
			var clickfixbutton = modnodeinstance.find_child("ClickFix")
			modnamelabel.text = moddata["ModName"]
			modcategorylabel.text = moddata["ModCategory"]
			modtoggle.toggled.connect(func(toggledon: bool):
				moddata["Toggled"] = toggledon
			)
			modtoggle.button_pressed = moddata["Toggled"]
			clickfixbutton.input_event.connect(func(_viewport, event, _shape_idx):
				if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
					autoload.selectedmod = moddata
					if not moddata.has("ModPreview2d") or moddata["ModPreview2d"]:
						#2d preview shit
						$"Preview/Control/3DPreviewContainer".hide()
						var image = Image.new()
						image.load(moddata["ModPreviewPath"])
						var imagetexture = ImageTexture.new()
						imagetexture.set_image(image)
						$"Preview/Control/TextureRect".texture = imagetexture
					else:
						#3d preview shit
						$"Preview/Control/3DPreviewContainer".show()
						var image = Image.new()
						image.load(moddata["ModPreviewPath"])
						var imagetexture = ImageTexture.new()
						imagetexture.set_image(image)
						var mesh = $"Preview/Control/3DPreviewContainer/SubViewport/3D Preview/SkateLab SDK/Armature/Skeleton3D/Deck"
						var material_one = mesh.get_active_material(0)
						material_one.albedo_texture = imagetexture
						mesh.set_surface_override_material(0, material_one)
					$"Preview/Control/RichTextLabel".text = moddata["ModDescription"]
			)

func storevalue(foldername,modname,modimagepath,moddescription,modpath,modcategory,modpreview2d):
	autoload.loadmods[foldername] = {}
	autoload.loadmods[foldername]["ModName"] = str(modname)
	autoload.loadmods[foldername]["ModDescription"] = str(moddescription)
	autoload.loadmods[foldername]["ModPreviewPath"] = str(modimagepath)
	autoload.loadmods[foldername]["ModPath"] = str(modpath)
	autoload.loadmods[foldername]["ModCategory"] = str(modcategory)
	autoload.loadmods[foldername]["ModLoadedInGameFiles"] = false
	autoload.loadmods[foldername]["Toggled"] = true
	autoload.loadmods[foldername]["FolderName"] = str(foldername)
	autoload.loadmods[foldername]["ModPreview2d"] = modpreview2d
func unzip(zippath:String):
	var zipreader = ZIPReader.new()
	var newfolder = zippath.get_file()
	var modpath = ""
	var pakpath = autoload.modmanagerstoragedir
	var modsettingspath = null
	var modimagepath = null
	var modname = "Set A Mod Name"
	var foldername = ""
	var moddescription = "Set A Description"
	var modcategory = "Misc"
	var modpreview2d = true
	if zipreader.open(zippath) == OK:
		for filepath in zipreader.get_files():
			var zipextractloc :String = autoload.modmanagerstoragedir #this is stored in game files
			var diraccess = DirAccess.open(zipextractloc)
			newfolder = newfolder.trim_suffix(".zip")
			foldername = newfolder
			modname = newfolder #removes .zip from name
			diraccess.make_dir(zipextractloc+newfolder) #creates folder named after zip
			zipextractloc = zipextractloc+newfolder+"\\"
			modpath = zipextractloc #makes it so files get written to new folder
			var fileaccess = FileAccess.open(zipextractloc+filepath, FileAccess.WRITE)#write file to path
			fileaccess.store_buffer(zipreader.read_file(filepath))
			fileaccess.close()
			if filepath.get_extension() == "pak":
				print(filepath +" !!pak found")
				pakpath = modpath+filepath
			elif filepath.get_extension() == "txt":
				print("TXT Found")
				modsettingspath = modpath+filepath
				print(modsettingspath+" Path To The TXT")
			elif filepath.get_extension() == "png":
				print("Image Found Of Type " + filepath.get_extension())
				modimagepath = modpath+filepath
		if modsettingspath != null:
			var fa = FileAccess.open(modsettingspath, FileAccess.READ)
			var modset = str_to_var(fa.get_as_text())
			modname = modset[0]
			moddescription = modset[1]
			modcategory = modset[2]
			if modset.size() >= 4:
				modpreview2d = modset[3]
			else:
				print("not found")
		else:
			print("No Mod Settings Found In Zip.")
	storevalue(foldername,modname,modimagepath,moddescription,pakpath,modcategory,modpreview2d)
	save_data()

const save_file_path = "user://SkateLab-Mod-Manager.dat"
func save_data():
	var save_data = {
		save_mod = autoload.loadmods,
		save_gamedir = autoload.gamedir,
	}
	var save_file = FileAccess.open(save_file_path, FileAccess.WRITE)
	if save_file == null:
		print("ERROR CREATING SAVE FILE ", FileAccess.get_open_error())
		return
 
	var json_string = JSON.stringify(save_data)
	save_file.store_line(json_string)
 
func load_data():
	if !FileAccess.file_exists(save_file_path):
		print("save file not found")
		return
	var save_file = FileAccess.open(save_file_path, FileAccess.READ)
	var json_string = save_file.get_as_text()
	var json = JSON.new()
	var parse_result = json.parse(json_string)
	if not parse_result == OK:
		print("JSON parse error ", json.get_error_message(), " on line ", json.get_error_line())
		return
	var save_data = json.get_data()
	if "save_mod" in save_data:
		autoload.loadmods = save_data.save_mod
	if "save_gamedir" in save_data:
		autoload.gamedir = str(save_data.save_gamedir)
		print(autoload.gamedir)

func _on_start_game_pressed() -> void: #launch game from game directory path
	OS.execute(autoload.gamedir, [])

func _on_load_mods_pressed() -> void:
	var modpath :String = autoload.modsfolderdir
	$Window.popup()
	for mods in autoload.loadmods:
		if autoload.loadmods[mods]["Toggled"] == true and autoload.loadmods[mods]["ModLoadedInGameFiles"] == false:
			print(mods)
			print(autoload.loadmods[mods])
			var modfile = autoload.loadmods[mods]["ModPath"].get_file()
			var writeto = modpath+"\\"
			print(writeto)
			DirAccess.copy_absolute(autoload.loadmods[mods]["ModPath"],writeto+modfile)
			autoload.loadmods[mods]["ModLoadedInGameFiles"] = true
		if autoload.loadmods[mods]["Toggled"] == false and autoload.loadmods[mods]["ModLoadedInGameFiles"] == true:
			var modfile = autoload.loadmods[mods]["ModPath"].get_file()
			var writeto = modpath+"\\"
			print(writeto+modfile)
			OS.move_to_trash(writeto+modfile)
			autoload.loadmods[mods]["ModLoadedInGameFiles"] = false
	$Window.hide()
func _on_delete_mod_popup_confirmed() -> void:
	if autoload.selectedmod != null:
		var foldername = autoload.selectedmod["FolderName"]
		var modsfolder = autoload.modsfolderdir
		var datafolder = autoload.modmanagerstoragedir
		OS.move_to_trash(autoload.modsfolderdir+autoload.selectedmod["ModPath"].get_file())
		OS.move_to_trash(datafolder+foldername)
		autoload.loadmods.erase(foldername)
		save_data()
		get_tree().reload_current_scene()

func _on_remove_mod_pressed() -> void:
	$DeleteModPopup.popup_centered()
