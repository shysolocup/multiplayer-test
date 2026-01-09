using System;
using System.Linq;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass, Icon("uid://dfm372ed5c238")]
public partial class BehaviorFolder : Node
{

    [Export]
    public bool Enabled
    {
        get => ProcessMode != ProcessModeEnum.Disabled;
        set => ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    public Array<Behavior> GetBehaviors()
    {
		var result = new Array<Behavior>();
		foreach (var script in GetChildren().Where(n => n is Behavior).Cast<Behavior>())
		{
			result.Add(script);
		}
		return result;
    }

    public void MassCall<T>(StringName methodName, params Variant[] args) where T : Behavior
    {
        foreach (T script in GetBehaviors().Cast<T>())
        {
            script.Call(methodName, args);
        }
    }

    public void MassCallDeferred<T>(StringName methodName, params Variant[] args) where T : Behavior
    {
        foreach (T script in GetBehaviors().Cast<T>())
        {
            script.CallDeferred(methodName, args);
        }
    }
}