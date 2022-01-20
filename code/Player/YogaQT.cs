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
		string[] combos { get; set; } = { "0123", "11230", "001122", "21330", "011230" };

		/// <summary>
		/// The combo we're using for this QT
		/// </summary>
		string currentCombo { get; set; }

		/// <summary>
		/// The current index, starting at 0
		/// </summary>
		public int index { get; set; }


		public TSSPlayer Player { get; set; }

		public YogaQTPanel Panel;
		public TimeSince TimeSinceSpawned;

		public int pose;

		public override void Spawn()
		{
			base.Spawn();

			pose = Rand.Int( 0, 4 );

			currentCombo = combos[pose];

			Panel = new YogaQTPanel( this, new Vector2( Rand.Float( -200f, 200f ), Rand.Float( -200f, 200f ) ), currentCombo );
			Player = Entity.All.OfType<TSSPlayer>().First();
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

			

		}

		[Event.BuildInput]
		public void BuildYogaInput( InputBuilder input )
		{
			if ( currentCombo == null )
			{
				currentCombo = Rand.FromArray( combos );
			}

			index = index.Clamp( 0, currentCombo.Length - 1 );
			var type = currentCombo[index];

			bool b = CheckType( type , input);

			if ( CheckFailure( type , input) )
			{
				Panel.Failed = true;
				Panel.TimeSinceFinished = 0;
				Delete();
			}

			if ( b )
			{
				index++;
			}

			if ( index >= currentCombo.Length )
			{
				Panel.Finished = true;
				Panel.TimeSinceFinished = 0;
				ConsoleSystem.Run( "yoga_pose", pose + 1 );
				Delete();
			}

			if ( Player.CurrentExercise != Exercise.Yoga )
			{
				if ( IsServer )
				{
					Delete();
				}
				Panel?.Delete();
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
		/// TODO: I'm sure there's some bitwise shit we could do to figure this out
		/// </summary>
		bool CheckFailure( char type , InputBuilder input )
		{

			if ( type == '0' )
			{

				if ( input.Pressed( InputButton.Right ) || input.Pressed( InputButton.Left ) || input.Pressed( InputButton.Back ) )
				{
					return true;
				}
			}

			if ( type == '1' )
			{
				if ( input.Pressed( InputButton.Right ) || input.Pressed( InputButton.Left ) || input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			if ( type == '2' )
			{
				if ( input.Pressed( InputButton.Back ) || input.Pressed( InputButton.Left ) || input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			if ( type == '3' )
			{
				if ( input.Pressed( InputButton.Back ) || input.Pressed( InputButton.Right ) || input.Pressed( InputButton.Forward ) )
				{
					return true;
				}
			}

			return false;
		}

		public bool CheckType( char c , InputBuilder input )
		{
			switch ( c )
			{
				case '0':
					return input.Pressed( InputButton.Forward );
				case '1':
					return input.Pressed( InputButton.Back );
				case '2':
					return input.Pressed( InputButton.Right );
				case '3':
					return input.Pressed( InputButton.Left );
			}
			return false;
		}
	}
}
