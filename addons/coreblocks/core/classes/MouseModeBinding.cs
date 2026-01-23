using Godot;
using Godot.Collections;

public partial class MouseModeBinding : Resource
{
    public Array<Node> Actors = [];
    public Input.MouseModeEnum? Mode;
    public bool Persist = false;
}