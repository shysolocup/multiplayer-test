using System;


/// <summary>
/// behaviors usually wait until the player has connected to run, this makes it so it doesn't
/// <para/>
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class PrerunnerAttribute : Attribute
{
}