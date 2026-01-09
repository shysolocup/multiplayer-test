using Godot;
using System;

/// <summary>
/// class exclusively for light energy scaling
/// </summary>
public partial class Sunlight : DirectionalLight3D
{
	[Export]
	public float MaxEnergy = 1;
}
