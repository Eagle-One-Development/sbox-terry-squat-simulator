using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class PunchQT : Entity
{
	[Net]
	public int Type { get; set; }

	[Net, Predicted]
	public float MyTime { get; set; }

	[Net, Predicted]
	public float TargetTime { get; set; }

	[Net]
	public TSSPlayer Player { get; set; }

	

	public PunchQTPanel Panel;

	


	public override void Spawn()
	{
		base.Spawn();
		MyTime = 1f;
		Transmit = TransmitType.Always;

		//Log.Info( "I SPAWNED" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		//Log.Info( "I'm ON THE CLIENT, MA" );

		Panel = new PunchQTPanel( this, new Vector2( Rand.Float(-200f, 200f ), Rand.Float( -200f, 200f) ) );
		switch(Type)
		{
			case 0:
			Panel.Key.Text = Input.GetKeyWithBinding("+iv_forward");
				break;
			case 1:
				Panel.Key.Text = Input.GetKeyWithBinding( "+iv_back" );
				break;
			case 2:
				Panel.Key.Text = Input.GetKeyWithBinding( "+iv_left" );
				break;
			case 3:
				Panel.Key.Text = Input.GetKeyWithBinding( "+iv_right" );
				break;
		}
	}

	[Event.Tick]
	public void Sim()
	{
		if (Player == null )
		{
			return;
		}

		bool b = false;
		if (Type == 0 )
		{
			b = Input.Pressed( InputButton.Forward );
		}
		if ( Type == 1 )
		{
			b = Input.Pressed( InputButton.Back );
		}
		if ( Type == 2 )
		{
			b = Input.Pressed( InputButton.Left );
		}
		if ( Type == 3 )
		{
			b = Input.Pressed( InputButton.Right );
		}

		if(IsServer && Input.Pressed( InputButton.Reload ) )
		{
			Log.Info( "SERVER SIDED INPUT" );
		}

		MyTime -= Time.Delta;

		if(MyTime > -0.15f && MyTime < 0.15f )
		{
			if ( b )
			{
				Log.Info( IsServer );
				ConsoleSystem.Run( "Punch" );
				
				if ( IsClient )
				{
					Panel.Finished = true;
				}
				
				if ( IsServer )
				{
					Delete();
				}
			}
		}

		if(MyTime > 0.15f )
		{
			if ( Input.Pressed(InputButton.Forward) || Input.Pressed( InputButton.Back ) || Input.Pressed( InputButton.Right ) || Input.Pressed( InputButton.Left ) )
			{

				if ( IsClient )
				{
					Panel.Finished = true;
					Panel.Failed = true;
				}
				if ( IsServer )
				{
					Delete();
				}
				return;
			}
		}

		if ( Panel != null )
		{
			//DebugOverlay.ScreenText( new Vector2( Screen.Width / 2 + Panel.Pos.x, Screen.Height / 2 + Panel.Pos.y - 200f ), MyTime.ToString(), 0 );
		}

		if(MyTime < -0.1f )
		{
			if ( IsClient )
			{
				if ( !Panel.Finished )
				{
					Panel?.Delete( true );
				}
			}
			if ( IsServer )
			{
				Delete();
				return;
			}

		}



	}
}
