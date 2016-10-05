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
using Urho.Gui;

namespace Urho.Samples
{
	public class Decals : Sample
	{
		Scene scene;
		bool drawDebug;
		Camera camera;

		public Decals(ApplicationOptions options = null) : base(options) { }

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
			var ui = UI;
			var graphics = Graphics;

			var style = cache.GetXmlFile("UI/DefaultStyle.xml");
			var cursor = new Cursor();
			cursor.SetStyleAuto(style);
			ui.Cursor = cursor;
			cursor.SetPosition(graphics.Width / 2, graphics.Height / 2);

			SimpleCreateInstructionsWithWasd(
				"\nLMB to paint decals, RMB to rotate view\n" +
				"Space to toggle debug geometry\n" +
				"7 to toggle occlusion culling");
		}

		void SubscribeToEvents()
		{
			Engine.SubscribeToPostRenderUpdate(args =>
				{
					// If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
					// bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
					// bones properly
					if (drawDebug)
						Renderer.DrawDebugGeometry(drawDebug);
				});
		}

		protected override void OnUpdate(float timeStep)
		{
			UI ui = UI;
			var input = Input;
			ui.Cursor.Visible = !input.GetMouseButtonDown(MouseButton.Right);

			const float mouseSensitivity = .1f;
			const float moveSpeed = 40f;

			if (UI.FocusElement != null)
				return;

			if (!ui.Cursor.Visible)
			{
				var mouseMove = input.MouseMove;
				Yaw += mouseSensitivity * mouseMove.X;
				Pitch += mouseSensitivity * mouseMove.Y;
				Pitch = MathHelper.Clamp(Pitch, -90, 90);
			}

			CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);

			if (input.GetKeyDown(Key.W))
				CameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.S))
				CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.A))
				CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
			if (input.GetKeyDown(Key.D))
				CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);

			if (Input.GetKeyPress(Key.Space))
				drawDebug = !drawDebug;

			if (UI.Cursor.Visible && Input.GetMouseButtonPress(MouseButton.Left))
				PaintDecal();
			base.OnUpdate(timeStep);
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
			var planeNode = scene.CreateChild("Plane");
			planeNode.Scale = new Vector3(100.0f, 1.0f, 100.0f);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Plane.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

			// Create a Zone component for ambient lighting & fog control
			var zoneNode = scene.CreateChild("Zone");
			var zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.AmbientColor=new Color(0.15f, 0.15f, 0.15f);
			zone.FogColor = new Color(0.5f, 0.5f, 0.7f);
			zone.FogStart=100.0f;
			zone.FogEnd=300.0f;

			// Create a directional light to the world. Enable cascaded shadows on it
			var lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
			var light = lightNode.CreateComponent<Light>();
			light.LightType= LightType.Directional;
			light.CastShadows=true;
			light.ShadowBias=new BiasParameters(0.00025f, 0.5f);
			// Set cascade splits at 10, 50 and 200 world units, fade shadows out at 80% of maximum shadow distance
			light.ShadowCascade=new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);

			// Create some mushrooms
			const uint numMushrooms = 240;
			for (uint i = 0; i < numMushrooms; ++i)
			{
				var mushroomNode = scene.CreateChild("Mushroom");
				mushroomNode.Position=new Vector3(NextRandom(90.0f) - 45.0f, 0.0f, NextRandom(90.0f) - 45.0f);
				mushroomNode.Rotation=new Quaternion(0.0f, NextRandom(360.0f), 0.0f);
				mushroomNode.SetScale(0.5f + NextRandom(2.0f));
				var mushroomObject = mushroomNode.CreateComponent<StaticModel>();
				mushroomObject.Model=cache.GetModel("Models/Mushroom.mdl");
				mushroomObject.SetMaterial(cache.GetMaterial("Materials/Mushroom.xml"));
				mushroomObject.CastShadows=true;
			}

			// Create randomly sized boxes. If boxes are big enough, make them occluders. Occluders will be software rasterized before
			// rendering to a low-resolution depth-only buffer to test the objects in the view frustum for visibility
			const uint numBoxes = 20;
			for (uint i = 0; i < numBoxes; ++i)
			{
				var boxNode = scene.CreateChild("Box");
				float size = 1.0f + NextRandom(10.0f);
				boxNode.Position=new Vector3(NextRandom(80.0f) - 40.0f, size * 0.5f, NextRandom(80.0f) - 40.0f);
				boxNode.SetScale(size);
				var boxObject = boxNode.CreateComponent<StaticModel>();
				boxObject.Model=cache.GetModel("Models/Box.mdl");
				boxObject.SetMaterial(cache.GetMaterial("Materials/Stone.xml"));
				boxObject.CastShadows=true;
				if (size >= 3.0f)
					boxObject.Occluder = true;
			}

			// Create the camera. Limit far clip distance to match the fog
			CameraNode = scene.CreateChild("Camera");
			camera = CameraNode.CreateComponent<Camera>();
			camera.FarClip = 300.0f;
			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 5.0f, 0.0f);
		}

		bool Raycast(float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
		{
			hitDrawable = null;
			hitPos = Vector3.Zero;
		
			var graphics = Graphics;
			var ui = UI;

			IntVector2 pos = ui.CursorPosition; 
			// Check the cursor is visible and there is no UI element in front of the cursor
			if (!ui.Cursor.Visible || ui.GetElementAt(pos, true) != null)
				return false;

			Ray cameraRay = camera.GetScreenRay((float) pos.X/graphics.Width, (float) pos.Y/graphics.Height);
			var result = scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, maxDistance, DrawableFlags.Geometry, uint.MaxValue);
			if (result != null)
			{
				hitPos = result.Value.Position;
				hitDrawable = result.Value.Drawable;
				return true;
			}
			return false;
		}

		void PaintDecal()
		{
			Vector3 hitPos;
			Drawable hitDrawable;

			if (Raycast(250.0f, out hitPos, out hitDrawable))
			{
				var targetNode = hitDrawable.Node;
				var decal = targetNode.GetComponent<DecalSet>();

				if (decal == null)
				{
					var cache = ResourceCache;
					decal = targetNode.CreateComponent<DecalSet>();
					decal.Material = cache.GetMaterial("Materials/UrhoDecal.xml");
				}

				// Add a square decal to the decal set using the geometry of the drawable that was hit, orient it to face the camera,
				// use full texture UV's (0,0) to (1,1). Note that if we create several decals to a large object (such as the ground
				// plane) over a large area using just one DecalSet component, the decals will all be culled as one unit. If that is
				// undesirable, it may be necessary to create more than one DecalSet based on the distance
				decal.AddDecal(hitDrawable, hitPos, CameraNode.Rotation, 0.5f, 1.0f, 1.0f, Vector2.Zero,
					Vector2.One, 0.0f, 0.1f, uint.MaxValue);
			}
		}

		/// <summary>
		/// Set custom Joystick layout for mobile platforms
		/// </summary>
		protected override string JoystickLayoutPatch =>
			"<patch>" +
			"    <remove sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/attribute[@name='Is Visible']\" />" +
			"    <replace sel=\"/element/element[./attribute[@name='Name' and @value='Button0']]/element[./attribute[@name='Name' and @value='Label']]/attribute[@name='Text']/@value\">Paint</replace>" +
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
