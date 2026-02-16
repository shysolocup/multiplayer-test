using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

#nullable enable

public partial class ActorBinding<T> where T : struct
{
    public Array<Node> Actors = [];
    public T? Value { get; set; }
    /// <summary>
    /// (when implemented) if the binding should reset or not when actors are emptied
    /// <para/> this is useless on its own but useful in implementations
    /// </summary>
    public bool Persist = false;

    public bool Acted => Actors.Count > 0;
}

#nullable disable