using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TSS
{
	public enum SquatState { 
		UP,
		DOWN
	}

	public partial class TSSPlayer : Player
	{

		#region Exercise Members
		/// <summary>
		/// The position in the world where the exercise is taking place. Camera centers around this point so terry can move indepdent of the camera if needed.
		/// </summary>
		[Net]
		public Vector3 ExercisePosition { get; set; }

		/// <summary>
		/// Effectively the "score" used for the game, you get points for doing exercises and this is the variable that tracks that.
		/// </summary>
		[Net]
		public int ExercisePoints { get; set; }

		/// <summary>
		/// Our 'current' exercise. Helps decide which mini game to simulate
		/// </summary>
		[Net]
		public Exercise CurrentExercise { get; set; }

		/// <summary>
		/// The time since we last stopped exercising. Used for handling stuff like the slow down of the camera and other effects when you stop or fail the mini-game
		/// </summary>
		[Net]
		public TimeSince TimeSinceExerciseStopped { get; set; }

		/// <summary>
		/// A variable representing how long it's been since we've pressed the up key
		/// TODO: this system could be replaced with a "TimeSinceAnyKeyPressed", there's no reason to have two of these
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceUpPressed { get; set; }

		/// <summary>
		/// A variable representing how long it's been since we've pressed the down key
		/// TODO: this system could be replaced with a "TimeSinceAnyKeyPressed", there's no reason to have two of these
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceDownPressed { get; set; }

		/// <summary>
		/// Once all the exercises have been introduced, the point ceiling becomes relevant. It determines when we move to the next exercise, picking at random.
		/// </summary>
		[Net]
		public int PointCeiling { get; set; } = 500;

		[Net]
		public ExerciseComponent CurrentExerciseComponent { get; set; }

		/// <summary>
		/// A variable representing how fast or often we are exercising. This decays over timeand controls things like the camera movement
		/// </summary>
		public float CurrentExerciseSpeed;
		/// <summary>
		/// This is a target value which current sped move towards.
		/// </summary>
		public float TargetExerciseSpeed;


		public void InitializeComponenets()
		{
			Components.Create<SquatComponenet>();
			Components.Create<RunComponent>();
			Components.Create<PunchComponent>();
			Components.Create<YogaComponent>();

			foreach (var e in Components.GetAll<ExerciseComponent>() )
			{
				e.Initialize();
			}
		}


		#region Soda
		/// <summary>
		/// Reference to the soda can that we drink with the soda power up.
		/// </summary>
		[Net]
		public ModelEntity SodaCan { get; set; }

		/// <summary>
		/// Basically a way of stopping the soda animation when its done
		/// </summary>
		[Net, Predicted]
		public TimeSince TimeSinceSoda { get; set; }


		#endregion
		#endregion

		public int HeavenThreshold => 500;

		#region Methods

		#region Visual
		//Creates a ragdoll
		[ClientRpc]
		void BecomeRagdollOnClient( Vector3 force, int forceBone )
		{

			ModelEntity ent = new();
			ent.Position = Position;
			ent.Rotation = Rotation;
			ent.MoveType = MoveType.Physics;
			ent.UsePhysicsCollision = true;
			ent.SetInteractsAs( CollisionLayer.Debris );
			ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			ent.Model = this.Model;

			if ( !SkipIntro )
			{
				_ = new ModelEntity( "models/clothes/fitness/shorts_fitness.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/shirt_fitness.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/shoes_sneakers.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/sweatband_wrists.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/sweatband_head.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/hair_head.vmdl", ent );
				_ = new ModelEntity( "models/clothes/fitness/hair_body.vmdl", ent );
				
			}

			ent.CopyBonesFrom( this );
			ent.TakeDecalsFrom( this );
			ent.SetRagdollVelocityFrom( this );
			ent.DeleteAsync( 5.0f );

			ent.PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = ent.GetBonePhysicsBody( forceBone );
				if ( body != null )
				{
					body.ApplyForce( force * 1000 );
				}
				else
				{
					ent.PhysicsGroup.AddVelocity( force );
				}
			}

			Corpse = ent;
		}

		/// <summary>
		/// Once the player has been ragdolled, they blink back into existence for a couple of seconds.
		/// </summary>
		private void HandleInvincibilityBlink()
		{
			float alph = 0f;
			if ( TimeSinceRagdolled < 3f )
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

			foreach ( ModelEntity m in Children.OfType<ModelEntity>().ToList() )
			{

				if ( m.GetModelName() != SodaCan.GetModelName() )
				{
					m.EnableDrawing = this.EnableDrawing;
					m.RenderColor = co;
				}
			}
		}

		/// <summary>
		/// Create the particle that acts as the heaven void towards the end of the game. Creates the particle and sets its positions
		/// </summary>
		[ClientRpc]
		public void CreateNearEndParticle()
		{
			//Get the camera
			var localCam = (Camera as TSSCamera);
			//Get a position roughlt up the middle of the player
			var pos = ExercisePosition + Vector3.Up * 45f;
			//Get a vector representing the direction from the pos to the local cameras position
			var dir = (pos - localCam.Position).Normal;

			//Set the position variables of the particle and create it
			SickoModePositionTar = pos + dir * 200f;
			SickoModePosition = SickoModePositionTar;
			SickoMode = Particles.Create( "particles/sicko_mode/sicko_mode.vpcf", pos + dir * 200f );
		}

		/// <summary>
		/// A method that handles the positioning of the white void particle at the end of the game
		/// </summary>
		public void HandleNearEndParticle()
		{
			if ( IsClient )
			{
				if ( SickoMode != null )
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
				}

			}
		}

		/// <summary>
		/// Client command which activates the title card for each exercise. This way people know what to do. 
		/// </summary>
		[ClientRpc]
		private async void SetTitleCardActive()
		{
			await Task.DelaySeconds( 0.1f );
			titleCardActive = true;
		}

		/// <summary>
		/// Resets all the animgraph parameters on the player
		/// </summary>
		public void ClearAnimation()
		{
			SetAnimInt( "squat", -1 );
			SetAnimInt( "punch", -1 );
			SetAnimInt( "YogaPoses", 0 );
			SetAnimBool( "b_grounded", CurrentExercise != Exercise.Yoga );
			SetAnimFloat( "move_x", 0 );
		}

		/// <summary>
		/// Dress terry in the 90s outfit
		/// </summary>
		void Dress()
		{
			_ = new ModelEntity( "models/clothes/fitness/shorts_fitness.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/shirt_fitness.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/shoes_sneakers.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_wrists.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_head.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/hair_head.vmdl", this );
			_ = new ModelEntity( "models/clothes/fitness/hair_body.vmdl", this );
			if ( ExercisePoints > HeavenThreshold )
			{
				_ = new ModelEntity( "models/clothes/fitness/glasses_hdme.vmdl", this );
			}
		}

		/// <summary>
		/// A method run on simulate which helps us manage our particle effects.
		/// </summary>
		public void HandleEffectsAndAnims()
		{
			//For client simulated particles
			if ( IsClient )
			{
				//Here we have a sweat value
				float sweatValue = 150f;

				//First lets only start sweating once we've got exercise points and if we're exercising fast enough
				float val = (TimeSinceExerciseStopped / 3f).Clamp( 0, 1 );
				//Basically give us more sweat as we approach 500 exercise points
				float val2 = ((ExercisePoints - 150) / 350f).Clamp( 0, 1f );


				//Adding this here so we don't sweat when we're ragdolles
				float mult = 1f;

				if(TimeSinceRagdolled < 3f  ){
					mult = 0f;
				}

				//Then we set our sweat value
				SweatSystem.SetPosition( 1, new Vector3( mult * sweatValue * MathF.Pow( val2, 0.32f ) * (1f - val), 0, 0 ) );
			}

			//If the end white void particle exists, update its position and rotation
			HandleNearEndParticle();

			//Basically stop the soda animation once it's reached it's end.
			EvaluateSodaAnim();

			//Clear the animation parameters every frame to avoid conflicts
			ClearAnimation();
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
				c.TextScale += f * CurrentExerciseSpeed;
			}
		}

		/// <summary>
		/// Handled various aspects of the counter like its opacity
		/// </summary>
		public void HandleCounter()
		{
			TSSCamera cam = (Camera as TSSCamera);
			if ( cam.SCounter != null )
			{
				var c = cam.SCounter;
				c.l?.SetText( ExercisePoints.ToString() );
				c.Opacity += Time.Delta * CurrentExerciseSpeed * 0.4f;


				c.TextScale = cam.SCounter.TextScale.LerpTo( 1.5f * MathX.Clamp( CurrentExerciseSpeed + 0.8f, 0, 1 ), Time.Delta * 2f );
				float anim = MathF.Sin( Time.Now );
				c.Rotation = Rotation.From( 0, 90, anim * CurrentExerciseSpeed * 1f * (ExercisePoints / 100f) );
			}
		}

		/// <summary>
		/// Sets the scale of the player for extra juice when getting points
		/// </summary>
		/// <param name="f">The scale to set the player</param>
		public async void SetScale( float f )
		{
			await GameTask.DelaySeconds( 0.05f );
			Scale = f;
		}

		#endregion

		/// <summary>
		/// Does the handling of the 'exercise speed' variables which drive various animations and events.
		/// </summary>
		public void HandleExerciseSpeed()
		{
			//Clamp our exercise speeds
			TargetExerciseSpeed = TargetExerciseSpeed.Clamp( 0, 1 );
			CurrentExerciseSpeed = CurrentExerciseSpeed.Clamp( 0, 1 );
			var mult = MathX.LerpInverse( TimeSinceExerciseStopped, 0, 1 );
			

			TargetExerciseSpeed = TargetExerciseSpeed.LerpTo( 0f, Time.Delta * 0.25f * (mult * 4f) );
			CurrentExerciseSpeed = CurrentExerciseSpeed.LerpTo( TargetExerciseSpeed, Time.Delta * 2f );
		}

		/// <summary>
		/// Changes the current exercise and moves the player to a given position and rotation
		/// </summary>
		/// <param name="exercise">The exercise we're moving to</param>
		public void ChangeExercise( Exercise exercise )
		{
			//Define an entity who's transform represents the position and rotation of the given exercise
			Entity ent = null;

			CurrentExerciseComponent = Components.GetAll<ExerciseComponent>().Where( x => x.ExerciseType == exercise ).First();

			//Perform various pieces of code to clean up the previous exercise
			//TODO: Probably can be its own function
			if ( CurrentExercise != exercise )
			{
				switch ( CurrentExercise )
				{
					case Exercise.Squat:
						break;
					case Exercise.Yoga:
						;
						break;
					case Exercise.Run:
						break;
				}
			
			}

			foreach(var e in Components.GetAll<ExerciseComponent>() )
			{
				e.Cleanup();
			}

			//Look at each exercise and set ent to the appropriate TSS spawn with the requested spawn type
			//Also used to initalize each exercise
			//TODO: Move exercises to their own class
			switch ( exercise )
			{
				case Exercise.Run:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Run );
					if ( ent != null )
					{
						ExercisePosition = ent.Transform.Position;
					}
					break;
				case Exercise.Squat:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Squat );
					var squat = Components.GetAll<SquatComponenet>().First();
					squat.StartSquatting();
					break;
				case Exercise.Punch:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Punch );
					break;
				case Exercise.Yoga:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Yoga );
					break;
			}

			//Basically if our exercise points are greater than the threshold, we move the player to the heaven void. This is done so we can switch exercises in this void
			//and not have them jump back to the gym
			//TODO: Do this better, or at least replace can go to heaven with an exercise event
			if ( ExercisePoints > HeavenThreshold && CanGoToHeaven && !SkipIntro)
			{
				ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Heaven );
				TSSGame.Current.QueueTrack( "queue4" );
			}

			if ( SkipIntro )
			{
				ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.NatureExercise );
			}

			//If the ent isn't null, then set our exercise position and transform to that entity.
			if ( ent != null )
			{
				ExercisePosition = ent.Transform.Position;
				Position = ent.Transform.Position;
				Rotation = ent.Transform.Rotation;
			}
			
			//Set the current exercise to this
			CurrentExercise = exercise;

			//Basically after the first exercise is introduced, set the title card to be active. This flashes a bright sign in front of the player which indicates which exercise they're doing.
			if ( ExercisePoints > 100 )
			{
				SetTitleCardActive();
			}
		}

		/// <summary>
		/// Moves the player to a random exercise
		/// </summary>
		[Event("rand_exercise")]
		public void RandomExercise()
		{
			if(TimeSinceIntro < 23.16f )
			{
				return;
			}

			PointCeiling = ExercisePoints + Rand.Int( 20, 50 );
			var exercises = new Exercise[] { Exercise.Squat, Exercise.Run, Exercise.Punch, Exercise.Yoga }.Where( ( e ) => e != CurrentExercise ).ToArray();
			ChangeExercise( exercises[Rand.Int( 0, exercises.Count() - 1 )] );
		}
		#endregion

		#region Animation
		
		#endregion


		
		

		



	}
}
