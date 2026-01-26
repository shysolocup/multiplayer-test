using Godot;

public static class Enum
{
    public enum PriorityChannel
    {
        Master,
		Ui,
		Camera
    }

    public enum CameraType
    {
        ThirdPerson = 0,
		FirstPerson = 2,
		Custom = 3
    }

    public enum RunContext
    {
        Client = 0,
		Server = 1
    }

    public enum TextChannel
    {
        General = 0,
        Team = 1
    }
}