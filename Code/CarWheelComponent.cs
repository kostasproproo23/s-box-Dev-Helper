using Sandbox;

public sealed class CarWheelComponent : Component
{
	[Property] public float SpringStrenght {get; set;} = 10000f;
	[Property] public float SpringDamping {get; set;} = 400;
	[Property] public float RestDistance {get; set;} = 15f;
	[Property] public float RaycastLenght {get; set;} = 30f;
	[Property] public float WheelRadius {get; set;} = 15f;
	[Property] public bool isMotor {get; set;} = false;
	[Property] public bool isFrontWheel {get; set;} = false;
	[Property] public float WheelTraction {get; set;} = .05f;
	[Property] public Curve GripCurve {get; set;}
	[Property] public float TireLerpSpeed {get; set;} = 5f;
	[Property] public float BrakesBackwardsTraction {get; set;} = .25f;

	[Property] public GameObject WheelRenderer {get; set;}
	[Property] public GameObject SteeringGameObject {get; set;}
	[Property] public GameObject Wheel {get; set;}
}
