// meta-name: On Server Behavior Template
// meta-description: A simple base template for a behavior/script set to run the server server
// meta-default: true

using _BINDINGS_NAMESPACE_;
using System;


// check the hover text for the base Behavior class for method names
[OnServer]
public partial class _CLASS_ : _BASE_
{
    // Called when the script node and its dependencies are ready.
    public override async void OnReady()
    {
        print("Hello World!");

        print(workspace);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override async void OnProcess(double delta)
    {
    }
}