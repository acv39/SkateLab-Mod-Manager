extends Node
#autoload because i am stupid and didn't realize my vars reset every time scene changes so we store them here
var loadmods = {
	
}
var gamedir := ""
var moddir := ""
var modmanagerstoragedir :String = ""
var modsfolderdir := ""
var scene :bool = true #if the main scene is true or false, bandaid fix for a crash
var isgamedirset :bool = false #makes sure game dir is set, so shit dont break
var modset = "skibidi"
var ModNode = preload("res://Other/mod.tscn")
var creatednodes = {}
var selectedmod = null
var extractscript = load("res://Scripts/ZipExtract.cs")
var extractnode = extractscript.new()
var filesscene = preload("res://Other/files.tscn")
