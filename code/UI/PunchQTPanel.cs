using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PunchQTPanel : Panel
{
	public PunchQT MyQT;
	public Vector2 Pos;
	public PunchQTPanel(PunchQT p, Vector2 p2)
	{
		Parent = TSSHud.Instance.RootPanel;
		MyQT = p;
		Pos = p2;
		StyleSheet.Load( "/UI/PunchQTPanel.scss" );
		Log.Info( "I EXIST" );
	}

	public override void Tick()
	{
		base.Tick();
		PanelTransform pt = new PanelTransform();
		pt.AddTranslateX( Length.Pixels( (Screen.Width / 2) + Pos.x) );
		pt.AddTranslateY( Length.Pixels( (Screen.Height / 2) + Pos.y ) );
		Style.Transform = pt;
		Style.Dirty();

		DebugOverlay.ScreenText( new Vector2( Screen.Width / 2 + Pos.x, Screen.Height / 2 + Pos.y ), MyQT.MyTime.ToString() );
	}
}
