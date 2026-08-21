using Sandbox;

public sealed class MultiplayerTips : Component
{
	//With the following in mind, multiplayer compatability will be easy

	/* When creating multiplayer you need to have one thing in mind,
	that every script in the scene that is shared among the players
	is run by all the players. Lets see an example */

	protected override void OnStart()
	{
		/*The code here will run on all the players, but we want only our own player to run 
		this code. In that case we can use:*/
		
		if (IsProxy)
		{
			//this code is run by the clients that dont own this gameobject
			Log.Info("Hello, other player here");
		}
		else
		{
			//this code is run by the client that owns the gameobject
			Log.Info("I am the proud owner of this player");
		}
		
		//As long as an object has an owner we can use IsProxy to see if our client is an owner of that object and determen what we want to do with that information
	}

	//Now to share information along the other clients we can use these variables

	[Property] [Sync] public bool isCrouching {get; set;} = false;

	//Every property that has [Sync] can be changed only by the owner of the object and the variable is networked across all the clients.
	//These are super usefull for transfering information to other players, like inventory and other stuff

	//Lets make a simple example of the player pressing a button

	[Property] [Sync] public bool PressingForwardKey {get; set;} = false;

	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			//We dont want other players here

			//We can use Input.Down to see if a spesific action is actively being holded down. You can find default action names or create your own in the settings --> input
			if (Input.Down("Forward")) PressingForwardKey = true; else PressingForwardKey = false;
		}
		else
		{
			//This code is run by the other players
			if (PressingForwardKey == true) Log.Info(PressingForwardKey); //if the player owner is pressign the forward key, then we print true on the rest of the clients
		}
	}
}
