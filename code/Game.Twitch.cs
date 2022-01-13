using Sandbox;
using System.Collections.Generic;

namespace TSS
{
	public struct GenericMessage
	{
		public string DisplayName;
		public string Username;
		public string Message;
	}

	public partial class TSSGame : Game
	{
		public static Queue<GenericMessage> Queue = new Queue<GenericMessage>();

		public TimeSince TimeSinceExerciseChange;
		public TimeSince TimeSinceFood;

		[Event.Streamer.ChatMessage]
		public static void OnStreamMessage( StreamChatMessage message )
		{
			if ( !Host.IsClient )
				return;

			var msg = new GenericMessage()
			{
				Message = message.Message,
				DisplayName = message.DisplayName,
				Username = message.Username,
			};

			ProcessMessage( msg );
		}


		/// <summary>
		/// A mock function to simulate twitch messages.
		/// </summary>
		[ServerCmd( "twitch_simulate" )]
		public static void Say( string message )
		{
			Assert.NotNull( ConsoleSystem.Caller );

			if ( message.Contains( '\n' ) || message.Contains( '\r' ) ) 
				return;

			var msg = new GenericMessage() { 
				Message = message, 
				DisplayName = ConsoleSystem.Caller.Name, 
				Username = ConsoleSystem.Caller.Name, 
			};

			ProcessMessage( msg );
		}

		public static void ProcessMessage( GenericMessage message )
		{
			Log.Info( $"({message.DisplayName} | {message.Username}) - {message.Message}" );
			Queue.Enqueue( message );
		}

		public async static void DequeueLoop()
		{
			while(true)
			{
				await GameTask.Delay( 100 );

				if ( Queue.TryDequeue( out var msg ) )
				{
					Log.Info( "HMMM" );
					if ( msg.Message.Contains( "!soda" ) )
					{
						//Pawn.DrinkSoda();
					} else if ( msg.Message.Contains( "!burger" ) )
					{
						Log.Info( "HEY!" );
						if ( TSSGame.Current.TimeSinceFood > 1f )
						{
							_ = new Burger();
							TSSGame.Current.TimeSinceFood = 0f;
						}


					} else if ( msg.Message.Contains( "!cheer" ) )
					{
						Sound.FromScreen( $"cheering_0{Rand.Int( 1, 3 )}" );
					} else if ( msg.Message.Contains( "!fries" ) )
					{
						if ( TSSGame.Current.TimeSinceFood > 1f )
						{
							_ = new FrenchFries();
							TSSGame.Current.TimeSinceFood = 0f;
						}

					}
					else if ( msg.Message.Contains( "!sandwhich" ) )
					{
						if ( TSSGame.Current.TimeSinceFood > 1f )
						{
							_ = new Sandwhich();
							TSSGame.Current.TimeSinceFood = 0f;
						}

					}
					else if ( msg.Message.Contains( "!exercise" ) )
					{
						Log.Info( "Random Exercise" );
						if ( TSSGame.Current.TimeSinceExerciseChange > 10f )
						{
							TSSGame.Current.TimeSinceExerciseChange = 0f;
							Event.Run( "rand_exercise" );
						}
					}
					// tomato
					// random excerise
					// quicktime events
					// medicine ball and ragdoll
					// cheer
					// gym hottie confusion
					// pay gym subscription
					// 
				}
			}
		}

		/// <summary>
		/// Event called when a chat command comes in
		/// </summary>
		/*
		public class OnChatCommand : LibraryMethod
		{
			public string TargetName { get; set; }

			public static string User { get; internal set; }

			public OnChatCommand( string targetName )
			{
				TargetName = targetName;
			}
		}
		*/


		/*
		[Event.Streamer.JoinChat]
		public static void OnStreamJoinEvent( string user )
		{
			if ( !Host.IsClient )
				return;

			Log.Info( $"{user} joined" );
		}

		[Event.Streamer.LeaveChat]
		public static void OnStreamLeaveEvent( string user )
		{
			if ( !Host.IsClient )
				return;

			Log.Info( $"{user} left" );

			//RemovePlayerCommand( user );
		}
		*/
	}
}
