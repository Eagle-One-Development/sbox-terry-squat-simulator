using System;
using Sandbox;
using TSS.UI;

public enum CameraState
{
	Static = 0,
	Follow = 1,
	Rotate = 2,
	Ground,
	Intro
};


namespace TSS
{

	//public enum Scene
	//{
	//	JOSH_PRESENT,
	//	ASSOCATION
	//}

	public partial class TSSCamera : Camera
	{
		public float CamHeight;
		public float CamDistance;
		public Vector3 CamOffset;
		public CameraState CamState;
		public float Progress;
		private float TimedProgress;
		float yaw;
		float yawTar;
		public bool IntroComplete;
		public float TimeSinceState;

		//Credit Panels (Definitely a better way to do this but this is just for the intro)
		private CreditPanel JoshWilson;
		private CreditPanel Presents;
		private CreditPanel Assoc;
		private CreditPanel Dawdle;
		private CreditPanel Mungus;
		private CreditPanel Jacob;
		private CreditPanel Kabubu;
		private CreditPanel TSS;
		public CreditPanel Up;
		public CreditPanel Down;
		public CreditPanel TreadmillTutorial;
		public TimeSince TimeSinceStart;

		//public Scene CurrentScene;

		public bool Active;

		Material _myPostProcessingMaterial;
		[Event( "render.postprocess" )]
		public void DoPostProcess()
		{
			if ( _myPostProcessingMaterial != null )
			{
				Render.CopyFrameBuffer( false );
				Render.Material = _myPostProcessingMaterial;
				Render.DrawScreenQuad();
			}
		}

		public CreditPanel SCounter;

		public override void Activated()
		{
			base.Activated();
			if ( !Active )
			{
				CamState = CameraState.Intro;
				IntroComplete = false;
				JoshWilson = null;
				TimeSinceStart = 0;
				Log.Info( "WAIT WHAT" );
				Active = true;
			}
		}

		public override void Update()
		{
			var pawn = Local.Pawn as TSSPlayer;
			yaw = yaw.LerpTo( yawTar, Time.Delta * 2f );

			if ( pawn.CurrentExercise != Exercise.Squat )
			{
				Progress = 1.0f;
			}

			switch ( CamState )
			{
				case CameraState.Follow:
					FollowPlayer();
					break;
				case CameraState.Rotate:
					FollowPlayer();
					break;
				case CameraState.Intro:
					AdvanceIntro();
					break;

				case CameraState.Ground:
					Ground();
					break;

				case CameraState.Static:
					StaticPlayer();
					break;
			}

			if ( pawn.GetAnimBool( "Drink" ) && pawn.TimeSinceSoda > 0.05f )
			{
				var transform = pawn.GetBoneTransform( "Camera" );
				Position = transform.Position;
				Rotation = transform.Rotation * Rotation.From( 90, 0, -90 );
			}

			Progress = Math.Clamp( Progress, 0f, 1f );
			float f = (pawn.TimeSinceExerciseStopped - 1f) / 3f;
			f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
			TimeSinceState += Time.Delta * (1f - f);
			TimedProgress = TimedProgress.LerpTo( TimeSinceState / 5f, Time.Delta * 8f );
			TimedProgress = TimedProgress.Clamp( 0f, 1f );

			if ( TSS != null && IntroComplete )
			{
				TSS.Opacity -= Time.Delta * pawn.curSpeed * 0.5f;

				if ( TSS.Opacity <= 0f )
				{
					TSS?.Delete();
					TSS = null;
				}

				Down?.Delete();
				Down = null;
				Up?.Delete();
				Down = null;
			}

			//For now make the score face in the forward direction of the player
			if ( Local.Pawn is TSSPlayer t )
			{
				if ( SCounter != null )
				{
					SCounter.Rotation = t.Rotation;
					SCounter.Position = pawn.Position + Vector3.Up * 30f + pawn.Rotation.Forward * -50f;
				}
			}
		}

