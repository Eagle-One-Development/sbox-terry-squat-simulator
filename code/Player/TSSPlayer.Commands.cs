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
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				//Find the spawn with the tpye heaven
				var ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Heaven );
				//Set our exercise position to that ents position
				player.ExercisePosition = ent.Transform.Position;
				//Move our player there
				player.Position = ent.Transform.Position;
				player.Rotation = ent.Transform.Rotation;
				//Create the particle that goes behind the player
				player.CreateNearEndParticle( To.Single( player ) );
				//Set a variable tracking if we've gone to heaven or not to true
				player.CanGoToHeaven = true;
				//Set the exercise to the squat exercise
				player.ChangeExercise( Exercise.Squat );

				player.PointCeiling = player.ExercisePoints + 300;
			}

		}

		[ServerCmd("queue_random_track")]
		public static void QueueRandomTrack()
		{
			int i = Rand.Int( 0, 3 );
			TSSGame.Current.QueueTrack( $"layer{i}" );
		}

		/// <summary>
		/// Command for creating the punch QT event
		/// </summary>
		[ServerCmd( "create_punch" )]
		public static void CreatePunchQT()
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				var pt = new PunchQT();
				pt.Player = player;
				pt.TargetTime = 1f;
				pt.MyTime = (60f / 140f) * 2f;
				pt.Type = Rand.Int( 0, 3 );
			}

		}

		/// <summary>
		/// Sets the pose on both the server and client, updating the yoga pose terry is using during the yoga exercise
		/// </summary>
		/// <param name="i"></param>
		[ServerCmd( "yoga_pose" )]
		public static void SetPose( int i )
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				var component = player.Components.GetAll<YogaComponent>().First();
				if ( player.CurrentExercise != Exercise.Yoga )
				{
					return;
				}

				component.CurrentYogaPosition = i;
				player.GivePoints( 5 );
			}

		}
	}
}
