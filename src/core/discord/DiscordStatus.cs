using Godot;

#nullable enable

[GlobalClass, Icon("uid://bhib8x7fhxxwd")]
public partial class DiscordStatus : Resource
{

    static public DiscordStatus Default = new DiscordStatus() {
        Details = "lorem ipsum",
        State = "you're not meant to see this",
        LargeImage = DiscordStatusImage.placeholder
    };

    [Export] public string Details = "lorem ipsum";
    [Export] public string? State;
    [Export] public DiscordStatusImage LargeImage = DiscordStatusImage.placeholder;
    [Export] public string? LargeImageText;
    [Export] public DiscordStatusImage SmallImage = DiscordStatusImage.placeholder;
    [Export] public string? SmallImageText;
    [Export] public int StartTimestamp;
    [Export] public int EndTimestamp;
}