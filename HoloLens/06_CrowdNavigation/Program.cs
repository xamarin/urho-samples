using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.HoloLens;
using Urho.Navigation;
using Urho.Shapes;

namespace CrowdNavigation
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<CrowdApp>("Data"));
	}


	public class CrowdApp : HoloApplication
	{
		Node environmentNode;
		Material spatialMaterial;
		Node armyNode;
		CrowdManager crowdManager;
		bool debug = true;
		bool surfaceIsValid;
		bool positionIsSelected;
		Node positionSelectorNode;
		Box positionSelectorModel;

		readonly Color validPositionColor = Color.Green;
		readonly Color invalidPositionColor = Color.Red;

		const string WalkingAnimation = @"Animations/Mutant_Run.ani";
		const string IdleAnimation = @"Animations/Mutant_Idle0.ani";
		const string DeathAnimation = @"Animations/Mutant_Death.ani";

		public CrowdApp(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();
			environmentNode = Scene.CreateChild();
			
			// Allow tap gesture
			EnableGestureTapped = true;

			positionSelectorNode = Scene.CreateChild();
			positionSelectorNode.Scale = new Vector3(0.3f, 0.15f, 0.2f);
			positionSelectorModel = positionSelectorNode.CreateComponent<Box>();
			positionSelectorModel.ViewMask = 0x80000000;
			
			// Material for spatial surfaces
			spatialMaterial = new Material();
			spatialMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);

			DirectionalLight.Brightness += 0.3f;

			var microphoneAllowed = await RegisterCortanaCommands(new Dictionary<string, Action> {
				{ "debug mode", () => debug = !debug },
				{ "kill all", KillAll },
				{ "die", KillAll },
			});
			var spatialMappingAllowed = await StartSpatialMapping(new Vector3(50, 50, 10), 1200, onlyAdd: true);
		}

		void SubscribeToEvents()
		{
			Engine.PostRenderUpdate += args => {
				if (debug)
				{
					Scene.GetComponent<NavigationMesh>().DrawDebugGeometry(true);
					crowdManager.DrawDebugGeometry(true);
				}
			};

			crowdManager.SubscribeToCrowdAgentReposition(args =>
			{
				Node node = args.Node;
				Vector3 velocity = args.Velocity * -1;
				AnimationController animCtrl = node.GetComponent<AnimationController>();
				if (animCtrl != null)
				{
					float speed = velocity.Length;
					if (animCtrl.IsPlaying(WalkingAnimation))
					{
						float speedRatio = speed / args.CrowdAgent.MaxSpeed;
						// Face the direction of its velocity but moderate the turning speed based on the speed ratio as we do not have timeStep here
						node.SetRotationSilent(Quaternion.FromRotationTo(Vector3.UnitZ, velocity));
						// Throttle the animation speed based on agent speed ratio (ratio = 1 is full throttle)
						animCtrl.SetSpeed(WalkingAnimation, speedRatio);
					}
					else
						animCtrl.Play(WalkingAnimation, 0, true, 0.1f);

					// If speed is too low then stopping the animation
					if (speed < args.CrowdAgent.Radius)
					{
						animCtrl.Stop(WalkingAnimation, 0.8f);
						animCtrl.Play(IdleAnimation, 0, true, 0.2f);
					}
				}
			});
		}

		void KillAll()
		{
			foreach (var node in armyNode.Children.ToArray())
			{
				var anim = node.GetComponent<AnimationController>();
				var agent = node.GetComponent<CrowdAgent>();
				agent?.Remove();
				anim.Play(DeathAnimation, 0, false, 0.4f);
			}
		}

		Vector3? Raycast()
		{
			Ray cameraRay = LeftCamera.GetScreenRay(0.5f, 0.5f);
			var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (result != null)
			{
				return result.Value.Position;
			}
			return null;
		}

		void SpawnMutant(Vector3 pos, string name = "Mutant")
		{
			Node mutantNode = armyNode.CreateChild(name);
			mutantNode.Position = pos;
			mutantNode.SetScale(0.1f);
			var modelObject = mutantNode.CreateComponent<AnimatedModel>();
			
			modelObject.CastShadows = true;
			modelObject.Model = ResourceCache.GetModel("Models/Mutant.mdl");
			modelObject.SetMaterial(ResourceCache.GetMaterial("Materials/mutant_M.xml"));
			mutantNode.CreateComponent<AnimationController>().Play(IdleAnimation, 0, true, 0.2f);

			// Create the CrowdAgent
			var agent = mutantNode.CreateComponent<CrowdAgent>();
			agent.Height = 0.2f;
			agent.NavigationPushiness = NavigationPushiness.Medium;
			agent.MaxSpeed = 0.4f;
			agent.MaxAccel = 0.4f;
			agent.Radius = 0.03f;
			agent.NavigationQuality = NavigationQuality.High;
		}
		
		protected override void OnUpdate(float timeStep)
		{
			if (positionIsSelected)
				return;

			Ray cameraRay = RightCamera.GetScreenRay(0.5f, 0.5f);
			var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (result != null)
			{
				var angle = Vector3.CalculateAngle(new Vector3(0, 1, 0), result.Value.Normal);
				surfaceIsValid = angle < 0.3f; //allow only horizontal surfaces
				positionSelectorNode.Position = result.Value.Position;
				positionSelectorModel.Color = surfaceIsValid ? validPositionColor : invalidPositionColor;
			}
			else
			{
				// no spatial surfaces found
				surfaceIsValid = false;
				positionSelectorModel.Color = invalidPositionColor;
			}
		}

		public override void OnGestureTapped()
		{
			var hitPos = Raycast();
			if (hitPos == null)
				return;

			NavigationMesh navMesh;

			if (surfaceIsValid && !positionIsSelected)
			{
				positionIsSelected = true;
				positionSelectorNode.Remove();
				positionSelectorNode = null;
				Scene.CreateComponent<SpatialCursor>();

				navMesh = Scene.CreateComponent<NavigationMesh>();

				//this plane is a workaround 
				//TODO: build a navmesh using spatial data
				var planeNode = Scene.CreateChild();
				var plane = planeNode.CreateComponent<StaticModel>();
				plane.Model = CoreAssets.Models.Plane;
				plane.SetMaterial(spatialMaterial);
				planeNode.Scale = new Vector3(20, 1, 20);
				planeNode.Position = hitPos.Value;

				Scene.CreateComponent<Navigable>();

				navMesh.CellSize = 0.2f;
				navMesh.CellHeight = 0.02f;
				navMesh.DrawOffMeshConnections = true;
				navMesh.DrawNavAreas = true;
				navMesh.TileSize = 2;
				navMesh.AgentRadius = 0.05f;

				navMesh.Build();

				crowdManager = Scene.CreateComponent<CrowdManager>();
				var parameters = crowdManager.GetObstacleAvoidanceParams(0);
				parameters.VelBias = 0.5f;
				parameters.AdaptiveDivs = 7;
				parameters.AdaptiveRings = 3;
				parameters.AdaptiveDepth = 3;
				crowdManager.SetObstacleAvoidanceParams(0, parameters);
				armyNode = Scene.CreateChild();

				SubscribeToEvents();

				int mutantIndex = 1;
				for (int i = 0; i < 3; i++)
					for (int j = 0; j < 3; j++)
						SpawnMutant(new Vector3(hitPos.Value.X + 0.15f * i, hitPos.Value.Y, hitPos.Value.Z + 0.13f * j), "Mutant " + mutantIndex++);

				return;
			}

			if (positionIsSelected)
			{
				Scene.GetComponent<SpatialCursor>().ClickAnimation();
				navMesh = Scene.GetComponent<NavigationMesh>();
				Vector3 pathPos = navMesh.FindNearestPoint(hitPos.Value, new Vector3(0.1f, 0.1f, 0.1f));
				Scene.GetComponent<CrowdManager>().SetCrowdTarget(pathPos, Scene);
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

			if (isNew)
			{
				staticModel.SetMaterial(spatialMaterial);
				node.CreateComponent<Obstacle>();
			}
			else
			{
				//Update Collision shape
			}
		}
	}
}