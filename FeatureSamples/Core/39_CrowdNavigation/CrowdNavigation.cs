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

using System.Linq;
using Urho.Navigation;
using Urho.Resources;
using Urho.Gui;

namespace Urho.Samples
{
	public class CrowdNavigation : Sample
	{
		Scene scene;
		bool drawDebug;
		CrowdManager crowdManager;

		public CrowdNavigation(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			CreateUI();
			SetupViewport();
			SubscribeToEvents();
		}

		void CreateUI()
		{
			var cache = ResourceCache;

			// Create a Cursor UI element because we want to be able to hide and show it at will. When hidden, the mouse cursor will
			// control the camera, and when visible, it will point the raycast target
			XmlFile style = cache.GetXmlFile("UI/DefaultStyle.xml");
			Cursor cursor = new Cursor();
			cursor.SetStyleAuto(style);
			UI.Cursor = cursor;

			// Set starting position of the cursor at the rendering window center
			var graphics = Graphics;
			cursor.SetPosition(graphics.Width / 2, graphics.Height / 2);

			// Construct new Text object, set string to display and font to use
			var instructionText = new Text();

			instructionText.Value =
				"Use WASD keys to move, RMB to rotate view\n" +
				"LMB to set destination, SHIFT+LMB to spawn a Jack\n" +
				"CTRL+LMB to teleport main agent\n" +
				"MMB to add obstacles or remove obstacles/agents\n" +
				"F5 to save scene, F7 to load\n" +
				"Space to toggle debug geometry";

			instructionText.SetFont(cache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
			// The text has multiple rows. Center them in relation to each other
			instructionText.TextAlignment = HorizontalAlignment.Center;

			// Position the text relative to the screen center
			instructionText.HorizontalAlignment = HorizontalAlignment.Center;
			instructionText.VerticalAlignment = VerticalAlignment.Center;
			instructionText.SetPosition(0, UI.Root.Height / 4);
			UI.Root.AddChild(instructionText);
		}

		protected override void OnUpdate(float timeStep)
		{
			MoveCamera(timeStep);
			base.OnUpdate(timeStep);
		}

		void SubscribeToEvents()
		{
			Engine.SubscribeToPostRenderUpdate(args =>
				{
					if (drawDebug)
					{
						// Visualize navigation mesh, obstacles and off-mesh connections
						scene.GetComponent<DynamicNavigationMesh>().DrawDebugGeometry(true);
						// Visualize agents' path and position to reach
						crowdManager.DrawDebugGeometry(true);
					}
				});

			crowdManager.SubscribeToCrowdAgentFailure(args =>
				{
					Node node = args.Node;
					CrowdAgentState agentState = (CrowdAgentState)args.CrowdAgentState;

					// If the agent's state is invalid, likely from spawning on the side of a box, find a point in a larger area
					if (agentState == CrowdAgentState.StateInvalid)
					{
						// Get a point on the navmesh using more generous extents
						Vector3 newPos = scene.GetComponent<DynamicNavigationMesh>().FindNearestPoint(node.Position, new Vector3(5.0f, 5.0f, 5.0f));
						// Set the new node position, CrowdAgent component will automatically reset the state of the agent
						node.Position = newPos;
					}
				});

			crowdManager.SubscribeToCrowdAgentReposition(args =>
				{
					string WALKING_ANI = "Models/Jack_Walk.ani";

					Node node = args.Node;
					Vector3 velocity = args.Velocity;

					// Only Jack agent has animation controller
					AnimationController animCtrl = node.GetComponent<AnimationController>();
					if (animCtrl != null)
					{
						float speed = velocity.Length;
						if (animCtrl.IsPlaying(WALKING_ANI))
						{
							float speedRatio = speed / args.CrowdAgent.MaxSpeed;
							// Face the direction of its velocity but moderate the turning speed based on the speed ratio as we do not have timeStep here
							node.Rotation = Quaternion.Slerp(node.Rotation, Quaternion.FromRotationTo(Vector3.UnitZ, velocity), 10f * args.TimeStep * speedRatio);
							// Throttle the animation speed based on agent speed ratio (ratio = 1 is full throttle)
							animCtrl.SetSpeed(WALKING_ANI, speedRatio);
						}
						else
							animCtrl.Play(WALKING_ANI, 0, true, 0.1f);

						// If speed is too low then stopping the animation
						if (speed < args.CrowdAgent.Radius)
							animCtrl.Stop(WALKING_ANI, 0.8f);
					}
				});
		}

		void MoveCamera(float timeStep)
		{
			// Right mouse button controls mouse cursor visibility: hide when pressed
			UI ui = UI;
			Input input = Input;
			ui.Cursor.Visible = !input.GetMouseButtonDown(MouseButton.Right);

			// Do not move if the UI has a focused element (the console)
			if (ui.FocusElement != null)
				return;

			// Movement speed as world units per second
			const float moveSpeed = 20.0f;
			// Mouse sensitivity as degrees per pixel
			const float mouseSensitivity = 0.1f;

			// Use this frame's mouse motion to adjust camera node yaw and pitch. Clamp the pitch between -90 and 90 degrees
			// Only move the camera when the cursor is hidden
			if (!ui.Cursor.Visible)
			{
				IntVector2 mouseMove = input.MouseMove;
				Yaw += mouseSensitivity * mouseMove.X;
				Pitch += mouseSensitivity * mouseMove.Y;
				Pitch = MathHelper.Clamp(Pitch, -90.0f, 90.0f);

				// Construct new orientation for the camera scene node from yaw and pitch. Roll is fixed to zero
				CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0.0f);
			}

			// Read WASD keys and move the camera scene node to the corresponding direction if they are pressed
			if (input.GetKeyDown(Key.W)) CameraNode.Translate( Vector3.UnitZ * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.D)) CameraNode.Translate( Vector3.UnitX * moveSpeed * timeStep);

