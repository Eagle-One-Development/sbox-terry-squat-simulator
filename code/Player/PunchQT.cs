using Sandbox;
using TSS.UI;
using System.Linq;

namespace TSS
{
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

		[Net, Predicted]
		public TimeSince TimeSinceSpawned { get; set; }

		public PunchQTPanel Panel;

		public override void Spawn()
		{
			base.Spawn();
			MyTime =  ((60f/140f) * 2f);
			Transmit = TransmitType.Always;
			TimeSinceSpawned = 0f;
		}

		public new void Delete()
		{
			base.Delete();
			Panel?.Delete( true );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			TimeSinceSpawned = 0f;

			Panel = new PunchQTPanel( this, new Vector2( Rand.Float( -20f, 20f ), Rand.Float( -20f, 20f ) ) );
			switch ( Type )
			{
				case 0:
					Panel.Key.Text = Input.GetKeyWithBinding( "+iv_forward" ).ToUpper();
					break;
				case 1:
					Panel.Key.Text = Input.GetKeyWithBinding( "+iv_back" ).ToUpper();
					break;
				case 2:
					Panel.Key.Text = Input.GetKeyWithBinding( "+iv_left" ).ToUpper();
					break;
				case 3:
					Panel.Key.Text = Input.GetKeyWithBinding( "+iv_right" ).ToUpper();
					break;
			}
			MyTime = ((60f / 140f) * 2f);
		}

		[Event.BuildInput]
		public void BuildPunchInput( InputBuilder input )
		{
			bool pressed = false;
			if ( Type == 0 )
			{
				pressed = Input.Pressed( InputButton.Forward );
			}
			if ( Type == 1 )
			{
				pressed = Input.Pressed( InputButton.Back );
			}
			if ( Type == 2 )
			{
				pressed = Input.Pressed( InputButton.Left );
			}
			if ( Type == 3 )
			{
				pressed = Input.Pressed( InputButton.Right );
			}



			if ( TimeSinceSpawned > MyTime - 0.15f && TimeSinceSpawned < MyTime + 0.15f )
			{
				if ( pressed )
				{
					ConsoleSystem.Run( "Punch" );

					if ( IsClient )
					{
						Panel.Finished = true;
					}

					ConsoleSystem.Run( "delete_punch", this.NetworkIdent );
				}
			}
		}

		[ServerCmd("delete_punch")]
		public static void DeletePunch(int i)
		{
			var ent = Entity.All.Where( x => x.NetworkIdent == i ).Any();
			if ( ent )
			{
				Entity.All.Where( x => x.NetworkIdent == i ).First().Delete();
			}
		}

		[Event.Tick]
		public void Simulate()
		{
			if ( Player == null )
			{
				return;
			}
			
			if( Player.CurrentExercise != Exercise.Punch )
			{
				if ( IsServer )
				{
					Delete();
				}
				Panel?.Delete();
			}

			

			if ( TimeSinceSpawned < MyTime - 0.15f )
			{
				if ( Input.Pressed( InputButton.Forward ) || Input.Pressed( InputButton.Back ) || Input.Pressed( InputButton.Right ) || Input.Pressed( InputButton.Left ) )
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

			if ( TimeSinceSpawned > MyTime+0.15f )
			{
				if ( IsClient )
				{
					if ( Panel != null && !Panel.Finished )
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
}
