using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.HoloLens;

namespace SpatialMapping
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<SpatialMappingApp>());
	}


	public class SpatialMappingApp : HoloApplication
	{
		bool wireframe;
		Vector3 envPositionBeforeManipulations;
		Node environmentNode;

		public SpatialMappingApp(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();

			//change the default brigtness
			Zone.AmbientColor = new Color(0.2f, 0.2f, 0.2f);
			DirectionalLight.Brightness = 0.5f;

			environmentNode = Scene.CreateChild();
			EnableGestureTapped = true;

			await RegisterCortanaCommands(new Dictionary<string, Action> {
					{ "show results", ShowResults },
					{ "stop spatial mapping", StopSpatialMapping }
				});

			// requires 'spatialMapping' capabilaty
			await StartSpatialMapping(new Vector3(50, 50, 50));
		}

		async void ShowResults()
		{
			EnableGestureManipulation = true;
			environmentNode.Position = LeftCamera.Node.Position + LeftCamera.Node.Direction * 0.5f - new Vector3(0, 0.5f, 0);
			await environmentNode.RunActionsAsync(new EaseOut(new ScaleTo(1f, 0.03f), 1f));
		}

		public override void OnGestureDoubleTapped()
		{
			wireframe = !wireframe;
			foreach (var node in environmentNode.Children)
			{
				var material = node.GetComponent<StaticModel>().GetMaterial(0);
				material.FillMode = wireframe ? FillMode.Wireframe : FillMode.Solid;
			}
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
			Color endColor = new Color(0.8f, 0.8f, 0.8f);

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

		public override void OnGestureManipulationStarted()
		{
			envPositionBeforeManipulations = environmentNode.Position;
		}

		public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition)
		{
			environmentNode.Position = relativeHandPosition + envPositionBeforeManipulations;
		}
	}
}