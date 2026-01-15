using System;


/// <summary>
/// tells the script to have the script or method run on the client
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class OnServerAttribute : Attribute
{
}