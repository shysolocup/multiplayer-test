using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class OnServerAttribute : Attribute
{
    public Behavior.ContextEnum Context = Behavior.ContextEnum.Server;
}