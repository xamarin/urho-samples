using System;
using System.Collections.Generic;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace Urho.Samples
{
	public class Charts : Application
	{
		bool movementsEnabled;
		Node plotNode;

		public Charts(ApplicationOptions options = null) : base(SetOptions(options)) {}

		private static ApplicationOptions SetOptions(ApplicationOptions options)
		{
			options.TouchEmulation = true;
			return options;
		}

		protected override async void Start()
		{
			base.Start();
			Input.SubscribeToKeyDown(k => { if (k.Key == Key.Esc) Engine.Exit(); });

			// 3D scene with Octree
			var scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			// Camera
			var CameraNode = scene.CreateChild(name: "camera");
			CameraNode.Position = new Vector3(10, 14, 10);
			CameraNode.Rotation = new Quaternion(-0.121f, 0.878f, -0.305f, -0.35f);
			Camera camera = CameraNode.CreateComponent<Camera>();

			// Light
			Node lightNode = CameraNode.CreateChild(name: "light");
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 100;
			light.Brightness = 1.5f;

			// Viewport
			Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));

			plotNode = scene.CreateChild();
			var baseNode = plotNode.CreateChild().CreateChild();
			var plane = baseNode.CreateComponent<StaticModel>();
			plane.Model = ResourceCache.GetModel("Models/Plane.mdl");

			int size = 5;
			baseNode.Scale = new Vector3(size * 1.5f, 1, size * 1.5f);
			for (var i = 0f; i < size * 1.5f; i += 1.5f)
			{
				for (var j = 0f; j < size * 1.5f; j += 1.5f)
				{
					var boxNode = plotNode.CreateChild();
					boxNode.Position = new Vector3(size / 2f - i + 0.5f, 0, size / 2f - j + 0.5f);
					var box = boxNode.CreateComponent<Bar>();
					box.UpdateData((Math.Abs(i) + Math.Abs(j) + 1) / 2f, h => Math.Round(h, 1).ToString(), new Color(Sample.NextRandom(), Sample.NextRandom(), Sample.NextRandom(), 0.9f));
				}
			}
			await plotNode.RunActionsAsync(new EaseBackOut(new RotateBy(2f, 0, 360, 0)));
			movementsEnabled = true;
		}

		protected override void OnUpdate(float timeStep)
		{
			if (Input.NumTouches > 0)
			{
				var touchDelta = Input.GetTouch(0).Delta;
				plotNode.Rotate(new Quaternion(0, -touchDelta.X, 0), TransformSpace.Local);
			}
			base.OnUpdate(timeStep);
		}
	}

	public class Bar : Component
	{
		Func<float, string> heightToTextFunc;
		Node barNode;
		Node textNode;
		Box box;
		Text3D text3D;

		public Bar() { ReceiveSceneUpdates = true; }

		public override void OnAttachedToNode(Node node)
		{
			barNode = node.CreateChild();
			barNode.Scale = new Vector3(1, 0, 1);
			box = barNode.CreateComponent<Box>();

			textNode = node.CreateChild();
			textNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
			textNode.Position = new Vector3(0, 10, 0);
			text3D = textNode.CreateComponent<Text3D>();
			text3D.SetFont(Application.ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 60);
			text3D.TextEffect = TextEffect.Shadow;

			base.OnAttachedToNode(node);
		}

		public void UpdateData(float height, Func<float, string> heightToTextFunc, Color color)
		{
			this.heightToTextFunc = heightToTextFunc;
			box.Color = Color.Red;
			var duration = Sample.NextRandom(3f, 15f);
			barNode.RunActionsAsync(new Parallel(
				new EaseBackOut(new ScaleTo(duration, 1, height, 1)),
				new EaseBackOut(new TintTo(duration, color.R, color.G, color.B))));
		}

		protected override void OnUpdate(float timeStep)
		{
			var pos = barNode.Position;
			var scale = barNode.Scale;
			barNode.Position = new Vector3(pos.X, scale.Y / 2f, pos.Z);
			textNode.Position = new Vector3(0.5f, scale.Y + 0.2f, 0);
			text3D.Text = heightToTextFunc(scale.Y);
		}
	}
}
