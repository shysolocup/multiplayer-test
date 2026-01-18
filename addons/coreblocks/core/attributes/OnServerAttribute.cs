using System;


/// <summary>
/// tells the script or method to run on the server by default
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class OnServerAttribute : Attribute
{
}