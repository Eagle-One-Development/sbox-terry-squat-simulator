using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox;
using Sandbox.UI.Construct;

namespace TSS.UI
{
	public class TwitchPanel : Panel
	{
		public static TwitchPanel Instance;

		public TwitchPanel()
		{
			StyleSheet.Load( "/ui/TwitchPanel.scss" );

			var container = AddChild<Panel>( "container" );

			container.Add.Label( "Twitch Commands", "title" );
			container.Add.Label( "!burger - Send a Cheeseburger Flying at Terry.", "command" );
			container.Add.Label( "!fries - Send some fries to terry.", "command" );
			container.Add.Label( "!sandwhich - Give terry a healthy sandwhich.", "command" );
			container.Add.Label( "!cheer - Give Terry some inspiration!", "command" );
			container.Add.Label( "!exercise - Set a random workout.", "command" );
			container.Add.Label( "!kill - Make Terry Collapse From Exhaustion.", "command" );

			Instance = this;
		}

		public void AddMessage( GenericMessage msg )
		{
			var _pan = AddChild<TwitchChatEntry>("twichchat");
			_pan.Message = msg;
			Log.Info( "CALLED?!" );

		}

		public override void Tick()
		{
			base.Tick();
			if ( Local.Pawn is TSSPlayer p )
			{
				if ( !p.IntroPlayed || p.TimeSinceIntro < 25f  || p.EndingInitiated )
				{
					SetClass( "inactive", true );
				}
				else
				{
					SetClass( "inactive", false );
				}
			}

			if ( Local.Pawn is BuffPawn b )
			{
				SetClass( "inactive", true );
			}

			if( !Streamer.IsActive )
			{
				SetClass( "inactive", true );
			}
		}
	}
}
