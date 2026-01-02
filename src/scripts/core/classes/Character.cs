using Godot;
using System;

[GlobalClass]
public partial class Character : CharacterBody3D
{
	
	public MeshInstance3D Mesh { get; set; }
	public CollisionShape3D Collision { get; set; }


	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 4.5f;


	public override void _Ready() 
	{
		base._Ready();
		
		Mesh = GetNode<MeshInstance3D>("./mesh");
		Collision = GetNode<CollisionShape3D>("./collision");
	}
}
