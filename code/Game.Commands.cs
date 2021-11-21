
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace TSS
{
	public partial class TSSGame : Game
	{
		[ServerCmd]
		public static void CreateFood()
		{

		}


		[ServerCmd( "set_ep" )]
		public static void SetPoints( int p )
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{

				player.ExercisePoints += p;
			}
		}

		[ServerCmd( "punch" )]
		public static void Punch()
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				player.Punch();
			}
		}
	}

}
