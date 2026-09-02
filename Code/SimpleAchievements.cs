using Sandbox;

public sealed class SimpleAchievements : Component
{

	//This is super easy, just giving achievements to player!
	//Put this script inside the player prefab

	protected override void OnStart()
	{
		//We dont want other players here, but it does not really matter in this example
		if(IsProxy) return;

		//Give the achievement, simple as that! (Create your achievements inside your game and make sure you put the correct name in there!)
		Sandbox.Services.Achievements.Unlock("playedgame");
	}

	//Now lets make a more complext achievement

	[Property] public int ClickAmountForAchievement {get; set;} = 10;
	private int AmountOfTimesClicked = 0;
	protected override void OnUpdate()
	{
		if (IsProxy) return;

		//for more info on this look at SimpleFirstPersonFlashlight
		if (Input.Pressed("Attack1")) AmountOfTimesClicked += 1;

		//Now we need to check how many times we have pressed click and if we exceed the amount required give the achievement
		if (AmountOfTimesClicked >= ClickAmountForAchievement) Sandbox.Services.Achievements.Unlock("clicker");
	}
}
