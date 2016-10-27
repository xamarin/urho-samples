using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Gui;
using Urho.HoloLens;
using Urho.Physics;
using Urho.Resources;

namespace Physics
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<Progam>("Data"));
	}

	public class Progam : HoloApplication
	{
		Node environmentNode;
		Material spatialMaterial;
		Material bucketMaterial;
		Material ballMaterial;
		bool surfaceIsValid;
		bool positionIsSelected;
		Node bucketNode;
		Node textNode;

		const int MaxBalls = 5;
		readonly Queue<Node> balls = new Queue<Node>();
		readonly Color validPositionColor = Color.Gray;
		readonly Color invalidPositionColor = Color.Red;

		public Progam(ApplicationOptions assets) : base(assets) { }

		protected override async void Start()
		{
			base.Start();
			environmentNode = Scene.CreateChild();

			// Allow tap gesture
			EnableGestureTapped = true;

			// Create a bucket
			bucketNode = Scene.CreateChild();
			bucketNode.SetScale(0.1f);

			// Create instructions
			textNode = bucketNode.CreateChild();
			var text3D = textNode.CreateComponent<Text3D>();
			text3D.HorizontalAlignment = HorizontalAlignment.Center;
			text3D.VerticalAlignment = VerticalAlignment.Top;
			text3D.ViewMask = 0x80000000; //hide from raycasts
			text3D.Text = "Place on a horizontal\n  surface and click";
			text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 26);
			text3D.SetColor(Color.White);
			textNode.Translate(new Vector3(0, 3f, -0.5f));

			// Model and Physics for the bucket
			var bucketModel = bucketNode.CreateComponent<StaticModel>();
			bucketMaterial = Material.FromColor(validPositionColor);
			bucketModel.Model = ResourceCache.GetModel("Models/bucket.mdl");
			bucketModel.SetMaterial(bucketMaterial);
			bucketModel.ViewMask = 0x80000000; //hide from raycasts
			bucketNode.CreateComponent<RigidBody>();
			var shape = bucketNode.CreateComponent<CollisionShape>();
			shape.SetTriangleMesh(bucketModel.Model, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);

			ballMaterial = Material.FromColor(new Color(Randoms.Next(0.2f, 1f), Randoms.Next(0.2f, 1f), Randoms.Next(0.2f, 1f)));

			// Material for spatial surfaces
			spatialMaterial = new Material();
			spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);

			// make sure 'spatialMapping' capabilaty is enabled in the app manifest.
			var spatialMappingAllowed = await StartSpatialMapping(new Vector3(50, 50, 10), 1200);
		}

		protected override void OnUpdate(float timeStep)
		{
			if (positionIsSelected)
				return;

			textNode.LookAt(LeftCamera.Node.WorldPosition, new Vector3(0, 1, 0), TransformSpace.World);
			textNode.Rotate(new Quaternion(0, 180, 0), TransformSpace.World);

			Ray cameraRay = RightCamera.GetScreenRay(0.5f, 0.5f);
			var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (result != null)
			{
				var angle = Vector3.CalculateAngle(new Vector3(0, 1, 0), result.Value.Normal);
				surfaceIsValid = angle < 0.3f; //allow only horizontal surfaces
				bucketMaterial.SetShaderParameter("MatDiffColor", surfaceIsValid ? validPositionColor : invalidPositionColor);
				bucketNode.Position = result.Value.Position;
			}
			else
			{
				// no spatial surfaces found
				surfaceIsValid = false;
				bucketMaterial.SetShaderParameter("MatDiffColor", validPositionColor);
			}
		}

		public override void OnGestureTapped()
		{
			if (positionIsSelected)
				ThrowBall();

			if (surfaceIsValid && !positionIsSelected)
			{
				positionIsSelected = true;
				textNode.Remove();
				textNode = null;
			}

			base.OnGestureTapped();
		}

		void ThrowBall()
		{
			// Create a ball (will be cloned)
			var ballNode = Scene.CreateChild();
			ballNode.Position = RightCamera.Node.Position;
			ballNode.Rotation = RightCamera.Node.Rotation;
			ballNode.SetScale(0.15f);

			var ball = ballNode.CreateComponent<StaticModel>();
			ball.Model = CoreAssets.Models.Sphere;
			ball.SetMaterial(ballMaterial);
			ball.ViewMask = 0x80000000; //hide from raycasts

			var ballRigidBody = ballNode.CreateComponent<RigidBody>();
			ballRigidBody.Mass = 1f;
			ballRigidBody.RollingFriction = 0.5f;
			var ballShape = ballNode.CreateComponent<CollisionShape>();
			ballShape.SetSphere(1, Vector3.Zero, Quaternion.Identity);

			ball.GetComponent<RigidBody>().SetLinearVelocity(RightCamera.Node.Rotation * new Vector3(0f, 0.25f, 1f) * 9 /*velocity*/);

			balls.Enqueue(ballNode);
			if (balls.Count > MaxBalls)
				balls.Dequeue().Remove();
		}

		public override void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
		{
			bool isNew = false;
			StaticModel staticModel = null;
			Node node = environmentNode.GetChild(surface.SurfaceId, false);
			if (node != null)
			{
				isNew = false;
				staticModel = node.GetComponent<StaticModel>();
			}
			else
			{
				isNew = true;
				node = environmentNode.CreateChild(surface.SurfaceId);
				staticModel = node.CreateComponent<StaticModel>();
			}

			node.Position = surface.BoundsCenter;
			node.Rotation = surface.BoundsRotation;
			staticModel.Model = generatedModel;

			if (isNew)
			{
				staticModel.SetMaterial(spatialMaterial);
				var rigidBody = node.CreateComponent<RigidBody>();
				rigidBody.RollingFriction = 0.5f;
				rigidBody.Friction = 0.5f;
				var collisionShape = node.CreateComponent<CollisionShape>();
				collisionShape.SetTriangleMesh(generatedModel, 0, Vector3.One, Vector3.Zero, Quaternion.Identity);
			}
			else
			{
				//Update Collision shape
			}
		}
	}
}