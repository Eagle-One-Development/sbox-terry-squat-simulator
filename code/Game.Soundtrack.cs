using Sandbox;
using System.Collections.Generic;

namespace TSS
{
	public partial class TSSGame : Game
	{
		public List<Sound> Music;
		public float[] volumes;
		public float[] tarVolumes;
		public RealTimeSince RealTimeSinceSongStart;

		public double SongStartTime;

		// This is going to require more explanation. It's basically a way of tracking when a "beat" in a song happens, we can use this for some basic effects.
		#region Beats
		public int Beats;
		public double SongPosInBeats;
		public double SecondsPerBeat;
		#endregion

		public void FrameBeats()
		{
			SongPosInBeats = (Time.Sound - SongStartTime) / SecondsPerBeat;
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
			SongStartTime = Time.Sound;
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
			}

			FrameBeats();

			for ( int j = 0; j < volumes.Length; j++ )
			{
				volumes[j] = volumes[j].LerpTo( tarVolumes[j], Time.Delta * 2f );
			}
		}
	}

}
