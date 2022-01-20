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
		public Panel Container;
		public TwitchPanel()
		{
			StyleSheet.Load( "/ui/TwitchPanel.scss" );

			Container = AddChild<Panel>( "container" );

			Container.Add.Label( "Twitch Commands", "title" );
			Container.Add.Label( "!burger - Send a Cheeseburger Flying at Terry.", "command" );
			Container.Add.Label( "!fries - Send some fries to terry.", "command" );
			Container.Add.Label( "!sandwhich - Give terry a healthy sandwhich.", "command" );
			Container.Add.Label( "!cheer - Give Terry some inspiration!", "command" );
			Container.Add.Label( "!exercise - Set a random workout.", "command" );
			Container.Add.Label( "!kill - Make Terry Collapse From Exhaustion.", "command" );

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
				
				if ( !p.IntroPlayed || p.TimeSinceIntro < 25f  || p.EndingInitiated || p.ExercisePoints < 50f)
				{
					
					Container.SetClass( "inactive", true );
				}
				else
				{
					
					if ( Streamer.IsActive )
					{

						Container.SetClass( "inactive", false );
					}
				}

				if ( p.ExercisePoints > p.HeavenThreshold && !p.SkipIntro)
				{
					Style.BorderColor = Color.Black;
					Style.FontColor = Color.Black;
					Container.Style.FontColor = Color.Black;
					Container.Style.BorderColor = Color.Black;
				}
			}

			if ( Local.Pawn is BuffPawn b )
			{
				Container.SetClass( "inactive", true );
			}



			
		}
	}
}
