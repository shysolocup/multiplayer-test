using Godot;
using System;
using Core;

[GlobalClass, Icon("uid://fmiylsiygwmg")]
public partial class CharacterSystem : Node3D
{	
	[Export]
	public Character StarterCharacter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StarterCharacter ??= GetNode<Character>("./starterCharacter");
	}
	
	public override void _PhysicsProcess(double delta)
	{

		foreach (var child in GetChildren())
		{
			if (child is Character character)
			{
				Vector3 velocity = character.Velocity;

				if (!character.IsOnFloor())
				{
					velocity += character.GetGravity() * (float)delta;
				}

				if (Input.IsActionJustPressed("ui_accept") && character.IsOnFloor())
				{
					velocity.Y = character.JumpVelocity;
				}

				Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
				Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

				if (direction != Vector3.Zero)
				{
					velocity.X = direction.X * character.Speed;
					velocity.Z = direction.Z * character.Speed;
				}
				
				else
				{
					velocity.X = Mathf.MoveToward(character.Velocity.X, 0, character.Speed);
					velocity.Z = Mathf.MoveToward(character.Velocity.Z, 0, character.Speed);
				}

				character.Velocity = velocity;
				character.MoveAndSlide();		
			}
		}
	}
}
