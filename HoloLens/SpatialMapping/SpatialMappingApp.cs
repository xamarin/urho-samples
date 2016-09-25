using Urho;
using Urho.Holographics;

namespace SpatialMapping
{
	public class SpatialMappingApp : HoloApplication
	{
		bool wireframe;
		Node environmentNode;

		public SpatialMappingApp(string pak, bool emulation) : base(pak, emulation) { }

		protected override async void Start()
		{
			base.Start();

			environmentNode = Scene.CreateChild();
			EnableGestureTapped = true;

			// make sure 'spatialMapping' capabilaty is enabled in the app manifest.
			var spatialMappingAllowed = await StartSpatialMapping(new Vector3(50, 50, 10), 1200);
		}

		public override void OnGestureTapped(GazeInfo gaze)
		{
			wireframe = !wireframe;
			foreach (var node in environmentNode.Children)
			{
				var material = node.GetComponent<StaticModel>().GetMaterial(0);
				material.FillMode = wireframe ? FillMode.Wireframe : FillMode.Solid;
			}
		}

		public override Model GenerateModelFromSpatialSurface(SpatialMeshInfo surface)
		{
			//NOTE: not the main thread
			return base.GenerateModelFromSpatialSurface(surface);
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

			Material mat;
			Color startColor;
			Color endColor = new Color(0.3f, 0.3f, 0.3f);

			if (isNew)
			{
				startColor = Color.Blue;
				mat = Material.FromColor(endColor);
				staticModel.SetMaterial(mat);
			}
			else
			{
				startColor = Color.Red;
				mat = staticModel.GetMaterial(0);
			}

			mat.FillMode = wireframe ? FillMode.Wireframe : FillMode.Solid;
			var specColorAnimation = new ValueAnimation();
			specColorAnimation.SetKeyFrame(0.0f, startColor);
			specColorAnimation.SetKeyFrame(1.5f, endColor);
			mat.SetShaderParameterAnimation("MatDiffColor", specColorAnimation, WrapMode.Once, 1.0f);
		}
	}
}
