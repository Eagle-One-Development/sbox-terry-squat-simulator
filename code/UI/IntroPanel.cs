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
	public class IntroPanel : Panel
	{
		public static IntroPanel Instance;
		public bool IntroStarted;
		TerryRenderScene Terry;
		public TimeSince TimeSinceIntroStarted;
		public Panel Back;
		public Label Play;
		public Label date;
		public Label Click;
		public IntroPanel()
		{
			Instance = this;
			StyleSheet.Load( "/UI/IntroPanel.scss" );
			Back = Add.Panel( "back" );
			var p = Add.Panel( "textBack" );
			Play = p.Add.Label( "> PLAY", "text" );
			date = p.Add.Label( "OOO", "date" );
			Terry = AddChild<TerryRenderScene>("scene");
			Click = Add.Label( $"Press {Input.GetKeyWithBinding( "+iv_attack" )} to begin.", "prompt" );

		}

		public override void Tick()
		{
			base.Tick();
			if ( !IntroStarted )
			{
				Terry.TimeSinceIntroStarted = 0f;
				TimeSinceIntroStarted = 0f;
			}

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				Click.Delete( true );
			}

			if ( Terry != null && Terry.TimeSinceIntroStarted > 23.161f )
			{
				Terry.Delete( true );
				
			}
			Play.Style.Opacity = 0f;
			date.Style.Opacity = 0f;
			if ( TimeSinceIntroStarted > 24.726f)
			{
				Back.Style.Opacity = 0f;
				date.Style.Opacity = 0f;
				float f = (TimeSinceIntroStarted - 24.726f);
				float sin = MathF.Sin( Time.Now * 10f );
				if(f < 4f )
				{
					date.Style.Opacity = 1f;
					if ( sin > 0 ) {
						Play.Style.Opacity = 1f;
					}
					else
					{
						Play.Style.Opacity = 0f;
					}
				}

			}

			DateTime dt = DateTime.Now.AddYears( -35 );

			string s = dt.ToString( @"tt hh:mm" );
			s += "\n";

			s += dt.ToString( @"MMM. dd yyyy" );
			date.Text = s;



		}


	}
}
