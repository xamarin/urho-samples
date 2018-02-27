using System;
using Urho;
using ARKit;
using Urho.iOS;
using Urho.Resources;
using Urho.Gui;
using Urho.Physics;
using System.Linq;

namespace ARKitXamarinDemo
{
	public class VerticalSurfaces : SimpleApplication
	{
		ARKitComponent arkitComponent;
		Node anchorsNode;
		Text loadingLabel;

		[Preserve]
		public VerticalSurfaces(ApplicationOptions options) : base(options) {}

		protected override void Start()
		{
			base.Start();

			//Scene.CreateComponent<PhysicsWorld>();
			Scene.CreateComponent<DebugRenderer>();

			Zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));

			new MonoDebugHud(this).Show(Color.Green, 45);

			arkitComponent = Scene.CreateComponent<ARKitComponent>();
			arkitComponent.Orientation = UIKit.UIInterfaceOrientation.Portrait;
			arkitComponent.ARConfiguration = new ARWorldTrackingConfiguration {
				PlaneDetection = 
				ARPlaneDetection.Horizontal
#if false // uncomment once Xamarin.iOS with xcode93 is released:
				| ARPlaneDetection.Vertical,
#endif
			};

			arkitComponent.DidAddAnchors += ArkitComponent_DidAddAnchors;
			arkitComponent.DidRemoveAnchors += ArkitComponent_DidRemoveAnchors;
			arkitComponent.DidUpdateAnchors += ArkitComponent_DidUpdateAnchors;
			arkitComponent.ARFrame += ArkitComponent_ARFrame;
			arkitComponent.RunEngineFramesInARKitCallbakcs = Options.DelayedStart;
			arkitComponent.Run();

			anchorsNode = Scene.CreateChild();

			loadingLabel = new Text {
				Value = "Detecting planes...",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = HorizontalAlignment.Center,
			};
			loadingLabel.SetColor(new Color(0.5f, 0.9f, 0.1f));
			loadingLabel.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 35);
			UI.Root.AddChild(loadingLabel);

			Input.TouchEnd += Input_TouchEnd;
		}

		void ArkitComponent_ARFrame(ARFrame frame)
		{
		}

		void ArkitComponent_DidUpdateAnchors(ARAnchor[] anchors)
		{
			loadingLabel?.Remove();
			loadingLabel = null;

			foreach (var anchor in anchors)
			{
				var node = anchorsNode.GetChild(anchor.Identifier.ToString());
				UpdateAnchor(node, anchor);
			}
		}

		void ArkitComponent_DidRemoveAnchors(ARAnchor[] anchors)
		{
			foreach (var anchor in anchors)
				anchorsNode.GetChild(anchor.Identifier.ToString())?.Remove();
		}

		void ArkitComponent_DidAddAnchors(ARAnchor[] anchors)
		{
			foreach (var anchor in anchors)
				UpdateAnchor(null, anchor);
		}

		void UpdateAnchor(Node node, ARAnchor anchor)
		{
			var planeAnchor = anchor as ARPlaneAnchor;
			if (planeAnchor == null)
				return;
			
			Material tileMaterial = null;
			Node planeNode = null;
			if (node == null)
			{
				var id = planeAnchor.Identifier.ToString();
				node = anchorsNode.CreateChild(id);
				planeNode = node.CreateChild("SubPlane");
				var plane = planeNode.CreateComponent<StaticModel>();
				planeNode.Position = new Vector3();
				plane.Model = CoreAssets.Models.Plane;

				tileMaterial = new Material();
				tileMaterial.SetTexture(TextureUnit.Diffuse, CoreAssets.Textures.PlaneTile);
				var tech = new Technique();
				var pass = tech.CreatePass("alpha");
				pass.DepthWrite = false;
				pass.BlendMode = BlendMode.Alpha;
				pass.PixelShader = "PlaneTile"; //defined in CoreData (see PlaneTile.glsl)
				pass.VertexShader = "PlaneTile";
				tileMaterial.SetTechnique(0, tech);
				tileMaterial.SetShaderParameter("MeshColor", Randoms.NextColor());
				tileMaterial.SetShaderParameter("MeshAlpha", 0.8f); // set 0.0f if you want to hide them
				tileMaterial.SetShaderParameter("MeshScale", 30.0f);

				var planeRb = planeNode.CreateComponent<RigidBody>();
				planeRb.Friction = 1.5f;
				CollisionShape shape = planeNode.CreateComponent<CollisionShape>();
				shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);

				plane.Material = tileMaterial;
				//var animation = new ValueAnimation();
				//animation.SetKeyFrame(0.0f, 1f);
				//animation.SetKeyFrame(0.8f, 0.0f);
				//tileMaterial.SetShaderParameterAnimation("MeshAlpha", animation, WrapMode.Once, 1.0f);
			}
			else
			{
				planeNode = node.GetChild("SubPlane");
				tileMaterial = planeNode.GetComponent<StaticModel>().Material;
			}

			arkitComponent.ApplyOpenTkTransform(node, planeAnchor.Transform, true);

			planeNode.Scale = new Vector3(planeAnchor.Extent.X, 0.1f, planeAnchor.Extent.Z);
			planeNode.Position = new Vector3(planeAnchor.Center.X, planeAnchor.Center.Y, -planeAnchor.Center.Z);
		}


		void ThrowBall()
		{
			// Create a ball (will be cloned)
			var ballNode = Scene.CreateChild();
			ballNode.Position = CameraNode.Position;
			ballNode.Rotation = CameraNode.Rotation;
			ballNode.SetScale(0.3f); 

			var ball = ballNode.CreateComponent<StaticModel>();
			ball.Model = CoreAssets.Models.Sphere;
			ball.SetMaterial(Randoms.Next() > 0.5f ? 
			                 Material.FromImage("Textures/Earth.jpg", "Textures/Earth_NormalsMap.png") :
			                 Material.FromImage("Textures/Moon.jpg", "Textures/Moon_NormalsMap.png"));

			var body = ballNode.CreateComponent<RigidBody>();
			body.Mass = 0.5f;
			body.Friction = 0.5f;
			body.RollingFriction = 0.5f;
			var shape = ballNode.CreateComponent<CollisionShape>();
			shape.SetSphere(1f, Vector3.Zero, Quaternion.Identity);
			body.SetLinearVelocity(CameraNode.Rotation * new Vector3(0f, 0.25f, 2f) * 3 /*velocity*/);
		}

		void Input_TouchEnd(TouchEndEventArgs e)
		{
			ThrowBall();
		}
	}
}
