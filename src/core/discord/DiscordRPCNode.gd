class_name DiscordRPCNode
extends Node

var AppId: int = 1335860356379312178
var Details: String = "Test"
var State: String = "Test"
var LargeImage: String = "placeholder"
var LargeImageText: String = "placeholder"
var SmallImage: String = "placeholder"
var SmallImageText: String = "placeholder"
var StartTimestamp: int
var EndTimestamp: int

func IsWorking():
	return DiscordRPC.get_is_discord_working()
	
func RefreshRPC():
	DiscordRPC.app_id = AppId
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