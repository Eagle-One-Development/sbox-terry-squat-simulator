using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MinimalExample
{

	public partial class TSSGame : Game
	{
		public List<Sound> Music;
		public float[] volumes;
		public float[] tarVolumes;

		[ClientRpc]
		public void StartMusic()
		{
			Music = new List<Sound>();
			Music.Clear();
			volumes = new float[8];
			tarVolumes = new float[8];
			for ( int i = 0; i < 8; i++ )
			{
				string str = $"layer{i}";

				Music.Add( Sound.FromScreen( str ) );
				volumes[i] = 0;
				tarVolumes[i] = 1;
				Music[i].SetVolume( 0f );
			}
			//Sound.FromScreen( "music_tens07" );
		}




		[ClientRpc]
		public void SetTarVolume( int v )
		{
			tarVolumes[v] = 1f;
		}

		[Event.Frame]
		public void Volume()
		{
			if ( Music == null )
			{
				return;
			}
			for ( int i = 0; i < Music.Count; i++ )
			{
				Music[i].SetVolume( volumes[i] );
				//DebugOverlay.ScreenText( new Vector2( 100, 100 + i * 50 ), $"Volume({i}):{volumes[i]}" );
			}


			for ( int j = 0; j < volumes.Length; j++ )
			{
				volumes[j] = volumes[j].LerpTo( tarVolumes[j], Time.Delta * 2f );
			}
		}
	}

}
