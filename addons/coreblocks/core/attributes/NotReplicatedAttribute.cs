using System;
using Godot;


/// <summary>
/// makes it so a class can be excluded from auto replication
/// <para/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class NotReplicatedAttribute : Attribute
{
}