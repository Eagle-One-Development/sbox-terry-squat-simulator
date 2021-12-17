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
		/// This is the command used to send the player to the white void once they've reached their 'heaven threshold'. This basically transitions
		/// us to the start of the "ending" of the game.
		/// </summary>
		[ServerCmd( "heaven" )]
		public static void GoToHeaven()
		{
			//Find the spawn with the tpye heaven
			var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Heaven );
			//Set our exercise position to that ents position
			TSSPlayer.Instance.ExercisePosition = ent.Transform.Position;
			//Move our player there
			TSSPlayer.Instance.Position = ent.Transform.Position;
			TSSPlayer.Instance.Rotation = ent.Transform.Rotation;
			//Create the particle that goes behind the player
			TSSPlayer.Instance.CreateNearEndParticle( To.Single( TSSPlayer.Instance ) );
			//Set a variable tracking if we've gone to heaven or not to true
			TSSPlayer.Instance.CanGoToHeaven = true;
			//Set the exercise to the squat exercise
			TSSPlayer.Instance.ChangeExercise( Exercise.Squat );

			TSSPlayer.Instance.PointCeiling = TSSPlayer.Instance.ExercisePoints + 300;

		}


		/// <summary>
		/// Command for creating the punch QT event
		/// </summary>
		[ServerCmd( "create_punch" )]
		public static void CreatePunchQT()
		{
			var pt = new PunchQT();
			pt.Player = Instance;
			pt.TargetTime = 1f;
			pt.MyTime = (60f / 140f) * 2f;
			pt.Type = Rand.Int( 0, 3 );

		}

		/// <summary>
		/// Sets the pose on both the server and client, updating the yoga pose terry is using during the yoga exercise
		/// </summary>
		/// <param name="i"></param>
		[ServerCmd( "yoga_pose" )]
		public static void SetPose( int i )
		{
			if ( Instance.CurrentExercise != Exercise.Yoga )
			{
				return;
			}

			var pawn = TSSGame.Pawn;


			Instance.CurrentYogaPosition = i;
			Instance.GivePoints( 5 );


		}
	}
}