			const int qualShift = 1;

			// Set destination or spawn a new jack with left mouse button
			if (input.GetMouseButtonPress(MouseButton.Left))
				SetPathPoint(input.GetQualifierDown(qualShift));
			// Add or remove objects with middle mouse button, then rebuild navigation mesh partially
			if (input.GetMouseButtonPress(MouseButton.Middle))
				AddOrRemoveObject();

			// Check for loading/saving the scene. Save the scene to the file Data/Scenes/CrowdNavigation.xml relative to the executable
			// directory
			if (input.GetKeyPress(Key.F5))
				scene.SaveXml(FileSystem.ProgramDir + "Data/Scenes/CrowdNavigation.xml");

			if (input.GetKeyPress(Key.F7))
				scene.LoadXml(FileSystem.ProgramDir + "Data/Scenes/CrowdNavigation.xml");

			// Toggle debug geometry with space
			if (input.GetKeyPress(Key.Space))
				drawDebug = !drawDebug;
		}

		bool Raycast(float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
		{
			hitDrawable = null;
			hitPos = Vector3.Zero;

			UI ui = UI;
			IntVector2 pos = ui.CursorPosition;
			// Check the cursor is visible and there is no UI element in front of the cursor
			if (!ui.Cursor.Visible || ui.GetElementAt(pos, true) != null)
				return false;

			var graphics = Graphics;
			Camera camera = CameraNode.GetComponent<Camera>();
			Ray cameraRay = camera.GetScreenRay((float)pos.X / graphics.Width, (float)pos.Y / graphics.Height);
			// Pick only geometry objects, not eg. zones or lights, only get the first (closest) hit

			var result = scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, maxDistance, DrawableFlags.Geometry, uint.MaxValue);
			if (result != null)
			{
				hitPos = result.Value.Position;
				hitDrawable = result.Value.Drawable;
				return true;
			}

			return false;
		}

		void SetPathPoint(bool spawning)
		{
			Vector3 hitPos;
			Drawable hitDrawable;

			if (Raycast(250.0f, out hitPos, out hitDrawable))
			{
				DynamicNavigationMesh navMesh = scene.GetComponent<DynamicNavigationMesh>();
				Vector3 pathPos = navMesh.FindNearestPoint(hitPos, new Vector3(1.0f, 1.0f, 1.0f));
				Node jackGroup = scene.GetChild("Jacks", false);
				if (spawning)
					// Spawn a jack at the target position
					SpawnJack(pathPos, jackGroup);
				else
					// Set crowd agents target position
					scene.GetComponent<CrowdManager>().SetCrowdTarget(pathPos, jackGroup);
			}
		}

