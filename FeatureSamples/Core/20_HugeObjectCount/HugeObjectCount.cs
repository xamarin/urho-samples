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
using System.Collections.Generic;

namespace Urho.Samples
{
	public class HugeObjectCount : Sample
	{
		Scene scene;
		Camera camera;
		bool animate;
		bool useGroups;
		List<Node> boxNodes;

		public HugeObjectCount(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd(
				"\nSpace to toggle animation\n" +
				"G to toggle object group optimization");
			SetupViewport();
		}
	
		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			// Toggle animation with space
			Input input = Input;
			if (input.GetKeyPress(Key.Space))
				animate = !animate;

			// Toggle grouped / ungrouped mode
			if (input.GetKeyPress(Key.G))
			{
				useGroups = !useGroups;
				CreateScene();
			}

			SimpleMoveCamera3D(timeStep);

			if (animate)
				AnimateObjects(timeStep);
		}

		void AnimateObjects(float timeStep)
		{
			const float rotateSpeed = 15.0f;
			// Rotate about the Z axis (roll)
			Quaternion rotateQuat = Quaternion.FromAxisAngle(Vector3.UnitZ, rotateSpeed * timeStep);

			foreach (var boxNode in boxNodes)
			{
				boxNode.Rotate(rotateQuat, TransformSpace.Local);
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
			if (scene == null)
				scene = new Scene();
			else
			{
				scene.Clear(true, true);
				boxNodes.Clear();
				GC.Collect(); //recreation of scene with a lot of nodes
			}
			boxNodes = new List<Node>();

			// Create the Octree component to the scene so that drawable objects can be rendered. Use default volume
			// (-1000, -1000, -1000) to (1000, 1000, 1000)
			scene.CreateComponent<Octree>();

			// Create a Zone for ambient light & fog control
			Node zoneNode = scene.CreateChild("Zone");
			Zone zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
			zone.FogColor = new Color(0.2f, 0.2f, 0.2f);
			zone.FogStart = 200.0f;
			zone.FogEnd = 300.0f;

			// Create a directional light
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(-0.6f, -1.0f, -0.8f)); // The direction vector does not need to be normalized
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;

			if (!useGroups)
			{
				light.Color = new Color(0.7f, 0.35f, 0.0f);

				// Create individual box StaticModels in the scene
				for (int y = -125; y < 125; ++y)
				{
					for (int x = -125; x < 125; ++x)
					{
						var boxNode = scene.CreateChild("Box");
						boxNode.Position = new Vector3(x*0.3f, 0.0f, y*0.3f);
						boxNode.SetScale(0.25f);
						var boxObject = boxNode.CreateComponent<StaticModel>();
						boxObject.Model = cache.GetModel("Models/Box.mdl");
						boxNodes.Add(boxNode);
					}
				}
			}
			else
			{
				light.Color = new Color(0.6f, 0.6f, 0.6f);
				light.SpecularIntensity = 1.5f;

				// Create StaticModelGroups in the scene
				StaticModelGroup lastGroup = null;

				for (int y = -125; y< 125; ++y)
				{
					for (int x = -125; x< 125; ++x)
					{
						// Create new group if no group yet, or the group has already "enough" objects. The tradeoff is between culling
						// accuracy and the amount of CPU processing needed for all the objects. Note that the group's own transform
						// does not matter, and it does not render anything if instance nodes are not added to it
						if (lastGroup == null || lastGroup.NumInstanceNodes >= 50 * 50)
						{
							using (var boxGroupNode = scene.CreateChild("BoxGroup"))
							{
								lastGroup = boxGroupNode.CreateComponent<StaticModelGroup>();
								lastGroup.Model = cache.GetModel("Models/Box.mdl");
							}
						}

						var boxNode = scene.CreateChild("Box");
						boxNode.Position = new Vector3(x*0.3f, 0.0f, y*0.3f);
						boxNode.SetScale(0.25f);
						boxNodes.Add(boxNode);
						lastGroup.AddInstanceNode(boxNode);
					}
				}
			}

			// Create the camera. Create it outside the scene so that we can clear the whole scene without affecting it
			if (CameraNode == null)
			{
				// Create the camera. Limit far clip distance to match the fog
				CameraNode = new Node();
				CameraNode.Position = new Vector3(0.0f, 10.0f, -100.0f);
				camera = CameraNode.CreateComponent<Camera>();
				camera.FarClip = 300.0f;
			}
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithGroupAndAnimationButtons;

	}
}
