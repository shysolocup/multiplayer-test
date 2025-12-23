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


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();		
	}
}
