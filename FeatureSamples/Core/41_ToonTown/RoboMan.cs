//
// Copyright (c) 2014-2015, THUNDERBEAST GAMES LLC All rights reserved
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Urho.Samples
{
	public class RoboMan : Component
	{
		// Grounded flag for movement.
		bool onGround;
		// Jump flag.
		bool okToJump;
		// In air timer. Due to possible physics inaccuracy, character can be off ground for max. 1/10 second and still be allowed to move.
		float inAirTimer;
		RigidBody body;
		AnimationController animCtrl;

		const float MoveForce = 0.8f;
		const float InairMoveForce = 0.02f;
		const float BrakeForce = 0.2f;
		const float JumpForce = 7.0f;
		const float InairThresholdTime = 0.1f;
		
		// Movement controls. Assigned by the main program each frame.
		public Controls Controls { get; set; } = new Controls();

		public RoboMan(IntPtr handle) : base(handle) { }

		public RoboMan(Context context) : base(context)
		{
			okToJump = true;
		}

		public void Start()
		{
			// Component has been inserted into its scene node. Subscribe to events now
			Node.SubscribeToNodeCollision(HandleNodeCollision);
		}

		public void FixedUpdate(float timeStep)
		{
			animCtrl = animCtrl ?? GetComponent<AnimationController>();
			body = body ?? GetComponent<RigidBody>();

			// Update the in air timer. Reset if grounded
			if (!onGround)
				inAirTimer += timeStep;
			else
				inAirTimer = 0.0f;
			// When character has been in air less than 1/10 second, it's still interpreted as being on ground
			bool softGrounded = inAirTimer < InairThresholdTime;

			// Update movement & animation
			var rot = Node.Rotation;
			Vector3 moveDir = Vector3.Zero;
			var velocity = body.LinearVelocity;
			// Velocity on the XZ plane
			Vector3 planeVelocity = new Vector3(velocity.X, 0.0f, velocity.Z);

			if (Controls.IsDown(ToonTown.CtrlForward)) moveDir += Vector3.UnitZ;
			if (Controls.IsDown(ToonTown.CtrlBack)) moveDir += -Vector3.UnitZ;
			if (Controls.IsDown(ToonTown.CtrlLeft)) moveDir += -Vector3.UnitX;
			if (Controls.IsDown(ToonTown.CtrlRight)) moveDir += Vector3.UnitX;

			// Normalize move vector so that diagonal strafing is not faster
			if (moveDir.LengthSquared > 0.0f)
				moveDir.Normalize();

			// If in air, allow control, but slower than when on ground
			body.ApplyImpulse(rot * moveDir * (softGrounded ? MoveForce : InairMoveForce));

			if (softGrounded)
			{
				// When on ground, apply a braking force to limit maximum ground velocity
				Vector3 brakeForce = -planeVelocity * BrakeForce;
				body.ApplyImpulse(brakeForce);

				// Jump. Must release jump control inbetween jumps
				if (Controls.IsDown(ToonTown.CtrlJump))
				{
					if (okToJump)
					{
						body.ApplyImpulse(Vector3.UnitY * JumpForce);
						okToJump = false;
					}
				}
				else
					okToJump = true;
			}

			// Play walk animation if moving on ground, otherwise fade it out
			if (softGrounded && !moveDir.Equals(Vector3.Zero))
				animCtrl.PlayExclusive("Models/Jack_Walk.ani", 0, true, 0.2f);
			else
				animCtrl.Stop("Models/Jack_Walk.ani", 0.2f);
			// Set walk animation speed proportional to velocity
			animCtrl.SetSpeed("Models/Jack_Walk.ani", planeVelocity.Length * 0.3f);

			// Reset grounded flag for next frame
			onGround = false;
		}

		void HandleNodeCollision(NodeCollisionEventArgs args)
		{
			foreach (var contact in args.Contacts)
			{
				// If contact is below node center and mostly vertical, assume it's a ground contact
				if (contact.ContactPosition.Y < (Node.Position.Y + 1.0f))
				{
					float level = Math.Abs(contact.ContactNormal.Y);
					if (level > 0.75)
						onGround = true;
				}
			}
		}
	}
}