		void AddOrRemoveObject()
		{
			// Raycast and check if we hit a mushroom node. If yes, remove it, if no, create a new one
			Vector3 hitPos;
			Drawable hitDrawable;

			if (Raycast(250.0f, out hitPos, out hitDrawable))
			{
				Node hitNode = hitDrawable.Node;

				// Note that navmesh rebuild happens when the Obstacle component is removed
				if (hitNode.Name == "Mushroom")
					hitNode.Remove();
				else if (hitNode.Name == "Jack")
					hitNode.Remove();
				else
					CreateMushroom(hitPos);
			}
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
			// Also create a DebugRenderer component so that we can draw debug geometry
			scene.CreateComponent<Octree>();
			scene.CreateComponent<DebugRenderer>();

			// Create scene node & StaticModel component for showing a static plane
			Node planeNode = scene.CreateChild("Plane");
			planeNode.Scale = new Vector3(100.0f, 1.0f, 100.0f);
			StaticModel planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = (cache.GetModel("Models/Plane.mdl"));
			planeObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

			// Create a Zone component for ambient lighting & fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor = new Color(0.15f, 0.15f, 0.15f);
			zone.FogColor = new Color(0.5f, 0.5f, 0.7f);
			zone.FogStart = 100.0f;
			zone.FogEnd = 300.0f;

			// Create a directional light to the world. Enable cascaded shadows on it
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;
			light.CastShadows = true;
			light.ShadowBias = new BiasParameters(0.00025f, 0.5f);
			// Set cascade splits at 10, 50 and 200 world units, fade shadows out at 80% of maximum shadow distance
			light.ShadowCascade = new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);

			// Create randomly sized boxes. If boxes are big enough, make them occluders
			const uint numBoxes = 20;
			Node boxGroup = scene.CreateChild("Boxes");
			for (uint i = 0; i < numBoxes; ++i)
			{
				Node boxNode = boxGroup.CreateChild("Box");
				float size = 1.0f + NextRandom(10.0f);
				boxNode.Position = (new Vector3(NextRandom(80.0f) - 40.0f, size * 0.5f, NextRandom(80.0f) - 40.0f));
				boxNode.SetScale(size);
				StaticModel boxObject = boxNode.CreateComponent<StaticModel>();
				boxObject.Model = (cache.GetModel("Models/Box.mdl"));
				boxObject.SetMaterial(cache.GetMaterial("Materials/Stone.xml"));
				boxObject.CastShadows = true;
				if (size >= 3.0f)
					boxObject.Occluder = true;
			}

			// Create a DynamicNavigationMesh component to the scene root
			DynamicNavigationMesh navMesh = scene.CreateComponent<DynamicNavigationMesh>();
			// Set the agent height large enough to exclude the layers under boxes
			navMesh.AgentHeight = 10.0f;
			navMesh.CellHeight = 0.05f;
			navMesh.DrawObstacles = true;
			navMesh.DrawOffMeshConnections = true;
			// Create a Navigable component to the scene root. This tags all of the geometry in the scene as being part of the
			// navigation mesh. By default this is recursive, but the recursion could be turned off from Navigable
			scene.CreateComponent<Navigable>();
			// Add padding to the navigation mesh in Y-direction so that we can add objects on top of the tallest boxes
			// in the scene and still update the mesh correctly
			navMesh.Padding = new Vector3(0.0f, 10.0f, 0.0f);
			// Now build the navigation geometry. This will take some time. Note that the navigation mesh will prefer to use
			// physics geometry from the scene nodes, as it often is simpler, but if it can not find any (like in this example)
			// it will use renderable geometry instead
			navMesh.Build();

			// Create an off-mesh connection to each box to make them climbable (tiny boxes are skipped). A connection is built from 2 nodes.
			// Note that OffMeshConnections must be added before building the navMesh, but as we are adding Obstacles next, tiles will be automatically rebuilt.
			// Creating connections post-build here allows us to use FindNearestPoint() to procedurally set accurate positions for the connection
			CreateBoxOffMeshConnections(navMesh, boxGroup);

			// Create some mushrooms
			const uint numMushrooms = 100;
			for (uint i = 0; i < numMushrooms; ++i)
				CreateMushroom(new Vector3(NextRandom(90.0f) - 45.0f, 0.0f, NextRandom(90.0f) - 45.0f));


