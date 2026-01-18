@icon("uid://bhib8x7fhxxwd")
class_name DiscordRPCNode
extends Node

@export var AppId: String = "1335860356379312178"
@export var Details: String = "Test"
@export var State: String = "Test"
@export var LargeImage: String = "placeholder"
@export var LargeImageText: String = "placeholder"
@export var SmallImage: String = "placeholder"
@export var SmallImageText: String = "placeholder"
@export var StartTimestamp: int
@export var EndTimestamp: int

func IsWorking():
	return DiscordRPC.get_is_discord_working()
	
func RefreshRPC():
	DiscordRPC.app_id = int(AppId)
	DiscordRPC.details = Details
	DiscordRPC.state = State
	DiscordRPC.large_image = LargeImage
	DiscordRPC.large_image_text = LargeImageText
	DiscordRPC.small_image = SmallImage
	DiscordRPC.small_image_text = SmallImageText
	
	NewTimestamp()
	
	DiscordRPC.start_timestamp = StartTimestamp
	DiscordRPC.end_timestamp = EndTimestamp
	
	DiscordRPC.refresh() 
	
func NewTimestamp():
	StartTimestamp = int(Time.get_unix_time_from_system())
	EndTimestamp = int(Time.get_unix_time_from_system()) + 3600
