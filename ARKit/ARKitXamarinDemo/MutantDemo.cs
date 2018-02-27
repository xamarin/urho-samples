using System;
using Urho;
using ARKit;
using Urho.iOS;
using Urho.Resources;

namespace ARKitXamarinDemo
{
	public class MutantDemo : SimpleApplication
	{
		[Preserve]
		public MutantDemo(ApplicationOptions opts) : base(opts) { }

		ARKitComponent arkitComponent;
		Node mutantNode;
		bool scaling;

		protected override unsafe void Start()
		{
			UnhandledException += OnUnhandledException;
			Log.LogLevel = LogLevel.Debug;

			base.Start();

			arkitComponent = Scene.CreateComponent<ARKitComponent>();
			arkitComponent.Orientation = UIKit.UIInterfaceOrientation.Portrait;
			arkitComponent.ARConfiguration = new ARWorldTrackingConfiguration { 
				PlaneDetection = ARPlaneDetection.Horizontal,
			};
			arkitComponent.RunEngineFramesInARKitCallbakcs = Options.DelayedStart;
			arkitComponent.Run();

			// Mutant
			mutantNode = Scene.CreateChild();
			mutantNode.SetScale(0.2f);
			mutantNode.Position = new Vector3(0, -0.5f, 0.75f);

			var planeNode = mutantNode.CreateChild();
			planeNode.Scale = new Vector3(10, 0.1f, 10);
			//var plane = planeNode.CreateComponent<Urho.SharpReality.TransparentPlaneWithShadows>();

			var model = mutantNode.CreateComponent<AnimatedModel>();
			model.CastShadows = true;
			model.Model = ResourceCache.GetModel("Models/Mutant.mdl");
			model.Material = ResourceCache.GetMaterial("Materials/mutant_M.xml");

			var animation = mutantNode.CreateComponent<AnimationController>();
			animation.Play("Animations/Mutant_HipHop1.ani", 0, true, 0.2f);

			Input.TouchBegin += OnTouchBegin;
			Input.TouchEnd += OnTouchEnd;
		}


		void OnTouchBegin(TouchBeginEventArgs e)
		{
			scaling = false;
		}

		void OnTouchEnd(TouchEndEventArgs e)
		{
			if (scaling)
				return;

			var pos = arkitComponent.HitTest(e.X / (float)Graphics.Width, e.Y / (float)Graphics.Height);
			if (pos != null)
				mutantNode.Position = pos.Value;
		}

		protected override void OnUpdate(float timeStep)
		{
			// Scale up\down
			if (Input.NumTouches == 2)
			{
				scaling = true;
				var state1 = Input.GetTouch(0);
				var state2 = Input.GetTouch(1);
				var distance1 = IntVector2.Distance(state1.Position, state2.Position);
				var distance2 = IntVector2.Distance(state1.LastPosition, state2.LastPosition);
				mutantNode.SetScale(mutantNode.Scale.X + (distance1 - distance2) / 10000f);
			}

			base.OnUpdate(timeStep);
		}

		void OnUnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			System.Console.WriteLine(e);
		}
	}
}