			// Create a CrowdManager component to the scene root
			crowdManager = scene.CreateComponent<CrowdManager>();
			var parameters = crowdManager.GetObstacleAvoidanceParams(0);
			// Set the params to "High (66)" setting
			parameters.VelBias = 0.5f;
			parameters.AdaptiveDivs = 7;
			parameters.AdaptiveRings = 3;
			parameters.AdaptiveDepth = 3;
			crowdManager.SetObstacleAvoidanceParams(0, parameters);

			// Create some movable barrels. We create them as crowd agents, as for moving entities it is less expensive and more convenient than using obstacles
			CreateMovingBarrels(navMesh);

			// Create Jack node that will follow the path
			SpawnJack(new Vector3(-5.0f, 0.0f, 20.0f), scene.CreateChild("Jacks"));

			// Create the camera. Limit far clip distance to match the fog
			CameraNode = new Node();
			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;

			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 50.0f, 0.0f);
			Pitch = 80.0f;
			CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0.0f);
		}

		void SpawnJack(Vector3 pos, Node jackGroup)
		{
			var cache = ResourceCache;
			Node jackNode = jackGroup.CreateChild("Jack");
			jackNode.Position = pos;
			AnimatedModel modelObject = jackNode.CreateComponent<AnimatedModel>();
			modelObject.Model = (cache.GetModel("Models/Jack.mdl"));
			modelObject.SetMaterial(cache.GetMaterial("Materials/Jack.xml"));
			modelObject.CastShadows = true;
			jackNode.CreateComponent<AnimationController>();

			// Create the CrowdAgent
			var agent = jackNode.CreateComponent<CrowdAgent>();
			agent.Height = 2.0f;
			agent.MaxSpeed = 3.0f;
			agent.MaxAccel = 3.0f;
		}

		void CreateMushroom(Vector3 pos)
		{
			var cache = ResourceCache;

			Node mushroomNode = scene.CreateChild("Mushroom");
			mushroomNode.Position = (pos);
			mushroomNode.Rotation = new Quaternion(0.0f, NextRandom(360.0f), 0.0f);
			mushroomNode.SetScale(2.0f + NextRandom(0.5f));
			StaticModel mushroomObject = mushroomNode.CreateComponent<StaticModel>();
			mushroomObject.Model = (cache.GetModel("Models/Mushroom.mdl"));
			mushroomObject.SetMaterial(cache.GetMaterial("Materials/Mushroom.xml"));
			mushroomObject.CastShadows = true;
			// Create the navigation obstacle
			Obstacle obstacle = mushroomNode.CreateComponent<Obstacle>();
			obstacle.Radius = mushroomNode.Scale.X;
			obstacle.Height = mushroomNode.Scale.Y;
		}

		void CreateBoxOffMeshConnections(DynamicNavigationMesh navMesh, Node boxGroup)
		{
			foreach (var box in boxGroup.Children.ToArray())
			{
				var boxPos = box.Position;
				float boxHalfSize = box.Scale.X / 2;
				var connectionStart = box.CreateChild("ConnectionStart");
				connectionStart.SetWorldPosition(navMesh.FindNearestPoint(boxPos + new Vector3(boxHalfSize, -boxHalfSize, 0), Vector3.One));
				var connectionEnd = box.CreateChild("ConnectionEnd");
				connectionEnd.SetWorldPosition(navMesh.FindNearestPoint(boxPos + new Vector3(boxHalfSize, boxHalfSize, 0), Vector3.One));

				OffMeshConnection connection = connectionStart.CreateComponent<OffMeshConnection>();
				connection.EndPoint = connectionEnd;
			}
		}

		void CreateMovingBarrels(DynamicNavigationMesh navMesh)
		{
			ResourceCache cache = ResourceCache;
			Node barrel = scene.CreateChild("Barrel");
			StaticModel model = barrel.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Cylinder.mdl");
			Material material = cache.GetMaterial("Materials/StoneTiled.xml");
			model.SetMaterial(material);
			material.SetTexture(TextureUnit.Diffuse, cache.GetTexture2D("Textures/TerrainDetail2.dds"));
			model.CastShadows = true;
			for (int i = 0; i < 20; ++i)
			{
				Node clone = barrel.Clone(CreateMode.Replicated);
				float size = 0.5f + NextRandom(1.0f);
				clone.Scale = new Vector3(size / 1.5f, size * 2.0f, size / 1.5f);
				clone.Position = navMesh.FindNearestPoint(new Vector3(NextRandom(80.0f) - 40.0f, size * 0.5f, NextRandom(80.0f) - 40.0f), Vector3.One);
				CrowdAgent agent = clone.CreateComponent<CrowdAgent>();
				agent.Radius = clone.Scale.X * 0.5f;
				agent.Height = size;
				agent.NavigationQuality = NavigationQuality.Low;
			}
			barrel.Remove();
		}

		/// <summary>
		/// Set custom Joystick layout for mobile platforms
		/// </summary>
		protected override string JoystickLayoutPatch =>
			"<patch>" +
			"    <add sel=\"/element\">" +
			"        <element type=\"Button\">" +
			"            <attribute name=\"Name\" value=\"Button3\" />" +
			"            <attribute name=\"Position\" value=\"-120 -120\" />" +
			"            <attribute name=\"Size\" value=\"96 96\" />" +
			"            <attribute name=\"Horiz Alignment\" value=\"Right\" />" +
			"            <attribute name=\"Vert Alignment\" value=\"Bottom\" />" +
			"            <attribute name=\"Texture\" value=\"Texture2D;Textures/TouchInput.png\" />" +
			"            <attribute name=\"Image Rect\" value=\"96 0 192 96\" />" +
			"            <attribute name=\"Hover Image Offset\" value=\"0 0\" />" +
			"            <attribute name=\"Pressed Image Offset\" value=\"0 0\" />" +
			"            <element type=\"Text\">" +
			"                <attribute name=\"Name\" value=\"Label\" />" +
			"                <attribute name=\"Horiz Alignment\" value=\"Center\" />" +
			"                <attribute name=\"Vert Alignment\" value=\"Center\" />" +
			"                <attribute name=\"Color\" value=\"0 0 0 1\" />" +
			"                <attribute name=\"Text\" value=\"Teleport\" />" +
			"            </element>" +
			"            <element type=\"Text\">" +
			"                <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"                <attribute name=\"Text\" value=\"LSHIFT\" />" +
			"            </element>" +
			"            <element type=\"Text\">" +
			"                <attribute name=\"Name\" value=\"MouseButtonBinding\" />" +
			"                <attribute name=\"Text\" value=\"LEFT\" />" +
			"            </element>" +
			"        </element>" +
			"        <element type=\"Button\">" +
			"            <attribute name=\"Name\" value=\"Button4\" />" +
			"            <attribute name=\"Position\" value=\"-120 -12\" />" +
			"            <attribute name=\"Size\" value=\"96 96\" />" +
			"            <attribute name=\"Horiz Alignment\" value=\"Right\" />" +
			"            <attribute name=\"Vert Alignment\" value=\"Bottom\" />" +
			"            <attribute name=\"Texture\" value=\"Texture2D;Textures/TouchInput.png\" />" +
			"            <attribute name=\"Image Rect\" value=\"96 0 192 96\" />" +
			"            <attribute name=\"Hover Image Offset\" value=\"0 0\" />" +
			"            <attribute name=\"Pressed Image Offset\" value=\"0 0\" />" +
			"            <element type=\"Text\">" +
			"                <attribute name=\"Name\" value=\"Label\" />" +
			"                <attribute name=\"Horiz Alignment\" value=\"Center\" />" +
			"                <attribute name=\"Vert Alignment\" value=\"Center\" />" +
			"                <attribute name=\"Color\" value=\"0 0 0 1\" />" +
			"                <attribute name=\"Text\" value=\"Obstacles\" />" +
			"            </element>" +
			"            <element type=\"Text\">" +
			"                <attribute name=\"Name\" value=\"MouseButtonBinding\" />" +
			"                <attribute name=\"Text\" value=\"MIDDLE\" />" +
			"            </element>" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Set</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"MouseButtonBinding\" />" +
			"            <attribute name=\"Text\" value=\"LEFT\" />" +
			"        </element>" +
			"    </add>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Debug</replace>" +
			"    <add sel=\"/element/element[./attribute[@name='Name' and @value='Button1']]\">" +
			"        <element type=\"Text\">" +
			"            <attribute name=\"Name\" value=\"KeyBinding\" />" +
			"            <attribute name=\"Text\" value=\"SPACE\" />" +
			"        </element>" +
			"    </add>" +
			"</patch>";
	}
}
