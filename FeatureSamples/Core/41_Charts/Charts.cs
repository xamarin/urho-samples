using System;
using System.Collections.Generic;
using System.Linq;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace Urho.Samples
{
	public class Charts : Application
	{
		bool movementsEnabled;
		Node plotNode;
		Camera camera;
		Octree octree;
		Bar selectedBar;

		public Charts(ApplicationOptions options = null) : base(SetOptions(options)) {}

		private static ApplicationOptions SetOptions(ApplicationOptions options)
		{
			options.TouchEmulation = true;
			return options;
		}

		protected override async void Start()
		{
			base.Start();
			Input.SubscribeToKeyDown(k => { if (k.Key == Key.Esc) Exit(); });
			Input.SubscribeToTouchEnd(OnTouched);

			// 3D scene with Octree
			var scene = new Scene(Context);
			octree = scene.CreateComponent<Octree>();

			// Camera
			var cameraNode = scene.CreateChild(name: "camera");
			cameraNode.Position = new Vector3(10, 14, 10);
			cameraNode.Rotation = new Quaternion(-0.121f, 0.878f, -0.305f, -0.35f);
			camera = cameraNode.CreateComponent<Camera>();

			// Light
			Node lightNode = cameraNode.CreateChild(name: "light");
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 100;
			light.Brightness = 1.3f;

			// Viewport
			var viewport = new Viewport(Context, scene, camera, null);
			Renderer.SetViewport(0, viewport);
			viewport.SetClearColor(new Color(0.4f, 0.4f, 0.4f));

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
					var box = new Bar(h => h.ToString("F1"), new Color(Sample.NextRandom(), Sample.NextRandom(), Sample.NextRandom(), 0.9f));
					boxNode.AddComponent(box);
					box.Value = (Math.Abs(i) + Math.Abs(j) + 1) / 2f;
				}
			}
			await plotNode.RunActionsAsync(new EaseBackOut(new RotateBy(2f, 0, 360, 0)));
			movementsEnabled = true;
		}

		private void OnTouched(TouchEndEventArgs e)
		{
			Ray cameraRay = camera.GetScreenRay((float)e.X / Graphics.Width, (float)e.Y / Graphics.Height);
			var result = octree.RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);
			if (result != null)
			{
				var bar = result.Value.Node?.Parent?.GetComponent<Bar>();
				if (selectedBar != bar)
				{
					selectedBar?.Deselect();
					selectedBar = bar;
					selectedBar?.Select();
				}
			}
		}

		protected override void OnUpdate(float timeStep)
		{
			if (Input.NumTouches == 1 && movementsEnabled)
			{
				var touch = Input.GetTouch(0);
				plotNode.Rotate(new Quaternion(0, -touch.Delta.X, 0), TransformSpace.Local);
			}
			base.OnUpdate(timeStep);
		}
	}

	public class Bar : Component
	{
		Func<float, string> heightToTextFunc;
		Node barNode;
		Node textNode;
		Text3D text3D;
		Color color;
		float value;

		public float Value
		{
			get { return value; }
			set
			{
				this.value = value;
				barNode.RunActionsAsync(new EaseBackOut(new ScaleTo(Sample.NextRandom(3f, 15f), 1, value, 1)));
			}
		}

		public Bar(Func<float, string> heightToTextFunc, Color color)
		{
			this.heightToTextFunc = heightToTextFunc;
			this.color = color;
			ReceiveSceneUpdates = true;
		}

		public override void OnAttachedToNode(Node node)
		{
			barNode = node.CreateChild();
			barNode.Scale = new Vector3(1, 0, 1); //means zero height
			var box = barNode.CreateComponent<Box>();
			box.Color = color;

			textNode = node.CreateChild();
			textNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);
			textNode.Position = new Vector3(0, 10, 0);
			text3D = textNode.CreateComponent<Text3D>();
			text3D.SetFont(Application.ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 60);
			text3D.TextEffect = TextEffect.None;
			//textNode.LookAt() //Look at camera

			base.OnAttachedToNode(node);
		}

		protected override void OnUpdate(float timeStep)
		{
			var pos = barNode.Position;
			var scale = barNode.Scale;
			barNode.Position = new Vector3(pos.X, scale.Y / 2f, pos.Z);
			textNode.Position = new Vector3(0.5f, scale.Y + 0.2f, 0);
			text3D.Text = heightToTextFunc(scale.Y);
		}

		public void Deselect()
		{
			barNode.RemoveAllActions();//TODO: remove only "selection" action
			barNode.RunActionsAsync(new EaseBackOut(new TintTo(1f, color.R, color.G, color.B)));
		}

		public void Select()
		{
			// "blinking" animation
			barNode.RunActionsAsync(new RepeatForever(new TintTo(0.3f, 1f, 1f, 1f), new TintTo(0.3f, color.R, color.G, color.B)));
		}
	}
}
