using Godot;

[GlobalClass]
public partial class Character : CharacterBody3D
{
	
	public Node3D Mesh { get; set; }
	public Node3D Rig { get; set; }
	
	public MeshInstance3D Face { get; set; }
	public MeshInstance3D Head { get; set; }
	public MeshInstance3D LeftArm { get; set; }
	public MeshInstance3D LeftLeg { get; set; }
	public MeshInstance3D RightArm { get; set; }
	public MeshInstance3D RightLeg { get; set; }
	public MeshInstance3D Torso { get; set; }
	public AnimationPlayer Animator { get; set; }
	public MeshInstance3D Username { get; set; }

	public CollisionShape3D Collision { get; set; }


	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 15f;


	public override void _Ready() 
	{
		base._Ready();
		
		Mesh = GetNode<Node3D>("./mesh");
		Collision = GetNode<CollisionShape3D>("./collision");
		Rig = Mesh.GetNode<Node3D>("./Rig");
		Animator = Mesh.GetNode<AnimationPlayer>("./AnimationPlayer");

		Face = Mesh.GetNode<MeshInstance3D>("./Face");
		Head = Mesh.GetNode<MeshInstance3D>("./Head");
		LeftArm = Mesh.GetNode<MeshInstance3D>("./Left Arm");
		LeftLeg = Mesh.GetNode<MeshInstance3D>("./Left Leg");
		RightArm = Mesh.GetNode<MeshInstance3D>("./Right Arm");
		RightLeg = Mesh.GetNode<MeshInstance3D>("./Right Leg");
		Torso = Mesh.GetNode<MeshInstance3D>("./Torso");
	}
}
