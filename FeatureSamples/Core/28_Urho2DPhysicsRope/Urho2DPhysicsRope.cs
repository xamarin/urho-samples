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
using Urho.Urho2D;

namespace Urho.Samples
{
	public class Urho2DPhysicsRope : Sample
	{
		Scene scene;
		const uint NumObjects = 10;

		public Urho2DPhysicsRope(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd(", Use PageUp PageDown to zoom.");
			SetupViewport();
		}
		
		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera2D(timeStep);
			scene.GetComponent<PhysicsWorld2D>().DrawDebugGeometry();
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void CreateScene()
		{
			scene = new Scene();
			scene.CreateComponent<Octree>();
			scene.CreateComponent<DebugRenderer>();
			// Create camera node
			CameraNode = scene.CreateChild("Camera");
			// Set camera's position
			CameraNode.Position = new Vector3(0.0f, 5.0f, -10.0f);

			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.Orthographic = true;

			var graphics = Graphics;
			camera.OrthoSize = graphics.Height * 0.05f;
			camera.Zoom = 1.5f * Math.Min(graphics.Width / 1280.0f, graphics.Height / 800.0f); // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.5) is set for full visibility at 1280x800 resolution)

			// Create 2D physics world component
			PhysicsWorld2D physicsWorld = scene.CreateComponent<PhysicsWorld2D>();
			physicsWorld.DrawJoint = (true);

			// Create ground
			Node groundNode = scene.CreateChild("Ground");
			// Create 2D rigid body for gound
			RigidBody2D groundBody = groundNode.CreateComponent<RigidBody2D>();
			// Create edge collider for ground
			CollisionEdge2D groundShape = groundNode.CreateComponent<CollisionEdge2D>();
			groundShape.SetVertices(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

			const float y = 15.0f;
			RigidBody2D prevBody = groundBody;

			for (uint i = 0; i < NumObjects; ++i)
			{
				Node node = scene.CreateChild("RigidBody");

				// Create rigid body
				RigidBody2D body = node.CreateComponent<RigidBody2D>();
				body.BodyType= BodyType2D.Dynamic;

				// Create box
				CollisionBox2D box = node.CreateComponent<CollisionBox2D>();
				// Set friction
				box.Friction = 0.2f;
				// Set mask bits.
				box.MaskBits = 0xFFFF & ~0x0002;

				if (i == NumObjects - 1)
				{
					node.Position = new Vector3(1.0f * i, y, 0.0f);
					body.AngularDamping = 0.4f;
					box.SetSize(3.0f, 3.0f);
					box.Density = 100.0f;
					box.CategoryBits = 0x0002;
				}
				else
				{
					node.Position = new Vector3(0.5f + 1.0f * i, y, 0.0f);
					box.SetSize(1.0f, 0.25f);
					box.Density = 20.0f;
					box.CategoryBits = 0x0001;
				}

				ConstraintRevolute2D joint = node.CreateComponent<ConstraintRevolute2D>();
				joint.OtherBody = prevBody;
				joint.Anchor = new Vector2(i, y);
				joint.CollideConnected = false;

				prevBody = body;
			}

			ConstraintRope2D constraintRope = groundNode.CreateComponent<ConstraintRope2D>();
			constraintRope.OtherBody = prevBody;
			constraintRope.OwnerBodyAnchor=new Vector2(0.0f, y);
			constraintRope.MaxLength = NumObjects - 1.0f + 0.01f;

		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithZoomInAndOut;
	}
}
