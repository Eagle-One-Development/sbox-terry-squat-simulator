
using Sandbox;

namespace TSS
{
	public partial class TSSGame : Game
	{
		public TSSGame() {
			if (IsServer)
			{
				_ = new TSSHud();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new TSSPlayer();
			client.Pawn = player;

			player.Respawn();
		}

		public static new TSSGame Current => Game.Current as TSSGame;
	}
}
