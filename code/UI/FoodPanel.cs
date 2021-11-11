using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class FoodPanel: WorldPanel
{
	public Color TextColor;
	public string Text;
	public FoodPanel(Vector2 size, Color color, string text )
	{
		StyleSheet.Load( "/ui/FoodPanel.scss" );
		float width = size.x;
		float height = size.y;
		PanelBounds = new Rect( -(width / 2), -height, width, height );
	}
}
