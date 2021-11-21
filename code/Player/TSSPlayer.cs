using Sandbox;
using System;
using System.Linq;

public enum Exercise
{
	Squat = 0,
	Run = 1,
	Punch = 2,
	Yoga = 3
}

namespace TSS
{
	public partial class TSSPlayer : Player
	{
		[Net]
		public int ExercisePoints { get; set; }

		public float ScaleTar;

		[Net]
		public bool IntroComplete { get; set; }

		[Net]
		public Exercise CurrentExercise { get; set; }

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

		public ModelEntity Barbell;

		[Net, Predicted]
		public TimeSince TimeSinceDownPressed { get; set; }
		private TimeSince aTime;

		public float curSpeed;
		private float tCurSpeed;

		[Net, Predicted]
		public TimeSince TimeSincePunch { get; set; }

		[Net]
		public float TimeToNextPunch { get; set; }

		public TimeSince TimeSinceYoga { get; set; }

		[Net]
		public bool MusicStarted { get; set; }

		public ModelEntity SodaCan;

		[Net, Predicted]
		public TimeSince TimeSinceSoda { get; set; }


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
			Instance = this;

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			SetModel( "models/terry/terry.vmdl" );
			Dress();

			Animator = new TSSPlayerAnimator();
			Camera = new TSSCamera();

			ChangeExercise( Exercise.Squat );

			TimeSinceExerciseStopped = 4f;

			SodaCan = new ModelEntity();
			SodaCan.SetModel( "models/soda/soda.vmdl" );
			SodaCan.SetParent( this, "Soda" );
			SodaCan.LocalPosition = Vector3.Zero;
			SodaCan.LocalRotation = Rotation.Identity;
			SodaCan.EnableDrawing = false;

			base.Respawn();
		}

		void Dress()
		{
			_ = new ModelEntity( "models/clothes/fitness/shorts_fitness.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/shirt_fitness.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/shoes_sneakers.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_wrists.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_head.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/hair_head.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/hair_body.vmdl", this );
		}

		/// <summary>
		/// For some reason this can't be called in spawn without being delayed, otherwise the music plays for like a 10th of a second then cuts out entirely.
		/// </summary>
		public async void PlayMusic()
		{
			await GameTask.Delay( 1000 );
			TSSGame.Current.StartMusic();
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
		/// This method can be called from the server to initiate the soda drinking animation.
		/// </summary>
		public void DrinkSoda()
		{
			if ( Barbell.IsValid() )
			{
				Barbell.EnableDrawing = false;
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
			TSSCamera cam = (Camera as TSSCamera);

			if ( TimeSinceSoda > 1.7f )
			{
				if ( Barbell.IsValid() )
				{
					Barbell.EnableDrawing = true;
				}
				if ( SodaCan.IsValid() )
				{
					SodaCan.EnableDrawing = false;
				}

				StopSoda();
			}

			DetectClick();

			switch ( CurrentExercise )
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
				case Exercise.Yoga:
					Yogaing( cam );
					break;
			}

			AdvanceExerciseState();

			// Basically curSpeed increases based on how fast the player is squatting etc

			tCurSpeed = tCurSpeed.Clamp( 0, 1 );
			curSpeed = curSpeed.Clamp( 0, 1 );
			var mult = MathX.LerpInverse( TimeSinceExerciseStopped, 0, 1 );
			if ( CurrentExercise == Exercise.Run )
			{
				mult = MathX.LerpInverse( TimeSinceRun, 0, 1 );
			}

			tCurSpeed = tCurSpeed.LerpTo( 0f, Time.Delta * 0.25f * (mult * 4f) );
			curSpeed = curSpeed.LerpTo( tCurSpeed, Time.Delta * 2f );

			Scale = Scale.LerpTo( 1, Time.Delta * 10f );

			if ( cam.SCounter != null )
			{
				var c = cam.SCounter;
				c.l?.SetText( ExercisePoints.ToString() );
				c.Opacity += Time.Delta * curSpeed * 0.4f;

				c.TextScale = cam.SCounter.TextScale.LerpTo( 1.5f * MathX.Clamp( curSpeed + 0.8f, 0, 1 ), Time.Delta * 2f );
				float anim = MathF.Sin( aTime );
				c.Rotation = Rotation.From( 0, 90, anim * curSpeed * 1f * (ExercisePoints / 100f) );
			}


			SimulateActiveChild( cl, ActiveChild );
		}


		/// <summary>
		/// This runs a trace which will click on a food item in the world and consume or destroy it.
		/// </summary>
		public void DetectClick()
		{
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				TraceResult clickTrace = Trace.Ray( Input.Cursor, 1000f ).HitLayer( CollisionLayer.All, true ).Run();

				if ( clickTrace.Hit )
				{
					if ( IsServer && clickTrace.Entity is Food food )
					{
						food.RemoveFood();
					}
				}
			}
		}


		/// <summary>
		/// This is basically a rough function that will switch the exercises as you reach a certain number of points
		/// Eventually after ever exercise has been discovered, we should just cycle between them at random
		/// TODO: Move this to a better system
		/// </summary>
		public void AdvanceExerciseState()
		{
			if ( IsServer )
			{
				if ( ExercisePoints == 200 )
				{
					ChangeExercise( Exercise.Run );
					return;
				}
				else if ( ExercisePoints == 300 )
				{
					ChangeExercise( Exercise.Punch );
					return;
				}
				else if ( ExercisePoints == 400 )
				{
					ChangeExercise( Exercise.Yoga );
					return;
				}
			}

			if ( ExercisePoints > 50 )
			{
				SetAnimBool( "Angry", TimeSinceExerciseStopped < 2f );
			}
		}

		/// <summary>
		/// Sets the scale of the counter for extra juice when getting points
		/// </summary>
		/// <param name="f">The scale to set the counter to</param>
		public async void CounterBump( float f )
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
		public async void SetScale( float f )
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

			if ( squat == 0 )
			{
				squat = 1;
				return;
			}

			if ( squat == 1 )
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
}
