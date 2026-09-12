using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Xsl;
using Sandbox;
public sealed class RaycastCar : Component
{

	// [WARNING] This is not currently finished. The breaks are currently not working and the collisions are a bit junky!
	// [NOTICE] This is still not a tutorial, i will add comments so you can understand the code at a later time! (Statement was written at 12th September 2026)

	//This is a simple raycast car that also works in multiplayer!
	//This particular raycast car is single ray only so it is not very acurrate on small bumps!

	//This code ofcourse goes together with the wheel component!

	[Property] [Category("Wheels")] List<CarWheelComponent> Wheels {get; set;}

	[Property] [Category("RigidBody")] Rigidbody RB {get; set;}

	[Property] [Category("Vehicle")] float Acceleration {get; set;} = 500000f;
	[Property] [Category("Vehicle")] Curve AccelerationCurve {get; set;}
	[Property] [Category("Vehicle")] float MaxSpeed {get; set;} = 20f;
	[Property] [Category("Vehicle")] float TireTurnSpeed {get; set;} = 100f;
	[Property] [Category("Vehicle")] int TireMaxTurnDegrees {get; set;} = 25;
	[Property] [Category("Vehicle")] GameObject DriverSeat {get; set;}
	[Property] [Category("Vehicle")] GameObject CarBody {get; set;}

	[Property] [Category("SteeringWheel")] GameObject SteeringWheel {get; set;}
	[Property] [Category("SteeringWheel")] GameObject SteeringWheelTurnedRight {get; set;}
	[Property] [Category("SteeringWheel")] GameObject SteeringWheelTurnedLeft {get; set;}
	[Property] [Category("SteeringWheel")] GameObject SteeringWheelTurnedMiddle {get; set;}
	[Property] [Category("SteeringWheel")] float SteeringWheelTurneSpeed {get; set;} = 5f;

	[Property] [Category("Sounds")] SoundPointComponent EngineSound {get; set;}
	[Property] [Category("Sounds")] float EngineSoundLenght {get; set;} = 37;
	[Property] [Category("Sounds")] SoundEvent SitSoundEvent {get; set;}

	private int motorInput = 0;
	[Sync] [Property] public float TurnInput {get; private set;} = 0;
	private bool HandBreak = false;
	private bool IsBraking = false;
	private float MultipliedTurnInput = 0;
	private bool DriverSeatOccupied = false;
	private List<BaseChair> CarChairs = new List<BaseChair>();

	private TimeUntil _EndEngineSound;

