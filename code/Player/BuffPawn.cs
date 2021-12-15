using Sandbox;
using System;
using System.Linq;
using TSS.UI;
using TSS;


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

		(Camera as ThirdPersonCamera).ZNear = 0.001f;

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

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
		Log.Info( other.Tags.List.First() );
		if ( other.Tags.List.First()  == "ending" )
		{
			StartEnding();
		}
	}

	[ClientRpc]
	public void StartEnding()
	{
		EndingPanel.Instance.CanGoToNature = true;
	}

	[ServerCmd("nature")]
	public static void GoToNature()
	{
		var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Nature );
		ConsoleSystem.Caller.Pawn.Position = ent.Position;
		ConsoleSystem.Caller.Pawn.Rotation = ent.Rotation;
		TSSGame.Current.StopInstrumental();
		TSSGame.Current.StartNature();
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
		(Camera as ThirdPersonCamera).ZNear = 0.2f;

	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}
