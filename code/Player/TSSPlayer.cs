using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;
using TSS.UI;
public enum Exercise
{
	Squat = 0,
	Run = 1,
	Punch = 2,
	Yoga = 3
}

namespace TSS
{

	/// <summary>
	/// This is a class intended to replace the abuse of "do this" booleans in the main player class.
	/// The main idea here is that when a certain exercise point threshhold is met, it will run a function
	/// </summary>
	public class ExerciseEvent : BaseNetworkable
	{
		/// <summary>
		/// Whether or not we've triggered this event
		/// </summary>
		public bool Triggered { get; protected set; }

		/// <summary>
		/// The action we run when this event has been reached
		/// </summary>
		public Action Action { get; protected set; }

		/// <summary>
		/// The condition that is required to be met to trigger this exercise event
		/// </summary>
		public Func<bool> Condition { get; protected set; }


		/// <summary>
		/// The constructor for our exercise event
		/// </summary>
		/// <param name="con">Condition</param>
		/// <param name="a">Action</param>
		public ExerciseEvent(Func<bool> con, Action a )
		{
			Condition = con;
			Action = a;
		}

		/// <summary>
		/// A function that will determine if the points passed in will trigger our event
		/// </summary>
		/// <param name="points"></param>
		public void Simulate( int points )
		{
			//If we aren't triggered
			if ( !Triggered )
			{
				//And the incoming points is equal to our point thresh
				if(Condition())
				{
					Triggered = true;
					Action();
				}
			}
		}
	}

	public partial class TSSPlayer : Player
	{
		#region Public Members

		#region Visuals
		/// <summary>
		/// The "scale" of terry. Used client side to make him "bump", which sets this value to a specific level. It goes down to 1 via a lerp
		/// but Terry scales with this value, make him appear to bump when he recieves points.
		/// </summary>
		public float ScaleTar;

		/// <summary>
		/// The time since the player has last been ragdolled. 
		/// </summary>
		[Net]
		public TimeSince TimeSinceRagdolled { get; set; }

		/// <summary>
		/// Basically a way of stopping the soda animation when its done
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceEnding { get; set; }

		/// <summary>
		/// The particle system used for sweating
		/// </summary>
		private Particles SweatSystem;

		/// <summary>
		/// The particle system used for the white void and colorful lasers at the end of the game.
		/// </summary>
		private Particles SickoMode;

		/// <summary>
		/// The position of the sickomode particle
		/// </summary>
		private Vector3 SickoModePosition;

		/// <summary>
		/// The target position of the sickomode particle. Used with a lerp.
		/// </summary>
		private Vector3 SickoModePositionTar;

		#endregion

		/// <summary>
		/// A singleton instance referring to the player
		/// TODO: Replace calls to this with client.pawn
		/// </summary>
		[Net]
		public static TSSPlayer Instance { get; set; }

		#endregion

		#region Private Members

		#endregion

		#region Pre-Timeline Variables
		//A set of variables that likely can be replaced with exercise events


		/// <summary>
		/// If we are able to go to 'heaven'
		/// </summary>
		[Net]
		public bool CanGoToHeaven { get; set; }

		/// <summary>
		/// If the ending has been initiated
		/// </summary>
		[Net]
		public bool EndingInitiated { get; set; }

		/// <summary>
		/// I don't know what the difference between this and ending initiated are. I need to investigate
		/// </summary>
		[Net]
		public bool EndingConditionMet { get; set; }


		/// <summary>
		/// Whether we've introduced running or not
		/// </summary>
		[Net]
		public bool IntroRunning { get; set; }

		/// <summary>
		/// Whether we've introduced the punching mini-game or not
		/// </summary>
		[Net]
		public bool IntroPunching { get; set; }
		/// <summary>
		/// Whether we've introduced the yoga game or not
		/// </summary>
		[Net]
		public bool IntroYoga { get; set; }

		/// <summary>
		/// This is an array of booleans used to check if various things are true. Unsure what's being used,
		/// but these need to be replaced with the exercise events where relevant
		/// </summary>
		[Net]
		public bool[] TimeLines { get; set; } = new bool[20];

		/// <summary>
		/// Wether the intro has been played or not
		/// </summary>
		public bool IntroPlayed;
		#endregion

		#region Uncategorized Members


		private bool titleCardActive;
		private UI.CreditPanel titleCard;
		[Net]
		public bool MusicStarted { get; set; }
		#endregion

		#region Methods

		#region Overrides
		public override void CreateHull()
		{
			//Set up collisions for the player
			CollisionGroup = CollisionGroup.Player;
			AddCollisionLayer( CollisionLayer.Player );
			SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -8, -8, 0 ), new Vector3( 8, 8, 72 ) );

