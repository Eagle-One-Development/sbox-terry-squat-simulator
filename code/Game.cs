
using Sandbox;
using System.Linq;

namespace TSS
{
	public partial class TSSGame : Game
	{
		public TSSGame() {
			if (IsServer)
			{
				_ = new TSSHud();
				DequeueLoop();
			}

			if ( IsClient )
			{
				PostProcess.Add( new VHSPostProcess() );
				var vhsInvert = PostProcess.Get<VHSPostProcess>();
				vhsInvert.Enabled = true;
			}
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			//Do nothing
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			// Kick anyone who isn't host.
			if (!client.IsListenServerHost)
			{
				client.Kick();
			}

			var player = new TSSPlayer();
			client.Pawn = player;
			player.Respawn();
		}

		// Helper field that casts game.
		public static new TSSGame Current => Game.Current as TSSGame;

		// Get the player, there should only be one.
		public static TSSPlayer Pawn => All.OfType<TSSPlayer>().First();
	}
}
