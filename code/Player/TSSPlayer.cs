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

		Dress();

		Instance = this;


		SodaCan = new ModelEntity();
		SodaCan.SetModel( "models/soda/soda.vmdl" );
		SodaCan.SetParent( this, "Soda" );
		SodaCan.LocalPosition = Vector3.Zero;
		SodaCan.LocalRotation = Rotation.Identity;
		

		base.Respawn();
	}

	public void ChangeExercise( Exercise state, Vector3 pos, Rotation rot)
	{
		Position = pos;
		Rotation = rot;
		MyExercise = state;
	}

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


	public void GivePoints( int i, bool fall = false )
	{
		SetScale( 1.2f );
		CounterBump( 0.5f );
		ExercisePoints += i;
		TimeSinceExerciseStopped = 0;
		tCurSpeed += 0.1f;
		CreatePoint( 1, true, 5 );
	}

	public void GivePointsAtPosition( int i,Vector3 pos, bool fall = false )
	{
		SetScale( 1.2f );
		CounterBump( 0.5f );
		ExercisePoints += i;
		TimeSinceExerciseStopped = 0;
		tCurSpeed += 0.1f;
		CreatePointAtPosition( 1, pos, fall, i );
	}


	[ClientRpc]
	public void CreatePoint( int i, bool fall = false, int count = 1)
	{

		Log.Info( "HEY");
		if ( IsClient )
		{
			for ( int j = 0; j < count; j++ )
			{
				var p = new ExercisePointPanel( i, ExercisePoints );
				Vector3 pos = Position + Vector3.Up * 48f;

				Vector3 dir = ((Camera as TSSCamera).Position - pos).Normal;
				Rotation dirRand = Rotation.From( Rand.Float( -45f, 45f ), Rand.Float( -45f, 45f ), Rand.Float( -45f, 45f ) );
				p.Position = pos + (dir * dirRand) * 28f;

				p.InitialPosition = p.Position;
				p.TextScale = 1.5f;
				p.Fall = fall;
			}
		}
		
	}

	[ClientRpc]
	public void CreatePointAtPosition( int i, Vector3 pos,bool fall = false, int count = 1 )
	{

		Log.Info( "HEY" );
		if ( IsClient )
		{
			for ( int j = 0; j < count; j++ )
			{
				var p = new ExercisePointPanel( i, ExercisePoints );
				p.Position = pos;
				p.InitialPosition = p.Position;
				p.TextScale = 1.5f;
				p.Fall = fall;
			}
		}

	}


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
			SetAnimBool( "Drink", true );
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

	public async void CounterBump(float f )
	{
		await GameTask.DelaySeconds( 0.1f );
		if ( (Camera as TSSCamera).SCounter != null )
		{
			var c = (Camera as TSSCamera).SCounter;
 			c.TextScale += f * curSpeed;
		}
	}

	public async void SetScale(float f )
	{
		await GameTask.DelaySeconds( 0.1f );
		Scale = f;
	}

	public void Running(TSSCamera cam )
	{
		SetAnimInt( "squat", -1 );
		SetAnimFloat("move_x", MathX.LerpTo( 0, 350f, (curSpeed * 4f).Clamp(0,1f)) );

		if ( cam == null )
		{

			return;
		}


		if ( TimeSinceRun < 3f && squat != -1)
		{
			cam.Progress += Time.Delta * 0.35f;
		}

		if ( Input.Pressed( InputButton.Right ) && Input.Pressed( InputButton.Left ) )
		{
			return;
		}

		if ( Input.Pressed( InputButton.Right ) && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f )
		{
			if ( squat == 0 )
			{
				tCurSpeed += 0.1f;
				CreatePoint(1);
				SetScale( 1.2f );
				CounterBump( 0.5f );
				TimeSinceExerciseStopped = 0;
				ExercisePoints++;
				if ( cam.Up != null )
					cam.Up.TextScale += 0.3f;
			}
			squat = 1;
			TimeSinceUpPressed = 0;
		}

		if ( Input.Pressed( InputButton.Left ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f )
		{
			squat = 0;
			TimeSinceDownPressed = 0;
			if ( cam.Down != null )
				cam.Down.TextScale += 0.3f;
		}



	}

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

	public void Punching( TSSCamera cam )
	{

		if ( cam == null )
		{
			return;
		}

		SetAnimInt( "punch", squat );
		SetAnimInt( "squat", -1 );

		if(TimeSincePunch > TimeToNextPunch )
		{
			TimeSincePunch = 0;

			if ( IsServer )
			{
				var pt = new PunchQT();
				pt.Player = this;
				pt.TargetTime = 1f;
				pt.MyTime = 1f;
				pt.Type = Rand.Int( 0, 3 );
			}

			
		}


		



	}


	public void Squatting(TSSCamera cam)
	{

		if(cam == null )
		{
			return;
		}
		 
		SetAnimInt( "squat", squat );

	if ( TimeSinceExerciseStopped < 3f && squat != -1 && !IntroComplete )
	{
		float f = (TimeSinceExerciseStopped - 1f) / 3f;
		f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
		cam.Progress += Time.Delta * 0.025f * (1 - f);


		if ( cam.Progress >= 1f )
		{
			IntroComplete = true;
		}
	}

	if ( TimeSinceExerciseStopped < 3f && squat != -1 && IntroComplete )
	{
		cam.Progress += Time.Delta * 0.35f;
	}

	if( Input.Pressed( InputButton.Forward )  && Input.Pressed( InputButton.Back ) )
	{
		return;
	}

		if ( Input.Pressed( InputButton.Forward )  && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f)
		{
			if ( squat == 0 )
			{

				ExercisePoints++;
				tCurSpeed += 0.1f;
				CreatePoint(1);
				SetScale( 1.2f );
 				CounterBump( 0.5f );
				TimeSinceExerciseStopped = 0;
				Log.Info( $"SQUAT: {ExercisePoints}" );
				if (cam.Up != null)
					cam.Up.TextScale += 0.3f;
			}
			squat = 1;
			TimeSinceUpPressed = 0;

		}

		if ( Input.Pressed( InputButton.Back ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f)
		{
			squat = 0;
			TimeSinceDownPressed = 0;
			if ( cam.Down != null )
				cam.Down.TextScale += 0.3f;
		}

		

	}

	public void SetState(Exercise state )
	{
		MyExercise = state;
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}

