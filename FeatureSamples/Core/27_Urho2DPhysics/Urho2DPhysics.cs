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
	public class Urho2DPhysics : Sample
	{
		Scene scene;
		const uint NumObjects = 100;

		public Urho2DPhysics(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd(", use PageUp PageDown keys to zoom.");
			SetupViewport();
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera2D(timeStep);
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
			CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));

			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.Orthographic = true;

			var graphics = Graphics;
			camera.OrthoSize=(float)graphics.Height * PixelSize;
			camera.Zoom = 1.2f * Math.Min((float)graphics.Width / 1280.0f, (float)graphics.Height / 800.0f); // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.2) is set for full visibility at 1280x800 resolution)

			// Create 2D physics world component
			scene.CreateComponent<PhysicsWorld2D>();

			var cache = ResourceCache;
			Sprite2D boxSprite = cache.GetSprite2D("Urho2D/Box.png");
			Sprite2D ballSprite = cache.GetSprite2D("Urho2D/Ball.png");

			// Create ground.
			Node groundNode = scene.CreateChild("Ground");
			groundNode.Position = (new Vector3(0.0f, -3.0f, 0.0f));
			groundNode.Scale=new Vector3(200.0f, 1.0f, 0.0f);

			// Create 2D rigid body for gound
			/*RigidBody2D groundBody = */
			groundNode.CreateComponent<RigidBody2D>();

			StaticSprite2D groundSprite = groundNode.CreateComponent<StaticSprite2D>();
			groundSprite.Sprite=boxSprite;

			// Create box collider for ground
			CollisionBox2D groundShape = groundNode.CreateComponent<CollisionBox2D>();
			// Set box size
			groundShape.Size=new Vector2(0.32f, 0.32f);
			// Set friction
			groundShape.Friction = 0.5f;

			for (uint i = 0; i < NumObjects; ++i)
			{
				Node node = scene.CreateChild("RigidBody");
				node.Position = (new Vector3(NextRandom(-0.1f, 0.1f), 5.0f + i * 0.4f, 0.0f));

				// Create rigid body
				RigidBody2D body = node.CreateComponent<RigidBody2D>();
				body.BodyType = BodyType2D.Dynamic; 
				StaticSprite2D staticSprite = node.CreateComponent<StaticSprite2D>();

				if (i % 2 == 0)
				{
					staticSprite.Sprite = boxSprite;

					// Create box
					CollisionBox2D box = node.CreateComponent<CollisionBox2D>();
					// Set size
					box.Size=new Vector2(0.32f, 0.32f);
					// Set density
					box.Density=1.0f;
					// Set friction
					box.Friction = 0.5f;
					// Set restitution
					box.Restitution=0.1f;
				}
				else
				{
					staticSprite.Sprite=ballSprite;

					// Create circle
					CollisionCircle2D circle = node.CreateComponent<CollisionCircle2D>();
					// Set radius
					circle.Radius=0.16f;
					// Set density
					circle.Density=1.0f;
					// Set friction.
					circle.Friction = 0.5f;
					// Set restitution
					circle.Restitution=0.1f;
				}
			}
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithZoomInAndOut;
	}
}
