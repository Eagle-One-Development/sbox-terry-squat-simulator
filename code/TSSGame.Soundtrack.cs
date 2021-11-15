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
		public RealTimeSince RealTimeSinceSongStart;

		//This is going to require more explanation. It's basically a way of tracking when a "beat" in a song happens, we can use this for some basic effects
		#region Beats
		public int Beats;
		public float SongPosInBeats;
		public float SecondsPerBeat;
		#endregion

		public void FrameBeats()
		{
			SongPosInBeats = Music[0].ElapsedTime / SecondsPerBeat;
			if(Beats < SongPosInBeats )
			{
				Beats++;
				
				if(Beats % 4 == 0 )
				{
					
				}

				if ( Beats % 2 == 0 )
				{
					Event.Run( "OtherBeat" );
				}

				if (Beats % 140 == 0 )
				{
					Log.Info( $"BEAT TO THE BOP: {Beats}\nCurrent Time:{RealTimeSinceSongStart}" );
					Log.Info( "1 Minute!" );
				}

				if ( Music[0].Finished )
				{
					Log.Info( "FINISHED LOOP" );
				}

				DebugOverlay.ScreenText( new Vector2( 200, 200 ), $"Music 0: {Music[0].ElapsedTime}\nMusic 1: {Music[1].ElapsedTime}", 0.41f );
			}
		}

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
				tarVolumes[i] = 0f;
				Music[i].SetVolume( 0f );
			}
			SecondsPerBeat = 60f / 140f;
			RealTimeSinceSongStart = 0;
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

			FrameBeats();

			for ( int j = 0; j < volumes.Length; j++ )
			{
				volumes[j] = volumes[j].LerpTo( tarVolumes[j], Time.Delta * 2f );
			}
		}
	}

}
