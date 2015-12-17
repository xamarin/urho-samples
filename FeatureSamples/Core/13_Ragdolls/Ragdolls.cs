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
using Urho.Physics;

namespace Urho.Samples
{
	public class Ragdolls : Sample
	{
		Scene scene;
		bool drawDebug;
		Camera camera;

		public Ragdolls(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd(     
				"\nLMB to spawn physics objects\n" +
				"F5 to save scene, F7 to load\n" +
				"Space to toggle physics debug geometry");
			SetupViewport();
			SubscribeToEvents();
		}

		void SubscribeToEvents()
		{
			Engine.SubscribeToPostRenderUpdate(args =>
				{
					// If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
					// bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
					// bones properly
					if (drawDebug)
						Renderer.DrawDebugGeometry(false);
				});
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			SimpleMoveCamera3D(timeStep);
			var input = Input;
			// "Shoot" a physics object with left mousebutton
			if (input.GetMouseButtonPress(MouseButton.Left))
				SpawnObject();

			// Check for loading / saving the scene
			if (input.GetKeyPress(Key.F5))
				scene.SaveXml(FileSystem.ProgramDir + "Data/Scenes/Ragdolls.xml", "\t");

			if (input.GetKeyPress(Key.F7))
				scene.LoadXml(FileSystem.ProgramDir + "Data/Scenes/Ragdolls.xml");

			if (Input.GetKeyPress(Key.Space))
				drawDebug = !drawDebug;
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void CreateScene()
		{
			var cache = ResourceCache;
			scene = new Scene();

			// Create octree, use default volume (-1000, -1000, -1000) to (1000, 1000, 1000)
			// Create a physics simulation world with default parameters, which will update at 60fps. Like the Octree must
			// exist before creating drawable components, the PhysicsWorld must exist before creating physics components.
			// Finally, create a DebugRenderer component so that we can draw physics debug geometry
			scene.CreateComponent<Octree>();
			scene.CreateComponent<PhysicsWorld>();
			scene.CreateComponent<DebugRenderer>();
	
			// Create a Zone component for ambient lighting & fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor=(new Color(0.15f, 0.15f, 0.15f));
			zone.FogColor=new Color(0.5f, 0.5f, 0.7f);
			zone.FogStart=100.0f;
			zone.FogEnd=300.0f;
	
			// Create a directional light to the world. Enable cascaded shadows on it
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
			Light light = lightNode.CreateComponent<Light>();
			light.LightType=LightType.Directional;
			light.CastShadows=true;
			light.ShadowBias=new BiasParameters(0.00025f, 0.5f);
			// Set cascade splits at 10, 50 and 200 world units, fade shadows out at 80% of maximum shadow distance
			light.ShadowCascade=new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);
	
			{
				// Create a floor object, 500 x 500 world units. Adjust position so that the ground is at zero Y
				Node floorNode = scene.CreateChild("Floor");
				floorNode.Position=new Vector3(0.0f, -0.5f, 0.0f);
				floorNode.Scale=new Vector3(500.0f, 1.0f, 500.0f);
				StaticModel floorObject = floorNode.CreateComponent<StaticModel>();
				floorObject.Model=cache.GetModel("Models/Box.mdl");
				floorObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

				// Make the floor physical by adding RigidBody and CollisionShape components
				RigidBody body = floorNode.CreateComponent<RigidBody>();
				// We will be spawning spherical objects in this sample. The ground also needs non-zero rolling friction so that
				// the spheres will eventually come to rest
				body.RollingFriction = 0.15f;
				CollisionShape shape = floorNode.CreateComponent<CollisionShape>();
				// Set a box shape of size 1 x 1 x 1 for collision. The shape will be scaled with the scene node scale, so the
				// rendering and physics representation sizes should match (the box model is also 1 x 1 x 1.)
				shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);
			}

			// Create animated models
			for (int z = -1; z <= 1; ++z)
			{
				for (int x = -4; x <= 4; ++x)
				{
					Node modelNode = scene.CreateChild("Jack");
					modelNode.Position=new Vector3(x * 5.0f, 0.0f, z * 5.0f);
					modelNode.Rotation=new Quaternion(0.0f, 180.0f, 0.0f);
					AnimatedModel modelObject = modelNode.CreateComponent<AnimatedModel>();
					modelObject.Model=cache.GetModel("Models/Jack.mdl");
					modelObject.SetMaterial(cache.GetMaterial("Materials/Jack.xml"));
					modelObject.CastShadows=true;
					// Set the model to also update when invisible to avoid staying invisible when the model should come into
					// view, but does not as the bounding box is not updated
					modelObject.UpdateInvisible=true;

					// Create a rigid body and a collision shape. These will act as a trigger for transforming the
					// model into a ragdoll when hit by a moving object
					RigidBody body = modelNode.CreateComponent<RigidBody>();
					// The Trigger mode makes the rigid body only detect collisions, but impart no forces on the
					// colliding objects
					body.Trigger=true;
					CollisionShape shape = modelNode.CreateComponent<CollisionShape>();
					// Create the capsule shape with an offset so that it is correctly aligned with the model, which
					// has its origin at the feet
					shape.SetCapsule(0.7f, 2.0f, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity);

					// Create a custom component that reacts to collisions and creates the ragdoll
					modelNode.AddComponent(new Ragdoll());
				}
			}

			// Create the camera. Limit far clip distance to match the fog
			CameraNode = new Node();
			camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;
			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 3.0f, -20.0f);
		}

		void SpawnObject()
		{
			var cache = ResourceCache;
	
			Node boxNode = scene.CreateChild("Sphere");
			boxNode.Position=CameraNode.Position;
			boxNode.Rotation=CameraNode.Rotation;
			boxNode.SetScale(0.25f);
			StaticModel boxObject = boxNode.CreateComponent<StaticModel>();
			boxObject.Model=cache.GetModel("Models/Sphere.mdl");
			boxObject.SetMaterial(cache.GetMaterial("Materials/StoneSmall.xml"));
			boxObject.CastShadows=true;

			RigidBody body = boxNode.CreateComponent<RigidBody>();
			body.Mass = 1.0f;
			body.RollingFriction = 0.15f;
			CollisionShape shape = boxNode.CreateComponent<CollisionShape>();
			shape.SetSphere(1.0f, Vector3.Zero, Quaternion.Identity);
	
			const float objectVelocity = 10.0f;
	
			// Set initial velocity for the RigidBody based on camera forward vector. Add also a slight up component
			// to overcome gravity better
			body.SetLinearVelocity(CameraNode.Rotation * new Vector3(0.0f, 0.25f, 1.0f) * objectVelocity);
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithFireAndDebugButtons;
	}
}