using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class CreditPanel : WorldPanel
{
	public float Opacity;
	public float FontSize;
	public Label l;
	public float TextScale;
	public CreditPanel(string s, int x, int y)
	{
		Opacity = 1f;
		FontSize = 200f;
		float width = x;
		float height = y;
		PanelBounds = new Rect( -(width/2), -height, width, height);
		TextScale = 1f;
		l = Add.Label( s, "title" );

		StyleSheet.Load( "/ui/CreditPanel.scss" );
	}

	public override void Tick()
	{
		base.Tick();
		Style.Opacity = Opacity;
		
		
		Style.Dirty();
		l.Style.FontSize = Length.Pixels( FontSize * TextScale);
		
	}
}