		public void AdvanceIntro()
		{

			var pawn = Local.Pawn as TSSPlayer;

			if ( Progress > 0.01f )
			{
				TSSGame.Current.SetTarVolume( 6 );
			}

			if ( Progress < 0.25f )
			{

				JoshWilson ??= new CreditPanel( "Josh Wilson", 3200, 3200 );
				JoshWilson.Position = pawn.Position + Vector3.Up * 10f + pawn.Rotation.Forward * -20f;
				JoshWilson.Rotation = Rotation.From( 0, 90, 0 );
				JoshWilson.Opacity = ((Progress - 0.01f) / 0.05f).Clamp( 0, 1f );
				JoshWilson.Bop = true;

				Presents ??= new CreditPanel( "Presents", 3200, 3200 );
				Presents.Position = pawn.Position + Vector3.Up * -50f + pawn.Rotation.Forward * 9f;
				Presents.Rotation = Rotation.From( 0, 90, 0 );
				Presents.Opacity = ((Progress - 0.1f) / 0.05f).Clamp( 0, 1f );
				Presents.Bop = true;

				float f = ((TimeSinceStart - 2f) / 5f).Clamp( 0, 1f );

				Up ??= new CreditPanel( Input.GetKeyWithBinding( "+iv_forward" ), 200, 200 );
				Up.Position = pawn.Position + Vector3.Up * 55f + pawn.Rotation.Right * -22f + pawn.Rotation.Forward * 12f;
				Up.Rotation = Rotation.From( 0, 90, 0 );
				Up.Opacity = (1f - ((Progress - 0.1f) / 0.05f).Clamp( 0, 1f )) * f;
				Up.TextScale = Up.TextScale.LerpTo( 1, Time.Delta * 10f );

				Down ??= new CreditPanel( Input.GetKeyWithBinding( "+iv_back" ), 200, 200 );
				Down.Position = pawn.Position + Vector3.Up * 25f + pawn.Rotation.Right * -22f + pawn.Rotation.Forward * 12f;
				Down.Rotation = Rotation.From( 0, 90, 0 );
				Down.Opacity = (1f - ((Progress - 0.1f) / 0.05f).Clamp( 0, 1f )) * f;
				Down.TextScale = Down.TextScale.LerpTo( 1, Time.Delta * 10f );

				var center = pawn.Position + Vector3.Up * CamHeight;

				CamDistance = 125f - 50f * (Progress / 0.25f);
				CamHeight = 45f;
				Position = center + pawn.Rotation.Forward * CamDistance;
				Rotation = Rotation.LookAt( (center - Position), Vector3.Up );
				yaw = 30f;
				yawTar = 30f;
			}

			if (Progress >= 0.25f)
			{
				JoshWilson?.Delete();
				JoshWilson = null;
				Presents?.Delete();
				Presents = null;
			}

			if ( Progress >= 0.25f && Progress < 0.5f )
			{

				Assoc ??= new CreditPanel( "and", 3200, 1600 );
				Assoc.Rotation = Rotation.From( 0, 90, 0 );
				Assoc.Position = pawn.Position + pawn.Rotation.Forward * 12f;
				Assoc.Opacity = 1f;
				Assoc.Bop = true;
				Assoc.FontSize = 100f;

				Dawdle ??= new CreditPanel( "Dawdle", 3200, 400 );
				Dawdle.Rotation = Rotation.From( 0, 55, 0 );
				Dawdle.Position = pawn.Position + pawn.Rotation.Right * -50f + Vector3.Up * -3f;
				Dawdle.Opacity = 1f;
				Dawdle.FontSize = 200f;

				Mungus ??= new CreditPanel( "Mungus", 3200, 400 );
				Mungus.Rotation = Rotation.From( 0, -55 + 180, 0 );
				Mungus.Position = pawn.Position + pawn.Rotation.Right * 50f + Vector3.Up * -3f;
				Mungus.Opacity = 1f;
				Mungus.FontSize = 200f;

				Kabubu ??= new CreditPanel( "Kabubu", 3200, 400 );
				Kabubu.Rotation = Rotation.From( 0, 55, 0 );
				Kabubu.Position = pawn.Position + pawn.Rotation.Right * -50f + Vector3.Up * 64f;
				Kabubu.Opacity = 1f;
				Kabubu.FontSize = 200f;

				Jacob ??= new CreditPanel( "Jac0xb", 3200, 400 );
				Jacob.Rotation = Rotation.From( 0, -55 + 180, 0 );
				Jacob.Position = pawn.Position + pawn.Rotation.Right * 50f + Vector3.Up * 64f;
				Jacob.Opacity = 1f;
				Jacob.FontSize = 200f;

				float p = (Progress - 0.25f) / 0.24f;
				p = p.Clamp( 0, 1f );
				CamDistance = 150f;
				CamHeight = 20f;
				var center = pawn.Position + Vector3.Up * CamHeight;

				yawTar = MathX.LerpTo( 30f, 120f, p );
				Position = center + Rotation.FromYaw( yaw ).Forward * CamDistance;
				Rotation = Rotation.LookAt( center - Position, Vector3.Up );

				TSSGame.Current.SetTarVolume( 0 );

			}

			if ( Progress >= 0.5f )
			{
				Assoc?.Delete();
				Assoc = null;
				Dawdle?.Delete();
				Dawdle = null;
				Mungus?.Delete();
				Mungus = null;
				Kabubu?.Delete();
				Kabubu = null;
				Jacob?.Delete();
				Jacob = null;
				Up?.Delete();
				Up = null;
				Down?.Delete();
				Up = null;
			}

			if ( Progress >= 0.5f && Progress < 0.75f )
			{
				float p = (Progress - 0.5f) / 0.25f;
				p = p.Clamp( 0, 1f );
				CamDistance = 50f;
				CamHeight = 32f + 32f * p;

				var center = pawn.Position + Vector3.Up * CamHeight;
				Position = center + pawn.Rotation.Forward * CamDistance;
				Rotation = Rotation.LookAt( (center - Position), Vector3.Up );

				TSSGame.Current.SetTarVolume( 5 );
			}

			if ( Progress >= 0.75f && Progress <= 1f )
			{
				float p = (Progress - 0.75f) / 0.24f;
				p = p.Clamp( 0, 1f );
				CamDistance = 50f + 50f * p;
				CamHeight = 64f - 19f * p;

				TSS ??= new CreditPanel( "Terry\nSquat\nSimulator", 3200, 3200 );
				TSS.Position = pawn.Position + Vector3.Up * -26f + pawn.Rotation.Forward * 20f;
				TSS.Rotation = Rotation.From( 0, 90, 0 );
				TSS.Opacity = p * 2f;
				TSS.Bop = true;

				TSSGame.Current.SetTarVolume( 4 );


				var center = pawn.Position + Vector3.Up * CamHeight;
				Rotation = Rotation.LookAt( (center - Position), Vector3.Up );
				Position = center + pawn.Rotation.Forward * CamDistance;
			}

			if ( Progress >= 1.0f )
			{
				IntroComplete = true;
				CamState = CameraState.Static;
				Progress = 0f;
				TimeSinceState = 0f;

				TSS.Opacity = 1f;
				TSS.Delete();
				TSS = null;

				SCounter ??= new CreditPanel( "Squats: 0", 3200, 3200 );
				SCounter.Position = pawn.Position + Vector3.Up * 30f + pawn.Rotation.Forward * -50f;
				SCounter.Rotation = Rotation.From( 0, 90, 0 );
				SCounter.Opacity = 0.0f;
				SCounter.TextScale = 1.0f;
			}
		}

