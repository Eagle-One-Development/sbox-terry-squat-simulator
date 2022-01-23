using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TSS;

namespace Twitch.Commands
{
	/// <summary>
	/// A class used to better process twitch commands
	/// </summary>
	public class TwitchCommand
	{
		public string Command = "";
		protected TSSPlayer Pl;

		public TwitchCommand() { }

		public TwitchCommand(string s, TSSPlayer p )
		{
			Pl = p;
			Command = s;
		}

		public virtual void Evalulate( GenericMessage msg )
		{
			if ( msg.Message.Contains( Command ) )
			{
				Evaluation( msg );
			}
		}

		public virtual void Evaluation( GenericMessage msg )
		{

		}

	}

	public class FoodCommand : TwitchCommand
	{
		public TimeSince TimeSinceEaten;
		public string FoodItem;

		public FoodCommand( string s, TSSPlayer p, string f)
		{
			Pl = p;
			Command = s;
			FoodItem = f;
		}

		public override void Evaluation( GenericMessage msg )
		{
			if(TimeSinceEaten > 3f )
			{
				var f = Library.Create<Entity>( FoodItem );
				TimeSinceEaten = 0f;
				TSSGame.Current.AddHudMessage( (f as Food).Description , msg.DisplayName, msg.Color );
			}
		}
	}

	public class ExerciseCommand : TwitchCommand
	{
		public TimeSince TimeSinceExerciseLastChanged;

		public ExerciseCommand( string s, TSSPlayer p )
		{
			Pl = p;
			Command = s;
		}

		public override void Evaluation( GenericMessage msg )
		{
			if( TimeSinceExerciseLastChanged > 10f && Pl.ExercisePoints > 400f)
			{
				Event.Run( "rand_exercise" );
				TSSGame.Current.AddHudMessage( "shakes things up!", msg.DisplayName, msg.Color );
				TimeSinceExerciseLastChanged = 0f;
			}
		}
	}

	public class KillCommand : TwitchCommand
	{
		public TimeSince TimeSinceLastKilled;

		public KillCommand( string s, TSSPlayer p )
		{
			Pl = p;
			Command = s;
		}

		public override void Evaluation( GenericMessage msg )
		{
			if ( TimeSinceLastKilled > 10f && Pl.ExercisePoints > 110f )
			{
				Pl.KillTerry( Vector3.Zero );
				TSSGame.Current.AddHudMessage( "smites Terry!", msg.DisplayName, msg.Color );
				TimeSinceLastKilled = 0f;
			}
		}
	}

	public class CheerCommand : TwitchCommand
	{
		public TimeSince TimeSinceCheered;

		public CheerCommand( string s, TSSPlayer p )
		{
			Pl = p;
			Command = s;
		}

		public override void Evaluation( GenericMessage msg )
		{
			Log.Info( "CHER" );
			if ( TimeSinceCheered > 0.5f )
			{
				Sound.FromScreen( $"cheering_0{Rand.Int( 1, 3 )}" );
				TSSGame.Current.AddHudMessage( "motivates Terry!", msg.DisplayName, msg.Color );
				TimeSinceCheered = 0f;
			}
		}
	}

}
