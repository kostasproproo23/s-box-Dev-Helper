using Sandbox;

public sealed class SimpleNpc : Component
{
	//In this simple Npc, we will make it so it just follows the closest player!
	//For more information on how this npc is setup, just download the files from my github! (link in my discord)
	//IMPORTANT! MAKE SURE THE NPC GAMEOBJECT IS SET TO NETWORKED GAMEOBJECT WITH AN OWNER!

	//Here we set up some variables
	[Property] public NavMeshAgent Agent {get; set;}
	[Property] public SkinnedModelRenderer Body {get; set;}
	private GameObject CurrentTarget;

	//We need to run this on this, so it does not lag a lot.
	protected override void OnFixedUpdate()
	{
		if (IsProxy) return;

		//We only need this code to run on the owner (i would suggest you make the host the owner or do whatever, i dont care)

		//Here we just make a table of all the gameobjects with the player tag
		var AllPlayers = Scene.FindAllWithTag("player");

		//Make sure closest distance is something big
		var closestDistance = 99999999999999999999999f;
		var CurrentPlayer = GameObject;

		//now we will loop through each one
		//and we will try to find the closest player
		foreach (var player in AllPlayers)
		{
			//We might have ragdolls with the player tag, this ensures that actuall players are calculated
			if (player.Components.Get<PlayerController>() == null) continue;

			//Simple code, just get the distance
			var Distance = Vector3.DistanceBetween(player.WorldPosition, GameObject.WorldPosition);

			//If we are closer than the closest player we have found, update the variables to match the player
			if (Distance < closestDistance)
			{
				//We update the new stuff

				CurrentPlayer = player;
				closestDistance = Distance;
			}
		}

		//This means we found no player. (This will normally not be needed, but it is good to be safe)
		if (CurrentPlayer != GameObject)
		{
			//Now we need to start following!
			CurrentTarget = CurrentPlayer;
			Agent.MoveTo(CurrentTarget.WorldPosition);
		}
	}

	//Now this is a bonus, if you have a animgraph set up inside the body of your npc
	//you can update some variables so animations can work
	//Here is something very simple
	protected override void OnUpdate()
	{
		//This happens regardless of owner or not, because we want all players to see the npc animations
		Body.Set("Velocity", Agent.Velocity.Length);

		//What we are doing here is simple, we are just passing the velocity of the Agent to the animgraph, and then it can 
		//determen if Velocity > 0 = play walking animation, if Velocity = 0 = play idle animation

		if (IsProxy) return;

		//This code will make the npc turn smoothly torwards the direction it is looking
		var TargetRot = Rotation.LookAt(Agent.WishVelocity, Vector3.Up);
		WorldRotation = Rotation.Slerp(WorldRotation, TargetRot, Time.Delta * 8.0f);
	}
}
