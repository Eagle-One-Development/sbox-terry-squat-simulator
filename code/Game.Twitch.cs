using Sandbox;
using System.Collections.Generic;

namespace MinimalExample
{
	public partial class TSSGame : Game
	{

		public Queue<StreamChatMessage> Queue = new Queue<StreamChatMessage>();

		[Event.Streamer.ChatMessage]
		public static void OnStreamMessage( StreamChatMessage message )
		{
			if ( !Host.IsClient )
				return;

			(Current as TSSGame).Queue.Enqueue( message );
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
