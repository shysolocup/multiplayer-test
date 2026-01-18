using System;


/// <summary>
/// behaviors usually wait until the player has connected to run, this makes it so it doesn't
/// <para/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PrerunnerAttribute : Attribute
{
}