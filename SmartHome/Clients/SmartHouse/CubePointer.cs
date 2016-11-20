using System;
using System.Linq;
using Urho;
using Urho.Actions;
using Urho.Shapes;

namespace SmartHouse
{
	public class CubePointer : Component
	{
		DateTime touchStarted;
		Node cubeNode;
		Camera camera;
		Octree octree;

		public CubePointer() {}
		public CubePointer(IntPtr handle) : base (handle) {}

		public event Action<Vector3> PositionChanged;

		public override void OnAttachedToNode(Node node)
		{
			base.OnAttachedToNode(node);
			Application.Input.TouchEnd += Input_TouchEnd;
			Application.Input.TouchBegin += Input_TouchBegin;

			cubeNode = node.CreateChild();
			cubeNode.Position = new Vector3(1000, 1000, 1000);
			cubeNode.SetScale(0.02f);
			var box = cubeNode.CreateComponent<Box>();
			box.Color = Color.White;

			var moveAction = new MoveBy(0.5f, new Vector3(0, 0.005f, 0));
			cubeNode.RunActionsAsync(new RepeatForever(new RotateBy(1f, 0, 120, 0)));
			cubeNode.RunActionsAsync(new RepeatForever(moveAction, moveAction.Reverse()));

			camera = Scene.GetChildrenWithComponent<Camera>(true).First().GetComponent<Camera>();
			octree = Scene.GetComponent<Octree>(true);
		}

		void Input_TouchEnd(TouchEndEventArgs e)
		{
			if (DateTime.UtcNow - touchStarted > TimeSpan.FromMilliseconds(200))
				return;

			Ray cameraRay = camera.GetScreenRay(e.X / (float)Application.Graphics.Width, e.Y / (float)Application.Graphics.Height);
			var result = octree.Raycast(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (result != null && result.Count > 0)
			{
				var item = result.First();
				cubeNode.Position = item.Position;
				cubeNode.Translate(item.Normal / 50, TransformSpace.Local);
				PositionChanged?.Invoke(cubeNode.Position);
			}
		}

		void Input_TouchBegin(TouchBeginEventArgs e)
		{
			touchStarted = DateTime.UtcNow;
		}
	}
}
