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
using Urho.Actions;

namespace Urho.Samples
{
	public class Actions : Sample
	{
		Scene scene;
		Node boxNode;

		public override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructions("Use WASD to call Actions.");
		}

		protected override void OnUpdate(float timeStep)
		{
			var input = Input;

			const float duration = 2f; //2s
			FiniteTimeAction action = null;

			if (input.GetKeyPress(Key.W))
			{
				//move forward with easing
				MoveBy moveBy = new MoveBy(duration, new Vector3(0, 0, 15));
				action = new EaseBackOut(moveBy);
			}
			if (input.GetKeyPress(Key.S))
			{
				//move backward with rotation (parallel actions)
				MoveBy moveBy = new MoveBy(duration, new Vector3(0, 0, -15));
				RotateBy rotateBy = new RotateBy(2f, 0, 360, 0);
				action = new Parallel(moveBy, rotateBy);
			}
			if (input.GetKeyPress(Key.A))
			{
				//move left, increase scale
				MoveBy moveBy = new MoveBy(duration, new Vector3(-15, 0, 0));
				ScaleBy scaleBy = new ScaleBy(duration, 2f);
				action = new Parallel(moveBy, scaleBy);
			}
			if (input.GetKeyPress(Key.D))
			{
				//move right
				MoveBy moveBy = new MoveBy(duration, new Vector3(15, 0, 0));
				action = new EaseOut(moveBy, 2);
			}

			if (action != null)
			{
				//can be awaited
				boxNode.RunActionsAsync(action);
			}
			base.OnUpdate(timeStep);
		}

		void CreateScene()
		{
			var cache = ResourceCache;
			scene = new Scene(Context);

			scene.CreateComponent<Octree>();
			var planeNode = scene.CreateChild("Plane");
			planeNode.Scale = new Vector3(100, 1, 100);
			var planeObject = planeNode.CreateComponent<StaticModel>();
			planeObject.Model = cache.GetModel("Models/Plane.mdl");
			planeObject.SetMaterial(cache.GetMaterial("Materials/StoneTiled.xml"));

			var lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f));
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;

			boxNode = scene.CreateChild("Mushroom");
			boxNode.Position = new Vector3(0, 1, -40);
			boxNode.SetScale(2f);

			var boxModel = boxNode.CreateComponent<StaticModel>();
			boxModel.Model = cache.GetModel("Models/Box.mdl");
			boxModel.SetMaterial(cache.GetMaterial("Materials/StoneEnvMapSmall.xml"));
			boxModel.CastShadows = true;

			CameraNode = scene.CreateChild("camera");
			var camera = CameraNode.CreateComponent<Camera>();
			CameraNode.Position = new Vector3(0, 20, -60);
			CameraNode.Rotation = new Quaternion(30f, 0f, 0f);
			Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));
		}
	}
}
