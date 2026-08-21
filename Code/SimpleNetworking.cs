using System;
using System.Threading.Tasks;
using Sandbox.MovieMaker;
using Sandbox.Network;

namespace Sandbox;

public sealed class SimpleNetworking : Component, Component.INetworkListener
{
	[Property] [Category("Multiplayer")] public GameObject PlayerPrefab { get; set; } // Here you will put the player prefab. If you dont have one, take the default player controller right click and turn it into a prefab
	[Property] [Category("Multiplayer")] public GameObject SpawnPoint { get; set; } //Here is the spawn position the player will spawn in
	[Property] [Category("Multiplayer")] public bool StartServer { get; set; } = true; // Just keep this true so it automatically creates a server when a player joins

	public void OnActive( Connection channel )
	{	
		//This code runs when a new connection appears in the server and is only run on the host
		if ( !PlayerPrefab.IsValid() ) return;

		//Cloning the player prefab at the spawning position
		var player = PlayerPrefab.Clone( SpawnPoint.WorldPosition );

		//Make the player gameobject a networked gameobject with the owner being the player. This makes it so the player is visible to the other players
		player.NetworkSpawn( channel );
	}

	protected override async Task OnLoad()
	{
		//These come with the default network helper, just leave this here.
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby( new() );
		}
	}
}