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
using Urho.Gui;

namespace Urho.Samples
{
	public class LightAnimation : Sample
	{
		Scene scene;

		public LightAnimation(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			SetupInstructions();
			CreateScene();
			SetupViewport();
		}

		void SetupInstructions()
		{
			var instructions = new Text()
			{
				Value = "Use WASD keys and mouse/touch to move",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			var font = ResourceCache.GetFont("Fonts/Anonymous Pro.ttf");
			instructions.SetFont(font, 15);
			UI.Root.AddChild(instructions);

			// Animating text
			Text text = new Text();
			text.Name = "animatingText";
			text.SetFont(font, 15);
			text.HorizontalAlignment = HorizontalAlignment.Center;
			text.VerticalAlignment = VerticalAlignment.Center;
			text.SetPosition(0, UI.Root.Height/4 + 20);
			UI.Root.AddChild(text);
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera3D(timeStep);
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

			// Create the Octree component to the scene. This is required before adding any drawable components, or else nothing will
			// show up. The default octree volume will be from (-1000, -1000, -1000) to (1000, 1000, 1000) in world coordinates; it
			// is also legal to place objects outside the volume but their visibility can then not be checked in a hierarchically
			// optimizing manner
			scene.CreateComponent<Octree>();

			// Create a child scene node (at world origin) and a StaticModel component into it. Set the StaticModel to show a simple
			// plane mesh with a "stone" material. Note that naming the scene nodes is optional. Scale the scene node larger
			// (100 x 100 world units)
			Node planeNode = scene.CreateChild("Plane");
			planeNode.Scale=new Vector3(100.0f, 1.0f, 100.0f);
			StaticModel planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = (cache.GetModel("Models/Plane.mdl"));
			planeObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

			// Create a point light to the world so that we can see something. 
			Node lightNode = scene.CreateChild("PointLight");
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = (10.0f);

			// Create light animation
			ObjectAnimation lightAnimation=new ObjectAnimation();

			// Create light position animation
			ValueAnimation positionAnimation=new ValueAnimation();
			// Use spline interpolation method
			positionAnimation.InterpolationMethod= InterpMethod.Spline;
			// Set spline tension
			positionAnimation.SplineTension=0.7f;

			positionAnimation.SetKeyFrame(0.0f, new Vector3(-30.0f, 5.0f, -30.0f));
			positionAnimation.SetKeyFrame(1.0f, new Vector3(30.0f, 5.0f, -30.0f));
			positionAnimation.SetKeyFrame(2.0f, new Vector3(30.0f, 5.0f, 30.0f));
			positionAnimation.SetKeyFrame(3.0f, new Vector3(-30.0f, 5.0f, 30.0f));
			positionAnimation.SetKeyFrame(4.0f, new Vector3(-30.0f, 5.0f, -30.0f));
			// Set position animation
			lightAnimation.AddAttributeAnimation("Position", positionAnimation, WrapMode.Loop, 1f);

			// Create text animation
			ValueAnimation textAnimation=new ValueAnimation();
			textAnimation.SetKeyFrame(0.0f, "WHITE");
			textAnimation.SetKeyFrame(1.0f, "RED");
			textAnimation.SetKeyFrame(2.0f, "YELLOW");
			textAnimation.SetKeyFrame(3.0f, "GREEN");
			textAnimation.SetKeyFrame(4.0f, "WHITE");
			var uiElement = UI.Root.GetChild("animatingText", false);
			uiElement.SetAttributeAnimation("Text", textAnimation, WrapMode.Loop, 1f);

			// Create light color animation
			ValueAnimation colorAnimation=new ValueAnimation();
			colorAnimation.SetKeyFrame(0.0f, Color.White);
			colorAnimation.SetKeyFrame(1.0f, Color.Red);
			colorAnimation.SetKeyFrame(2.0f, Color.Yellow);
			colorAnimation.SetKeyFrame(3.0f, Color.Green);
			colorAnimation.SetKeyFrame(4.0f, Color.White);
			// Set Light component's color animation
			lightAnimation.AddAttributeAnimation("@Light/Color", colorAnimation, WrapMode.Loop, 1f);

			// Apply light animation to light node
			lightNode.ObjectAnimation=lightAnimation;

			// Create more StaticModel objects to the scene, randomly positioned, rotated and scaled. For rotation, we construct a
			// quaternion from Euler angles where the Y angle (rotation about the Y axis) is randomized. The mushroom model contains
			// LOD levels, so the StaticModel component will automatically select the LOD level according to the view distance (you'll
			// see the model get simpler as it moves further away). Finally, rendering a large number of the same object with the
			// same material allows instancing to be used, if the GPU supports it. This reduces the amount of CPU work in rendering the
			// scene.
			const uint numObjects = 200;
			for (uint i = 0; i < numObjects; ++i)
			{
				Node mushroomNode = scene.CreateChild("Mushroom");
				mushroomNode.Position = (new Vector3(NextRandom(90.0f) - 45.0f, 0.0f, NextRandom(90.0f) - 45.0f));
				mushroomNode.Rotation=new Quaternion(0.0f, NextRandom(360.0f), 0.0f);
				mushroomNode.SetScale(0.5f + NextRandom(2.0f));
				StaticModel mushroomObject = mushroomNode.CreateComponent<StaticModel>();
				mushroomObject.Model = (cache.GetModel("Models/Mushroom.mdl"));
				mushroomObject.SetMaterial(cache.GetMaterial("Materials/Mushroom.xml"));
			}

			// Create a scene node for the camera, which we will move around
			// The camera will use default settings (1000 far clip distance, 45 degrees FOV, set aspect ratio automatically)
			CameraNode = scene.CreateChild("Camera");
			CameraNode.CreateComponent<Camera>();

			// Set an initial position for the camera scene node above the plane
			CameraNode.Position = (new Vector3(0.0f, 5.0f, 0.0f));
		}
	}
}
