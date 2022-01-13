using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Twitch.Commands;
using TSS.UI;

namespace TSS
{
	public struct GenericMessage
	{
		public string DisplayName;
		public string Username;
		public string Message;
		public string Color;
	}

	public partial class TSSGame : Game
	{
		public static Queue<GenericMessage> Queue = new Queue<GenericMessage>();

		public TimeSince TimeSinceExerciseChange;
		public TimeSince TimeSinceFood;
		public TimeSince TimeSinceKilled;
		public static float FoodCoolDown = 3f;

		public List<TwitchCommand> TwitchCommands;

		public void InitializeCommands()
		{
			TwitchCommands = new List<TwitchCommand>();
			var pawn = Entity.All.OfType<TSSPlayer>().FirstOrDefault();

			TwitchCommands.Add( new FoodCommand("!burger",pawn,"food_burger") );
			TwitchCommands.Add( new FoodCommand( "!sandwhich", pawn, "food_sandwhich" ) );
			TwitchCommands.Add( new FoodCommand( "!fries", pawn, "food_fries" ) );
			TwitchCommands.Add( new CheerCommand( "!cheer", pawn ));
			TwitchCommands.Add( new KillCommand( "!kill", pawn ) );
			TwitchCommands.Add( new ExerciseCommand( "!exercise", pawn ) );
		}

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
				Color = message.Color
			};
			
			ProcessMessage( msg );
		}

		[ClientRpc]
		public void AddHudMessage(string msg, string disp, string col)
		{
			var item = new GenericMessage()
			{
				Message = msg,
				DisplayName = disp,
				Color = col
			};

			TwitchPanel.Instance.AddMessage(item);


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
				Color = Color.Random.ToString()
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
			//Delay this ten seconds so the player has time to spawn
			await GameTask.DelaySeconds( 10f );

			//Get a reference to pawn
			var pawn = Entity.All.OfType<TSSPlayer>().FirstOrDefault();

			//Initialize the list of commands
			TSSGame.Current.InitializeCommands();

			while(true)
			{
				await GameTask.Delay( 100 );

				if ( Queue.TryDequeue( out var msg ) && !pawn.EndingInitiated && pawn.IntroPlayed && pawn.TimeSinceIntro > 25f)
				{
					var CommandList = TSSGame.Current.TwitchCommands;

					foreach(TwitchCommand t in CommandList )
					{
						t.Evalulate( msg );
					}

					
				}
			}
		}
	}
}
