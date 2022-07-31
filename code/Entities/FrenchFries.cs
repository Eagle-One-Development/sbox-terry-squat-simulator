using System;
using System.Collections.Generic;using Sandbox;
using TSS;
using TSS.UI;



[Library( "food_fries" )]
public partial class FrenchFries : Food {

	public override string Description => "gives unhealthy fries.";
	public override Vector2 GetPanelSize()
	{
		return new Vector2( 100, 150 );
	}

	public override string GetFoodModel()
	{
		return "models/food/fries.vmdl";
	}

	public override Vector3 GetInitialPosition()
	{
		return Player.EyePosition + new Angles( 0, Rand.Float( 0, 360 ), 0 ).Direction * 256f;
	}

	public override int GetClickPoints()
	{
		return 5;
	}

	[ClientRpc]
	public override void CreatePanel()
	{
		FoodPan = new FoodPanel( GetPanelSize(), Color.White, "FRIES", "-10" );

	}

	protected override void OnConsume()
	{
		Player.GivePoints( -10, true );
	}

	protected override void OnClick()
	{
		Player.GivePointsAtPosition( 5, Position, true );
	}
}
