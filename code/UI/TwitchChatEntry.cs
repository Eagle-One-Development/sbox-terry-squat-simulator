using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using TSS;
using Sandbox.UI.Construct;
/// <summary>
/// 
/// </summary>
public class TwitchChatEntry : Panel
{
	public TimeSince TimeSinceSpawned;

	public Label Name;
	public Label Msg;
	public GenericMessage Message;
	private float padding => (Screen.Height * 0.15f);
	private Vector2 pos;
	public TwitchChatEntry()
	{
		StyleSheet.Load( "/ui/TwitchPanel.scss" );
		TimeSinceSpawned = 0f;
		Name = Add.Label( "NAME", "name" );
		Msg = Add.Label( "Message", "msg" );

		pos = new Vector2( Rand.Float( padding, Screen.Width - padding ), Rand.Float( padding, Screen.Height - padding ) );
	}

	public override void Tick()
	{
		base.Tick();

		if(TimeSinceSpawned > 1.5f )
		{
			Delete( true );
		}

		float value = TimeSinceSpawned / 0.5f;

		var p = new PanelTransform();
		p.AddScale(new Vector3( Easing.BounceOut( value.Clamp( 0f, 1f ) ),1f,1f) );
		Style.Transform = p;
		Style.Left = Length.Pixels( pos.x );
		Style.Top = Length.Pixels( pos.y );

		Name.Text = Message.DisplayName;
		Msg.Text = Message.Message;
		Name.Style.FontColor = Color.Parse( Message.Color );


	}

}
