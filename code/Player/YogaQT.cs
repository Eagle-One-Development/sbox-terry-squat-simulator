using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TSS.UI;

namespace TSS
{
	public partial class YogaQT : Entity
	{
		/// <summary>
		/// A list of the possible combos for the yoga positions
		/// </summary>
		[Net]
		string[] combos { get; set; } = { "0123", "11230", "001122", "21330", "011230" };

		/// <summary>
		/// The combo we're using for this QT
		/// </summary>
		string currentCombo { get; set; }

		/// <summary>
		/// The current index, starting at 0
		/// </summary>
		public int index { get; set; }

		[Net]
		public TSSPlayer Player { get; set; }

		public YogaQTPanel Panel;
		public TimeSince TimeSinceSpawned;

		public int pose;

		public override void Spawn()
		{
			base.Spawn();
			Log.Info( "SPAWN" );

			pose = Rand.Int( 0, 4 );

			currentCombo = combos[pose];

			Panel = new YogaQTPanel( this, new Vector2( Rand.Float( -200f, 200f ), Rand.Float( -200f, 200f ) ), currentCombo );
			TimeSinceSpawned = 0;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();


		}

		[Event.Tick]
		public void Sim()
		{
			if ( IsServer )
			{
				return;
			}

			if ( currentCombo == null )
			{
				currentCombo = Rand.FromArray( combos );
			}

			index = index.Clamp( 0, currentCombo.Length - 1 );
			var type = currentCombo[index];

			bool b = CheckType( type );



			if ( CheckFailure( type ) )
			{

				Log.Info( "FAILED OR WRONG KEY PRESS YOU MORON" );
				Panel.Failed = true;
				Panel.TimeSinceFinished = 0;
				Delete();


			}

			if ( b )
			{
				Log.Info( "YOU HAVE PRESSED THE CORRECT KEY" );
				index++;
			}

			if ( index >= currentCombo.Length )
			{
				Log.Info( "FINISHED" );
				Panel.Finished = true;
				Panel.TimeSinceFinished = 0;
				ConsoleSystem.Run( "yoga_pose", pose + 1 );
				Player.GivePoints( 5 );
				Delete();
			}



			if ( TimeSinceSpawned > 3f )
			{
				Panel.Failed = true;
				Panel.TimeSinceFinished = 0;
				Delete();
			}

		}


		/// <summary>
		/// In theory there's a way better way to do this, but I'm not really sure how
		/// </summary>
		/// <param name="type"></param>
		bool CheckFailure( char type )
		{

			if ( type == '0' )
			{

				if ( Input.Pressed( InputButton.Right ) || Input.Pressed( InputButton.Left ) || Input.Pressed( InputButton.Back ) )
				{
					return true;
				}
			}

			if ( type == '1' )
			{
				if ( Input.Pressed( InputButton.Right ) || Input.Pressed( InputButton.Left ) || Input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			if ( type == '2' )
			{
				if ( Input.Pressed( InputButton.Back ) || Input.Pressed( InputButton.Left ) || Input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			if ( type == '3' )
			{
				if ( Input.Pressed( InputButton.Back ) || Input.Pressed( InputButton.Right ) || Input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			return false;
		}

		public bool CheckType( char c )
		{
			switch ( c )
			{
				case '0':
					return Input.Pressed( InputButton.Forward );
				case '1':
					return Input.Pressed( InputButton.Back );
				case '2':
					return Input.Pressed( InputButton.Right );
				case '3':
					return Input.Pressed( InputButton.Left );
			}
			return false;
		}
	}
}
