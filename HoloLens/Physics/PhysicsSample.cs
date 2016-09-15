using System;
using Urho;
using Urho.Holographics;
using Urho.Physics;
using Urho.Shapes;

namespace Physics
{
	public class PhysicsSample : HoloApplication
	{
		Node environmentNode;

		public PhysicsSample(string pak, bool emulator) : base(pak, emulator) { }

		protected override async void Start()
		{
			base.Start();
			environmentNode = Scene.CreateChild();
			EnableGestureTapped = true;
			bool success = await StartSpatialMapping(new Vector3(10, 10, 5));
		}

		public override void OnGestureTapped(GazeInfo gaze)
		{
			ThrowBall();
			base.OnGestureTapped(gaze);
		}

		void ThrowBall()
		{
			var ballNode = Scene.CreateChild();
			ballNode.Position = LeftCamera.Node.Position;
			ballNode.Rotation = LeftCamera.Node.Rotation;
			ballNode.SetScale(0.1f);

			var ball = ballNode.CreateComponent<Sphere>();
			ball.Color = new Color(Randoms.Next(), Randoms.Next(), Randoms.Next());

			var body = ballNode.CreateComponent<RigidBody>();
			body.Mass = 0.25f;
			body.Friction = 0.75f;
			var shape = ballNode.CreateComponent<CollisionShape>();
			shape.SetSphere(1, Vector3.Zero, Quaternion.Identity);

			const float objectVelocity = 5.0f;
			body.SetLinearVelocity(LeftCamera.Node.Rotation * new Vector3(0f, 0.25f, 1f) * objectVelocity);
		}

		public override void OnSurfaceAddedOrUpdated(string surfaceId, DateTimeOffset lastUpdateTimeUtc, 
			float[] vertexData, short[] indexData, 
			Vector3 boundsCenter, Quaternion boundsRotation)
		{
			environmentNode.GetChild(surfaceId, false)?.Remove();

			var node = environmentNode.CreateChild(surfaceId);
			node.Position = boundsCenter;
			node.Rotation = boundsRotation;
			var model = CreateModelFromVertexData(vertexData, indexData);

			var rigidBody = node.CreateComponent<RigidBody>();
			var collisionShape = node.CreateComponent<CollisionShape>();
			collisionShape.SetTriangleMesh(model, 0, Vector3.One * 1.2f, Vector3.Zero, Quaternion.Identity);
			rigidBody.RollingFriction = 0.15f;
		}
	}
}