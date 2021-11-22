﻿using Sandbox;
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

		/// <summary>
		/// Basically a way of stopping the soda animation when its done
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceSoda { get; set; }

		
		/// <summary>
		/// Whether we've introduced running or not
		/// </summary>
		[Net]
		public bool IntroRunning { get; set; }
		/// <summary>
		/// Whether we've introduced the punching mini-game or not
		/// </summary>
		[Net]
		public bool IntroPunching { get; set; }
		/// <summary>
		/// Whether we've introduced the yoga game or not
		/// </summary>
		[Net]
		public bool IntroYoga { get; set; }

		/// <summary>
		/// The time we've been doing the current exercise
		/// </summary>
		[Net,Predicted]
		public TimeSince TimeSinceState { get; set; }

		[Net]
		public float MaxTimeInState { get; set; } = 10f;

		[Net]
		public bool[] TimeLines { get; set; } = new bool[20];

		/// <summary>
		/// Just a variable to introduce if we've introduced all the exercise or not
		/// </summary>
		[Net]
		public bool ExercisesIntroduced { 
			get
			{
				return IntroRunning && IntroPunching && IntroYoga;
			}
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

			MaxTimeInState = 30f;

			base.Respawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			PlayMusic();
		}

		public async void PlayMusic()
		{
			await GameTask.Delay( 1000 );
			TSSGame.Current.StartMusic();
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

			float f = (TimeSinceExerciseStopped - 1f) / 3f;
			f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
			TimeSinceState += Time.Delta * (1f - f);

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
				if ( ExercisePoints >= 200 && !IntroRunning)
				{
					ChangeExercise( Exercise.Run );
					TSSGame.Current.SetTarVolume( 1 );
					TSSGame.Current.SetTarVolume( 7 );
					IntroRunning = true;
					return;
				}
				if ( ExercisePoints >= 300 && !IntroPunching)
				{
					ChangeExercise( Exercise.Punch );
					IntroPunching = true;
					return;
				}
				if ( ExercisePoints >= 400 && !IntroYoga)
				{
					ChangeExercise( Exercise.Yoga );
					IntroYoga = true;
					return;
				}
			}

			if(ExercisePoints >= 100 && !TimeLines[0])
			{
				TSSGame.Current.SetTarVolume( 3 );
				TSSGame.Current.SetTarVolume( 2 );
				TimeLines[0] = true;
			}


			if ( ExercisePoints > 50 )
			{
				SetAnimBool( "Angry", TimeSinceExerciseStopped < 4f );
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
			ExercisePoints += 3;
			tCurSpeed += 0.1f;
			CreatePoint( 3 );
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
