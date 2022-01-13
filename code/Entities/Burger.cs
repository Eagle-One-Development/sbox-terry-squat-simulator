using System;
using System.Collections.Generic;
using Sandbox;
using TSS;
using TSS.UI;

[Library("food_burger")]
[Hammer.Skip]
public partial class Burger : Food
{
	public override string Description => "tries to clog some arteries.";
	public override Vector2 GetPanelSize()
	{
		return new Vector2( 300, 150 );
	}

	public override void Spawn()
	{
		base.Spawn();
		MoveToPlayer = true;
	}

	public override string GetFoodModel()
	{
		return "models/food/burger.vmdl";
	}

	[ClientRpc]
	public override void CreatePanel()
	{
		FoodPan = new FoodPanel( GetPanelSize(), Color.White, "BURGER", "-20" );

	}

	public override Vector3 GetInitialPosition()
	{
		return Player.Position + Vector3.Up * Rand.Float( 32f, 64f ) + new Angles( 0, Rand.Float( 0, 180 ), 0 ).Direction * 150f;
	}

	public override int GetClickPoints()
	{
		return 5;
	}

	protected override void OnConsume()
	{
		Player.GivePoints( -20, true );
	}

	protected override void OnClick()
	{
		Player.GivePointsAtPosition( 5, Position, true );
	}
}
