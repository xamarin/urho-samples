//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using Urho.Physics;

namespace Urho.Samples
{
	/// <summary>
	/// Vehicle component, responsible for physical movement according to controls.
	/// </summary>
	public class Vehicle : Component
	{
		public const int CtrlForward = 1;
		public const int CtrlBack = 2;
		public const int CtrlLeft = 4;
		public const int CtrlRight = 8;

		public const float YawSensitivity = 0.1f;
		public const float EnginePower = 10.0f;
		public const float DownForce = 10.0f;
		public const float MaxWheelAngle = 22.5f;

		// Movement controls.
		public Controls Controls { get; set; } = new Controls();

		// Wheel scene nodes.
		Node frontLeft;
		Node frontRight;
		Node rearLeft;
		Node rearRight;

		// Steering axle constraints.
		Constraint frontLeftAxis;
		Constraint frontRightAxis;

		// Hull and wheel rigid bodies.
		RigidBody hullBody;
		RigidBody frontLeftBody;
		RigidBody frontRightBody;
		RigidBody rearLeftBody;
		RigidBody rearRightBody;

		// IDs of the wheel scene nodes for serialization.
		uint frontLeftId;
		uint frontRightId;
		uint rearLeftId;
		uint rearRightId;

		/// Current left/right steering amount (-1 to 1.)
		float steering;

		public Vehicle(IntPtr handle) : base(handle) { }

		public Vehicle() {}

		public void FixedUpdate(float timeStep)
		{
			float newSteering = 0.0f;
			float accelerator = 0.0f;

			// Read controls
			if (Controls.IsDown(CtrlLeft))
				newSteering = -1.0f;
			if (Controls.IsDown(CtrlRight))
				newSteering = 1.0f;
			if (Controls.IsDown(CtrlForward))
				accelerator = 1.0f;
			if (Controls.IsDown(CtrlBack))
				accelerator = -0.5f;

			// When steering, wake up the wheel rigidbodies so that their orientation is updated
			if (newSteering != 0.0f)
			{
				frontLeftBody.Activate();
				frontRightBody.Activate();
				steering = steering * 0.95f + newSteering * 0.05f;
			}
			else
				steering = steering * 0.8f + newSteering * 0.2f;

			// Set front wheel angles
			Quaternion steeringRot = new Quaternion(0, steering * MaxWheelAngle, 0);
			frontLeftAxis.SetOtherAxis(steeringRot * new Vector3(-1f, 0f, 0f));
			frontRightAxis.SetOtherAxis(steeringRot * Vector3.UnitX);

			Quaternion hullRot = hullBody.Rotation;
			if (accelerator != 0.0f)
			{
				// Torques are applied in world space, so need to take the vehicle & wheel rotation into account
				Vector3 torqueVec = new Vector3(EnginePower * accelerator, 0.0f, 0.0f);

				frontLeftBody.ApplyTorque(hullRot * steeringRot * torqueVec);
				frontRightBody.ApplyTorque(hullRot * steeringRot * torqueVec);
				rearLeftBody.ApplyTorque(hullRot * torqueVec);
				rearRightBody.ApplyTorque(hullRot * torqueVec);
			}

			// Apply downforce proportional to velocity
			Vector3 localVelocity = Quaternion.Invert(hullRot) * hullBody.LinearVelocity;
			hullBody.ApplyForce(hullRot * new Vector3(0f, -1f, 0f) * Math.Abs(localVelocity.Z) * DownForce);
		}

		public void Init()
		{
			// This function is called only from the main program when initially creating the vehicle, not on scene load
			var node = Node;
			StaticModel hullObject = node.CreateComponent<StaticModel>();
			hullBody = node.CreateComponent<RigidBody>();
			CollisionShape hullShape = node.CreateComponent<CollisionShape>();

			node.Scale = new Vector3(1.5f, 1.0f, 3.0f);
			hullObject.Model = Application.ResourceCache.GetModel("Models/Box.mdl");
			hullObject.SetMaterial(Application.ResourceCache.GetMaterial("Materials/Stone.xml"));
			hullObject.CastShadows = true;
			hullShape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);
			hullBody.Mass = 4.0f;
			hullBody.LinearDamping = 0.2f; // Some air resistance
			hullBody.AngularDamping = 0.5f;
			hullBody.CollisionLayer = 1;

