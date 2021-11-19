using Sandbox;
using System;
using System.Linq;
using MinimalExample;


public partial class TSSPlayer : Player
{
	/// <summary>
	/// Changes the current exercise and moves the player to a given position and rotation
	/// </summary>
	/// <param name="state">The state we're moving to</param>
	/// <param name="pos">The position the player will be at</param>
	/// <param name="rot">The rotation the player will be at</param>
	public void ChangeExercise( Exercise state, Vector3 pos, Rotation rot )
	{
		Position = pos;
		Rotation = rot;
		MyExercise = state;
	}

	/// <summary>
	/// The running exercise
	/// </summary>
	/// <param name="cam"></param>
	public void Running( TSSCamera cam )
	{
		SetAnimInt( "squat", -1 );
		SetAnimFloat( "move_x", MathX.LerpTo( 0, 350f, (curSpeed * 4f).Clamp( 0, 1f ) ) );

		if ( cam == null )
		{

			return;
		}


		if ( TimeSinceRun < 3f && squat != -1 )
		{
			cam.Progress += Time.Delta * 0.35f;
		}

		if ( Input.Pressed( InputButton.Right ) && Input.Pressed( InputButton.Left ) )
		{
			return;
		}

		if ( Input.Pressed( InputButton.Right ) && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f )
		{
			if ( squat == 0 )
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
			squat = 1;
			TimeSinceUpPressed = 0;
		}

		if ( Input.Pressed( InputButton.Left ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f )
		{
			squat = 0;
			TimeSinceDownPressed = 0;
			if ( cam.Down != null )
				cam.Down.TextScale += 0.3f;
		}



	}

	/// <summary>
	/// The punching exercise
	/// </summary>
	/// <param name="cam"></param>
	public void Punching( TSSCamera cam )
	{

		if ( cam == null )
		{
			return;
		}

		SetAnimInt( "punch", squat );
		SetAnimInt( "squat", -1 );

		if ( TimeSincePunch > TimeToNextPunch )
		{
			TimeSincePunch = 0;

			if ( IsServer )
			{
				var pt = new PunchQT();
				pt.Player = this;
				pt.TargetTime = 1f;
				pt.MyTime = 1f;
				pt.Type = Rand.Int( 0, 3 );
			}


		}






	}

	[ServerCmd("yoga_pose")]
	public static void SetPose(int i)
	{
		Log.Info( "YOGA!" );
		TSSPlayer.Instance.SetAnimInt( "YogaPoses", i );
		TSSPlayer.Instance.SetClientPose( i );
	}

	[ClientRpc]
	public void SetClientPose(int i )
	{
		SetAnimInt( "YogaPoses", i );
	}
	

	public void Yogaing(TSSCamera cam )
	{
		if ( cam == null )
		{
			return;
		}
		
		
		SetAnimBool( "b_grounded", false );

		if ( TimeSinceYoga > 3.05f )
		{
			TimeSinceYoga = 0;

			if ( IsClient )
			{
				var pt = new YogaQT();
				pt.Player = this;

			}


		}
	}

	/// <summary>
	/// The squatting exercise
	/// </summary>
	/// <param name="cam"></param>
	public void Squatting( TSSCamera cam )
	{

		if ( cam == null )
		{
			return;
		}

		SetAnimInt( "squat", squat );

		if ( TimeSinceExerciseStopped < 3f && squat != -1 && !IntroComplete )
		{
			float f = (TimeSinceExerciseStopped - 1f) / 3f;
			f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
			cam.Progress += Time.Delta * 0.025f * (1 - f);


			if ( cam.Progress >= 1f )
			{
				IntroComplete = true;
			}
		}

		if ( TimeSinceExerciseStopped < 3f && squat != -1 && IntroComplete )
		{
			cam.Progress += Time.Delta * 0.35f;
		}

		if ( Input.Pressed( InputButton.Forward ) && Input.Pressed( InputButton.Back ) )
		{
			return;
		}

		if ( Input.Pressed( InputButton.Forward ) && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f )
		{
			if ( squat == 0 )
			{

				ExercisePoints++;
				tCurSpeed += 0.1f;
				CreatePoint( 1 );
				SetScale( 1.2f );
				CounterBump( 0.5f );
				TimeSinceExerciseStopped = 0;
				Log.Info( $"SQUAT: {ExercisePoints}" );
				if ( cam.Up != null )
					cam.Up.TextScale += 0.3f;
			}
			squat = 1;
			TimeSinceUpPressed = 0;

		}

		if ( Input.Pressed( InputButton.Back ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f )
		{
			squat = 0;
			TimeSinceDownPressed = 0;
			if ( cam.Down != null )
				cam.Down.TextScale += 0.3f;
		}



	}
}
