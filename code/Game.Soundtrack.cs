using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using TSS.UI;

namespace TSS
{
	/// <summary>
	/// A class we are going to use to manage our music layers
	/// </summary>
	public class MusicLayer{

		/// <summary>
		/// Reference to our sound
		/// </summary>
		Sound MySound;
		/// <summary>
		/// The current volume of the sound
		/// </summary>
		private float volume;
		/// <summary>
		/// The target volume of the sound
		/// </summary>
		private float targetVolume;

		/// <summary>
		/// The speed at which we move towards our target volume;
		/// </summary>
		private float fadeSpeed;

		/// <summary>
		/// The name of our sound
		/// </summary>
		private string soundName;

		public MusicLayer(string name)
		{
			volume = 0f;
			targetVolume = 0;
			fadeSpeed = 1f;
			soundName = name;
			MySound = Sound.FromScreen( name );
			MySound.SetVolume( volume );
			
		}

		/// <summary>
		/// Fades a sound to a specific volume at a given speed
		/// </summary>
		/// <param name="v">The target volume</param>
		/// <param name="speed">How long in seconds you want to fade to the given value</param>
		public void FadeTo(float v, float speed )
		{
			targetVolume = v;
			//This is basically gonna do some really basic math since we use Time.Delta. Basically, take the difference between the current volume and the target volume and divide it by the number of
			//"seconds", or speed. What this results in is the number of steps to increase the volume by "per frame".
			fadeSpeed = (MathF.Abs(v - volume)/speed);
		}

		/// <summary>
		/// Sets the volume directly regardless of fading
		/// </summary>
		/// <param name="v"></param>
		public void SetVolume(float v )
		{
			volume = v;
			targetVolume = v;
			fadeSpeed = 0f;
			MySound.SetVolume( v );

		}

		/// <summary>
		/// Stops our sound
		/// </summary>
		public void StopSound()
		{
			MySound.Stop();
		}

		/// <summary>
		/// Completely restarts the sound
		/// </summary>
		public void RestartSound()
		{
			MySound.Stop();
			MySound = Sound.FromScreen( soundName );
			volume = 1f;
			targetVolume = 1f;
			fadeSpeed = 1f;
		}

		/// <summary>
		/// A function that will handle volume when run every frame
		/// </summary>
		public void Simulate()
		{
			volume = Approach(volume,targetVolume,fadeSpeed * Time.Delta);
			MySound.SetVolume( volume );
		}

		private float Approach(float current, float target, float delta )
		{
			if(current < target )
			{
				return MathF.Min( current + delta, target );
			}
			else
			{
				return MathF.Max( current - delta, target );
			}
		}

	}


	public partial class TSSGame : Game
	{
		public List<Sound> Music;
		public float[] volumes;
		public float[] tarVolumes;
		public SoundStream[] streams;
		public RealTimeSince RealTimeSinceSongStart;

		public MusicLayer RantInstrumental;
		public MusicLayer NatureSounds;

		Queue<MusicLayer> TrackQueue;
		public List<MusicLayer> Tracks;

		public double SongStartTime;

		public static readonly float QUARTER_NOTE_DURATION = (60f / 140f) / 4f;
		public static readonly float WHOLE_NOTE_DURATION = (60f / 140f);

		public RealTimeSince TimeSinceLastBeat;

		public int BeatNonce { get; set; }

		[Event( "OtherBeat" )]
		public void HandleBeat()
		{
			BeatNonce++;
		}

		// This is going to require more explanation. It's basically a way of tracking when a "beat" in a song happens, we can use this for some basic effects.
		#region Beats
		public int Beats;
		public double SongPosInBeats;
		public double SecondsPerBeat;
		#endregion

		public void FrameBeats()
		{

			if ( TimeSinceLastBeat > WHOLE_NOTE_DURATION )
			{
				TimeSinceLastBeat = 0;
				
			}

			SongPosInBeats = (RealTimeSinceSongStart) / SecondsPerBeat;

			if(Beats < SongPosInBeats )
			{
				Beats++;

				if ( Beats % 2 == 0 )
				{
					Event.Run( "OtherBeat" );
				}

				if ( (Beats - 1)% 32 == 0 )
				{
					Log.Info( "8 MEASURES BITCH" );
					if(TrackQueue.Count != 0 )
					{
						Log.Info( "DEQUEING TRACK" );
						Silence();
						var track = TrackQueue.Dequeue();
						track.RestartSound();
						track.SetVolume( 1f );
						
					}
				}
			}
		}

