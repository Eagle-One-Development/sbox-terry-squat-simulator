using Sandbox;
using System;
using System.Linq;
using TSS.UI;

namespace TSS
{
	public partial class TSSPlayer : Player
	{
		/// <summary>
		/// This will give a certain number of points to the player, add those points to the player's exercise points
		/// in addition to initiating several effects.
		/// </summary>
		/// <param name="i">The number of poitns to give</param>
		/// <param name="fall">Whether the points 'fall' or not when created</param>
		public void GivePoints( int i, bool fall = false )
		{
			SetScale( 1.2f );
			CounterBump( 0.5f );
			ExercisePoints += i;
			TimeSinceExerciseStopped = 0;
			tCurSpeed += 0.1f;
			CreatePoint( 1, true, i );
		}

		/// <summary>
		/// Does the same as GivePoints but spawns them at a position rather than randomly around the player
		/// </summary>
		/// <param name="i">The number of points to give</param>
		/// <param name="pos">The position to spawn them out</param>
		/// <param name="fall">Whether the points 'fall' or not when created</param>
		public void GivePointsAtPosition( int i, Vector3 pos, bool fall = false )
		{
			SetScale( 1.2f );
			CounterBump( 0.5f );
			ExercisePoints += i;
			TimeSinceExerciseStopped = 0;
			tCurSpeed += 0.1f;
			CreatePointAtPosition( 1, pos, fall, i );
		}

		/// <summary>
		/// Creates a point panl at a radnom position around the player
		/// </summary>
		/// <param name="i">The amount of points per point panel to give</param>
		/// <param name="fall">Whether or not the point panel falls</param>
		/// <param name="count">The number of point panels to create</param>
		[ClientRpc]
		public void CreatePoint( int i, bool fall = false, int count = 1 )
		{
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

		/// <summary>
		/// Creates a point panel at a position instead of randomly around the player
		/// </summary>
		/// <param name="i">The amount of points to give</param>
		/// <param name="pos">The position to spawn the point</param>
		/// <param name="fall">Whether or not the point falls</param>
		/// <param name="count">The number of points to spawn</param>
		[ClientRpc]
		public void CreatePointAtPosition( int i, Vector3 pos, bool fall = false, int count = 1 )
		{
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


	}
}