			InitWheel("FrontLeft", new Vector3(-0.6f, -0.4f, 0.3f), out frontLeft, out frontLeftId);
			InitWheel("FrontRight", new Vector3(0.6f, -0.4f, 0.3f), out frontRight, out frontRightId);
			InitWheel("RearLeft", new Vector3(-0.6f, -0.4f, -0.3f), out rearLeft, out rearLeftId);
			InitWheel("RearRight", new Vector3(0.6f, -0.4f, -0.3f), out rearRight, out rearRightId);

			GetWheelComponents();
		}

		void InitWheel(string name, Vector3 offset, out Node wheelNode, out uint wheelNodeId)
		{
			// Note: do not parent the wheel to the hull scene node. Instead create it on the root level and let the physics
			// constraint keep it together
			wheelNode = Scene.CreateChild(name);
			wheelNode.Position = Node.LocalToWorld(offset);
			wheelNode.Rotation = Node.Rotation * (offset.X >= 0.0 ? new Quaternion(0.0f, 0.0f, -90.0f) : new Quaternion(0.0f, 0.0f, 90.0f));
			wheelNode.Scale = new Vector3(0.8f, 0.5f, 0.8f);
			// Remember the ID for serialization
			wheelNodeId = wheelNode.ID;

			StaticModel wheelObject = wheelNode.CreateComponent<StaticModel>();
			RigidBody wheelBody = wheelNode.CreateComponent<RigidBody>();
			CollisionShape wheelShape = wheelNode.CreateComponent<CollisionShape>();
			Constraint wheelConstraint = wheelNode.CreateComponent<Constraint>();

			wheelObject.Model = (Application.ResourceCache.GetModel("Models/Cylinder.mdl"));
			wheelObject.SetMaterial(Application.ResourceCache.GetMaterial("Materials/Stone.xml"));
			wheelObject.CastShadows = true;
			wheelShape.SetSphere(1.0f, Vector3.Zero, Quaternion.Identity);
			wheelBody.Friction = (1.0f);
			wheelBody.Mass = 1.0f;
			wheelBody.LinearDamping = 0.2f; // Some air resistance
			wheelBody.AngularDamping = 0.75f; // Could also use rolling friction
			wheelBody.CollisionLayer = 1;
			wheelConstraint.ConstraintType = ConstraintType.Hinge;
			wheelConstraint.OtherBody = GetComponent<RigidBody>(); // Connect to the hull body
			wheelConstraint.SetWorldPosition(wheelNode.Position); // Set constraint's both ends at wheel's location
			wheelConstraint.SetAxis(Vector3.UnitY); // Wheel rotates around its local Y-axis
			wheelConstraint.SetOtherAxis(offset.X >= 0.0 ? Vector3.UnitX : new Vector3(-1f, 0f, 0f)); // Wheel's hull axis points either left or right
			wheelConstraint.LowLimit = new Vector2(-180.0f, 0.0f); // Let the wheel rotate freely around the axis
			wheelConstraint.HighLimit = new Vector2(180.0f, 0.0f);
			wheelConstraint.DisableCollision = true; // Let the wheel intersect the vehicle hull
		}

		void GetWheelComponents()
		{
			frontLeftAxis = frontLeft.GetComponent<Constraint>();
			frontRightAxis = frontRight.GetComponent<Constraint>();
			frontLeftBody = frontLeft.GetComponent<RigidBody>();
			frontRightBody = frontRight.GetComponent<RigidBody>();
			rearLeftBody = rearLeft.GetComponent<RigidBody>();
			rearRightBody = rearRight.GetComponent<RigidBody>();
		}
	}
}
