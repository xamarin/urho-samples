using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Shapes;

namespace Urho.Samples
{
	public class Charts : Sample
	{
		public Charts(ApplicationOptions options = null) : base(options) {}

		protected override void Start()
		{
			base.Start();
			IsLogoVisible = false;

			// 3D scene with Octree
			var scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			// Camera
			CameraNode = scene.CreateChild(name: "camera");
			CameraNode.Position = new Vector3(-15, 12, 10);
			CameraNode.Rotation = new Quaternion(27, -125, 23);
			var	a = CameraNode.Rotation.ToEulerAngles().ToString();
			Camera camera = CameraNode.CreateComponent<Camera>();


			var aa = new Quaternion(27, -125, 23).ToEulerAngles().ToString();


			// Light
			Node lightNode = CameraNode.CreateChild(name: "light");
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Point;
			light.Range = 50;

			// Viewport
			Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));

			var plotNode = scene.CreateChild();
			var baseNode = plotNode.CreateChild();
			baseNode.CreateComponent<Shapes.Plane>().Color = Color.White;
			baseNode.Scale = new Vector3(10, 1, 10);

			//  var plotPlane = plotNode.CreateComponent<Shapes.Plane>();
			//  plotPlane.Color = Color.White;
			//  plotNode.Scale = new Vector3(10, 1, 10);

			var boxNode = plotNode.CreateChild();
			boxNode.CreateComponent<Box>();
		}


		protected override void OnUpdate(float timeStep)
		{
			//SimpleMoveCamera3D(timeStep);
			MonoDebugHud.AdditionalText = CameraNode.Rotation.ToEulerAngles().ToString();
		}
	}
}
