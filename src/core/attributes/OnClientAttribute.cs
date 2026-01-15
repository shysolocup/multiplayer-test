using System;


/// <summary>
/// by default all scripts run on the client, this just tells it specifically for methods
/// <para/>
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class OnClientAttribute : Attribute
{
}