using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TSS {
	public partial class TSSPlayer : Player
	{
		/// <summary>
		/// Changes the current exercise and moves the player to a given position and rotation
		/// </summary>
		/// <param name="exercise">The exercise we're moving to</param>
		public void ChangeExercise( Exercise exercise )
		{
			Entity ent = null;

			//Clear out a bunch of animation stuff
			SetAnimInt( "punch", -1 );
			SetAnimInt( "YogaPoses", 0 );
			SetAnimBool( "b_grounded", true );
			SetAnimFloat( "move_x", 0 );
			SetAnimFloat( "squat", -1 );


			switch ( exercise) {
				case Exercise.Run:
					ent = All.OfType<RunSpawn>().First();
					break;
				case Exercise.Squat:
					ent = All.OfType<SquatSpawn>().First();
					SetAnimFloat( "squat", squat );
					StartSquatting();
					break;
				case Exercise.Punch:
					ent = All.OfType<PunchSpawn>().First();
					break;
				case Exercise.Yoga:
					ent = All.OfType<PunchSpawn>().First();
					break;
			}

			



			if ( exercise != Exercise.Squat)
			{
				Barbell?.Delete();
				Barbell = null;
			}
			TimeSinceState = 0;
			MaxTimeInState = Rand.FromArray( new float[] { 15f, 20f, 25f, 30f } );

			Position = ent.Transform.Position;
			Rotation = ent.Transform.Rotation;
			CurrentExercise = exercise;
		}
		
		public void StartSquatting()
		{
			Barbell?.Delete();
			Barbell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
			Barbell.SetParent( this, "head" );
			Barbell.Rotation = Rotation * Rotation.From( 0, 0, 90 );
			squat = 0;
			lastSquat = -1;
		}

		public void StartPunching()
		{
			TimeToNextPunch = 1.1f;
		}

		/// <summary>
		/// The running exercise
		/// </summary>
		/// <param name="cam"></param>
		public void Running( TSSCamera cam )
		{
			SetAnimInt( "squat", -1 );
			SetAnimFloat( "move_x", MathX.LerpTo( 0, 350f, (curSpeed * 4f).Clamp( 0, 1f ) ) );

			if ( cam == null )
			{
				return;
			}

			if ( TimeSinceRun < 3f && squat != -1 )
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
					CreatePoint( 1 );
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

			if ( TimeSinceState >= MaxTimeInState && ExercisesIntroduced )
			{
				TimeSinceState = 0;
				MaxTimeInState = Rand.FromArray( new float[] { 10f, 15f, 15f, 15f, 30f } );
				ChangeExercise( Rand.FromArray( new Exercise[] { Exercise.Squat, Exercise.Punch, Exercise.Yoga } ) );
			}
		}

		/// <summary>
		/// The punching exercise
		/// </summary>
		/// <param name="cam"></param>
		public void Punching( TSSCamera cam )
		{

			if ( cam == null )
			{
				return;
			}

			SetAnimInt( "punch", squat );
			SetAnimInt( "squat", -1 );

			if ( TimeSincePunch > TimeToNextPunch )
			{
				TimeSincePunch = 0;

				if ( IsServer )
				{
					//var pt = new PunchQT();
					//pt.Player = this;
					//pt.TargetTime = 1f;
					//pt.MyTime = 1f;
					//pt.Type = Rand.Int( 0, 3 );
				}
				
			}

			if ( TimeSinceState >= MaxTimeInState && ExercisesIntroduced )
			{
				TimeSinceState = 0;

				MaxTimeInState = Rand.FromArray( new float[] { 10f, 15f, 15f, 15f, 30f } );
				ChangeExercise( Rand.FromArray( new Exercise[] { Exercise.Run, Exercise.Squat, Exercise.Yoga } ) );
				
			}
		}

		[Event("OtherBeat")]
		public void PunchBeat()
		{
			if ( CurrentExercise == Exercise.Punch )
			{
				ConsoleSystem.Run( "create_punch" );
			}
		}
		
		/// <summary>
		/// Command for creating the punch QT event
		/// </summary>
		[ServerCmd("create_punch")]
		public static void CreatePunchQT()
		{

		var pt = new PunchQT();
		pt.Player = TSSPlayer.Instance;
		pt.TargetTime = 1f;
		pt.MyTime = (60f/140f) * 2f;
		pt.Type = Rand.Int( 0, 3 );

		}

		[ServerCmd( "yoga_pose" )]
		public static void SetPose( int i )
		{
			if(Instance.CurrentExercise != Exercise.Yoga )
			{
				return;
			}
			Instance.SetAnimInt( "YogaPoses", i );
			Instance.SetClientPose( i );
			Instance.SetAnimBool( "b_grounded", false );
			Instance.GivePoints( 5 );
		}

		[ClientRpc]
		public void SetClientPose( int i )
		{
			SetAnimInt( "YogaPoses", i );
		}


		public void Yogaing( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			

			if ( TimeSinceYoga > 3.05f )
			{
				TimeSinceYoga = 0;

				if ( IsClient )
				{
					var pt = new YogaQT();
					pt.Player = this;

				}
			}

			//In theory this will work for introducing exercises at random 
			if(TimeSinceState >= MaxTimeInState && IntroYoga)
			{
				Log.Info( IsClient );
				SetAnimBool( "b_grounded", true );
				ChangeExercise( Rand.FromArray( new Exercise[] { Exercise.Run, Exercise.Punch, Exercise.Squat } ));
			}
		}

		/// <summary>
		/// The squatting exercise
		/// </summary>
		/// <param name="cam"></param>
		public void Squatting( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			if ( Barbell != null )
			{
				Barbell.LocalPosition = Vector3.Zero + Barbell.Transform.Rotation.Right * 15.5f;
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

			if ( Input.Pressed( InputButton.Forward ) && Input.Pressed( InputButton.Back ) )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Forward ) && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f )
			{
				if ( squat == 0 )
				{

					ExercisePoints++;
					tCurSpeed += 0.1f;
					CreatePoint( 1 );
					SetScale( 1.2f );
					CounterBump( 0.5f );
					TimeSinceExerciseStopped = 0;
					Log.Info( $"SQUAT: {ExercisePoints}" );
					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				squat = 1;
				TimeSinceUpPressed = 0;

			}

			if ( Input.Pressed( InputButton.Back ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f )
			{
				squat = 0;
				TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}

			if ( TimeSinceState >= MaxTimeInState && ExercisesIntroduced )
			{
				TimeSinceState = 0;
				MaxTimeInState = Rand.FromArray( new float[] { 10f, 15f, 15f, 15f, 30f } );
				ChangeExercise( Rand.FromArray( new Exercise[] { Exercise.Run, Exercise.Punch, Exercise.Yoga } ) );
			}
		}
	}
}
