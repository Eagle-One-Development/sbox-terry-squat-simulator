using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TSS;
using TSS.UI;


namespace TSS
{
	public partial class RunComponent : ExerciseComponent
	{
		#region Members
		/// <summary>
		/// A value to drive whether or not we're in the 'up' or 'down' squat position. Used to drive animation and figure out when we've completed a full squat.
		/// </summary>
		[Net, Predicted]
		public int Squat { get; set; }

		/// <summary>
		/// A vector3 representing the player slipping off the treadmill.
		/// </summary>
		[Net]
		public float RunPositionOffset { get; set; }

		/// <summary>
		/// The time since we stopped running. 
		/// TODO: Review for redundancy with TimeSinceExerciseStopped
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceRun { get; set; }

		#endregion

		public override void Initialize()
		{
			ExerciseType = Exercise.Run;
		}

		public override void Simulate( Client client )
		{
			base.Simulate( client );
			var cam = Entity.CameraMode as TSSCamera;
			SimulateRunning( cam );
		}

		public override void Cleanup()
		{
			TimeSinceRun = 0f;
		}


		/// <summary>
		/// The running exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateRunning( TSSCamera cam )
		{
			Entity.SetAnimParameter( "move_x", MathX.LerpTo( 0, 350f, (Entity.CurrentExerciseSpeed * 4f).Clamp( 0, 1f ) ) );

			//We're going to set our position to the RunPosition + some offset
			Entity.Position = Entity.ExercisePosition + Entity.Rotation.Forward * -RunPositionOffset;

			//Basically we're going to use our curSpeed, a value which determines how fast we are running, to determine if we're moving forward or backward on the treadmill
			float treadSpeed = (Entity.CurrentExerciseSpeed / 0.28f).Clamp( 0f, 1f );
			//Basically check and see if we're exercising fast enough, if not, uptick the run position offset to make us 
			if ( treadSpeed >= 0.6f )
			{
				RunPositionOffset -= Time.Delta * 25f;
			}
			else
			{
				RunPositionOffset += Time.Delta * (1f - treadSpeed) * 50f;
			}
			RunPositionOffset = RunPositionOffset.Clamp( -10f, 45f );


			if ( RunPositionOffset >= 45f )
			{
				Entity.BecomeRagdollOnClient(To.Single(Entity.Client), (Entity.Rotation.Forward * -1f + Vector3.Up).Normal * 250f, 0 );
				RunPositionOffset = 0f;
				Entity.CurrentExerciseSpeed = 1f;
				Entity.TimeSinceExerciseStopped = 0f;
				Entity.TimeSinceRagdolled = 0f;
			}


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

			if ( Input.Pressed( InputButton.Right ) && (Squat == 0 || Squat == -1) && Entity.TimeSinceDownPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				if ( Squat == 0 )
				{
					Entity.TargetExerciseSpeed += 0.1f;
					Entity.CreatePoint( 1 );
					Entity.SetScale( 1.2f );
					Entity.CounterBump( 0.5f );
					Entity.TimeSinceExerciseStopped = 0;
					Entity.ExercisePoints++;
					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				Squat = 1;
				Entity.TimeSinceUpPressed = 0;
			}

			if ( Input.Pressed( InputButton.Left ) && (Squat == 1 || Squat == -1) && Entity.TimeSinceUpPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				Squat = 0;
				Entity.TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}
		}



	}
}
