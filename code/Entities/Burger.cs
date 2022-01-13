using System;
using System.Collections.Generic;
using Sandbox;
using TSS;
using TSS.UI;

public partial class Burger : Food
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

	public override Vector3 GetInitialPosition()
	{
		return Player.EyePos + new Angles( 0, Rand.Float( 0, 360 ), 0 ).Direction * 256f;
	}

	public override int GetClickPoints()
	{
		return 5;
	}

	protected override void OnConsume()
	{
		Player.GivePoints( 5, true );
	}

	protected override void OnClick()
	{
		Player.GivePointsAtPosition( 5, Position, true );
	}
}
