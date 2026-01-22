using System;
using Godot;


/// <summary>
/// tells the script or method to run on the server by default
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class OnServerAttribute : Attribute
{
}