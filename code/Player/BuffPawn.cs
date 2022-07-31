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

	[Net] public TSSSpawn EndPoint { get; set; }

	[Net] public bool CreditsStarted { get; set; }

	[ClientRpc]
	public void StartCredits()
	{
		EndingPanel.Instance.StartCredits();
	}

	public override void Respawn()
	{
		SetModel( "models/terry_buff/terry_buff.vmdl" );

		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new BuffAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		CameraMode = new BuffCam();

		(CameraMode as BuffCam).ZNear = 0.001f;

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Void );
		Position = ent.Position;
		Rotation = ent.Rotation;

		EndPoint = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.NatureExercise );
		
		

		CreatePortal( To.Single( this ) );
		timeInSpace = 0f;

		base.Respawn();
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
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

	[ConCmd.Server( "nature" )]
	public static void GoToNature()
	{
		var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Nature );
		ConsoleSystem.Caller.Pawn.Position = ent.Position;
		ConsoleSystem.Caller.Pawn.Rotation = ent.Rotation;
		TSSGame.Current.StopInstrumental();
		TSSGame.Current.StartNature();
	}

	[ConCmd.Server( "credits" )]
	public static void ChangePawn()
	{
		var pl = new TSSPlayer();
		var oldPawn = ConsoleSystem.Caller.Pawn;
		(oldPawn as BuffPawn).CreateCredits();
		TSSGame.Current.PlayCredits();
		ConsoleSystem.Caller.Pawn = pl;
		pl.SkipIntro = true;
		pl.Respawn();
		oldPawn.Delete();
	}

	[ClientRpc]
	public void CreateCredits()
	{
		
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
		(CameraMode as BuffCam).ZNear = 0.2f;

		SetAnimParameter( "b_noclip", InSpace );

		if ( !InSpace )
		{
			timeInSpace = 0f;
		}

		if ( EndPoint != null )
		{

			if ( !IsClient )
			{

				if ( Vector3.DistanceBetween( Position, EndPoint.Position ) < 200f )
				{
					if ( !CreditsStarted )
					{
						StartCredits();
						CreditsStarted = true;
					}
					
				}
			}
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
		var port = All.OfType<EndPortal>().FirstOrDefault();
		PortalPartic = Particles.Create( "particles/void/stars_pull.vpcf", port.Position );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}
