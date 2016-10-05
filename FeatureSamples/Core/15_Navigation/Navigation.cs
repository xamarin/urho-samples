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

using System.Collections.Generic;
using System.Linq;
using Urho.Resources;
using Urho.Gui;
using Urho.Navigation;

namespace Urho.Samples
{
	public class Navigation : Sample
	{
		Scene scene;
		float yaw;
		float pitch;
		bool drawDebug;
		Node jackNode;
		Vector3 endPos;
		List<Vector3> currentPath = new List<Vector3>();

		public Navigation(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			CreateUI();
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

					if (currentPath.Count > 0)
					{
						// Visualize the current calculated path
						DebugRenderer debug = scene.GetComponent<DebugRenderer>();
						debug.AddBoundingBox(new BoundingBox(endPos - new Vector3(0.1f, 0.1f, 0.1f), endPos + new Vector3(0.1f, 0.1f, 0.1f)),
							new Color(1.0f, 1.0f, 1.0f), true);

						// Draw the path with a small upward bias so that it does not clip into the surfaces
						Vector3 bias = new Vector3(0.0f, 0.05f, 0.0f);
						debug.AddLine(jackNode.Position + bias, currentPath[0] + bias, new Color(1.0f, 1.0f, 1.0f), true);

						if (currentPath.Count > 1)
						{
							for (int i = 0; i < currentPath.Count - 1; ++i)
								debug.AddLine(currentPath[i] + bias, currentPath[i + 1] + bias, new Color(1.0f, 1.0f, 1.0f), true);
						}
					}
				});
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			MoveCamera(timeStep);
			FollowPath(timeStep);
		}

		void MoveCamera(float timeStep)
		{
			// Right mouse button controls mouse cursor visibility: hide when pressed
			UI.Cursor.Visible = !Input.GetMouseButtonDown(MouseButton.Right);

			// Do not move if the UI has a focused element (the console)
			if (UI.FocusElement != null)
				return;

			// Movement speed as world units per second
			const float moveSpeed = 20.0f;
			// Mouse sensitivity as degrees per pixel
			const float mouseSensitivity = 0.1f;

			// Use this frame's mouse motion to adjust camera node yaw and pitch. Clamp the pitch between -90 and 90 degrees
			// Only move the camera when the cursor is hidden
			if (!UI.Cursor.Visible)
			{
				IntVector2 mouseMove = Input.MouseMove;
				yaw += mouseSensitivity * mouseMove.X;
				pitch += mouseSensitivity * mouseMove.Y;
				pitch = MathHelper.Clamp(pitch, -90.0f, 90.0f);

				// Construct new orientation for the camera scene node from yaw and pitch. Roll is fixed to zero
				CameraNode.Rotation=new Quaternion(pitch, yaw, 0.0f);
			}

			// Read WASD keys and move the camera scene node to the corresponding direction if they are pressed
			if (Input.GetKeyDown(Key.W))
				CameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.S))
				CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.A))
				CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.D))
				CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);

			// Set destination or teleport with left mouse button
			if (Input.GetMouseButtonPress(MouseButton.Left))
				SetPathPoint();
			// Add or remove objects with middle mouse button, then rebuild navigation mesh partially
			if (Input.GetMouseButtonPress(MouseButton.Middle))
				AddOrRemoveObject();

			// Toggle debug geometry with space
			if (Input.GetKeyPress(Key.Space))
				drawDebug = !drawDebug;
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void CreateUI()
		{
			var cache = ResourceCache;
			
			// Create a Cursor UI element because we want to be able to hide and show it at will. When hidden, the mouse cursor will
			// control the camera, and when visible, it will point the raycast target
			XmlFile style = cache.GetXmlFile("UI/DefaultStyle.xml");
			Cursor cursor=new Cursor();
			cursor.SetStyleAuto(style);
			UI.Cursor=cursor;

			// Set starting position of the cursor at the rendering window center
			var graphics = Graphics;
			cursor.SetPosition(graphics.Width / 2, graphics.Height / 2);

			// Construct new Text object, set string to display and font to use
			var instructionText = new Text();
			instructionText.Value =
				"Use WASD keys to move, RMB to rotate view\n" +
				"LMB to set destination, SHIFT+LMB to teleport\n" +
				"MMB to add or remove obstacles\n" +
				"Space to toggle debug geometry";

			instructionText.SetFont(cache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
			// The text has multiple rows. Center them in relation to each other
			instructionText.TextAlignment= HorizontalAlignment.Center;

			// Position the text relative to the screen center
			instructionText.HorizontalAlignment = HorizontalAlignment.Center;
			instructionText.VerticalAlignment = VerticalAlignment.Center;
			instructionText.SetPosition(0, UI.Root.Height / 4);
			UI.Root.AddChild(instructionText);
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
			planeNode.Scale=new Vector3(100.0f, 1.0f, 100.0f);
			StaticModel planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Plane.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

			// Create a Zone component for ambient lighting & fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor=new Color(0.15f, 0.15f, 0.15f);
			zone.FogColor = new Color(0.5f, 0.5f, 0.7f);
			zone.FogStart = 100.0f;
			zone.FogEnd = 300.0f;

			// Create a directional light to the world. Enable cascaded shadows on it
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;
			light.CastShadows=true;
			light.ShadowBias=new BiasParameters(0.00025f, 0.5f);
			// Set cascade splits at 10, 50 and 200 world units, fade shadows out at 80% of maximum shadow distance
			light.ShadowCascade=new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);

			// Create some mushrooms
			const uint numMushrooms = 100;
			for (uint i = 0; i < numMushrooms; ++i)
				CreateMushroom(new Vector3(NextRandom(90.0f) - 45.0f, 0.0f, NextRandom(90.0f) - 45.0f));

			// Create randomly sized boxes. If boxes are big enough, make them occluders
			const uint numBoxes = 20;
			for (uint i = 0; i < numBoxes; ++i)
			{
				Node boxNode = scene.CreateChild("Box");
				float size = 1.0f + NextRandom(10.0f);
				boxNode.Position = new Vector3(NextRandom(80.0f) - 40.0f, size * 0.5f, NextRandom(80.0f) - 40.0f);
				boxNode.SetScale(size);
				StaticModel boxObject = boxNode.CreateComponent<StaticModel>();
				boxObject.Model = cache.GetModel("Models/Box.mdl");
				boxObject.SetMaterial(cache.GetMaterial("Materials/Stone.xml"));
				boxObject.CastShadows = true;
				if (size >= 3.0f)
					boxObject.Occluder = true;
			}

			// Create Jack node that will follow the path
			jackNode = scene.CreateChild("Jack");
			jackNode.Position = new Vector3(-5.0f, 0.0f, 20.0f);
			AnimatedModel modelObject = jackNode.CreateComponent<AnimatedModel>();
			modelObject.Model = cache.GetModel("Models/Jack.mdl");
			modelObject.SetMaterial(cache.GetMaterial("Materials/Jack.xml"));
			modelObject.CastShadows=true;

			// Create a NavigationMesh component to the scene root
			NavigationMesh navMesh = scene.CreateComponent<NavigationMesh>();
			// Create a Navigable component to the scene root. This tags all of the geometry in the scene as being part of the
			// navigation mesh. By default this is recursive, but the recursion could be turned off from Navigable
			scene.CreateComponent<Navigable>();
			// Add padding to the navigation mesh in Y-direction so that we can add objects on top of the tallest boxes
			// in the scene and still update the mesh correctly
			navMesh.Padding=new Vector3(0.0f, 10.0f, 0.0f);
			// Now build the navigation geometry. This will take some time. Note that the navigation mesh will prefer to use
			// physics geometry from the scene nodes, as it often is simpler, but if it can not find any (like in this example)
			// it will use renderable geometry instead
			navMesh.Build();

			// Create the camera. Limit far clip distance to match the fog
			CameraNode = scene.CreateChild("Camera");
			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;

			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 5.0f, 0.0f);
		}

		void SetPathPoint()
		{
			Vector3 hitPos;
			Drawable hitDrawable;
			NavigationMesh navMesh = scene.GetComponent<NavigationMesh>();

			if (Raycast(250.0f, out hitPos, out hitDrawable))
			{
				Vector3 pathPos = navMesh.FindNearestPoint(hitPos, new Vector3(1.0f, 1.0f, 1.0f));

				const int qualShift = 1;
				if (Input.GetQualifierDown(qualShift))
				{
					// Teleport
					currentPath.Clear();
					jackNode.LookAt(new Vector3(pathPos.X, jackNode.Position.Y, pathPos.Z), Vector3.UnitY, TransformSpace.World);
					jackNode.Position = (pathPos);
				}
				else
				{
					// Calculate path from Jack's current position to the end point
					endPos = pathPos;
					var result = navMesh.FindPath(jackNode.Position, endPos);
					currentPath = new List<Vector3>(result);
				}
			}
		}

		void AddOrRemoveObject()
		{
			// Raycast and check if we hit a mushroom node. If yes, remove it, if no, create a new one
			Vector3 hitPos;
			Drawable hitDrawable;

			if (Raycast(250.0f, out hitPos, out hitDrawable))
			{
				// The part of the navigation mesh we must update, which is the world bounding box of the associated
				// drawable component
				BoundingBox updateBox;

				Node hitNode = hitDrawable.Node;
				if (hitNode.Name == "Mushroom")
				{
					updateBox = hitDrawable.WorldBoundingBox;
					hitNode.Remove();
				}
				else
				{
					Node newNode = CreateMushroom(hitPos);
					updateBox = newNode.GetComponent<StaticModel>().WorldBoundingBox;
				}

				// Rebuild part of the navigation mesh, then recalculate path if applicable
				NavigationMesh navMesh = scene.GetComponent<NavigationMesh>();
				navMesh.Build(updateBox);
				if (currentPath.Count > 0)
					currentPath = new List<Vector3>(navMesh.FindPath(jackNode.Position, endPos));
			}
		}

		Node CreateMushroom(Vector3 pos)
		{
			var cache = ResourceCache;

			Node mushroomNode = scene.CreateChild("Mushroom");
			mushroomNode.Position=pos;
			mushroomNode.Rotation=new Quaternion(0.0f, NextRandom(360.0f), 0.0f);
			mushroomNode.SetScale(2.0f + NextRandom(0.5f));
			StaticModel mushroomObject = mushroomNode.CreateComponent<StaticModel>();
			mushroomObject.Model=(cache.GetModel("Models/Mushroom.mdl"));
			mushroomObject.SetMaterial(cache.GetMaterial("Materials/Mushroom.xml"));
			mushroomObject.CastShadows=true;
	
			return mushroomNode;
		}

		bool Raycast(float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
		{
			hitDrawable = null;
			hitPos = new Vector3();
			
			IntVector2 pos = UI.CursorPosition;
			// Check the cursor is visible and there is no UI element in front of the cursor
			if (!UI.Cursor.Visible || UI.GetElementAt(pos, true) != null)
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


		void FollowPath(float timeStep)
		{
			if (currentPath.Count > 0)
			{
				Vector3 nextWaypoint = currentPath[0]; // NB: currentPath[0] is the next waypoint in order

				// Rotate Jack toward next waypoint to reach and move. Check for not overshooting the target
				float move = 5.0f * timeStep;
				float distance = (jackNode.Position - nextWaypoint).Length;
				if (move > distance)
					move = distance;

				jackNode.LookAt(nextWaypoint, Vector3.UnitY, TransformSpace.World);
				jackNode.Translate(Vector3.UnitZ * move, TransformSpace.Local);

				// Remove waypoint if reached it
				if (distance < 0.1f)
					currentPath.RemoveAt(0);
			}
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
