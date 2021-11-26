using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TSS
{
	public partial class TSSPlayer : Player
	{
		/// <summary>
		/// Changes the current exercise and moves the player to a given position and rotation
		/// </summary>
		/// <param name="exercise">The exercise we're moving to</param>
		public void ChangeExercise( Exercise exercise )
		{
			Entity ent = null;

			// Cleanup on exercise change.
			if ( CurrentExercise != exercise )
			{
				switch ( CurrentExercise )
				{
					case Exercise.Squat:
						Barbell?.Delete();
						Barbell = null;
						break;
					case Exercise.Yoga:
						CurrentYogaPosition = 0;
						break;
				}
			}

			switch ( exercise )
			{
				case Exercise.Run:
					ent = All.OfType<RunSpawn>().First();
					break;
				case Exercise.Squat:
					ent = All.OfType<SquatSpawn>().First();
					StartSquatting();
					break;
				case Exercise.Punch:
					ent = All.OfType<PunchSpawn>().First();
					break;
				case Exercise.Yoga:
					ent = All.OfType<PunchSpawn>().First();
					break;
			}

			Position = ent.Transform.Position;
			Rotation = ent.Transform.Rotation;
			CurrentExercise = exercise;

			if ( ExercisePoints > 100 )
				SetTitleCardActive();
		}

		[ClientRpc]
		private async void SetTitleCardActive()
		{
			await Task.DelaySeconds( 0.1f );
			titleCardActive = true;
		}
		
		public void ClearAnimation()
		{
			SetAnimInt( "squat", -1 );
			SetAnimInt( "punch", -1 );
			SetAnimInt( "YogaPoses", 0 );
			SetAnimBool( "b_grounded", CurrentExercise != Exercise.Yoga );
			SetAnimFloat( "move_x", 0 );
		}

		public void StartSquatting()
		{
			Barbell?.Delete();
			Barbell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
			Barbell.SetParent( this, "head" );
			Barbell.Rotation = Rotation * Rotation.From( 0, 0, 90 );
			Squat = 0;
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
		public void SimulateRunning( TSSCamera cam )
		{
			SetAnimFloat( "move_x", MathX.LerpTo( 0, 350f, (curSpeed * 4f).Clamp( 0, 1f ) ) );

			if ( cam == null )
			{
				return;
			}

			if ( TimeSinceRun < 3f && Squat != -1 )
			{
				cam.Progress += Time.Delta * 0.35f;
			}

			if ( Input.Pressed( InputButton.Right ) && Input.Pressed( InputButton.Left ) )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Right ) && (Squat == 0 || Squat == -1) && TimeSinceDownPressed > 0.1f )
			{
				if ( Squat == 0 )
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
				Squat = 1;
				TimeSinceUpPressed = 0;
			}

			if ( Input.Pressed( InputButton.Left ) && (Squat == 1 || Squat == -1) && TimeSinceUpPressed > 0.1f )
			{
				Squat = 0;
				TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}
		}

		/// <summary>
		/// The punching exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulatePunching( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			SetAnimInt( "punch", Squat );

			if ( TimeSincePunch > TimeToNextPunch )
			{
				TimeSincePunch = 0;
			}
		}

		[Event( "OtherBeat" )]
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
		[ServerCmd( "create_punch" )]
		public static void CreatePunchQT()
		{

			var pt = new PunchQT();
			pt.Player = TSSPlayer.Instance;
			pt.TargetTime = 1f;
			pt.MyTime = (60f / 140f) * 2f;
			pt.Type = Rand.Int( 0, 3 );

		}

		[ServerCmd( "yoga_pose" )]
		public static void SetPose( int i )
		{
			if ( Instance.CurrentExercise != Exercise.Yoga )
			{
				return;
			}

			Instance.CurrentYogaPosition = i;
			Instance.GivePoints( 5 );
		}

		public void SimulateYoga( TSSCamera cam )
		{
			SetAnimInt( "YogaPoses", CurrentYogaPosition );
			SetAnimBool( "b_grounded", CurrentYogaPosition == 0 );

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
		}

		/// <summary>
		/// The Squatting exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateSquatting( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			if ( Barbell != null )
			{
				Barbell.LocalPosition = Vector3.Zero + Barbell.Transform.Rotation.Right * 15.5f;
			}

			SetAnimInt( "squat", Squat );

			if ( TimeSinceExerciseStopped < 3f && Squat != -1 && !cam.IntroComplete )
			{
				float f = (TimeSinceExerciseStopped - 1f) / 3f;
				f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
				cam.Progress += Time.Delta * 0.025f * (1 - f);
			}

			if ( TimeSinceExerciseStopped < 3f && Squat != -1 && cam.IntroComplete )
			{
				cam.Progress += Time.Delta * 0.35f;
			}

			if ( Input.Pressed( InputButton.Forward ) && Input.Pressed( InputButton.Back ) )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Forward ) && (Squat == 0 || Squat == -1) && TimeSinceDownPressed > 0.1f )
			{

				if ( Squat == 0 )
				{

					ExercisePoints++;
					tCurSpeed += 0.1f;
					CreatePoint( 1 );
					SetScale( 1.2f );
					CounterBump( 0.5f );
					TimeSinceExerciseStopped = 0;


					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				Squat = 1;
				TimeSinceUpPressed = 0;

			}

			if ( Input.Pressed( InputButton.Back ) && (Squat == 1 || Squat == -1) && TimeSinceUpPressed > 0.1f )
			{
				Squat = 0;
				TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}
		}
	}
}
