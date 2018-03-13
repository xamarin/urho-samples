using System;
using System.Linq;
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

		unsafe void UpdateAnchor(Node node, ARAnchor anchor)
		{
			var faceAnchor = anchor as ARFaceAnchor;
			if (faceAnchor == null)
				return;

			StaticModel faceModel;

			if (node == null)
			{
				var id = faceAnchor.Identifier.ToString();
				node = anchorsNode.CreateChild(id);
				faceModel = node.CreateComponent<StaticModel>();
			}
			else
				faceModel = node.GetComponent<StaticModel>();
			
			arkitComponent.ApplyOpenTkTransform(node, faceAnchor.Transform, true);
			
			var faceGeom = faceAnchor.Geometry;
			var finalVertices = faceGeom.GetVertices()
					.Select(v => new VertexBuffer.PositionNormalColor
					{
						Color = Color.Green.ToUInt(),
						Position = new Vector3(v.X, v.Y, -v.Z)
					}).ToArray();

			// calculate normals
			var indices = faceGeom.GetTriangleIndices();
			for (int i = 0; i < indices.Length; i += 3)
			{
				ref var v1 = ref finalVertices[indices[i]];
				ref var v2 = ref finalVertices[indices[i + 1]];
				ref var v3 = ref finalVertices[indices[i + 2]];

				Vector3 edge1 = v1.Position - v2.Position;
				Vector3 edge2 = v1.Position - v3.Position;

				var normal = Vector3.Cross(edge1, edge2);
				normal = new Vector3(normal.X, -normal.Y, -normal.Z);
				normal.Normalize();

				v1.Normal = normal;
				v2.Normal = normal;
				v3.Normal = normal;
			}
			var model = new Model();
			var vertexBuffer = new VertexBuffer(Context, false);
			var indexBuffer = new IndexBuffer(Context, false);
			var geometry = new Geometry();


			vertexBuffer.Shadowed = true;
			vertexBuffer.SetSize((uint)faceGeom.VertexCount, ElementMask.Position | ElementMask.Normal | ElementMask.Color, true);

			fixed (VertexBuffer.PositionNormalColor* p = &finalVertices[0])
				vertexBuffer.SetData((void*)p);

			indexBuffer.Shadowed = true;
			indexBuffer.SetSize((uint)indices.Length, false, true);
			indexBuffer.SetData(indices);

			geometry.SetVertexBuffer(0, vertexBuffer);
			geometry.IndexBuffer = indexBuffer;
			geometry.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)indices.Length, 0, (uint)faceGeom.VertexCount, true);

			model.NumGeometries = 1;
			model.SetGeometry(0, 0, geometry);
			model.BoundingBox = new BoundingBox(-Vector3.One * 0.01f, Vector3.One * 0.01f);
			faceModel.Model = model;

			if (faceModel.Material == null)
			{
				var mat = new Material();
				mat.SetTechnique(0, CoreAssets.Techniques.NoTextureVCol, 0);
				mat.CullMode = CullMode.Cw;
				faceModel.SetMaterial(0, mat);
			}

		}
	}
}