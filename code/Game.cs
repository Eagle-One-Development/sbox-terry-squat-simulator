
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

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new TSSPlayer();
			client.Pawn = player;
			player.Respawn();
		}

		public static new TSSGame Current => Game.Current as TSSGame;
		public static TSSPlayer Pawn => All.OfType<TSSPlayer>().First();
	}
}
