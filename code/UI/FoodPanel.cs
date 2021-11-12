using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class FoodPanel: WorldPanel
{
	public Color TextColor;
	public string Text;

	public Label Top;
	public Label Bottom;

	public FoodPanel(Vector2 size, Color color, string text )
	{
		StyleSheet.Load( "/ui/FoodPanel.scss" );
		float width = size.x;
		float height = size.y;
		PanelBounds = new Rect( -(width / 2), -height, width, height );
		Bottom = Add.Label( text, "text" );
		Top = Add.Label( "+5", "text" );
		Top.SetClass( "top", true );


	}

	public override void Tick()
	{
		base.Tick();
		if(Local.Pawn is TSSPlayer p )
		{
			if(p.Camera is TSSCamera c )
			{
				Rotation = Rotation.LookAt( (Position - c.Position) * -1f, Vector3.Up );
			}
		}
		
	}
}
