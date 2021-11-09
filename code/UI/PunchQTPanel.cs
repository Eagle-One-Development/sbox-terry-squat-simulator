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
	public Panel Back;
	public Panel Measure;
	public bool Finished;
	public Label Key;
	public TimeSince TimeSinceSpawned;
	public bool Failed;
	public PunchQTPanel(PunchQT p, Vector2 p2)
	{
		Parent = TSSHud.Instance.RootPanel;
		MyQT = p;
		Pos = p2;
		StyleSheet.Load( "/UI/PunchQTPanel.scss" );

		Back = Add.Panel( "back" );
		Key = Add.Label( "A", "key" );
		Measure = Add.Panel( "measure" );

		Finished = false;
	}

	public override void Tick()
	{
		base.Tick();
		float f = (1f - MathF.Pow( (TimeSinceSpawned / 0.5f).Clamp( 0, 1f ), 3.0f ));
		PanelTransform pt = new PanelTransform();
		pt.AddTranslateX( Length.Pixels( (Screen.Width / 2) + Pos.x) );
		pt.AddTranslateY( Length.Pixels( (Screen.Height / 2) + Pos.y ) );
		

		if ( !Finished )
		{
			TimeSinceSpawned = 0;
			Back.Style.Opacity = 0;
		}
		else
		{
			Back.Style.Opacity = f;
			Key.Style.Opacity = 0f;
			Style.Opacity = f;
			Measure.Style.Opacity = 0f;
			if ( Failed )
			{
				Back.Style.BackgroundColor = Color.Red;
			}
		}

		Key.Style.Dirty();

		if(TimeSinceSpawned > 1f )
		{
			Delete();
		}

		Style.Opacity = f;
		Style.Transform = pt;
		Style.Dirty();

		f = MathX.LerpTo( 0f, 1f, (MyQT.MyTime / 1f).Clamp( 0, 1f ) );



		Measure.Style.Width = Length.Fraction( 1.1f * (1 - f) );
		Measure.Style.Height= Length.Fraction( 1.1f * (1 - f) );
		Measure.Style.Dirty();

		
		//		DebugOverlay.ScreenText( new Vector2( Screen.Width / 2 + Pos.x, Screen.Height / 2 + Pos.y + 200f ), MyQT.MyTime.ToString() );
	}
}