			MoveType = MoveType.MOVETYPE_WALK;
			EnableHitboxes = true;
		}

		public override void Respawn()
		{
			//Set the instance
			//TODO: Replace calls to this with call to client.pawn
			Instance = this;

			//Enable various drawing stuff
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			//Set the model and dress it
			SetModel( "models/terry/terry.vmdl" );
			Dress();

			Animator = new TSSPlayerAnimator();
			Camera = new TSSCamera();

			//Set the initial exercise to squat
			ChangeExercise( Exercise.Squat );

			//Set this to 4 seconds so that the camera already starts 'stopped'
			TimeSinceExerciseStopped = 4f;

			//Spawn the soda can
			SodaCan = new ModelEntity();
			SodaCan.SetModel( "models/soda/soda.vmdl" );
			SodaCan.SetParent( this, "Soda" );
			SodaCan.LocalPosition = Vector3.Zero;
			SodaCan.LocalRotation = Rotation.Identity;
			SodaCan.EnableDrawing = false;


			//This is to prevent blinking at the beginning of the game
			TimeSinceRagdolled = 10f;

			base.Respawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			//Initiate the particle sweat system on the client
			SweatSystem = Particles.Create( "particles/sweat/sweat.vpcf", this );
			//Set itsp osition to be very sweaty
			SweatSystem.SetPosition( 1, new Vector3( 1000, 0, 0 ) );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );
			Log.Info( other );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}

		#endregion

		#region Uncategorized
		/// <summary>
		/// Waits a moment then plays the music on the game. This function probably doesn't need to be here.
		/// </summary>
		public async void PlayMusic()
		{
			await GameTask.Delay( 2000 );
			TSSGame.Current.StartMusic();
			TSSGame.Current.PlayIntro();
		}

		/// <summary>
		/// Starts the sound and animation for the transition from the white void to the ending speech
		/// </summary>
		[ClientRpc]
		public void StartEndingClient()
		{
			ClearAnimation();
			SetAnimBool( "Ending", true );
			Sound.FromScreen( "ending" );
			SickoMode?.SetPosition( 3, 0 );
		}

		/// <summary>
		/// This begins the fade out between the white void animation and the transition to the final speech
		/// </summary>
		[ClientRpc]
		public void StartEndingTransition()
		{
			EndingPanel.Instance.Alph = 2f;
			EndingPanel.Instance.FinalBlackout = true;
			TSSGame.Current.PlayRantInstrumental();
		}

		/// <summary>
		/// A method called which starts the animation which will transition from the white void to the ending speech
		/// </summary>
		public void StartEnding()
		{

			//Delete the barbell
			if ( Barbell.IsValid() )
			{
				Barbell.EnableDrawing = false;
			}
			Barbell?.Delete();

			//Reset the timer used to tell when the ending has occurred
			TimeSinceEnding = 0;
			//Start the ending on the client
			StartEndingClient();
			//Set this variable to true
			//TODO: Check and see if this can be replaced by an exercise event
			EndingInitiated = true;

			TSSGame.Current.Silence();
		}

		/// <summary>
		/// This runs a trace which will click on a food item in the world and consume or destroy it.
		/// </summary>
		public void DetectClick()
		{
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				TraceResult clickTrace = Trace.Ray( Input.Cursor, 1000f ).HitLayer( CollisionLayer.All, true ).Run();

				if ( clickTrace.Hit )
				{
					if ( IsServer && clickTrace.Entity is Food food )
					{
						food.Click();
					}
				}
			}
		}
		#endregion

		#region Soda
		/// <summary>
		/// Set the anim bool to drink the soda can
		/// </summary>
		[ClientRpc]
		public void InitiateSoda()
		{
			SetAnimBool( "Drink", true );
		}

		/// <summary>
		/// Resets the soda anim bool to false
		/// TODO: Check to see if we can just do this in the animgraph itself, this is wonky
		/// </summary>
		[ClientRpc]
		public void StopSoda()
		{
			SetAnimBool( "Drink", false );
		}


		/// <summary>
		/// This method can be called from the server to initiate the soda drinking animation.
		/// </summary>
		public void DrinkSoda()
		{
			//Disable drawing the barbell
			if ( Barbell.IsValid() )
			{
				Barbell.EnableDrawing = false;
			}
			//Enable drawing the soda can
			if ( SodaCan.IsValid() )
			{
				SodaCan.EnableDrawing = true;
			}

			//Reset the timer used to track the soda animation
			TimeSinceSoda = 0;

			//Start the soda animation
			InitiateSoda();
		}
		#endregion

		#region Ending
		public void HandleEnding()
		{
			if ( IsServer )
			{
				if ( ExercisePoints >= HeavenThreshold + 65 && !EndingConditionMet )
				{
					StartEnding();

					EndingConditionMet = true;
					ExercisePoints++;
				}
			}

			//Initiate the new pawn after the ending sound has played
			if ( TimeSinceEnding > 13.278f && EndingInitiated )
			{
				StartEndingTransition();
				if ( IsServer )
				{
					var pl = new BuffPawn();
					Client.Pawn = pl;
					pl.Respawn();
					Delete();
				}
			}
		}
		#endregion

		#endregion


		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			//This will play the intro once you press the left click
			if ( IsClient )
			{
				if(!IntroPlayed && Input.Pressed( InputButton.Attack1 ) && TimeSinceRagdolled > 12f)
				{
					IntroPlayed = true;
					PlayMusic();
				}
			}

			//Determine if we need to move to the ending or not
			HandleEnding();

			//Basically if the ending animation is occuring don't simulate the ending
			if ( EndingInitiated )
			{
				return;
			}

			//Handle clicking on food
			DetectClick();

			//Handles visual effects like the sweating and white void particle
			HandleEffectsAndAnims();

			//Get a reference to the camera
			TSSCamera cam = (Camera as TSSCamera);

			//Simulate the current exercise based on the current exercise variable
			switch ( CurrentExercise )
			{
				case Exercise.Squat:
					SimulateSquatting( cam );
					break;
				case Exercise.Run:
					SimulateRunning( cam );
					break;
				case Exercise.Punch:
					SimulatePunching( cam );
					break;
				case Exercise.Yoga:
					SimulateYoga( cam );
					break;
			}

			//Handling timeline is where we handle the progression of the game
			HandleTimeline();

			//Handle the exercise speed variables
			HandleExerciseSpeed();

			//Lerps our scale back down to 1 so we can do effects to make terry 'bump'
			Scale = Scale.LerpTo( 1, Time.Delta * 10f );

			//This block of code is going to cause the player to blink in and and out of existence after ragdolling
			//This indicates that the player has a period of invulnerability
			HandleInvincibilityBlink();

			//Handles the score counter behind the player
			HandleCounter();


			SimulateActiveChild( cl, ActiveChild );
		}

		public override void FrameSimulate( Client cl )
		{
			if ( IsClient && titleCardActive )
			{
				titleCard ??= new UI.CreditPanel( CurrentExercise.ToString().ToUpper(), 3200, 3200 )
				{
					Position = Position + Vector3.Up * -35f + Rotation.Forward * 35f,
					FontSize = 400f,
					Rotation = this.Rotation,
					Opacity = 5f,
					Bop = true,
				};

				titleCard.Opacity -= Time.Delta;

				if ( titleCard.Opacity < 1 )
					titleCard.TextScale = titleCard.TextScale.LerpTo( 0, Time.Delta * 5f );

				if ( titleCard.Opacity < 0 )
				{
					titleCardActive = false;
					titleCard.Delete( true );
					titleCard = null;
				}
			}
		}

		

		/// <summary>
		/// This is basically a rough function that will switch the exercises as you reach a certain number of points
		/// Eventually after ever exercise has been discovered, we should just cycle between them at random
		/// TODO: Move this to a better system
		/// </summary>
		public void HandleTimeline()
		{

			//Switch to the running exercise state
			if ( ExercisePoints >= 200 && !IntroRunning)
			{

				ChangeExercise( Exercise.Run );
				TSSGame.Current.SetTarVolume( 1 );
				TSSGame.Current.SetTarVolume( 7 );
				TimeSinceRun = 0f;
				IntroRunning = true;
				return;
			}
			//Introduce the punching exercise state
			if ( ExercisePoints >= 300 && !IntroPunching)
			{
				ChangeExercise( Exercise.Punch );
				IntroPunching = true;
				return;
			}
			//Introduce the yoga mini game
			if ( ExercisePoints >= 400 && !IntroYoga)
			{
				ChangeExercise( Exercise.Yoga );
				IntroYoga = true;
				return;
			}

			//Basically once our exercise points are above a certain point ceiling, switch randomly to other gamemodes.
			if (ExercisePoints >= PointCeiling)
			{
				PointCeiling = ExercisePoints + Rand.Int(20,50);
				var exercises = new Exercise[] { Exercise.Squat, Exercise.Run, Exercise.Punch, Exercise.Yoga }.Where((e) => e != CurrentExercise).ToArray();
				ChangeExercise( exercises[Rand.Int(0, exercises.Count() - 1)] );
			}

			//Basically, at 100 exercise points introduce some new music layers
			if (ExercisePoints >= 100 && !TimeLines[0])
			{
				TSSGame.Current.SetTarVolume( 3 );
				TSSGame.Current.SetTarVolume( 2 );
				TimeLines[0] = true;
			}

			//Basically, if we have more than 50 exercise points make Terry make an angry/determined facial pose.
			if ( ExercisePoints > 50 )
			{
				SetAnimBool( "Angry", TimeSinceExerciseStopped < 4f );
			}
		}

	}
}
