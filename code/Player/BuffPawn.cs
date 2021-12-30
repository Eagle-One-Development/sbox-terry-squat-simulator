using Sandbox;
using System;
using System.Linq;
using TSS.UI;
using TSS;


partial class BuffPawn : Player
{
	[Net] private bool InSpace { get; set; } = true;
	Particles PortalPartic;
	public TimeSince timeInSpace;

	public override void Respawn()
	{
		SetModel( "models/terry_buff/terry_buff.vmdl" );

		//
		// Use WalkController for movement (you can make your own PlayerController for 100% control)
		//
		//Controller = new WalkController();

		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new BuffAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		Camera = new BuffCam();

		(Camera as BuffCam).ZNear = 0.001f;

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		//Transmit = TransmitType.Always;

		var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Void );
		Position = ent.Position;
		Rotation = ent.Rotation;

		CreatePortal( To.Single( this ) );
		timeInSpace = 0f;

		base.Respawn();
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
		Log.Info( other.Tags.List.First() );
		if ( other.Tags.List.First() == "ending" )
		{
			//StartEnding();
		}
	}

	[ClientRpc]
	public void StartEndingCL()
	{
		EndingPanel.Instance.CanGoToNature = true;
		PortalPartic.Destroy( true );
	}

	public void StartEnding()
	{
		Controller = new WalkController();
	}

	[ServerCmd( "nature" )]
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
		(Camera as BuffCam).ZNear = 0.2f;

		SetAnimBool( "b_noclip", InSpace );

		if ( !InSpace )
		{
			timeInSpace = 0f;
		}

		if ( !IsClient && InSpace )
		{
			var port = All.OfType<EndPortal>().FirstOrDefault();
			var spawn = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Void );
			Position = spawn.Position.LerpTo( port.Position.WithZ(port.Position.z - 30), timeInSpace / 75f);
			if ( timeInSpace / 75f >= 0.95f )
			{
				StartEndingCL( To.Single( this ) );
				StartEnding();
				InSpace = false;
			}
		}
	}

	[ClientRpc]
	public void CreatePortal()
	{
		Log.Info( "Portal Particle created" );
		var port = All.OfType<EndPortal>().FirstOrDefault();
		PortalPartic = Particles.Create( "particles/void/stars_pull.vpcf", port.Position );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}
