using Sandbox;
using System;
using System.Linq;
using MinimalExample;


public enum Exercise
{
	Squat = 0,
	Run = 1,
	Punch = 2
}
public partial class TSSPlayer : Player
{
	[Net]
	public int ExercisePoints { get; set; }
	public float ScaleTar;
	[Net]
	public bool IntroComplete { get; set; }

	[Net]
	public Exercise MyExercise { get; set; }

	[Net]
	public static TSSPlayer Instance { get; set; }

	[Net, Predicted]
	public int squat { get; set; }
	[Net, Predicted]
	private int lastSquat { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceExerciseStopped { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceRun { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceUpPressed { get; set; }
	public ModelEntity DumbBell;
	[Net, Predicted]
	public TimeSince TimeSinceDownPressed { get; set; }
	private TimeSince aTime;
	public float curSpeed;
	private float tCurSpeed;

	[Net, Predicted]
	public TimeSince TimeSincePunch { get; set; }
	[Net]
	public float TimeToNextPunch { get; set; }

	[Net]
	public bool MusicStarted { get; set; }

	public ModelEntity SodaCan;
	[Net, Predicted]
	public TimeSince TimeSinceSoda { get; set; }

	void Dress()
	{
		var _pants = new ModelEntity( "models/clothes/fitness/shorts_fitness.vmdl", this );
		var _shirt = new ModelEntity( "models/clothes/fitness/shirt_fitness.vmdl", this );
		var _shoes = new ModelEntity( "models/clothes/fitness/shoes_sneakers.vmdl", this );
		var _wrist = new ModelEntity( "models/clothes/fitness/sweatband_wrists.vmdl", this );
		var _headBand = new ModelEntity( "models/clothes/fitness/sweatband_head.vmdl", this );

		var _hair = new ModelEntity( "models/clothes/fitness/hair_head.vmdl", this );
		var _bodyHair = new ModelEntity( "models/clothes/fitness/hair_body.vmdl", this );
	}

	public override void CreateHull()
	{
		CollisionGroup = CollisionGroup.Player;
		AddCollisionLayer( CollisionLayer.Player );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -8, -8, 0 ), new Vector3( 8, 8, 72 ) );

		MoveType = MoveType.MOVETYPE_WALK;
		EnableHitboxes = true;
	}

	public override void Respawn()
	{
		SetModel( "models/terry/terry.vmdl" );
		MyExercise = Exercise.Squat;
		Position = Entity.All.OfType<SquatSpawn>().First().Position;
		Rotation = Entity.All.OfType<SquatSpawn>().First().Rotation;
		
		squat = 0;
		lastSquat = -1;
		TimeSinceExerciseStopped = 4f;
		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new TSSPlayerAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		Camera = new TSSCamera();

		TimeToNextPunch = 1.1f;

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		DumbBell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
		DumbBell.SetParent( this, "head" );
		DumbBell.Rotation = this.Rotation * Rotation.From( 0, 0, 90 );
		//DumbBell.EnableDrawing = false;
		Dress();

		Instance = this;


		SodaCan = new ModelEntity();
		SodaCan.SetModel( "models/soda/soda.vmdl" );
		SodaCan.SetParent( this, "Soda" );
		SodaCan.LocalPosition = Vector3.Zero;
		SodaCan.LocalRotation = Rotation.Identity;
		SodaCan.EnableDrawing = false;
		

		base.Respawn();
	}

	
	/// <summary>
	/// This runs a trace which will click on a food item in the world and consume or destroy it
	/// </summary>
	public void ClickFood()
	{
		if ( Input.Pressed( InputButton.Attack1 ) )
		{
			TraceResult tr2 = Trace.Ray(Input.Cursor, 1000f ).HitLayer(CollisionLayer.All, true).Run();

			//DebugOverlay.Line( tr2.StartPos, tr2.EndPos, 10f, false );
			//DebugOverlay.Sphere( tr2.EndPos, 10f, Color.Red, false, 10f );

			if ( tr2.Hit )
			{
				Log.Info( tr2.Entity.GetType().ToString() );
				if ( tr2.Entity is Food food )
				{
					if ( IsServer )
					{
						food.RemoveFood();
					}
				}
			}
		}
	}

	/// <summary>
	/// For some reason this can't be called in spawn without being delayed, otherwise the music plays for like a 10th of a second then cuts out entirely.
	/// </summary>
	public async void PlayMusic()
	{
		await GameTask.Delay( 1000 );
		TSSGame.CurrentGame.StartMusic();
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );
		Log.Info( other );
	}


	
	[ClientRpc]
	public void InitiateSoda()
	{
		SetAnimBool( "Drink", true );


	}

	[ClientRpc]
	public void StopSoda()
	{
		SetAnimBool( "Drink", false );

	}

	


	/// <summary>
	/// This is basically a rough function that will switch the exercises as you reach a certain number of points
	/// Eventually after ever exercise has been discovered, we should just cycle between them at random
	/// TODO: Move this to a better system
	/// </summary>
	public void ExerciseTimeline()
	{
		//Switch to the punch state once we reach 200 exercie points
		//TODO: Move this kind of behavior to some kind of scripting system.
		if ( ExercisePoints == 200 && IsServer )
		{
			var ent = Entity.All.OfType<RunSpawn>().First();
			ChangeExercise( Exercise.Run, ent.Transform.Position, ent.Transform.Rotation );
			ExercisePoints++;
			DumbBell?.Delete();
			DumbBell = null;
			return;
		}

		//Switch to the punch state once we reach 200 exercie points
		//TODO: Move this kind of behavior to some kind of scripting system.
		if ( ExercisePoints == 300 && IsServer )
		{
			var ent = Entity.All.OfType<PunchSpawn>().First();
			ChangeExercise( Exercise.Punch, ent.Transform.Position, ent.Transform.Rotation );
			ExercisePoints++;
			DumbBell?.Delete();
			DumbBell = null;

			return;
		}

		if(ExercisePoints > 50 )
		{
			SetAnimBool( "Angry", TimeSinceExerciseStopped < 2f );
		}
	}

