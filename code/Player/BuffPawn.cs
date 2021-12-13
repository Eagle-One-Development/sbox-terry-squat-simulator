using Sandbox;
using System;
using System.Linq;


partial class BuffPawn : Player
{
	public override void Respawn()
	{
		SetModel( "models/terry_buff/terry_buff.vmdl" );

		//
		// Use WalkController for movement (you can make your own PlayerController for 100% control)
		//
		Controller = new WalkController();

		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new StandardPlayerAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		Camera = new ThirdPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		//Transmit = TransmitType.Always;

		var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Void );
		Position = ent.Position;
		Rotation = ent.Rotation;

		base.Respawn();
	}



	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );

	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}
