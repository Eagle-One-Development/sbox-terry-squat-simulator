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
		/// This is for the transition from the glowing crag of celestial light to the nature scene, but holy shit does this code base need a refactor.
		/// Transition Panel TWO? Very descriptive. Note to self: stop programming inebriated. And maybe start writing UML diagrams.
		/// </summary>
		public Panel TransitionPanel2;

		/// <summary>
		/// Basically whether or not we've entered the white void the player exercises in just before the ending
		/// </summary>
		public bool HeavenTransitioned;

		/// <summary>
		/// Once we've finished the transition to heaven.
		/// </summary>
		public bool HeavenTransitionedFinished;

		public bool FinalBlackout;

		public static EndingPanel Instance;

		/// <summary>
		/// A variable used for the opacity of the transition, initially set to 0
		/// </summary>
		public float Alph;

		public bool CanEndTheGame;
		public bool CreditsStarted;
		public TimeSince TimeSinceEnded;

		public Label Credits;
		

		public void StartCredits()
		{
			TimeSinceEnded = 0f;
			CanEndTheGame = true;
		}

		

		public bool CanGoToNature;
		public bool WentToNature;
		public TimeSince TimeSinceNatureTransition;
		public float offset;

		public EndingPanel()
		{
			StyleSheet.Load( "/ui/EndingPanel.scss" );
			Instance = this;
			TransitionPanel = Add.Panel( "transitions" );
			TransitionPanel2 = Add.Panel( "transitions" );
			Alph = 0;
			offset = 100;

		}

		public void HeavenTransition(TSSPlayer player,ref float alph, ref Color c )
		{
			if ( player.ExercisePoints > player.HeavenThreshold && !HeavenTransitioned  && !FinalBlackout)
			{
				Alph += Time.Delta;

				if ( Alph > 1.1f )
				{
					HeavenTransitioned = true;
					ConsoleSystem.Run( "heaven" );
					
				}
			}

			if ( HeavenTransitioned && !FinalBlackout )
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

		public string CreditsString()
		{
			string credits = "TERRY\nSQUAT\nSIMULATOR\n\n\n";
			credits += "Programming\nJosh Wilson\nJac0xb\nTaek Dev\nDoctor Gurke\n\n\nArt\nKabubu\n\n\nLevel Design\nGmo Man\n\nSound\nDawdle\nMungus\nJac0xb\n\n\nAnimation\nWhimsicalVR\nJosh Wilson\n\n\nVoice Acting\nMungus\nNavia Shetty\nPhar0\nGvarados\n\n\nSounds Provided\nFrom Freesound.Org\nInspectorJ";
			return credits;
		}

		public override void OnHotloaded()
		{
			base.OnHotloaded();

			
		}

		public override void Tick()
		{
			base.Tick();
			if(Local.Pawn is TSSPlayer player )
			{
				float a = Alph;
				Color c = Color.White;

				if ( !FinalBlackout )
				{
					HeavenTransition( player, ref Alph, ref c );
				}

				if ( FinalBlackout )
				{
					c = Color.Black;
					Alph = 1f;
				}

				if ( player.SkipIntro )
				{
					a = 0f;
				}

				TransitionPanel.Style.Opacity = a;
				TransitionPanel.Style.BackgroundColor = c;
				
			
			}

			if ( FinalBlackout && !CanEndTheGame )
			{
				TransitionPanel.Style.Opacity = Alph;
				if(Alph >= 0 )
				{
					Alph -= Time.Delta * 0.5f;
				}
				TransitionPanel.Style.BackgroundColor = Color.Black;
			}

			if ( CanGoToNature )
			{
				float f = 0f;

				if(TimeSinceNatureTransition < 2f )
				{
					f = TimeSinceNatureTransition / 2f;
				}

				if(TimeSinceNatureTransition > 2f )
				{
					if ( !WentToNature )
					{
						ConsoleSystem.Run( "nature" );
						WentToNature = true;
					}
					f = 1f - ((TimeSinceNatureTransition - 2f) / 5f);
				}

				TransitionPanel2.Style.BackgroundColor = Color.White;
				TransitionPanel2.Style.Opacity = f;

			}
			else
			{
				TimeSinceNatureTransition = 0f;
			}

			if ( CanEndTheGame )
			{
				float f = 0f;

				if ( TimeSinceEnded < 2f )
				{
					f = TimeSinceEnded / 2f;
				}

				if ( TimeSinceEnded > 2f )
				{
					if ( !CreditsStarted )
					{
						ConsoleSystem.Run( "credits" );
						CreditsStarted = true;
						Credits = Add.Label( "TEST", "credits" );
						Credits.Text = CreditsString();
					}
					f = 1f - ((TimeSinceEnded - 2f) / 5f);
				}

				TransitionPanel2.Style.BackgroundColor = Color.White;
				TransitionPanel2.Style.Opacity = f;

			}
			else
			{
				TimeSinceEnded = 0f;
			}

			if(Credits != null )
			{
				Credits.Style.Top = Length.Percent(offset);
				offset -= Time.Delta * 4f;
			}

		}
	}


}