	/// <summary>
	/// This method can be called from the server to initiate the soda drinking animation.
	/// </summary>
	public void DrinkSoda()
	{
		if ( DumbBell.IsValid() )
		{
			DumbBell.EnableDrawing = false;
		}
		if ( SodaCan.IsValid() )
		{
			SodaCan.EnableDrawing = true;
		}
		TimeSinceSoda = 0;
		InitiateSoda();
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( !MusicStarted )
		{
			PlayMusic();
			MusicStarted = true;
		}

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );
		TSSCamera cam = (Camera as TSSCamera);

		if ( Input.Pressed( InputButton.Reload ) && IsServer)
		{
			//Vector3 position = Transform.Position + Vector3.Up * 64f;
			//position += Rotation.Forward * Rand.Float( 30f, 50f );
			//position += Vector3.Up * Rand.Float( -40f, 20f );
			//
			//float f = Rand.FromArray(new[] { -1f, 1f });
			//
			//position += Rotation.Right * f * 20f;
			//position += Rotation.Right * f * Rand.Float( 0f, 40f );
			//var food = new Food();
			//food.Position = position;
			
		}


		if(TimeSinceSoda > 1.7f )
		{
			if ( DumbBell.IsValid() )
			{
				DumbBell.EnableDrawing = true;
			}
			if ( SodaCan.IsValid() )
			{
				SodaCan.EnableDrawing = false;
			}
			StopSoda();
		}


		ClickFood();

		switch ( MyExercise )
		{
			case Exercise.Squat:
				Squatting( cam );
				break;
			case Exercise.Run:
				Running( cam );
				break;
			case Exercise.Punch:
				Punching( cam );
				break;
		}



		ExerciseTimeline();

		if ( DumbBell != null )
		{
			DumbBell.LocalPosition = Vector3.Zero + DumbBell.Transform.Rotation.Right * 15.5f;

		}
		else
		{
			//Create a new dumbbell if we're squatting.
			if(MyExercise== Exercise.Squat )
			{
				if ( IsServer )
				{
					DumbBell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
					DumbBell.SetParent( this, "head" );
					DumbBell.Rotation = this.Rotation * Rotation.From( 0, 0, 90 );
				}
			}
		}

		// Basically curSpeed increases based on how fast the player is squatting etc

		tCurSpeed = tCurSpeed.Clamp( 0, 1 );
		curSpeed = curSpeed.Clamp( 0, 1 );
		var mult = MathX.LerpInverse( TimeSinceExerciseStopped, 0, 1 );
		if(MyExercise == Exercise.Run )
		{
			mult = MathX.LerpInverse( TimeSinceRun, 0, 1 );
		}

		tCurSpeed = tCurSpeed.LerpTo( 0f, Time.Delta * 0.25f * ( mult * 4f));
		curSpeed = curSpeed.LerpTo( tCurSpeed, Time.Delta * 2f );
		//Log.Info( "curSpeed: " + curSpeed );
		//Log.Info( "tCurSpeed: " + tCurSpeed );

		Scale = Scale.LerpTo( 1, Time.Delta * 10f );
		//Log.Info( Scale );

		if ( cam.SCounter != null )
		{
			var c = cam.SCounter;
			//c.l?.SetText( "Exercise Points: " + ExercisePoints );
			c.l?.SetText( ExercisePoints.ToString() );
			c.Opacity += Time.Delta * curSpeed * 0.4f;

 			c.TextScale = cam.SCounter.TextScale.LerpTo( 1.5f * MathX.Clamp( curSpeed + 0.8f, 0, 1), Time.Delta * 2f );
			float anim = MathF.Sin( aTime );
			c.Rotation = Rotation.From( 0, 90, anim * curSpeed * 1f * (ExercisePoints / 100f) );
		}
	}

	/// <summary>
	/// Sets the scale of the counter for extra juice when getting points
	/// </summary>
	/// <param name="f">The scale to set the counter to</param>
	public async void CounterBump(float f )
	{
		await GameTask.DelaySeconds( 0.1f );
		if ( (Camera as TSSCamera).SCounter != null )
		{
			var c = (Camera as TSSCamera).SCounter;
 			c.TextScale += f * curSpeed;
		}
	}

	/// <summary>
	/// Sets the scale of the player for extra juice when getting points
	/// </summary>
	/// <param name="f">The scale to set the player</param>
	public async void SetScale(float f )
	{
		await GameTask.DelaySeconds( 0.1f );
		Scale = f;
	}

	

	/// <summary>
	/// Makes the player punch and moves the 'squat' variable so it alternated between left and right punches
	/// </summary>
	public void Punch()
	{
		ExercisePoints++;
		tCurSpeed += 0.1f;
		CreatePoint( 1 );
		Scale = 1.2f;
		CounterBump( 0.5f );
		TimeSinceExerciseStopped = 0;

		

		Log.Info( "SORRY WHAT?" );

		if(squat == 0 )
		{
			squat = 1;
			return;
		}

		if(squat == 1 )
		{
			squat = 0;
			return;
		}
	}





	

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}

