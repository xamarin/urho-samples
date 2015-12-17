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

namespace Urho.Samples
{
	public class AnimatingScene : Sample
	{
		Scene scene;

		public AnimatingScene(ApplicationOptions options = null) : base(options) { }

		void CreateScene ()
		{
			var cache = ResourceCache;
			scene = new Scene ();

			// Create the Octree component to the scene so that drawable objects can be rendered. Use default volume
			// (-1000, -1000, -1000) to (1000, 1000, 1000)
			scene.CreateComponent<Octree> ();

			// Create a Zone component into a child scene node. The Zone controls ambient lighting and fog settings. Like the Octree,
			// it also defines its volume with a bounding box, but can be rotated (so it does not need to be aligned to the world X, Y
			// and Z axes.) Drawable objects "pick up" the zone they belong to and use it when rendering; several zones can exist
			var zoneNode = scene.CreateChild("Zone");
			var zone = zoneNode.CreateComponent<Zone>();
		
			// Set same volume as the Octree, set a close bluish fog and some ambient light
			zone.SetBoundingBox (new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor = new Color (0.05f, 0.1f, 0.15f);
			zone.FogColor = new Color (0.1f, 0.2f, 0.3f);
			zone.FogStart = 10;
			zone.FogEnd = 100;

			var boxesNode = scene.CreateChild("Boxes");
	
			const int numObjects = 2000;
			for (var i = 0; i < numObjects; ++i)
			{
				Node boxNode = new Node(); 
				boxesNode.AddChild(boxNode, 0);
				boxNode.Position = new Vector3(NextRandom (200f) - 100f, NextRandom (200f) - 100f, NextRandom (200f) - 100f);
				// Orient using random pitch, yaw and roll Euler angles
				boxNode.Rotation = new Quaternion(NextRandom(360.0f), NextRandom(360.0f), NextRandom(360.0f));

				using (var boxObject = boxNode.CreateComponent<StaticModel>())
				{
					boxObject.Model = cache.GetModel("Models/Box.mdl");
					boxObject.SetMaterial(cache.GetMaterial("Materials/Stone.xml"));
					//we don't need this component in C# anymore so let's just delete a MCW for it (howerver, we can access it anytime if we need via GetComponent<>) 
					//it's just an optimization to reduce cached objects count
				}
		
				// Add our custom Rotator component which will rotate the scene node each frame, when the scene sends its update event.
				// The Rotator component derives from the base class LogicComponent, which has convenience functionality to subscribe
				// to the various update events, and forward them to virtual functions that can be implemented by subclasses. This way
				// writing logic/update components in C++ becomes similar to scripting.
				// Now we simply set same rotation speed for all objects

				var rotationSpeed = new Vector3(10.0f, 20.0f, 30.0f);

				// First style: use a Rotator instance, which is a component subclass, and
				// add it to the boxNode.
				var rotator = new Rotator () { RotationSpeed = rotationSpeed };
				boxNode.AddComponent (rotator);
			}
			// Create the camera. Let the starting position be at the world origin. As the fog limits maximum visible distance, we can
			// bring the far clip plane closer for more effective culling of distant objects
			CameraNode = scene.CreateChild("Camera");
			var camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 100.0f;
	
			// Create a point light to the camera scene node
			var light = CameraNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 30.0f;
		}

		void SetupViewport ()
		{
			var renderer = Renderer;
			renderer.SetViewport (0, new Viewport (Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		protected override void Start ()
		{
			base.Start ();
			CreateScene ();
			SimpleCreateInstructionsWithWasd ();
			SetupViewport ();
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera3D(timeStep);
			if (Input.GetKeyPress(Key.Delete))
			{
				scene.GetChild("Boxes", false).RemoveAllChildren();
			}
			base.OnUpdate(timeStep);
		}

		class Rotator : Component
		{
			public Rotator()
			{
				//to receive OnUpdate:
				ReceiveSceneUpdates = true;
			}

			public Vector3 RotationSpeed { get; set; }

			protected override void OnUpdate(float timeStep)
			{
				Node.Rotate(new Quaternion(
					RotationSpeed.X * timeStep,
					RotationSpeed.Y * timeStep,
					RotationSpeed.Z * timeStep),
					TransformSpace.Local);
			}
		}
	}
}