		[ClientRpc]
		public void QueueTrack(string s)
		{

			var track = new MusicLayer( s );
			track.SetVolume( 0f );
			Tracks.Add( track );
			TrackQueue.Enqueue( track );

		}

		[ClientRpc]
		public void PlayCredits()
		{
			Sound.FromScreen( "end_song" );
		}

		[ClientRpc]
		public void PlayIntro()
		{
			Log.Info( "PLAYING INTRO SOUND" );
			Sound.FromScreen( "Intro" );
			IntroPanel.Instance.IntroStarted = true;
		}
		
		[ClientRpc]
		public void PlayRantInstrumental()
		{
			RantInstrumental = new MusicLayer( "rant_instrumental" );
			RantInstrumental.FadeTo( 1f, 10f );
			Silence();
			PlayRant();
		}

		[ClientRpc]
		public void StopInstrumental()
		{
			RantInstrumental.FadeTo( 0f, 1f );
		}

		[ClientRpc]
		public void StartNature()
		{
			NatureSounds = new MusicLayer( "naturewind" );
			NatureSounds.FadeTo( 1f, 10f );
		}


		public async void PlayRant()
		{
			await GameTask.Delay( 5000 );
			Sound.FromScreen( "rant_voice" );
		}

		[ClientRpc]
		public void StartMusic()
		{
			Music = new List<Sound>();
			Music.Clear();
			TrackQueue = new Queue<MusicLayer>();
			TrackQueue.Clear();
			Tracks = new List<MusicLayer>();
			Tracks.Clear();

			volumes = new float[7];
			tarVolumes = new float[7];

			for ( int i = 0; i < 7; i++ )
			{
				string str = $"intro{i}";

				Music.Add( Sound.FromScreen( str ) );
				volumes[i] = 0;
				tarVolumes[i] = 0f;
				Music[i].SetVolume( 0f );
			}
			RealTimeSinceSongStart = 0f;
			SecondsPerBeat = 60f / 140f;
			SongStartTime = Time.Sound;
			TimeSinceLastBeat = 0;

		}

		[ClientRpc]
		public void SetTarVolume( int v , float volume = 1f)
		{
			if(tarVolumes == null ) { return; }
			tarVolumes[v] = volume;
		}

		[ClientRpc]
		public void SetSingleTarVolume( int v, float volume = 1f )
		{
			if ( tarVolumes == null ) { return; }
			for ( int i = 0; i < tarVolumes.Count(); i++ )
			{
				tarVolumes[i] = 0f;
			}
			tarVolumes[v] = volume;
		}

		[ClientRpc]
		public void SetVolume(int v, float volume = 1f )
		{
			if ( tarVolumes == null ) { return; }
			tarVolumes[v] = volume;
			volumes[v] = volume;
		}

		[ClientRpc]
		public void SetSingleVolume( int v, float volume = 1f )
		{
			if ( tarVolumes == null ) { return; }
			for(int i = 0; i < tarVolumes.Count(); i++ )
			{
				volumes[i] = 0f;
				tarVolumes[i] = 0f;
			}
			tarVolumes[v] = volume;
			volumes[v] = volume;
		}


		[ClientRpc]
		public void Silence()
		{
			for(int i = 0; i < tarVolumes.Length; i++ )
			{
				tarVolumes[i] = 0f;
				volumes[i] = 0f;
				Music[i].SetVolume( 0f );
				Music[i].Stop();
			}

			for ( int i = 0; i < Tracks.Count; i++ )
			{

				Tracks[i].SetVolume(0f);

			}
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

			RantInstrumental?.Simulate();
			NatureSounds?.Simulate();
			for(int i = 0; i < Tracks.Count; i++ )
			{
				Tracks[i].Simulate();
			}

			FrameBeats();

			for ( int j = 0; j < volumes.Length; j++ )
			{
				volumes[j] = volumes[j].LerpTo( tarVolumes[j], Time.Delta * 2f );
			}
		}
	}

}
