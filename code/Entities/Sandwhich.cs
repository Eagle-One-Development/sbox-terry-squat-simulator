using System;
using System.Collections.Generic;
using Sandbox;
using TSS;
using TSS.UI;

public partial class Sandwhich : Food
{

	public override Vector2 GetPanelSize()
	{
		return new Vector2( 300, 150 );
	}

	public override void Spawn()
	{
		base.Spawn();
		MoveToPlayer = false;
	}

	public override string GetFoodModel()
	{
		return "models/food/burger.vmdl";
	}

	[ClientRpc]
	public override void CreatePanel()
	{
		FoodPan = new FoodPanel( GetPanelSize(), Color.White, "SANDWHICH", "+15" );

	}

	public override Vector3 GetInitialPosition()
	{
		return Player.Position + Vector3.Up * Rand.Float( 32f, 64f ) + new Angles( 0, Rand.Float( 0, 180 ), 0 ).Direction * 100f;
	}

	public override int GetClickPoints()
	{
		return 10;
	}

	protected override void OnConsume()
	{
		Player.GivePoints( 10, true );
	}

	protected override void OnClick()
	{
		Player.GivePointsAtPosition( 10, Position, true );
	}
}
