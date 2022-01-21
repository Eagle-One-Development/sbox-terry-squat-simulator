using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TSS.UI
{
	public class YogaQTPanel : Panel
	{
		public YogaQT MyQT;
		public Vector2 Pos;
		public bool Finished;
		public bool Failed;

		public Panel back;
		public Panel timer;
		public List<Label> Letters;
		public List<float> Scales;

		public TimeSince TimeSinceSpawned;
		public TimeSince TimeSinceFinished;

		public YogaQTPanel( YogaQT p, Vector2 p2, string combo )
		{
			Parent = TSSHud.Instance.RootPanel;
			MyQT = p;
			Pos = p2;
			StyleSheet.Load( "/UI/YogaQTPanel.scss" );



			Letters = new List<Label>();
			Scales = new List<float>();

			for ( int i = 0; i < combo.Length; i++ )
			{
				var pan = Add.Panel( "keyPanel" );

				var lab = pan.Add.Label( GetKey( combo[i] ), "label" );
				Letters.Add( lab );
				Scales.Add( 0f );
			}

			TimeSinceSpawned = 0f;


			back = Add.Panel( "back" );
			timer = Add.Panel( "timer" );


			Finished = false;
		}

		public string GetKey( char c )
		{
			switch ( c )
			{
				case '0':
					return Input.GetKeyWithBinding( "+iv_forward" ).ToUpper();
				case '1':
					return Input.GetKeyWithBinding( "+iv_back" ).ToUpper();
				case '3':
					return Input.GetKeyWithBinding( "+iv_left" ).ToUpper();
				case '2':
					return Input.GetKeyWithBinding( "+iv_right" ).ToUpper();
			}
			return "";
		}

		public float Out( float k )
		{
			k = k.Clamp( 0, 1f );
			if ( k == 0 ) return 0;
			if ( k == 1 ) return 1;
			return MathF.Pow( 2f, -10f * k ) * MathF.Sin( (k - 0.1f) * (2f * MathF.PI) / 0.4f ) + 1f;
		}

		public override void Tick()
		{
			base.Tick();


			Style.Left = Length.Percent( 50f + Pos.x );
			Style.Top = Length.Percent( 50f + Pos.y );
			Style.Dirty();

			for ( int i = 0; i < Letters.Count; i++ )
			{
				var letter = Letters[i];


				if ( i < MyQT.index )
				{
					Scales[i] = Scales[i].LerpTo( 1f, Time.Delta * 8f );
					letter.Style.FontColor = Color.White;
					if ( Local.Pawn is TSSPlayer pl2 )
					{
						if ( pl2.CanGoToHeaven )
						{
							letter.Style.FontColor = Color.Black;
						}
					}
				}
				else
				{
					if ( MyQT.TimeSinceSpawned > (i * 0.05f) )
					{
						Scales[i] = Scales[i].LerpTo( 0.7f, Time.Delta * 8f );
						letter.Style.FontColor = Color.Gray * 1.5f;
					}
					else
					{
						Scales[i] = 0f;

					}
				}

				var p = new PanelTransform();
				p.AddScale( Scales[i] );
				letter.Style.Transform = p;
			}

			if ( Failed )
			{
				timer.Style.Opacity = 0f;
				back.Style.BackgroundColor = Color.Red;
				back.Style.Opacity = 1.1f - (TimeSinceFinished / 0.5f);
				foreach ( var l in Letters )
				{
					l.Parent.Style.Opacity = 1.1f - (TimeSinceFinished / 0.5f);
					l.Style.FontColor = Color.Red;
				}

				if ( TimeSinceFinished > 0.5f )
				{
					Delete(true);
				}
			}

			if ( Finished )
			{
				timer.Style.Opacity = 0f;
				back.Style.BackgroundColor = Color.White;
				back.Style.Opacity = 1.1f - (TimeSinceFinished / 0.5f);
				foreach ( var l in Letters )
				{
					if (l != null && l.Parent != null )
					{
						l.Parent.Style.Opacity = 1.1f - (TimeSinceFinished / 0.5f);
						l.Style.FontColor = Color.White;
					}
				}

				if ( TimeSinceFinished > 0.5f )
				{
					Delete(true);
				}
			}

			if ( MyQT.TimeSinceSpawned > 3f && !Failed )
			{
				Failed = true;
				TimeSinceFinished = 0f;
			}

			timer.Style.Width = Length.Fraction( MyQT.TimeSinceSpawned / 3f );
			if ( Local.Pawn is TSSPlayer pl )
			{
				if ( pl.CanGoToHeaven )
				{
					timer.Style.BackgroundColor = Color.Black;
				}
			}


		}
	}
}
