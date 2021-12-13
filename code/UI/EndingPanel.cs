using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TSS.UI
{
	/// <summary>
	/// This is a panel that will handel any overlays and fades we need for the ending.
	/// </summary>
	public class EndingPanel : Panel
	{
		/// <summary>
		/// Reference to the panel covering the screen
		/// </summary>
		public Panel TransitionPanel;

		/// <summary>
		/// Basically whether or not we've entered the white void the player exercises in just before the ending
		/// </summary>
		public bool HeavenTransitioned;

		/// <summary>
		/// Once we've finished the transition to heaven.
		/// </summary>
		public bool HeavenTransitionedFinished;


		/// <summary>
		/// A variable used for the opacity of the transition, initially set to 0
		/// </summary>
		private float Alph;

		public EndingPanel()
		{
			StyleSheet.Load( "/ui/EndingPanel.scss" );
			TransitionPanel = Add.Panel( "transitions" );
			Alph = 0;

		}

		public void HeavenTransition(TSSPlayer player,ref float alph, ref Color c )
		{
			if ( player.ExercisePoints > player.HeavenThreshold && !HeavenTransitioned )
			{
				Alph += Time.Delta;

				if ( Alph > 1.1f )
				{
					HeavenTransitioned = true;
					ConsoleSystem.Run( "heaven" );
				}
			}

			if ( HeavenTransitioned )
			{
				if ( Alph >= 0 )
				{
					Alph -= Time.Delta;

				}

				if ( Alph < 0 )
				{
					Alph = 0;
				}
			}

		}

		public override void Tick()
		{
			base.Tick();
			if(Local.Pawn is TSSPlayer player )
			{
				float a = Alph;
				Color c = Color.White;

				HeavenTransition( player,ref Alph,ref c );
				
				TransitionPanel.Style.Opacity = a;
				TransitionPanel.Style.BackgroundColor = c;
			}
		}
	}


}
