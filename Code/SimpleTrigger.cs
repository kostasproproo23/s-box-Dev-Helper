using Sandbox;

public sealed class SimpleTrigger : Component, Component.ITriggerListener //You need this so it actually listens when a player enters the collider
{

	//Requirments
	//1) For this to work you will need to have a collider of any kind and have Trigger Enabled
	//2) Have the gameobject an appropriate tag and make sure the tag can collide with the player tag (you can skip this part). To make it collidable with the player tag, go to the project settings --> Physics and find your tag, if it does not exist just press the + and add it in. After you have seen it, make sure you have the tags on triggers!

	//3) have this function. Other, is the gameobject that has collided
	public void OnTriggerEnter(Collider other)
	{
		//Check if what we have collided with is a player
		if (other.Tags.Has("player"))
		{
			//Run whatever code you want in here
			Log.Info("Stepped Inside the trigger");
		}
	}
}
