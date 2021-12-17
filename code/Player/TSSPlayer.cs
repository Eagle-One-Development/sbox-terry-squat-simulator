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
	public class ExerciseEvent
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
		/// The value our given action will trigger at.
		/// I.e: At 200 exercise points trigger this action
		/// TODO: Abstract this system later to take a bool func so that people can determine their own conditions for an event
		/// </summary>
		public int PointThresh { get; protected set; }

		/// <summary>
		/// The constructor for our exercise event
		/// </summary>
		/// <param name="thresh"></param>
		/// <param name="a"></param>
		public ExerciseEvent(int thresh, Action a )
		{
			PointThresh = thresh;
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
				if(points == PointThresh )
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
		private TimeSince aTime;
		public float curSpeed;
		private float tCurSpeed;
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

		#endregion


		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			TSSCamera cam = (Camera as TSSCamera);

			if ( TimeSinceSoda > 1.7f )
			{
				if ( Barbell.IsValid() )
				{
					Barbell.EnableDrawing = true;
				}
				if ( SodaCan.IsValid() )
				{
					SodaCan.EnableDrawing = false;
				}

				StopSoda();
			}


			//This will play the intro once you press the left click
			if ( IsClient )
			{
				if(!IntroPlayed && Input.Pressed( InputButton.Attack1 ) && TimeSinceRagdolled > 12f)
				{
					IntroPlayed = true;
					PlayMusic();
				}
			}

			//Initiate the new pawn after the ending sound has played
			if(TimeSinceEnding > 13.278f && EndingInitiated)
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

			if ( EndingInitiated )
			{
				return;
			}

			DetectClick();
			ClearAnimation();
			ParticleEffects();

			if ( IsClient )
			{
				if(SickoMode != null )
				{
					var localCam = (Camera as TSSCamera);
					var pos = ExercisePosition + Vector3.Up * 45f;
					var dir = (pos - localCam.Position).Normal;
					SickoModePositionTar = pos + dir * 200f;
					SickoModePosition = Vector3.Lerp( SickoModePosition, SickoModePositionTar, Time.Delta * 8f );
					SickoMode.SetPosition( 0, SickoModePositionTar );
					float sin = (MathF.Sin( Time.Now * 2f ) + 1) / 2f;
					var beams = new Vector3( sin * 0.75f, 0, 0 );
					SickoMode.SetPosition( 3, beams );
					//DebugOverlay.Sphere( pos + dir * 200f, 1f, Color.Yellow,false,1);
					
				}

			}

			if ( IsServer )
			{
				if(ExercisePoints >= HeavenThreshold + 65 && !EndingConditionMet )
				{
					StartEnding();
					
					EndingConditionMet = true;
					ExercisePoints++;
				}
			}

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

			AdvanceExerciseState();

			// Basically curSpeed increases based on how fast the player is squatting etc

			float f = (TimeSinceExerciseStopped - 1f) / 3f;
			f = MathF.Pow( f.Clamp( 0, 1f ), 3f );

			tCurSpeed = tCurSpeed.Clamp( 0, 1 );
			curSpeed = curSpeed.Clamp( 0, 1 );
			var mult = MathX.LerpInverse( TimeSinceExerciseStopped, 0, 1 );
			if ( CurrentExercise == Exercise.Run )
			{
				mult = MathX.LerpInverse( TimeSinceRun, 0, 1 );
			}

			tCurSpeed = tCurSpeed.LerpTo( 0f, Time.Delta * 0.25f * (mult * 4f) );
			curSpeed = curSpeed.LerpTo( tCurSpeed, Time.Delta * 2f );

			Scale = Scale.LerpTo( 1, Time.Delta * 10f );

			//This block of code is going to cause the player to blink in and and out of existence after ragdolling
			//This indicates that the player has a period of invulnerability
			float alph = 0f;
			if (TimeSinceRagdolled < 3f )
			{
				EnableDrawing = false;
				TimeSinceExerciseStopped = 0f;

				if ( TimeSinceRagdolled > 1f )
				{
					EnableDrawing = true;
					float sin = MathF.Sin( TimeSinceRagdolled * 10f );
					if ( sin > 0 )
					{
						alph = 1f;
					}
					else
					{
						alph = 0.5f;
					}
				}
				else
				{
					alph = 0.5f;
				}
			}
			else
			{
				alph = 1f;
			}
			Color co = Color.White;
			co.a = alph;
			RenderColor = co; 

			foreach(ModelEntity m in Children.OfType<ModelEntity>().ToList() )
			{
				
				if ( m.GetModelName() != SodaCan.GetModelName())
				{
					m.EnableDrawing = this.EnableDrawing;
					m.RenderColor = co;
				}
			}



			if ( cam.SCounter != null )
			{
				var c = cam.SCounter;
				c.l?.SetText( ExercisePoints.ToString() );
				c.Opacity += Time.Delta * curSpeed * 0.4f;
				

				c.TextScale = cam.SCounter.TextScale.LerpTo( 1.5f * MathX.Clamp( curSpeed + 0.8f, 0, 1 ), Time.Delta * 2f );
				float anim = MathF.Sin( aTime );
				c.Rotation = Rotation.From( 0, 90, anim * curSpeed * 1f * (ExercisePoints / 100f) );
			}


			SimulateActiveChild( cl, ActiveChild );
		}

		/// <summary>
		/// A method run on simulate which helps us manage our particle effects.
		/// </summary>
		public void ParticleEffects()
		{
			//For client simulated particles
			if ( IsClient )
			{
				//Here we have a sweat value
				float sweatValue = 150f;

				//First lets only start sweating once we've got exercise points and if we're exercising fast enough
				float val = (TimeSinceExerciseStopped / 3f).Clamp( 0, 1 );
				//Basically give us more sweat as we approach 500 exercise points
				float val2 = ((ExercisePoints - 150) / 350f).Clamp(0,1f);

				//Then we set our sweat value
				SweatSystem.SetPosition( 1, new Vector3( sweatValue * MathF.Pow(val2,0.32f) * (1f - val), 0, 0 ) );
			}
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

		/// <summary>
		/// This is basically a rough function that will switch the exercises as you reach a certain number of points
		/// Eventually after ever exercise has been discovered, we should just cycle between them at random
		/// TODO: Move this to a better system
		/// </summary>
		public void AdvanceExerciseState()
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

		/// <summary>
		/// Sets the scale of the counter for extra juice when getting points
		/// </summary>
		/// <param name="f">The scale to set the counter to</param>
		public async void CounterBump( float f )
		{
			await GameTask.DelaySeconds( 0.1f );
			if ( (Camera as TSSCamera).SCounter != null )
			{
				var c = (Camera as TSSCamera).SCounter;
				c.TextScale += f * curSpeed;
			}
		}

		/// <summary>
		/// Sets the scale of the player for extra juice when getting points
		/// </summary>
		/// <param name="f">The scale to set the player</param>
		public async void SetScale( float f )
		{
			await GameTask.DelaySeconds( 0.1f );
			Scale = f;
		}

		/// <summary>
		/// Makes the player punch and moves the 'squat' variable so it alternated between left and right punches
		/// </summary>
		public void Punch()
		{
			ExercisePoints += 3;
			tCurSpeed += 0.1f;
			CreatePoint( 3 );
			Scale = 1.2f;
			CounterBump( 0.5f );
			TimeSinceExerciseStopped = 0;

			//var sound = PlaySound( $"squat_0{Rand.Int( 1, 7 )}" );
			//sound.SetPitch( (300f / (50 + Math.Max( 1f, ExercisePoints ))).Clamp( 0.5f, 1.0f ) );

			if ( Squat == 0 )
			{
				Squat = 1;
				return;
			}

			if ( Squat == 1 )
			{
				Squat = 0;
				return;
			}
		}


		
	}
}
