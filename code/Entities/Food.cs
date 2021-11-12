using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;


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
	public bool ConsumeOnCollide { get; set; }

	SceneObject FoodModel;
	public PickupTrigger PickupTrigger { get; protected set; }

	public override void Spawn()
	{
		base.Spawn();
		TimeSinceSpawned = 0;
		Player = Entity.All.OfType<TSSPlayer>().First();
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
		FoodPan = new FoodPanel( new Vector2( 300, 150 ), Color.White, "FOOD" );
		
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );


		Log.Info( other );

		if(other is TSSPlayer player && TimeSinceSpawned > 0.5f)
		{
			if ( ConsumeOnCollide )
			{
				Consume();
			}
			RemoveModel();
			FoodModel = null;
			Log.Info( "CONSUMED" );
			if ( IsServer ){ Delete(); }
		}
	}



	public override void ClientSpawn()
	{
		base.ClientSpawn();
		FoodModel = new SceneObject( Model.Load( "models/sbox_props/burger_box/burger_box.vmdl"), this.Transform);
		//FoodModel = new ModelEntity( "models/sbox_props/burger_box/burger_box.vmdl" );
		//FoodModel.Predictable = false;
	}

	public virtual void Consume()
	{
		Player.GivePointsAtPosition( 5, Position, true );
	}

	[ClientRpc]
	public void RemoveModel()
	{
		FoodModel.Delete();
		FoodPan.Delete( true );
	}

	public void RemoveFood()
	{
		Consume();
		RemoveModel();
		Delete();
	}

	[Event.Tick]
	public virtual void Sim()
	{
		if ( IsServer )
		{
			if ( MoveToPlayer ) {
				Vector3 dir = ((Player.Position + Vector3.Up * 48f)- Position).Normal;
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
		FoodModel.Transform = new Transform(FoodModel.Position,FoodModel.Rotation, Out( TimeSinceSpawned / 0.5f ));
		FoodPan.Position = this.Position;
	}

	public float Out( float k )
	{
		k = k.Clamp( 0, 1f );
		if ( k == 0 ) return 0;
		if ( k == 1 ) return 1;
		return MathF.Pow( 2f, -10f * k ) * MathF.Sin( (k - 0.1f) * (2f * MathF.PI) / 0.4f ) + 1f;
	}

}
