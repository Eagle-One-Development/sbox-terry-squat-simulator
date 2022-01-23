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
		public TimeSince TimeSinceEnded;

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

			float val = (MyQT.TimeSinceSpawned - (MyQT.MyTime + 0.15f)) / 0.5f;
			float growth = val.Clamp( 0f, 1f );

			if ( Local.Pawn is TSSPlayer pl )
			{
				if ( pl.CanGoToHeaven )
				{
					Style.BorderColor = Color.Black;
					Key.Style.FontColor = Color.Black;
					Measure.Style.BackgroundColor = Color.Black;
				}
			}

			if ( Finished )
			{
				Back.Style.Opacity = 1f - growth;
				Key.Style.Opacity = 0f;
				Style.Opacity = 1f - growth;
				Measure.Style.Opacity = 0f;
				Back.Style.BackgroundColor = (Failed) ? Color.Red : Color.Green;
				Style.BorderColor = (Failed) ? Color.Red : Color.Green;
			}
			else
			{
				Back.Style.Opacity = 0;
			}

			Key.Style.Dirty();

			if ( TimeSinceSpawned > MyQT.MyTime + 0.5f)
			{
				Delete(true);
			}

			if ( !Finished )
			{
				Style.Opacity = 1f;
			}
			Style.Left = Length.Percent( 50f + Pos.x );
			Style.Top = Length.Percent( 50f + Pos.y );
			Style.Dirty();

			growth = (MyQT.TimeSinceSpawned / (MyQT.MyTime + 0.15f)).Clamp( 0, 1f );

			Measure.Style.Width = Length.Fraction( 1f * growth);
			Measure.Style.Height = Length.Fraction( 1f * growth);
			Measure.Style.Dirty();

		}
	}
}
