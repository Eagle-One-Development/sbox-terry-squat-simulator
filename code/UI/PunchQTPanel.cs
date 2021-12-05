using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TSS.UI
{
	public class PunchQTPanel : Panel
	{
		public Vector2 Pos;
		public PunchQT MyQT;
		public Panel Back;
		public Panel Measure;
		public Label Key;
		public bool Finished;
		public bool Failed;
		public TimeSince TimeSinceSpawned;

		public PunchQTPanel( PunchQT p, Vector2 p2 )
		{
			Parent = TSSHud.Instance.RootPanel;
			MyQT = p;
			Pos = p2;
			StyleSheet.Load( "/UI/PunchQTPanel.scss" );

			Back = Add.Panel( "back" );
			Key = Add.Label( "A", "key" );
			Measure = Add.Panel( "measure" );

			Finished = false;
			TimeSinceSpawned = 0f;
		}

		public override void Tick()
		{
			base.Tick();

			PanelTransform pt = new();

			pt.AddTranslateX( Length.Pixels( (Screen.Width / 2) + Pos.x ) );
			pt.AddTranslateY( Length.Pixels( (Screen.Height / 2) + Pos.y ) );
			float growth = (1f - MathF.Pow( (TimeSinceSpawned / 0.9f).Clamp( 0, 1f ), 3.0f ));

			if ( Finished )
			{
				Back.Style.Opacity = growth;
				Key.Style.Opacity = 0f;
				Style.Opacity = 2 * growth;
				Measure.Style.Opacity = 0f;
				Back.Style.BackgroundColor = (Failed) ? Color.Red : Color.Green;

			}
			else
			{
				Back.Style.Opacity = 0;
			}

			Key.Style.Dirty();

			if ( TimeSinceSpawned > 1f )
			{
				Delete(true);
			}

			Style.Opacity = growth;
			Style.Transform = pt;
			Style.Dirty();

			growth = MathX.LerpTo( 0f, 1f, (MyQT.MyTime / 1f).Clamp( 0, 1f ) );

			Measure.Style.Width = Length.Fraction( 1.1f * (1 - growth) );
			Measure.Style.Height = Length.Fraction( 1.1f * (1 - growth) );
			Measure.Style.Dirty();

		}
	}
}