		public void FollowPlayer()
		{
			CamDistance = 125f;
			CamHeight = 45f;
			float p = TimedProgress;
			p = p.Clamp( 0, 1f );
			var pawn = Local.Pawn as AnimEntity;
			var center = pawn.Position + Vector3.Up * CamHeight;


			Position = center + pawn.Rotation.Forward * CamDistance + pawn.Rotation.Right * MathX.LerpTo( -100f, 100f, p );
			Rotation = Rotation.LookAt( (center - Position), Vector3.Up );

			if ( TimeSinceState > 5f )
			{
				TimeSinceState = 0f;
				CamState = CameraState.Ground;
				TimedProgress = 0f;
				Ground();
			}

		}


		public void Ground()
		{
			CamDistance = 125f;
			CamHeight = 15f;
			float p = TimedProgress;
			p = p.Clamp( 0, 1f );
			var pawn = Local.Pawn as AnimEntity;
			var center = pawn.Position + Vector3.Up * CamHeight;

			Position = center + pawn.Rotation.Forward * CamDistance + pawn.Rotation.Right * MathX.LerpTo( -50f, 50f, p );
			Rotation = Rotation.LookAt( pawn.Rotation.Forward * -1f, Vector3.Up );

			if ( TimeSinceState > 5f )
			{
				TimeSinceState = 0f;
				CamState = CameraState.Static;
				TimedProgress = 0f;
				StaticPlayer();
			}

		}

		public void StaticPlayer()
		{
			CamDistance = 100f;
			CamHeight = 45f;
			var pawn = Local.Pawn as AnimEntity;
			var center = pawn.Position + Vector3.Up * CamHeight;


			Position = center + pawn.Rotation.Forward * CamDistance;
			Rotation = Rotation.LookAt( (center - Position), Vector3.Up );

			if ( TimeSinceState > 5f )
			{
				TimeSinceState = 0f;
				CamState = CameraState.Follow;
				TimedProgress = 0f;
				FollowPlayer();
			}

		}
	}
}
