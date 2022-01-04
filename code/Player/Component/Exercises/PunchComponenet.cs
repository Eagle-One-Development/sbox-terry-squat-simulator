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
	public partial class PunchComponent : ExerciseComponent
	{
		#region Members
		/// <summary>
		/// A value to drive whether or not we're in the 'up' or 'down' squat position. Used to drive animation and figure out when we've completed a full squat.
		/// </summary>
		[Net, Predicted]
		public int Squat { get; set; }

		/// <summary>
		/// The time since we last punch
		/// TODO: Review for redundancy with TimeSinceExerciseStopped
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSincePunch { get; set; }

		/// <summary>
		/// The time between punches. Lets us know when to spawn another punch quick time event
		/// </summary>
		[Net]
		public float TimeToNextPunch { get; set; }

		#endregion

		public override void Initialize()
		{
			ExerciseType = Exercise.Punch;
			StartPunching();
		}

		/// <summary>
		/// Initializes the punch exercise state
		/// </summary>
		public void StartPunching()
		{
			TimeToNextPunch = 1.1f;
		}


		public override void Simulate( Client client )
		{
			base.Simulate( client );
			var cam = Entity.Camera as TSSCamera;
			SimulatePunching( cam );
		}

		public override void Cleanup()
		{
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

			Entity.SetAnimInt( "punch", Squat );

			if ( TimeSincePunch > TimeToNextPunch )
			{
				TimeSincePunch = 0;

			}
		}

		[Event( "OtherBeat" )]
		public void PunchBeat()
		{
			if ( Entity.EndingInitiated )
			{
				return;
			}
			if ( Entity.CurrentExercise == Exercise.Punch )
			{
				ConsoleSystem.Run( "create_punch" );
			}
		}

		/// <summary>
		/// Makes the player punch and moves the 'squat' variable so it alternated between left and right punches
		/// </summary>
		public void Punch()
		{
			Entity.ExercisePoints += 3;
			Entity.TargetExerciseSpeed += 0.1f;
			Entity.CreatePoint( 3 );
			Entity.Scale = 1.2f;
			Entity.CounterBump( 0.5f );
			Entity.TimeSinceExerciseStopped = 0;


			if ( Squat == 0 )
			{
				Squat = 1;
				return;
			}

			if ( Squat == 1 )
			{
				Squat = 0;
				return;
			}
		}


	}
}
