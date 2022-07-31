using System.Linq;
using Sandbox;

namespace TSS
{
	public partial class TSSGame : Game
	{
		[ConCmd.Server]
		public static void CreateFood()
		{

		}

		[ConCmd.Server( "set_ep" )]
		public static void SetPoints( int p )
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				player.ExercisePoints += p;
			}
		}

		[ConCmd.Server( "punch" )]
		public static void Punch()
		{
			if ( ConsoleSystem.Caller.Pawn is TSSPlayer player )
			{
				var component = player.Components.GetAll<PunchComponent>().First();
				component.Punch();
			}
		}
	}
}
