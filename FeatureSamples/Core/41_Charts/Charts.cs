using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Actions;
using Urho.Shapes;

namespace Urho.Samples
{
	public class Charts : Application
	{
		List<Node> bars = new List<Node>(); 

		public Charts(ApplicationOptions options = null) : base(options) {}

		protected override void Start()
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

			var plotNode = scene.CreateChild();
			var baseNode = plotNode.CreateChild().CreateChild();
			var plane = baseNode.CreateComponent<StaticModel>();
			plane.Model = ResourceCache.GetModel("Models/Plane.mdl");

			int size = 5;
			baseNode.Scale = new Vector3(size * 1.5f, 1, size * 1.5f);
			plotNode.RunActionsAsync(new EaseBackOut(new RotateBy(2f, 0, 360, 0)));
			for (var i = 0f; i < size * 1.5f; i += 1.5f)
			{
				for (var j = 0f; j < size * 1.5f; j += 1.5f)
				{
					var boxNode = plotNode.CreateChild();
					boxNode.Position = new Vector3(size / 2f - i + 0.5f, 0, size / 2f - j + 0.5f);
					boxNode.Scale = new Vector3(1, 0, 1);
					boxNode.RunActionsAsync(new EaseBackOut(new ScaleTo(4f + Sample.NextRandom(1, 3), 1, (Math.Abs(i) + Math.Abs(j) + 1) / 2f, 1)));
					bars.Add(boxNode);
					var box = boxNode.CreateComponent<Box>();
					box.Color = new Color(Sample.NextRandom(), Sample.NextRandom(), Sample.NextRandom(), 0.9f);
				}
			}
		}

		protected override void OnUpdate(float timeStep)
		{
			foreach (var bar in bars)
			{
				var pos = bar.Position;
				bar.Position = new Vector3(pos.X, bar.Scale.Y / 2f, pos.Z);
			}
		}
	}
}
