using System;
using Sandbox;
using TSS;
using TSS.UI;

public partial class Food : ModelEntity {

	[Net]
	public float Speed { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceSpawned { get; set; }

	[Net]
	public float RotationSpeed { get; set; } = 100f;

	[Net]
	public bool MoveToPlayer { get; set; }

	[Net]
	public TSSPlayer Player { get; set; }

	[Net]
	public bool ConsumeOnCollide { get; set; } = true;

	SceneObject FoodModel;
	public PickupTrigger PickupTrigger { get; protected set; }

	public override void Spawn()
	{

		base.Spawn();

		TimeSinceSpawned = 0;
		Player = TSSGame.Pawn;
		Position = GetInitialPosition();
		Transmit = TransmitType.Always;
		SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero + Vector3.Up * 2.5f, 5f );
		SetInteractsAs( CollisionLayer.Player);
		CollisionGroup = CollisionGroup.Player;
		EnableTouch = true;

		MoveToPlayer = true;

		PickupTrigger = new();
		PickupTrigger.Parent = this;
		PickupTrigger.ResetInterpolation();
		PickupTrigger.Position = Position;
		PickupTrigger.EnableAllCollisions = true;
		PickupTrigger.SetupPhysicsFromSphere( PhysicsMotionType.Keyframed, Vector3.Zero + Vector3.Up * 2.5f, 5f );
		CreatePanel();
	}

	public FoodPanel FoodPan;

	[ClientRpc]
	public virtual void CreatePanel()
	{
		FoodPan = new FoodPanel( GetPanelSize(), Color.White, "FOOD", GetClickPoints() );
		
	}

	public virtual Vector2 GetPanelSize()
	{
		return new Vector2( 300, 150 );
	}

	public virtual string GetFoodModel()
	{
		return "models/sbox_props/burger_box/burger_box.vmdl";
	}

	public virtual int GetClickPoints()
	{
		return 5;
	}

	public virtual Vector3 GetInitialPosition()
	{
		return Player.Position +
			Player.Rotation.Forward * 256 +
			Player.Rotation.Down * Rand.Int( -64, 64 ) +
			Player.Rotation.Left * Rand.Int( -64, 64 );
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if(other is TSSPlayer && TimeSinceSpawned > 0.5f)
		{
			if ( ConsumeOnCollide )
			{
				OnConsume();
			}
			RemoveModel();
			FoodModel = null;
			if ( IsServer ){ Delete(); }
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		FoodModel = new SceneObject( Model.Load( GetFoodModel()), Transform);
	}

	[ClientRpc]
	public void RemoveModel()
	{
		FoodModel.Delete();
		FoodPan.Delete( true );
	}

	public void Click()
	{
		OnClick();
		RemoveModel();
		Delete();
	}

	protected virtual void OnClick()
	{

	}

	protected virtual void OnConsume()
	{

	}

	[Event.Tick]
	public virtual void Simulate()
	{
		if ( IsServer )
		{
			if ( MoveToPlayer ) {
				Vector3 dir = (Player.Position + Vector3.Up * 48f - Position).Normal;
				Position += dir * 0.5f;
			}
		}
	}

	[Event.Frame]
	public virtual void Frame()
	{
		if ( !FoodModel.IsValid() )
		{
			return;
		}
		FoodModel.Position = Vector3.Lerp( FoodModel.Position, Transform.Position, Time.Delta * 20f );
		FoodModel.Rotation = FoodModel.Rotation.RotateAroundAxis( Vector3.Up, Time.Delta * RotationSpeed );
		FoodModel.Transform = new Transform(FoodModel.Position, FoodModel.Rotation, Out( TimeSinceSpawned / 0.5f ));
		FoodPan.Position = Position;
	}

	public float Out( float k )
	{
		k = k.Clamp( 0, 1f );
		if ( k == 0 ) return 0;
		if ( k == 1 ) return 1;
		return MathF.Pow( 2f, -10f * k ) * MathF.Sin( (k - 0.1f) * (2f * MathF.PI) / 0.4f ) + 1f;
	}

}
