using Sandbox;
using System;
using System.Linq;


public enum Exercise
{
	Squat = 0,
	Run = 1,
	Punch = 2
}
public partial class TSSPlayer : Player
{
	[Net]
	public int ExercisePoints { get; set; }
	public float ScaleTar;
	[Net]
	public bool IntroComplete { get; set; }

	public Exercise MyExercise;

	[Net, Predicted]
	public int squat { get; set; }
	[Net, Predicted]
	private int lastSquat { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceSquat { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceUpPressed { get; set; }
public ModelEntity DumbBell;
[Net, Predicted]
public TimeSince TimeSinceDownPressed { get; set; }

public override void Respawn()
	{
		SetModel( "models/terry/terry.vmdl" );
		MyExercise = Exercise.Squat;
		Position = Entity.All.OfType<SquatSpawn>().First().Position;
		Rotation = Entity.All.OfType<SquatSpawn>().First().Rotation;
		
		squat = 0;
		lastSquat = -1;
		TimeSinceSquat = 4f;
		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new TSSPlayerAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		Camera = new TSSCamera();
		

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		DumbBell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
		DumbBell.SetParent( this, "head" );
		DumbBell.Rotation = this.Rotation * Rotation.From( 0, 0, 90 );
		


		base.Respawn();
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );
		TSSCamera cam = (Camera as TSSCamera);

		if(cam == null )
		{
			return;
		}

		
		switch ( MyExercise )
		{
			case Exercise.Squat:
				Squatting(cam);
			break;
		}

		if ( DumbBell != null )
		{
			DumbBell.LocalPosition = Vector3.Zero + DumbBell.Transform.Rotation.Right * 15.5f;

		}

		Scale = Scale.LerpTo( 1, Time.Delta * 10f );

		
	}

	public async void SetScale(float f )
	{
		await GameTask.DelaySeconds( 0.1f );
		Scale = f;
	}

	public void Squatting(TSSCamera cam)
	{
		 
		SetAnimInt( "squat", squat );

	if ( TimeSinceSquat < 3f && squat != -1 && !IntroComplete )
	{
		float f = (TimeSinceSquat - 1f) / 3f;
		f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
		cam.Progress += Time.Delta * 0.025f * (1 - f);


		if ( cam.Progress >= 1f )
		{
			IntroComplete = true;
		}
	}

	if ( TimeSinceSquat < 3f && squat != -1 && IntroComplete )
	{
		cam.Progress += Time.Delta * 0.35f;
	}

	if( Input.Pressed( InputButton.Forward )  && Input.Pressed( InputButton.Back ) )
	{
		return;
	}

		if ( Input.Pressed( InputButton.Forward )  && (squat == 0 || squat == -1) && TimeSinceDownPressed > 0.1f)
		{
			if ( squat == 0 )
			{
				
				ExercisePoints++;
				SetScale( 1.2f );
				TimeSinceSquat = 0;
				Log.Info( $"SQUAT: {ExercisePoints}" );
			}
			squat = 1;
			TimeSinceUpPressed = 0;

		}

		if ( Input.Pressed( InputButton.Back ) && (squat == 1 || squat == -1) && TimeSinceUpPressed > 0.1f)
		{
			squat = 0;
		TimeSinceDownPressed = 0;
		
		}

		

	}

	public void SetState(Exercise state )
	{
		MyExercise = state;
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
	}
}