	private void SingleWheelSuspension(CarWheelComponent WheelComp)
	{
		//Fire a raycast
		var endPosition = WheelComp.Wheel.WorldPosition + (-WheelComp.Wheel.WorldTransform.Up * WheelComp.RaycastLenght);
		var Raycast =  Scene.Trace.Ray(WheelComp.Wheel.WorldPosition, endPosition).WithCollisionRules("carraycast").Run();
		if (Raycast.Hit)
		{
			//We have hit something lets do this!
			//* Move Wheels Up-Down

			//Some normal variables to use later
			var contact = Raycast.HitPosition;
			var springUpDirection = WheelComp.Wheel.WorldTransform.Up;
			var springLenght = Vector3.DistanceBetween(WheelComp.Wheel.WorldPosition, contact) - WheelComp.WheelRadius;
			var offsetDistance = WheelComp.RestDistance - springLenght;

			//Positon Wheels
			WheelComp.WheelRenderer.LocalPosition = Vector3.Lerp(WheelComp.WheelRenderer.LocalPosition, new Vector3(WheelComp.WheelRenderer.LocalPosition.x, WheelComp.WheelRenderer.LocalPosition.y, -springLenght), WheelComp.TireLerpSpeed * Time.Delta, false);

			//Calculate spring force
			var SpringForce = WheelComp.SpringStrenght * offsetDistance;

			var WorldVelocity = RB.GetVelocityAtPoint(WheelComp.Wheel.WorldPosition);
			var RelativeVelocity = springUpDirection.Dot(WorldVelocity);
			var springDampingForce = WheelComp.SpringDamping * RelativeVelocity;

			var ForceVector = (SpringForce - springDampingForce) * springUpDirection;
			var VerticalForceVector = new Vector3(0,0, ForceVector.z);

			RB.ApplyForceAt(WheelComp.Wheel.WorldPosition, VerticalForceVector);

			//* Rotate the wheel model! And also calculate some stuff for accelerating the car!
			var ForwardDirection = WheelComp.Wheel.WorldTransform.Rotation.Forward;
			var CarVelocity = Vector3.Dot(RB.Velocity, WorldRotation.Forward);

			WheelComp.WheelRenderer.WorldRotation = WheelComp.WheelRenderer.WorldRotation.RotateAroundAxis(Vector3.Forward, (float)(CarVelocity * Time.Delta / WheelComp.WheelRadius * 180f / Math.PI));

			//* Accelerate the car forwards
			//Move car
			if (motorInput != 0 && WheelComp.isMotor == true)
			{
				//Calculate speed and acceleration ratio so we get realistic acceleration
				var SpeedRatio = Math.Abs(CarVelocity / MaxSpeed);
				var AccelerationFactor = AccelerationCurve.Evaluate(SpeedRatio);

				var ForwardForceVector = ForwardDirection * Acceleration * motorInput * AccelerationFactor;

				RB.ApplyForceAt(WheelComp.Wheel.WorldPosition, ForwardForceVector);
			}

			//* Rotate Neccecary wheels
			var SteerSideDirection = WheelComp.SteeringGameObject.WorldTransform.Forward;
			var TireVelocity = WorldVelocity;
			var SteeringXVelocity = SteerSideDirection.Dot(TireVelocity);

			var GripFactor = Math.Abs(SteeringXVelocity/TireVelocity.Length);
			var YTraction = WheelComp.GripCurve.Evaluate(GripFactor); //! in the tutorial it is xTraction and xForce, because in godot it is that direction, but here it is Y

			if (HandBreak == true) YTraction = 0.1f;

			var Gravity = 9.8f;
			var YForce = -SteerSideDirection * SteeringXVelocity * YTraction * ((RB.Mass * Gravity)/4f);

			//FORCE WTRACTION (TRIBH)
			var forwardVelocity = CarVelocity;
			var ChosenTraction = WheelComp.WheelTraction;
			if (IsBraking == true) ChosenTraction = WheelComp.BrakesBackwardsTraction;
			var BackwardsForce = WheelComp.Wheel.WorldTransform.Backward * forwardVelocity * ChosenTraction * ((RB.Mass * Gravity)/4f);

			//Apply forces
			RB.ApplyForceAt(WheelComp.Wheel.WorldPosition, YForce);
			RB.ApplyForceAt(WheelComp.Wheel.WorldPosition, BackwardsForce);
		}
	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy) return;

