using System;
using System.Linq;
using Urho;
using Urho.Holographics;
using Urho.Physics;
using Urho.Shapes;

namespace Physics
{
	public class PhysicsSample : HoloApplication
	{
		Node environmentNode;
		Node pointerNode;
		Material spatialMaterial;

		public PhysicsSample(string pak, bool emulator) : base(pak, emulator) { }

		protected override async void Start()
		{
			base.Start();
			environmentNode = Scene.CreateChild();

			EnableGestureTapped = true;

			pointerNode = Scene.CreateChild("cursor");
			var pointerModelNode = pointerNode.CreateChild("cursor");
			pointerModelNode.SetScale(0.05f);
			var box = pointerModelNode.CreateComponent<StaticModel>();
			box.Model = CoreAssets.Models.Box;
			box.SetMaterial(Material.FromColor(Color.Magenta));
			box.ViewMask = 0x80000000; //hide from raycasts

			spatialMaterial = new Material();
			spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);

			var allowed = await StartSpatialMapping(new Vector3(50, 50, 10), 1200);
		}

		public override void OnGestureTapped(GazeInfo gaze)
		{
			ThrowBall();
			base.OnGestureTapped(gaze);
		}

		void ThrowBall()
		{
			var ballNode = Scene.CreateChild();
			ballNode.Position = RightCamera.Node.Position;
			ballNode.Rotation = RightCamera.Node.Rotation;
			ballNode.SetScale(0.22f);

			var ball = ballNode.CreateComponent<StaticModel>();
			ball.Model = CoreAssets.Models.Sphere;
			ball.SetMaterial(Material.FromColor(new Color(Randoms.Next(0.5f, 1f), Randoms.Next(0.5f, 1f), Randoms.Next(0.5f, 1f))));
			ball.ViewMask = 0x80000000; //hide from raycasts

			var body = ballNode.CreateComponent<RigidBody>();
			body.Mass = 4f;
			body.Friction = 0.5f;
			body.RollingFriction = 0.5f;
			var shape = ballNode.CreateComponent<CollisionShape>();
			shape.SetSphere(1, Vector3.Zero, Quaternion.Identity);

			body.SetLinearVelocity(RightCamera.Node.Rotation * new Vector3(0f, 0.25f, 1f) * 7 /*velocity*/);
		}

		protected override void OnUpdate(float timeStep)
		{
			Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
			var results = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (results != null && results.Count > 0)
				pointerNode.Position = results[0].Position;
			else
				pointerNode.Position = LeftCamera.Node.Rotation*new Vector3(0, 0, 6f);
			FocusWorldPoint = pointerNode.WorldPosition;
		}

		public override void OnSurfaceAddedOrUpdated(string surfaceId, DateTimeOffset lastUpdateTimeUtc, 
			SpatialVertex[] vertexData, short[] indexData, 
			Vector3 boundsCenter, Quaternion boundsRotation)
		{

			bool isNew = false;
			StaticModel staticModel = null;
			Node node = environmentNode.GetChild(surfaceId, false);
			if (node != null)
			{
				isNew = false;
				staticModel = node.GetComponent<StaticModel>();
			}
			else
			{
				isNew = true;
				node = environmentNode.CreateChild(surfaceId);
				staticModel = node.CreateComponent<StaticModel>();
			}

			node.Position = boundsCenter;
			node.Rotation = boundsRotation;
			var model = CreateModelFromVertexData(vertexData, indexData);
			//model.BoundingBox = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
			staticModel.Model = model;

			if (isNew)
				staticModel.SetMaterial(spatialMaterial);

			var rigidBody = node.CreateComponent<RigidBody>();
			rigidBody.RollingFriction = 0.5f;
			rigidBody.Friction = 0.5f;
			var collisionShape = node.CreateComponent<CollisionShape>();
			collisionShape.SetTriangleMesh(model, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);
		}
	}
}