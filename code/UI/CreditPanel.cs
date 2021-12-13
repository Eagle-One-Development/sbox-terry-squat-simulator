using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


namespace TSS.UI
{
	public class CreditPanel : WorldPanel
	{
		public float Opacity;
		public float FontSize;
		public Label l;
		public float TextScale;
		public bool Bop;
		private float Bump;
		public CreditPanel( string s, int x, int y )
		{
			Opacity = 1f;
			FontSize = 200f;
			float width = x;
			float height = y;
			PanelBounds = new Rect( -(width / 2), -height, width, height );
			TextScale = 1f;
			l = Add.Label( s, "title" );
			Bump = 1f;
			StyleSheet.Load( "/ui/CreditPanel.scss" );
		}

		[Event( "OtherBeat" )]
		public void BopToTheBeat()
		{
			if ( Bop )
			{
				Bump = 1.2f;
			}
		}

		public override void Tick()
		{
			base.Tick();
			Style.Opacity = Opacity;

			Bump = Bump.LerpTo( 1f, Time.Delta * 8f );

			if(Local.Pawn is TSSPlayer player )
			{
				if ( player.CanGoToHeaven )
				{
					l.Style.FontColor = Color.Black;
				}
			}

			Style.Dirty();
			l.Style.FontSize = Length.Pixels( FontSize * TextScale * Bump );

		}
	}
}