		//Do the wheel stuff
		foreach (var Wheel in Wheels)
		{
			SingleWheelSuspension(Wheel);
		}
	}

	private void TurnSteeringWheel()
	{
		//* Turn the steering wheel
		if (TurnInput == 0)
		{
			SteeringWheel.WorldRotation = Rotation.Slerp(SteeringWheel.WorldRotation, SteeringWheelTurnedMiddle.WorldRotation, Time.Delta * SteeringWheelTurneSpeed, true);
		}
		else
		{
			if (TurnInput == 1)
			{
				SteeringWheel.WorldRotation = Rotation.Slerp(SteeringWheel.WorldRotation, SteeringWheelTurnedLeft.WorldRotation, Time.Delta * SteeringWheelTurneSpeed, true);
			}
			else
			{
				SteeringWheel.WorldRotation = Rotation.Slerp(SteeringWheel.WorldRotation, SteeringWheelTurnedRight.WorldRotation, Time.Delta * SteeringWheelTurneSpeed, true);
			}
		}
	}

	private void TurnTheWheels()
	{
		//* Turn the car
		MultipliedTurnInput = TurnInput * TireTurnSpeed;

		foreach (var WheelComp in Wheels)
		{
			if (WheelComp.isFrontWheel == true)
			{
				if (MultipliedTurnInput != 0)
				{
					var newYaw = Math.Clamp(WheelComp.Wheel.LocalRotation.Angles().yaw + MultipliedTurnInput * Time.Delta, - TireMaxTurnDegrees, TireMaxTurnDegrees);

					WheelComp.Wheel.LocalRotation = WheelComp.Wheel.LocalTransform.Rotation.Angles().WithYaw( newYaw).ToRotation();
				}
				else
				{
					var newYaw = MathX.Approach(WheelComp.Wheel.LocalTransform.Rotation.Angles().yaw, 0, TireTurnSpeed * Time.Delta);
					WheelComp.Wheel.LocalRotation = WheelComp.Wheel.LocalTransform.Rotation.Angles().WithYaw( newYaw).ToRotation();
				}
			}
		}
	}

	protected override void OnStart()
	{
		_EndEngineSound = 1f;
	}

	protected override void OnUpdate()
	{
		//* Play Audio if someone sits on the chair
		var CarBodyChildren = CarBody.Children;
		foreach (var ChairGameObject in CarBodyChildren)
		{
			var Chair = ChairGameObject.GetComponent<BaseChair>();
			if (Chair == null) continue;

			if (Chair.IsOccupied == true && !CarChairs.Contains(Chair))
			{
				CarChairs.Add(Chair);
				Sound.Play(SitSoundEvent, ChairGameObject.WorldPosition);
			}
			else if (Chair.IsOccupied == false && CarChairs.Contains(Chair))
			{
				Sound.Play(SitSoundEvent, ChairGameObject.WorldPosition);
				CarChairs.Remove(Chair);
			}
		}
		

		//These happen for the owner and the other clients
		DriverSeatOccupied = DriverSeat.GetComponent<BaseChair>().IsOccupied;

		//* Turn the steering wheel
		TurnSteeringWheel();

		//* Turn the car
		TurnTheWheels();

		//* Car Sounds
		if (DriverSeatOccupied == false)
		{
			//Stop engine sounds
			EngineSound.StopSound();
			_EndEngineSound = .1f;

		}
		else
		{
			//Adjust the sound volume based on speed
			var CarVelocity = Vector3.Dot(RB.Velocity, WorldRotation.Forward);
			var SoundVolume = CarVelocity / 570 * 2.5f; //! Max Velocity

			EngineSound.Volume = SoundVolume;

			//See if we should play the sound or not
			if (_EndEngineSound)
			{
				_EndEngineSound = EngineSoundLenght;
				EngineSound.StopSound();
				EngineSound.StartSound();
			}
		}


		if (IsProxy) return;

		//* Check if a player has sut on the driver seat
		var CurrentSittingPlayer = DriverSeat.Components.GetInDescendants<PlayerController>();
		if (CurrentSittingPlayer != null && CurrentSittingPlayer.GameObject.IsProxy == true)
		{
			//We have found that a player who does not own the car has sut on the car, so we need to change the ownership!
			GameObject.Network.AssignOwnership(CurrentSittingPlayer.GameObject.Network.Owner);
			CarBody.Network.AssignOwnership(CurrentSittingPlayer.GameObject.Network.Owner);
		}

		//* See if someone has the seat occupied!
		if (DriverSeatOccupied == false )
		{
			//* All settings are default
			TurnInput = 0;
			HandBreak = false;
			IsBraking = false;
			motorInput = 0;
			MultipliedTurnInput = 0;
		}
		else
		{
			//* Forwards backwards
			//Accelerate
			if (Input.Pressed("CarAccelerate")) motorInput = 1;
			if (Input.Released("CarAccelerate")) motorInput = 0;

			//Descelerate
			if (Input.Pressed("CarDescelerate")) motorInput = -1;
			if (Input.Released("CarDescelerate")) motorInput = 0;

			//* Turning Right & Left
			TurnInput = 0;

			if (Input.Down("CarTurnLeft")) TurnInput += 1;
			if (Input.Down("CarTurnRight")) TurnInput -= 1;

			//* Handbreak
			if (Input.Down("CarHandBrake")) HandBreak = true; else HandBreak = false;
			//* Car brakes
			if (Input.Down("CarBrake")) IsBraking = true; else IsBraking = false;

		}
	}

}
