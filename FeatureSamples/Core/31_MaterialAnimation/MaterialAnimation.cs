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
	public class MaterialAnimation : Sample
	{
		Scene scene;
		bool drawDebug;

		public MaterialAnimation(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd();
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

			// Create a directional light to the world so that we can see something. The light scene node's orientation controls the
			// light direction; we will use the SetDirection() function which calculates the orientation from a forward direction vector.
			// The light will use default settings (white light, no shadows)
			Node lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f)); // The direction vector does not need to be normalized
			Light light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;

			// Create more StaticModel objects to the scene, randomly positioned, rotated and scaled. For rotation, we construct a
			// quaternion from Euler angles where the Y angle (rotation about the Y axis) is randomized. The mushroom model contains
			// LOD levels, so the StaticModel component will automatically select the LOD level according to the view distance (you'll
			// see the model get simpler as it moves further away). Finally, rendering a large number of the same object with the
			// same material allows instancing to be used, if the GPU supports it. This reduces the amount of CPU work in rendering the
			// scene.
			Material mushroomMat = cache.GetMaterial("Materials/Mushroom.xml");
			// Apply shader parameter animation to material
			ValueAnimation specColorAnimation=new ValueAnimation();

			specColorAnimation.SetKeyFrame(0.0f, new Color(0.1f, 0.1f, 0.1f, 16.0f));
			specColorAnimation.SetKeyFrame(1.0f, new Color(1.0f, 0.0f, 0.0f, 2.0f));
			specColorAnimation.SetKeyFrame(2.0f, new Color(1.0f, 1.0f, 0.0f, 2.0f));
			specColorAnimation.SetKeyFrame(3.0f, new Color(0.1f, 0.1f, 0.1f, 16.0f));
			// Optionally associate material with scene to make sure shader parameter animation respects scene time scale
			mushroomMat.Scene=scene;
			mushroomMat.SetShaderParameterAnimation("MatSpecColor", specColorAnimation, WrapMode.Loop, 1.0f);

			const uint numObjects = 200;
			for (uint i = 0; i < numObjects; ++i)
			{
				Node mushroomNode = scene.CreateChild("Mushroom");
				mushroomNode.Position = (new Vector3(NextRandom(90.0f) - 45.0f, 0.0f, NextRandom(90.0f) - 45.0f));
				mushroomNode.Rotation=new Quaternion(0.0f, NextRandom(360.0f), 0.0f);
				mushroomNode.SetScale(0.5f + NextRandom(2.0f));
				StaticModel mushroomObject = mushroomNode.CreateComponent<StaticModel>();
				mushroomObject.Model = (cache.GetModel("Models/Mushroom.mdl"));
				mushroomObject.SetMaterial(mushroomMat);
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
