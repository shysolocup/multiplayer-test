using System;
using Godot;
using Godot.Collections;


[GlobalClass]
public partial class EventBus : Singleton<EventBus>
{
    public OptimalEvent _CameraType => new(Game.Systems.CameraSystem, CameraSystem.SignalName.CameraTypeChanged);
    public OptimalEvent _CameraSubject => new(Game.Systems.CameraSystem, CameraSystem.SignalName.SubjectChanged);

    public static class Events
    {
        public static readonly StringName loaded = new("_Loaded");
        public static readonly StringName cameraType = new("_CameraType");
        public static readonly StringName cameraSubject = CameraSystem.SignalName.SubjectChanged;
        public static readonly StringName currentCamera = CameraSystem.SignalName.CurrentCameraChanged;
        public static readonly StringName freecamOn = CameraSystem.SignalName.FreecamEnabled;
        public static readonly StringName freecamOff = CameraSystem.SignalName.FreecamEnabled;
        public static readonly StringName shiftlockOn = CameraSystem.SignalName.ShiftLockEnabled;
        public static readonly StringName shiftlockOff = CameraSystem.SignalName.ShiftLockDisabled;
    }


    public void On(StringName @event, Action callback)
    {
        var prop = this.GetType().GetProperty(@event.ToString(), 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public
        );
        
        if (prop.GetValue(null) is OptimalEvent ev)
        {
            ev += callback;
        }
    }


    public MultiplayerRemote GetEvent(NodePath name)
        => GetNode(name) is MultiplayerRemote remote ? remote : null;


    public override async void _Ready()
    {
        base._Ready();

        var game = await Game.Instance();

        await Game.WaitUntilConnected();
    }
}