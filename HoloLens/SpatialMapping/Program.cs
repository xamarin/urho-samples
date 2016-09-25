using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.HoloLens;

namespace SpatialMapping
{
	/// <summary>
	/// Windows Holographic application using UrhoSharp.
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// Defines the entry point of the application.
		/// </summary>
		[MTAThread]
		private static void Main()
		{
			var exclusiveViewApplicationSource = new AppViewSource();
			CoreApplication.Run(exclusiveViewApplicationSource);
		}

		class AppViewSource : IFrameworkViewSource
		{
			public IFrameworkView CreateView() => UrhoAppView.Create<SpatialMappingApp>(null);
		}
	}


	public class SpatialMappingApp : HoloApplication
	{
		bool wireframe;
		bool mappingEnded;
		Vector3 envPositionBeforeManipulations;
		Node environmentNode;

		public SpatialMappingApp(string pak, bool emulation) : base(pak, emulation) { }

		protected override async void Start()
		{
			base.Start();

			environmentNode = Scene.CreateChild();
			EnableGestureTapped = true;

			// make sure 'spatialMapping' capabilaty is enabled in the app manifest.
			var cortanaAllowed = await RegisterCortanaCommands(new Dictionary<string, Action> {
					{ "show results" , ShowResults }
				});
			var spatialMappingAllowed = await StartSpatialMapping(new Vector3(50, 50, 10), 1000);
		}

		async void ShowResults()
		{
			EnableGestureManipulation = true;
			mappingEnded = true;
			StopSpatialMapping();

			var material = new Material();
			material.CullMode = CullMode.Ccw;
			material.SetTechnique(0, CoreAssets.Techniques.NoTexture, 1, 1);
			material.SetShaderParameter("MatDiffColor", new Color(0.1f, 0.8f, 0.1f));

			foreach (var node in environmentNode.Children)
				node.GetComponent<StaticModel>().SetMaterial(material);

			var previewPosition = LeftCamera.Node.Position + (LeftCamera.Node.Direction * 0.5f);
			environmentNode.Position = previewPosition;

			await environmentNode.RunActionsAsync(new EaseOut(new ScaleTo(1f, 0.03f), 1f));
		}

		public override void OnGestureTapped(GazeInfo gaze)
		{
			if (mappingEnded)
				return;

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
			if (mappingEnded)
				return;

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