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
using Urho.Resources;
using Urho.Gui;

namespace Urho.Samples
{
	public class SceneAndUILoad : Sample
	{
		Scene scene;

		public SceneAndUILoad(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();

			// Create the scene content
			CreateScene();

			// Create the UI content
			CreateUI();

			// Setup the viewport for displaying the scene
			SetupViewport();
		}

		void CreateScene()
		{
			var cache = ResourceCache;

			scene = new Scene();

			// Load scene content prepared in the editor (XML format). GetFile() returns an open file from the resource system
			// which scene.LoadXML() will read
			scene.LoadXmlFromCache(cache, "Scenes/SceneLoadExample.xml");

			// Create the camera (not included in the scene file)
			CameraNode = scene.CreateChild("Camera");
			CameraNode.CreateComponent<Camera>();

			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = new Vector3(0.0f, 2.0f, -10.0f);
		}

		void CreateUI()
		{
			var cache = ResourceCache;

			// Set up global UI style into the root UI element
			XmlFile style = cache.GetXmlFile("UI/DefaultStyle.xml");
			UI.Root.SetDefaultStyle(style);

			// Create a Cursor UI element because we want to be able to hide and show it at will. When hidden, the mouse cursor will
			// control the camera, and when visible, it will interact with the UI
			Cursor cursor=new Cursor();
			cursor.SetStyleAuto(null);
			UI.Cursor=cursor;
			// Set starting position of the cursor at the rendering window center
			var graphics = Graphics;
			cursor.SetPosition(graphics.Width / 2, graphics.Height / 2);

			// Load UI content prepared in the editor and add to the UI hierarchy
			UI.LoadLayoutToElement(UI.Root, cache, "UI/UILoadExample.xml");

			// Subscribe to button actions (toggle scene lights when pressed then released)
			var button1 = (Button) UI.Root.GetChild("ToggleLight1", true);
			var button2 = (Button) UI.Root.GetChild("ToggleLight2", true);

			button1.SubscribeToReleased (args => ToggleLight1 ());
			button2.SubscribeToReleased (args => ToggleLight2 ());
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			SimpleMoveCamera2D(timeStep);
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void ToggleLight1()
		{
			Node lightNode = scene.GetChild("Light1", true);
			if (lightNode != null)
				lightNode.Enabled = !lightNode.Enabled;
		}

		void ToggleLight2()
		{
			Node lightNode = scene.GetChild("Light2", true);
			if (lightNode != null)
				lightNode.Enabled = !lightNode.Enabled;
		}
	}
}
