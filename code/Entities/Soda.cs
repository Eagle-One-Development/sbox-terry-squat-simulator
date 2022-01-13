using System;
using System.Collections.Generic;
using Sandbox;
using TSS;
using TSS.UI;

public partial class Soda : Food
{

	public override Vector2 GetPanelSize()
	{
		return new Vector2( 300, 300 );
	}

	public override void Spawn()
	{
		base.Spawn();
		MoveToPlayer = false;
	}

	public override string GetFoodModel()
	{
		return "models/soda/soda.vmdl";
	}

	[ClientRpc]
	public override void CreatePanel()
	{
		FoodPan = new FoodPanel( GetPanelSize(), Color.White, "SODA", "");

	}

	public override Vector3 GetInitialPosition()
	{
		return Player.Position + Vector3.Up * Rand.Float( 32f, 64f ) + new Angles( 0, Rand.Float( 0, 180 ), 0 ).Direction * 20f;
	}

	public override int GetClickPoints()
	{
		return 5;
	}

	
	protected override void OnConsume()
	{
		
	}

	protected override void OnClick()
	{
		Player.DrinkSoda();
	}
}
