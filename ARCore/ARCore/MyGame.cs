using System;
using Com.Google.AR.Core;
using Urho;
using Urho.Droid;

namespace ARCore
{
	public class MyGame : Application
	{
		Viewport viewport;
		Zone zone;
		Node mutantNode;
		MonoDebugHud fps;
		bool gammaCorrected;
		bool scaling;
		Frame currentFrame;

		public ARCoreComponent ArCore { get; private set; }

		[Preserve]
		public MyGame(ApplicationOptions options) : base(options) { }

		protected override void Start()
		{
			// 3d scene with octree and ambient light
			var scene = new Scene(Context);
			var octree = scene.CreateComponent<Octree>();
			zone = scene.CreateComponent<Zone>();
			zone.AmbientColor = new Color(1, 1, 1) * 0.2f;

			// Camera
			var cameraNode = scene.CreateChild(name: "Camera");
			var camera = cameraNode.CreateComponent<Urho.Camera>();

			// Light
			var lightNode = cameraNode.CreateChild();
			lightNode.SetDirection(new Vector3(1f, -1.0f, 1f));
			var light = lightNode.CreateComponent<Light>();
			light.Range = 10;
			light.LightType = LightType.Directional;
			light.CastShadows = true;
			Renderer.ShadowMapSize *= 4;

			// Viewport
			viewport = new Viewport(Context, scene, camera, null);
			Renderer.SetViewport(0, viewport);

			// ARCore component
			ArCore = scene.CreateComponent<ARCoreComponent>();
			ArCore.ARFrameUpdated += OnARFrameUpdated;
			ArCore.ConfigRequested += ArCore_ConfigRequested;
			ArCore.Run();

			// Mutant
			mutantNode = scene.CreateChild();
			mutantNode.Position = new Vector3(0, -0.5f, 0.5f); // 50cm Y, 50cm Z
			mutantNode.SetScale(0.3f);
			var model = mutantNode.CreateComponent<AnimatedModel>();
			model.CastShadows = true;
			model.Model = ResourceCache.GetModel("Models/Mutant.mdl");
			model.Material = ResourceCache.GetMaterial("Materials/mutant_M.xml");
			var ani = mutantNode.CreateComponent<AnimationController>();
			ani.Play("Animations/Mutant_HipHop1.ani", 0, true, 1f);
			
			fps = new MonoDebugHud(this);
			fps.Show(Color.Blue, 20);

			// Add some post-processing (also, see CorrectGamma())
			viewport.RenderPath.Append(CoreAssets.PostProcess.FXAA2);

			Input.TouchBegin += OnTouchBegin;
			Input.TouchEnd += OnTouchEnd;
		}

		void ArCore_ConfigRequested(Config config)
		{
			config.SetPlaneFindingMode(Config.PlaneFindingMode.Horizontal);
			config.SetLightEstimationMode(Config.LightEstimationMode.AmbientIntensity);
			config.SetUpdateMode(Config.UpdateMode.LatestCameraImage); //non blocking
		}

		void OnTouchBegin(TouchBeginEventArgs e)
		{
			scaling = false;
		}

		void OnTouchEnd(TouchEndEventArgs e)
		{
			if (scaling)
				return;

			var hitTest = currentFrame.HitTest(e.X, e.Y);
			if (hitTest != null && hitTest.Count > 0)
			{
				var hitPos = hitTest[0].HitPose;
				mutantNode.Position = new Vector3(hitPos.Tx(), hitPos.Ty(), -hitPos.Tz());
			}
		}

		// game update
		protected override void OnUpdate(float timeStep)
		{
			// multitouch scaling:
			if (Input.NumTouches == 2)
			{
				scaling = true;
				var state1 = Input.GetTouch(0);
				var state2 = Input.GetTouch(1);
				var distance1 = IntVector2.Distance(state1.Position, state2.Position);
				var distance2 = IntVector2.Distance(state1.LastPosition, state2.LastPosition);
				mutantNode.SetScale(mutantNode.Scale.X + (distance1 - distance2) / 10000f);
			}
		}

		// called by the update loop
		void OnARFrameUpdated(Frame arFrame)
		{
			currentFrame = arFrame;
			var anchors = arFrame.UpdatedAnchors; 
			//TODO: visulize anchors (don't forget ARCore uses RHD coordinate system)

			// Adjust our ambient light based on the light estimates ARCore provides each frame
			var lightEstimate = arFrame.LightEstimate;
			fps.AdditionalText = lightEstimate?.PixelIntensity.ToString("F1");
			zone.AmbientColor = new Color(1,1,1) * ((lightEstimate?.PixelIntensity ?? 0.2f) / 2f);
		}

		public void CorrectGamma()
		{
			if (!gammaCorrected)
				viewport.RenderPath.Append(ResourceCache.GetXmlFile("PostProcess/MyGammaCorrection.xml"));
			gammaCorrected = true;
		}
	}
}
