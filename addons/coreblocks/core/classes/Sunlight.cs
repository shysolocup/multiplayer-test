using Godot;
using System;

/// <summary>
/// class exclusively for light energy scaling
/// </summary>
[Tool]
[GlobalClass]
public partial class Sunlight : DirectionalLight3D
{
	[Export]
	public float BaseEnergy = 1;
	[Export]
	public Color BaseColor = Colors.White;
}
