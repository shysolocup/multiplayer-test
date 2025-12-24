// meta-name: Behavior Template
// meta-description: A simple base behavior/script template
// meta-default: true

using _BINDINGS_NAMESPACE_;
using System;

[Tool]
public partial class _CLASS_ : _BASE_
{
    // Called when the script node and its dependencies are ready.
    public override void OnReady()
    {
        base.OnReady();

        print(workspace);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void OnProcess(double delta)
    {
        base.OnProcess(delta);
    }
}