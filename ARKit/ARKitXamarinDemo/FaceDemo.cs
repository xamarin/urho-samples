using System;
using ARKit;
using Urho;
using Urho.iOS;

namespace ARKitXamarinDemo
{
	public class FaceDemo : SimpleApplication
	{
		ARKitComponent arkitComponent;
		Node anchorsNode;

		[Preserve]
		public FaceDemo(ApplicationOptions opts) : base(opts) { }

		protected override unsafe void Start()
		{
			base.Start();

			anchorsNode = Scene.CreateChild();

			arkitComponent = Scene.CreateComponent<ARKitComponent>();
			arkitComponent.Orientation = UIKit.UIInterfaceOrientation.Portrait;
			arkitComponent.ARConfiguration = new ARFaceTrackingConfiguration();
			arkitComponent.DidAddAnchors += ArkitComponent_DidAddAnchors;
			arkitComponent.DidRemoveAnchors += ArkitComponent_DidRemoveAnchors;
			arkitComponent.DidUpdateAnchors += ArkitComponent_DidUpdateAnchors;
			arkitComponent.RunEngineFramesInARKitCallbakcs = Options.DelayedStart;
			arkitComponent.Run();
		}

		void ArkitComponent_DidUpdateAnchors(ARAnchor[] anchors)
		{
			foreach (var anchor in anchors)
			{
				var node = anchorsNode.GetChild(anchor.Identifier.ToString());
				UpdateAnchor(node, anchor);
			}
		}

		void ArkitComponent_DidAddAnchors(ARAnchor[] anchors)
		{
			foreach (var anchor in anchors)
			{
				UpdateAnchor(null, anchor);
			}
		}

		void ArkitComponent_DidRemoveAnchors(ARAnchor[] anchors)
		{
			foreach (var anchor in anchors)
			{
				anchorsNode.GetChild(anchor.Identifier.ToString())?.Remove();
			}
		}

		void UpdateAnchor(Node node, ARAnchor anchor)
		{
			var faceAnchor = anchor as ARFaceAnchor;
			if (faceAnchor == null)
				return;

			if (node == null)
			{
				var id = faceAnchor.Identifier.ToString();
				node = anchorsNode.CreateChild(id);
				var faceBoxNode = node.CreateChild();
				var box = faceBoxNode.CreateComponent<Urho.Shapes.Box>();
				faceBoxNode.Scale = new Vector3(0.15f, 0.15f, 0.01f);
			}

			arkitComponent.ApplyOpenTkTransform(node, faceAnchor.Transform);
		}
	}
}