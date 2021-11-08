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
		MyTime = TargetTime;
		Transmit = TransmitType.Always;
		Log.Info( "I SPAWNED" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Log.Info( "I'm ON THE CLIENT, MA" );

		Panel = new PunchQTPanel( this, new Vector2( Rand.Float(-200f, 200f ), Rand.Float( -200f, 200f) ) );
	}

	[Event.Tick]
	public void Sim()
	{
		
		

		bool b = false;

		if(Player == null )
		{
			return;
		}

		if(Type == 0 )
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
		if ( Type == 0 )
		{
			b = Input.Pressed( InputButton.Right );
		}

		MyTime -= Time.Delta * 0.1f;

		if(MyTime > -0.1f && MyTime < 0.1f )
		{
			if ( b )
			{
				Player.CreatePoint(1);
				Player.ExercisePoints++;
				Delete();
			}
		}

		DebugOverlay.Text( Position, MyTime.ToString() );

		if(MyTime < -0.1f )
		{
			if ( IsClient )
			{
				Panel?.Delete( true );
			}
			if ( IsServer )
			{
				Delete();
				return;
			}

		}



	}
}
