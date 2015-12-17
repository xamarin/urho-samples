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
	public class VehicleDemo : Sample
	{
		Scene scene;
		Vehicle vehicle;

		const float CameraDistance = 10.0f;

		public VehicleDemo(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();

			// Create static scene content
			CreateScene();

			// Create the controllable vehicle
			CreateVehicle();

			// Create the UI content
			SimpleCreateInstructionsWithWasd("\nF5 to save scene, F7 to load");

			// Subscribe to necessary events
			SubscribeToEvents();
		}

		void SubscribeToEvents()
		{
			Engine.SubscribeToPostUpdate(args =>
				{
					if (vehicle == null)
						return;

					Node vehicleNode = vehicle.Node;

					// Physics update has completed. Position camera behind vehicle
					Quaternion dir = Quaternion.FromAxisAngle(Vector3.UnitY, vehicleNode.Rotation.YawAngle);
					dir = dir * Quaternion.FromAxisAngle(Vector3.UnitY, vehicle.Controls.Yaw);
					dir = dir * Quaternion.FromAxisAngle(Vector3.UnitX, vehicle.Controls.Pitch);

					Vector3 cameraTargetPos = vehicleNode.Position - (dir * new Vector3(0.0f, 0.0f, CameraDistance));
					Vector3 cameraStartPos = vehicleNode.Position;

					// Raycast camera against static objects (physics collision mask 2)
					// and move it closer to the vehicle if something in between
					Ray cameraRay = new Ray(cameraStartPos, cameraTargetPos - cameraStartPos);
					float cameraRayLength = (cameraTargetPos - cameraStartPos).Length;
					PhysicsRaycastResult result = new PhysicsRaycastResult();
					scene.GetComponent<PhysicsWorld>().RaycastSingle(ref result, cameraRay, cameraRayLength, 2);
					if (result.Body != null)
					{
						cameraTargetPos = cameraStartPos + cameraRay.Direction * (result.Distance - 0.5f);
					}

					CameraNode.Position = cameraTargetPos;
					CameraNode.Rotation = dir;
				});

			scene.GetComponent<PhysicsWorld>().SubscribeToPhysicsPreStep(args => vehicle?.FixedUpdate(args.TimeStep));
		}
		
		protected override void OnUpdate(float timeStep)
		{
			Input input = Input;

			if (vehicle != null)
			{
				// Get movement controls and assign them to the vehicle component. If UI has a focused element, clear controls
				if (UI.FocusElement == null)
				{
					vehicle.Controls.Set(Vehicle.CtrlForward, input.GetKeyDown(Key.W));
					vehicle.Controls.Set(Vehicle.CtrlBack, input.GetKeyDown(Key.S));
					vehicle.Controls.Set(Vehicle.CtrlLeft, input.GetKeyDown(Key.A));
					vehicle.Controls.Set(Vehicle.CtrlRight, input.GetKeyDown(Key.D));

					// Add yaw & pitch from the mouse motion or touch input. Used only for the camera, does not affect motion
					if (TouchEnabled)
					{
						for (uint i = 0; i < input.NumTouches; ++i)
						{
							TouchState state = input.GetTouch(i);
							Camera camera = CameraNode.GetComponent<Camera>();
							if (camera == null)
								return;

							var graphics = Graphics;
							vehicle.Controls.Yaw += TouchSensitivity*camera.Fov/graphics.Height*state.Delta.X;
							vehicle.Controls.Pitch += TouchSensitivity*camera.Fov/graphics.Height*state.Delta.Y;
						}
					}
					else
					{
						vehicle.Controls.Yaw += (float)input.MouseMoveX * Vehicle.YawSensitivity;
						vehicle.Controls.Pitch += (float)input.MouseMoveY * Vehicle.YawSensitivity;
					}
					// Limit pitch
					vehicle.Controls.Pitch = MathHelper.Clamp(vehicle.Controls.Pitch, 0.0f, 80.0f);

					// Check for loading / saving the scene
					if (input.GetKeyPress(Key.F5))
					{
						scene.SaveXml(FileSystem.ProgramDir + "Data/Scenes/VehicleDemo.xml");
					}
					if (input.GetKeyPress(Key.F7))
					{
						scene.LoadXml(FileSystem.ProgramDir + "Data/Scenes/VehicleDemo.xml");
						// After loading we have to reacquire the weak pointer to the Vehicle component, as it has been recreated
						// Simply find the vehicle's scene node by name as there's only one of them
						Node vehicleNode = scene.GetChild("Vehicle", true);
						if (vehicleNode != null)
							vehicle = vehicleNode.GetComponent<Vehicle>();
					}
				}
				else
					vehicle.Controls.Set(Vehicle.CtrlForward | Vehicle.CtrlBack | Vehicle.CtrlLeft | Vehicle.CtrlRight, false);
			}
		}

		void CreateVehicle()
		{
			Node vehicleNode = scene.CreateChild("Vehicle");
			vehicleNode.Position = (new Vector3(0.0f, 5.0f, 0.0f));

			// Create the vehicle logic component
			vehicle = new Vehicle(); 
			vehicleNode.AddComponent(vehicle);
			// Create the rendering and physics components
			vehicle.Init();
		}


		void CreateScene()
		{
			var cache = ResourceCache;

			scene = new Scene();

			// Create scene subsystem components
			scene.CreateComponent<Octree>();
			scene.CreateComponent<PhysicsWorld>();

			// Create camera and define viewport. We will be doing load / save, so it's convenient to create the camera outside the scene,
			// so that it won't be destroyed and recreated, and we don't have to redefine the viewport on load
			CameraNode = new Node();
			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 500.0f;
			Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));

			// Create static scene content. First create a zone for ambient lighting and fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.AmbientColor = new Color(0.15f, 0.15f, 0.15f);
			zone.FogColor = new Color(0.5f, 0.5f, 0.7f);
			zone.FogStart = 300.0f;
			zone.FogEnd = 500.0f;
			zone.SetBoundingBox(new BoundingBox(-2000.0f, 2000.0f));

			// Create a directional light with cascaded shadow mapping
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.3f, -0.5f, 0.425f));
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;
			light.CastShadows = true;
			light.ShadowBias = new BiasParameters(0.00025f, 0.5f);
			light.ShadowCascade = new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);
			light.SpecularIntensity = 0.5f;

			// Create heightmap terrain with collision
			Node terrainNode = scene.CreateChild("Terrain");
			terrainNode.Position = (Vector3.Zero);
			Terrain terrain = terrainNode.CreateComponent<Terrain>();
			terrain.PatchSize = 64;
			terrain.Spacing = new Vector3(2.0f, 0.1f, 2.0f); // Spacing between vertices and vertical resolution of the height map
			terrain.Smoothing = true;
			terrain.SetHeightMap(cache.GetImage("Textures/HeightMap.png"));
			terrain.Material = cache.GetMaterial("Materials/Terrain.xml");
			// The terrain consists of large triangles, which fits well for occlusion rendering, as a hill can occlude all
			// terrain patches and other objects behind it
			terrain.Occluder = true;

			RigidBody body = terrainNode.CreateComponent<RigidBody>();
			body.CollisionLayer = 2; // Use layer bitmask 2 for static geometry
			CollisionShape shape = terrainNode.CreateComponent<CollisionShape>();
			shape.SetTerrain(0);

			// Create 1000 mushrooms in the terrain. Always face outward along the terrain normal
			const uint numMushrooms = 1000;
			for (uint i = 0; i < numMushrooms; ++i)
			{
				Node objectNode = scene.CreateChild("Mushroom");
				Vector3 position = new Vector3(NextRandom(2000.0f) - 1000.0f, 0.0f, NextRandom(2000.0f) - 1000.0f);
				position.Y = terrain.GetHeight(position) - 0.1f;
				objectNode.Position = (position);
				// Create a rotation quaternion from up vector to terrain normal
				objectNode.Rotation = Quaternion.FromRotationTo(Vector3.UnitY, terrain.GetNormal(position));
				objectNode.SetScale(3.0f);
				StaticModel sm = objectNode.CreateComponent<StaticModel>();
				sm.Model = (cache.GetModel("Models/Mushroom.mdl"));
				sm.SetMaterial(cache.GetMaterial("Materials/Mushroom.xml"));
				sm.CastShadows = true;

				body = objectNode.CreateComponent<RigidBody>();
				body.CollisionLayer = 2;
				shape = objectNode.CreateComponent<CollisionShape>();
				shape.SetTriangleMesh(sm.Model, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);
			}
		}
	}
}
