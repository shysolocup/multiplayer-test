using Godot;
using System;

[GlobalClass]
public partial class Character : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 4.5f;
}
