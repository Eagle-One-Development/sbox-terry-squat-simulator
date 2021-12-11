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
		/// <summary>
		/// The position in the world where the exercise is taking place. Camera centers around this point so terry can move indepdent of the camera if needed.
		/// </summary>
		[Net]
		public Vector3 ExercisePosition { get; set; }

		protected Sound treadmillSound { get; set; }


		public int YogaCount = 0;

		/// <summary>
		/// Changes the current exercise and moves the player to a given position and rotation
		/// </summary>
		/// <param name="exercise">The exercise we're moving to</param>
		public void ChangeExercise( Exercise exercise )
		{
			//Define the entity we're going to move the player to;
			Entity ent = null;

			// Cleanup on exercise change.
			if ( CurrentExercise != exercise )
			{
				switch ( CurrentExercise )
				{
					case Exercise.Squat:
						Barbell?.Delete();
						Barbell = null;
						break;
					case Exercise.Yoga:
						CurrentYogaPosition = 0;
						break;
					case Exercise.Run:
						StopClientRunning();
						break;
				}
			}

			switch ( exercise )
			{
				case Exercise.Run:
					ent = All.OfType<TSSSpawn>().ToList().Find(x => x.SpawnType == SpawnType.Run);
					//Run position is used for the mini-game for terry being pushed off the treadmill
					ExercisePosition = ent.Transform.Position;
					StartClientRunning();
					break;
				case Exercise.Squat:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Squat );
					StartSquatting();
					break;
				case Exercise.Punch:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Punch );
					break;
				case Exercise.Yoga:
					ent = All.OfType<TSSSpawn>().ToList().Find( x => x.SpawnType == SpawnType.Yoga );
					break;
			}

			ExercisePosition = ent.Transform.Position;
			Position = ent.Transform.Position;
			Rotation = ent.Transform.Rotation;
			CurrentExercise = exercise;

			if ( ExercisePoints > 100 )
				SetTitleCardActive();
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
		/// Resets all the proper animgraph parameters when switching between exercise states
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
		/// Initialize the squatting exercise state
		/// </summary>
		public void StartSquatting()
		{
			Barbell?.Delete();
			Barbell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
			Barbell.SetParent( this, "head" );
			Barbell.Rotation = Rotation * Rotation.From( 0, 0, 90 );
			Squat = 0;
			lastSquat = -1;
		}

		[ClientRpc]
		public void StartClientRunning()
		{
			treadmillSound.Stop();
			treadmillSound = PlaySound( "treadmill" );
		}

		[ClientRpc]
		public void StopClientRunning()
		{
			treadmillSound.Stop();
		}

		/// <summary>
		/// Initializes the punch exercise state
		/// </summary>
		public void StartPunching()
		{
			TimeToNextPunch = 1.1f;
		}


		/// <summary>
		/// The offset by which we are falling off of the tread mill
		/// </summary>
		[Net]
		public float RunPositionOffset { get; set; }

		/// <summary>
		/// The offset by which we are falling off of the tread mill
		/// </summary>
		[Net]
		public TimeSince TimeSinceRagdolled{ get; set; }

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

			ent.SetModel( this.GetModel() );

			//.SetBodyGroup(1, BodyGroup);
			//ent.RenderColor = PlayerColor;

			ent.CopyBonesFrom( this );
			ent.TakeDecalsFrom( this );
			ent.SetRagdollVelocityFrom( this );
			ent.DeleteAsync( 5.0f );

			ent.PhysicsGroup.AddVelocity( force );

			_ = new ModelEntity( "models/clothes/fitness/shorts_fitness.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/shirt_fitness.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/shoes_sneakers.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_wrists.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/sweatband_head.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/hair_head.vmdl", ent );
			_ = new ModelEntity( "models/clothes/fitness/hair_body.vmdl", ent );

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
		/// The running exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateRunning( TSSCamera cam )
		{
			SetAnimFloat( "move_x", MathX.LerpTo( 0, 350f, (curSpeed * 4f).Clamp( 0, 1f ) ) );

			//We're going to set our position to the RunPosition + some offset
			Position = ExercisePosition + Rotation.Forward * -RunPositionOffset;

			//Basically we're going to use our curSpeed, a value which determines how fast we are running, to determine if we're moving forward or backward on the treadmill
			float treadSpeed = (curSpeed / 0.28f).Clamp(0f,1f);
			//Basically check and see if we're exercising fast enough, if not, uptick the run position offset to make us 
			if(treadSpeed >= 0.6f )
			{
				RunPositionOffset -= Time.Delta * 25f;
			}
			else
			{
				RunPositionOffset += Time.Delta * (1f - treadSpeed) * 50f;
			}
			RunPositionOffset = RunPositionOffset.Clamp( -10f, 45f );
			//DebugOverlay.ScreenText( new Vector2( 100, 100 ), $"{(1f - treadSpeed)}" );

			if(RunPositionOffset >= 45f )
			{
				BecomeRagdollOnClient( (Rotation.Forward * -1f + Vector3.Up).Normal * 250f, 0 );
				RunPositionOffset = 0f;
				curSpeed = 1f;
				TimeSinceExerciseStopped = 0f;
				TimeSinceRagdolled = 0f;
			}
			

			if ( cam == null )
			{
				return;
			}

			if (IsClient)
			{
				if ( TimeSinceRagdolled > 0f && TimeSinceRagdolled < 1f )
				{
					treadmillSound.SetVolume( 0 );
				}
				else
				{
					treadmillSound.SetVolume(MathF.Max(0, MathF.Min(2.25f, 2.25f - 2*TimeSinceExerciseStopped)));
				}

				//treadmillSound.SetVolume( 3f );
			}

			if ( TimeSinceRun < 3f && Squat != -1 )
			{
				cam.Progress += Time.Delta * 0.35f;
			}

			if ( Input.Pressed( InputButton.Right ) && Input.Pressed( InputButton.Left ) )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Right ) && (Squat == 0 || Squat == -1) && TimeSinceDownPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				if ( Squat == 0 )
				{
					tCurSpeed += 0.1f;
					CreatePoint( 1 );
					SetScale( 1.2f );
					CounterBump( 0.5f );
					TimeSinceExerciseStopped = 0;
					ExercisePoints++;
					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				Squat = 1;
				TimeSinceUpPressed = 0;
			}

			if ( Input.Pressed( InputButton.Left ) && (Squat == 1 || Squat == -1) && TimeSinceUpPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				Squat = 0;
				TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}
		}

		/// <summary>
		/// The punching exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulatePunching( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			SetAnimInt( "punch", Squat );

			if ( TimeSincePunch > TimeToNextPunch )
			{
				TimeSincePunch = 0;

			}
		}

		[Event( "OtherBeat" )]
		public void PunchBeat()
		{
			if ( CurrentExercise == Exercise.Punch )
			{
				ConsoleSystem.Run( "create_punch" );
			}
		}

		/// <summary>
		/// Command for creating the punch QT event
		/// </summary>
		[ServerCmd( "create_punch" )]
		public static void CreatePunchQT()
		{
			var pt = new PunchQT();
			pt.Player = Instance;
			pt.TargetTime = 1f;
			pt.MyTime = (60f / 140f) * 2f;
			pt.Type = Rand.Int( 0, 3 );

		}


		/// <summary>
		/// Sets the pose on both the server and client, updating the yoga pose terry is using during the yoga exercise
		/// </summary>
		/// <param name="i"></param>
		[ServerCmd( "yoga_pose" )]
		public static void SetPose( int i )
		{
			if ( Instance.CurrentExercise != Exercise.Yoga )
			{
				return;
			}

			var pawn = TSSGame.Pawn;


			Instance.CurrentYogaPosition = i;
			Instance.GivePoints( 5 );
			var sound = pawn.PlaySound( $"yoga_0{1 + (pawn.YogaCount % 3)}" );
			pawn.YogaCount++;
			sound.SetVolume( 2.5f );

		}

		/// <summary>
		/// Simulate the yoga exercise state
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateYoga( TSSCamera cam )
		{
			SetAnimInt( "YogaPoses", CurrentYogaPosition );
			SetAnimBool( "b_grounded", CurrentYogaPosition == 0 );

			if ( cam == null )
			{
				return;
			}

			if ( TimeSinceYoga > 3.05f )
			{
				TimeSinceYoga = 0;

				if ( IsClient )
				{
					// Prevent duplicate yoga qt panels appearing when alt-tabbed.
					if (All.OfType<YogaQT>().Count() == 0)
					{
						var pt = new YogaQT();
						pt.Player = this;
					}
				}
			}
		}

		/// <summary>
		/// The Squatting exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateSquatting( TSSCamera cam )
		{
			if ( cam == null )
			{
				return;
			}

			if ( Barbell != null )
			{
				Barbell.LocalPosition = Vector3.Zero + Barbell.Transform.Rotation.Right * 15.5f;
			}

			SetAnimInt( "squat", Squat );

			if ( TimeSinceExerciseStopped < 3f && Squat != -1 && !cam.IntroComplete )
			{
				float f = (TimeSinceExerciseStopped - 1f) / 3f;
				f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
				cam.Progress += Time.Delta * 0.025f * (1 - f);
			}

			if ( TimeSinceExerciseStopped < 3f && Squat != -1 && cam.IntroComplete )
			{
				cam.Progress += Time.Delta * 0.35f;
			}

			if ( Input.Pressed( InputButton.Forward ) && Input.Pressed( InputButton.Back ) )
			{
				return;
			}

			if ( Input.Pressed( InputButton.Forward ) && (Squat == 0 || Squat == -1) && TimeSinceDownPressed > TSSGame.QUARTER_NOTE_DURATION )
			{

				if ( Squat == 0 )
				{

					ExercisePoints++;
					tCurSpeed += 0.1f;
					CreatePoint( 1 );
					SetScale( 1.2f );
					CounterBump( 0.5f );
					TimeSinceExerciseStopped = 0;


					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				Squat = 1;
				TimeSinceUpPressed = 0;
				var sound = PlaySound( $"squat_0{Rand.Int( 1, 7 )}" );
				sound.SetPitch( (100f / (50 + Math.Max( 1f, ExercisePoints ))).Clamp( 0.5f, 1.0f ));

			}

			if ( Input.Pressed( InputButton.Back ) && (Squat == 1 || Squat == -1) && TimeSinceUpPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				Squat = 0;
				TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;

				var sound = PlaySound( $"squat_0{Rand.Int(1, 7)}" );
				sound.SetPitch( (100f / (50 + Math.Max( 1f, ExercisePoints ))).Clamp( 0.5f, 1.0f ) );
			}
		}
	}
}
