using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public enum CameraState
{
	Static = 0,
	Follow = 1,
	Rotate = 2,
	Ground,
	Intro
};



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
	public override void Activated()
	{
		base.Activated();
		CamState = CameraState.Intro;
		IntroComplete = false;
		
		
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( input.Pressed( InputButton.Attack1 ) )
		{
			var pawn = Local.Pawn as TSSPlayer;
		}
	}

	public override void Update()
	{
		var pawn = Local.Pawn as TSSPlayer;

		yaw = yaw.LerpTo( yawTar, Time.Delta * 2f );

		switch ( CamState) {
			case CameraState.Follow:
				FollowPlayer();
			break;
			case CameraState.Rotate:
				FollowPlayer();
			break;
			case CameraState.Intro:
				Intro();
			break;
		
			case CameraState.Ground:
				Ground();
			break;
		
			case CameraState.Static:
				StaticPlayer();
			break;
		}

		Progress = Math.Clamp( Progress, 0f, 1f );
		float f = (pawn.TimeSinceSquat - 1f) / 3f;
		f = MathF.Pow(f.Clamp( 0, 1f ),3f);
		TimeSinceState += Time.Delta * (1f - f);
		TimedProgress = TimedProgress.LerpTo( TimeSinceState / 5f, Time.Delta * 8f );
		TimedProgress = TimedProgress.Clamp( 0f, 1f );
	}

	public void Intro()
	{
		
		var pawn = Local.Pawn as TSSPlayer;
		var center = pawn.Position + Vector3.Up * CamHeight;
		if ( Progress < 0.25f )
		{
			CamDistance = 125f - 50f * (Progress / 0.25f);
			CamHeight = 45f;
			center = pawn.Position + Vector3.Up * CamHeight;
			Position = center + pawn.Rotation.Forward * CamDistance;
			Rotation = Rotation.LookAt( (center - Position), Vector3.Up );
			yaw = 30f;
			yawTar = 30f;
		}

		if ( Progress >= 0.25f && Progress < 0.5f)
		{
			float p = (Progress - 0.25f)/ 0.24f;
			p = p.Clamp( 0, 1f );
			CamDistance = 150f;
			CamHeight = 20f;
			center = pawn.Position + Vector3.Up * CamHeight;
			
			yawTar = MathX.LerpTo( 30f, 120f, p );
			Position = center + Rotation.FromYaw(yaw).Forward * CamDistance;
			Rotation = Rotation.LookAt( center - Position, Vector3.Up );

		}

		

		if ( Progress >= 0.5f && Progress < 0.75f )
		{
			float p = (Progress - 0.5f) / 0.25f;
			p = p.Clamp( 0, 1f );
			CamDistance = 50f;
			CamHeight = 32f + 32f  * p;
			
			center = pawn.Position + Vector3.Up * CamHeight;
			Position = center + pawn.Rotation.Forward * CamDistance;
			Rotation = Rotation.LookAt( (center - Position), Vector3.Up );
		}

		if ( Progress >= 0.75f && Progress <= 1f )
		{
			float p = (Progress - 0.75f) / 0.24f;
			p = p.Clamp( 0, 1f );
			CamDistance = 50f + 50f * p;
			CamHeight = 64f - 19f * p;
			
			center = pawn.Position + Vector3.Up * CamHeight;
			Rotation = Rotation.LookAt( (center - Position), Vector3.Up );
			Position = center + pawn.Rotation.Forward * CamDistance;
		}

		if(Progress >= 1f )
		{
			IntroComplete = true;
			CamState = CameraState.Static;
			Progress = 0f;
			TimeSinceState = 0f;
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

		if (TimeSinceState > 5f )
		{
			TimeSinceState = 0f;
			CamState = CameraState.Follow;
			TimedProgress = 0f;
			FollowPlayer();
		}

	}
}
