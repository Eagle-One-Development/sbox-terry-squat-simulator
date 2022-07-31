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
		public void Simulate( )
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

		[Net]
		public bool SkipIntro { get; set; } = false;

		public TimeSince TimeSinceIntro;

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

		#endregion

		#region Private Members

		#region Timeline
		private List<ExerciseEvent> Timeline { get; set; }
		#endregion
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
		/// Wether the intro has been played or not
		/// </summary>
		[Net]
		public bool IntroPlayed { get; set; }
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

			InitializeComponenets();
			//Enable various drawing stuff
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			//Set the model and dress it
			if ( !SkipIntro )
			{
				SetModel( "models/terry/terry.vmdl" );
				Dress();
			}
			else
			{
				SetModel( "models/terry_buff/terry_buff.vmdl" );
			}

			Animator = new TSSPlayerAnimator();
			CameraMode = new TSSCamera();
			(CameraMode as TSSCamera).SkipIntro = SkipIntro;

			//Set the initial exercise to squat
			ChangeExercise( Exercise.Squat );
			CurrentExerciseComponent = Components.GetAll<SquatComponenet>().First();

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

			if ( SkipIntro )
			{
				ExercisePoints = HeavenThreshold + 201;
			}

			if ( !SkipIntro )
			{
				InitializeTimeline();
			}

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
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}

		public override void Spawn()
		{
			
			base.Spawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			//This will play the intro once you press the left click
			if ( !IntroPlayed && Input.Pressed( InputButton.PrimaryAttack ) && TimeSinceRagdolled > 12f && !SkipIntro )
			{
				TimeSinceIntro = 0f;
				IntroPlayed = true;
				PlayMusic();
				
			}

			if(TimeSinceIntro < 23.16f )
			{
				return;
			}

			var squat = Components.GetAll<SquatComponenet>().First();


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
			TSSCamera cam = (CameraMode as TSSCamera);

			//Simulate our current exercise
			Components.GetAll<ExerciseComponent>().Where( x => x.ExerciseType == CurrentExercise ).First().Simulate(cl);

			//Handling timeline is where we handle the progression of the game
			HandleTimeline();

			//Handle the exercise speed variables
			HandleExerciseSpeed();

			//Lerps our scale back down to 1 so we can do effects to make terry 'bump'
			Scale = Scale.LerpTo( 1f, Time.Delta * 10f );

			//This block of code is going to cause the player to blink in and and out of existence after ragdolling
			//This indicates that the player has a period of invulnerability
			HandleInvincibilityBlink();

			//Handles the score counter behind the player
			HandleCounter();

			SimulateActiveChild( cl, ActiveChild );
		}
		#endregion

		#region Uncategorized
		/// <summary>
		/// Waits a moment then plays the music on the game. This function probably doesn't need to be here.
		/// </summary>
		public async void PlayMusic()
		{
			await GameTask.Delay( 1000 );

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
			SetAnimParameter( "Ending", true );
			Sound.FromScreen( "ending" );
			SickoMode?.SetPosition( 3, 0 );
		}

		/// <summary>
		/// This begins the fade out between the white void animation and the transition to the final speech
		/// </summary>
		[ClientRpc]
		public void StartEndingTransition()
		{
			if ( !SkipIntro )
			{
				EndingPanel.Instance.Alph = 2f;
				EndingPanel.Instance.FinalBlackout = true;
				TSSGame.Current.PlayRantInstrumental();
			}
		}

		/// <summary>
		/// A method called which starts the animation which will transition from the white void to the ending speech
		/// </summary>
		public void StartEnding()
		{
			var squat = Components.GetAll<SquatComponenet>().First();
			//Delete the barbell
			if ( squat.Barbell.IsValid() )
			{
				squat.Barbell.EnableDrawing = false;
			}
			squat.Barbell?.Delete();

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
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				TraceResult clickTrace = Trace.Ray( Input.Cursor, 1000f ).HitLayer( CollisionLayer.All, true ).WithoutTags("wall").Run();

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
			SetAnimParameter( "Drink", true );
		}

		/// <summary>
		/// Resets the soda anim bool to false
		/// TODO: Check to see if we can just do this in the animgraph itself, this is wonky
		/// </summary>
		[ClientRpc]
		public void StopSoda()
		{
			SetAnimParameter( "Drink", false );
		}


		/// <summary>
		/// This method can be called from the server to initiate the soda drinking animation.
		/// </summary>
		public void DrinkSoda()
		{
			var squat = Components.GetAll<SquatComponenet>().First();
			
			//Disable drawing the barbell
			if ( squat.Barbell.IsValid() )
			{
				squat.Barbell.EnableDrawing = false;
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

		/// <summary>
		/// This function checks to see if we've finished the soda animation
		/// TODO: Move this to some sort of animation and event system.
		/// </summary>
		private void EvaluateSodaAnim()
		{
			var squat = Components.GetAll<SquatComponenet>().First();

			if ( TimeSinceSoda > 1.7f )
			{
				if ( squat.Barbell.IsValid() )
				{
					//squat.Barbell.EnableDrawing = true;
				}
				if ( SodaCan.IsValid() )
				{
					SodaCan.EnableDrawing = false;
				}

				StopSoda();
			}
		}
		#endregion

		#region Ending
		public void HandleEnding()
		{
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

		#region Timeline
		public void InitializeTimeline()
		{
			Timeline = new List<ExerciseEvent>();

			//Introduce the running exercise
			Timeline.Add( new ExerciseEvent( () => (ExercisePoints >= 200), () => {
				ChangeExercise( Exercise.Run );
				TSSGame.Current.QueueTrack( "queue1" );
			} ));

			//Add som new music
			Timeline.Add( new ExerciseEvent( () => (ExercisePoints >= 100), () => {
				TSSGame.Current.QueueTrack( "queue0" );
			} ) );

			//Introduce the punch exercise
			Timeline.Add( new ExerciseEvent( () => (ExercisePoints >= 300), () => {
				ChangeExercise( Exercise.Punch );
				TSSGame.Current.QueueTrack( "queue2" );

			} ) );

			//Introduce the yoga exercise
			Timeline.Add( new ExerciseEvent( () => (ExercisePoints >= 400), () => {
				ChangeExercise( Exercise.Yoga );
				TSSGame.Current.QueueTrack( "queue3" );
			} ) );

			//Initiate the ending
			Timeline.Add( new ExerciseEvent( () => (ExercisePoints >= HeavenThreshold + 201), () => {
				StartEnding();
			} ) );

		}
		
		/// <summary>
		/// Handles the timeline of events plus a few other re-occurring pieces of code that are reliant on the exercise points.
		/// Simulates the timeline
		/// </summary>
		public void HandleTimeline()
		{
			if ( Timeline != null )
			{
				for ( int i = 0; i < Timeline.Count; i++ )
				{
					Timeline[i].Simulate();
				}
			}

			//Basically once our exercise points are above a certain point ceiling, switch randomly to other gamemodes.
			if ( ExercisePoints >= PointCeiling )
			{
				PointCeiling = ExercisePoints + Rand.Int( 20, 50 );
				var exercises = new Exercise[] { Exercise.Squat, Exercise.Run, Exercise.Punch, Exercise.Yoga }.Where( ( e ) => e != CurrentExercise ).ToArray();
				ChangeExercise( exercises[Rand.Int( 0, exercises.Count() - 1 )] );
			}

			//Basically, if we have more than 50 exercise points make Terry make an angry/determined facial pose.
			if ( ExercisePoints > 50 )
			{
				SetAnimParameter( "Angry", TimeSinceExerciseStopped < 4f );
			}
		}
		#endregion

		#endregion

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

		

	}
}
