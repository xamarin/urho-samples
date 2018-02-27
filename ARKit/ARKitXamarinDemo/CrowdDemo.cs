using System;
using System.Linq;
using ARKit;
using Urho;
using Urho.Actions;
using Urho.Audio;
using Urho.Gui;
using Urho.iOS;
using Urho.Navigation;
using Urho.Physics;

namespace ARKitXamarinDemo
{
	public class CrowdDemo : SimpleApplication
	{
		Node armyNode;
		CrowdManager crowdManager;
		bool debug = true;
		bool surfaceIsValid;
		bool positionIsSelected;
		Node cursorNode;
		StaticModel cursorModel;
		Material lastMutantMat;
		DynamicNavigationMesh navMesh;
		Text loadingLabel;
		bool detectingFirstPlane = true;
		ARKitComponent arkitComponent;
		Node anchorsNode;
		bool PlaneDetectionEnabled = true;
		bool ContinuesHitTestAtCenter;
		Vector3? LastHitTest;

		const string WalkingAnimation = @"Animations/Mutant_Run.ani";
		const string IdleAnimation = @"Animations/Mutant_Idle0.ani";
		const string DeathAnimation = @"Animations/Mutant_Death.ani";
		const string DanceAnimation = @"Animations/Mutant_HipHop1.ani";
		const string MutantModel = @"Models/Mutant.mdl";
		const string MutantMaterial = @"Materials/mutant_M.xml";

		SoundSource themeSoundSource;
		SoundSource actionSoundSource;

		[Preserve]
		public CrowdDemo(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();

			arkitComponent = Scene.CreateComponent<ARKitComponent>();
			arkitComponent.Orientation = UIKit.UIInterfaceOrientation.Portrait;
			arkitComponent.ARConfiguration = new ARWorldTrackingConfiguration {
				PlaneDetection = ARPlaneDetection.Horizontal,
			};
			arkitComponent.DidAddAnchors += ArkitComponent_DidAddAnchors;
			arkitComponent.DidRemoveAnchors += ArkitComponent_DidRemoveAnchors;
			arkitComponent.DidUpdateAnchors += ArkitComponent_DidUpdateAnchors;
			arkitComponent.RunEngineFramesInARKitCallbakcs = Options.DelayedStart;
			arkitComponent.ARFrame += ArkitComponent_ARFrame;
			arkitComponent.Run();

			ContinuesHitTestAtCenter = true;

			Scene.CreateComponent<DebugRenderer>();

			cursorNode = Scene.CreateChild();
			cursorNode.Position = Vector3.UnitZ * 100; //hide cursor at start - pos at (0,0,100) 
			cursorModel = cursorNode.CreateComponent<Urho.Shapes.Plane>();
			cursorModel.ViewMask = 0x80000000; //hide from raycasts (Raycast() uses a differen viewmask so the cursor won't be visible for it)
			cursorNode.RunActions(new RepeatForever(new ScaleTo(0.3f, 0.15f), new ScaleTo(0.3f, 0.2f)));

			anchorsNode = Scene.CreateChild();

			var cursorMaterial = new Material();
			cursorMaterial.SetTexture(TextureUnit.Diffuse, ResourceCache.GetTexture2D("Textures/Cursor.png"));
			cursorMaterial.SetTechnique(0, CoreAssets.Techniques.DiffAlpha);
			cursorModel.Material = cursorMaterial;
			cursorModel.CastShadows = false;

			Input.TouchEnd += args => OnGestureTapped(args.X, args.Y);
			UnhandledException += OnUnhandledException;

			loadingLabel = new Text {
				Value = "Detecting planes...",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = HorizontalAlignment.Center,
			};

			loadingLabel.SetColor(new Color(0.5f, 1f, 0f));
			loadingLabel.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 42);
			UI.Root.AddChild(loadingLabel);

			actionSoundSource = Scene.CreateComponent<SoundSource>();

			Node musicNode = Scene.CreateChild("Music");
			themeSoundSource = musicNode.CreateComponent<SoundSource>();
			themeSoundSource.SetSoundType(SoundType.Music.ToString());
			themeSoundSource.Gain = 0.1f;
		}


		void ArkitComponent_ARFrame(ARFrame frame)
		{
			if (ContinuesHitTestAtCenter)
			{
				LastHitTest = arkitComponent.HitTest(frame, 0.5f, 0.5f);
			}
		}

		void ArkitComponent_DidUpdateAnchors(ARAnchor[] anchors)
		{
			if (!PlaneDetectionEnabled)
				return;
			
			foreach (var anchor in anchors.ToArray())
			{
				var node = anchorsNode.GetChild(anchor.Identifier.ToString());
				UpdateAnchor(node, anchor);
			}
		}

		void ArkitComponent_DidAddAnchors(ARAnchor[] anchors)
		{
			if (!PlaneDetectionEnabled)
				return;
			
			foreach (var anchor in anchors.ToArray())
			{
				UpdateAnchor(null, anchor);
			}
		}

		void ArkitComponent_DidRemoveAnchors(ARAnchor[] anchors)
		{
			if (!PlaneDetectionEnabled)
				return;
			
			foreach (var anchor in anchors.ToArray())
			{
				anchorsNode.GetChild(anchor.Identifier.ToString())?.Remove();
			}
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
				tileMaterial.SetTexture(TextureUnit.Diffuse, ResourceCache.GetTexture2D("Textures/PlaneTile.png"));
				var tech = new Technique();
				var pass = tech.CreatePass("alpha");
				pass.DepthWrite = false;
				pass.BlendMode = BlendMode.Alpha;
				pass.PixelShader = "PlaneTile";
				pass.VertexShader = "PlaneTile";
				tileMaterial.SetTechnique(0, tech);
				tileMaterial.SetShaderParameter("MeshColor", new Color(Randoms.Next(), 1, Randoms.Next()));
				tileMaterial.SetShaderParameter("MeshAlpha", 0.75f); // set 0.0f if you want to hide them
				tileMaterial.SetShaderParameter("MeshScale", 32.0f);

				var planeRb = planeNode.CreateComponent<RigidBody>();
				planeRb.Friction = 1.5f;
				CollisionShape shape = planeNode.CreateComponent<CollisionShape>();
				shape.SetBox(Vector3.One, Vector3.Zero, Quaternion.Identity);

				plane.Material = tileMaterial;
			}
			else
			{
				planeNode = node.GetChild("SubPlane");
				tileMaterial = planeNode.GetComponent<StaticModel>().Material;
			}

			arkitComponent.ApplyOpenTkTransform(node, planeAnchor.Transform, true);

			planeNode.Scale = new Vector3(planeAnchor.Extent.X, 0.1f, planeAnchor.Extent.Z);
			planeNode.Position = new Vector3(planeAnchor.Center.X, planeAnchor.Center.Y, -planeAnchor.Center.Z);

			//var animation = new ValueAnimation();
			//animation.SetKeyFrame(0.0f, 0.3f);
			//animation.SetKeyFrame(0.5f, 0.0f);
			//tileMaterial.SetShaderParameterAnimation("MeshAlpha", animation, WrapMode.Once, 1.0f);

			//Debug.WriteLine($"ARPlaneAnchor  Extent({planeAnchor.Extent}), Center({planeAnchor.Center}), Position({planeAnchor.Transform.Row3}");

		}

		void OnUnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			System.Console.WriteLine(e);
		}

		void SubscribeToEvents()
		{
			//debug:
			if (false) {
				Engine.PostRenderUpdate += e => {
					Scene.GetComponent<DynamicNavigationMesh>().DrawDebugGeometry(true);
					crowdManager.DrawDebugGeometry(true);
				};
			}

			crowdManager.CrowdAgentReposition += args => {
				Node node = args.Node;
				Vector3 velocity = args.Velocity * -1;
				var animCtrl = node.GetComponent<AnimationController>();
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
			};
		}

		async void KillAll()
		{
			actionSoundSource.Play(ResourceCache.GetSound("Sounds/death.wav"));
			foreach (var node in armyNode.Children.ToArray())
			{
				var anim = node.GetComponent<AnimationController>();
				var agent = node.GetComponent<CrowdAgent>();
				agent?.Remove();
				await Delay(Randoms.Next(0f, 0.5f));
				anim.Play(DeathAnimation, 0, false, 0.4f);
			}
		}

		Vector3? Raycast(float x, float y)
		{
			Ray cameraRay = Camera.GetScreenRay(x, y);
			var result = Scene.GetComponent<Octree>().RaycastSingle(cameraRay, 
				RayQueryLevel.Triangle, 100, DrawableFlags.Geometry, 0x70000000);
			if (result != null)
			{
				return result.Value.Position;
			}
			return null;
		}

		void HighlightMaterial(Material material, bool higlight)
		{
			material.SetShaderParameter("OutlineColor", higlight ? new Color(1f, 0.75f, 0, 0.5f) : Color.Transparent);
			material.SetShaderParameter("OutlineWidth", higlight ? 0.009f : 0f);
		}

		void SpawnMutant(Vector3 pos, string name = "Mutant")
		{
			Node mutantNode = armyNode.CreateChild(name);
			mutantNode.Position = pos;
			mutantNode.SetScale(0.2f);
			var modelObject = mutantNode.CreateComponent<AnimatedModel>();

			modelObject.CastShadows = true;
			modelObject.Model = ResourceCache.GetModel(MutantModel);
			modelObject.SetMaterial(ResourceCache.GetMaterial(MutantMaterial).Clone());
			modelObject.Material.SetTechnique(0, ResourceCache.GetTechnique("Techniques/DiffOutline.xml"));
			HighlightMaterial(modelObject.Material, false);

			var shadowPlaneNode = mutantNode.CreateChild();
			shadowPlaneNode.Scale = new Vector3(10, 1, 10);
			//shadowPlaneNode.CreateComponent<Urho.SharpReality.TransparentPlaneWithShadows>();

			mutantNode.CreateComponent<AnimationController>().Play(IdleAnimation, 0, true, 0.2f);

			// Create the CrowdAgent
			var agent = mutantNode.CreateComponent<CrowdAgent>();
			agent.Height = 0.2f;
			agent.NavigationPushiness = NavigationPushiness.Medium;
			agent.MaxSpeed = 0.6f;
			agent.MaxAccel = 0.6f;
			agent.Radius = 0.06f;
			agent.NavigationQuality = NavigationQuality.Medium;
		}

		protected override void OnUpdate(float timeStep)
		{
			if (lastMutantMat != null)
			{
				HighlightMaterial(lastMutantMat, false);
				lastMutantMat = null;
			}

			base.OnUpdate(timeStep);
			if (positionIsSelected)
			{
				Ray cameraRay = Camera.GetScreenRay(0.5f, 0.5f);
				var result = Octree.RaycastSingle(cameraRay);
				if (result?.Node?.Name?.StartsWith("Mutant") == true)
				{
					var mat = ((StaticModel)result.Value.Drawable).Material;
					HighlightMaterial(mat, true);
					lastMutantMat = mat;
				}

				return;
			}

			if (LastHitTest != null)
			{
				if (detectingFirstPlane)
					loadingLabel.Value = "Look around to create a navigation mesh\nTap anywhere when you finish.\nTry to create the mesh as big as possible.";
				detectingFirstPlane = false;
				surfaceIsValid = true;
				cursorNode.Position = LastHitTest.Value;
			}
			cursorModel.Material.SetShaderParameter(CoreAssets.ShaderParameters.MatDiffColor, surfaceIsValid ? Color.White : Color.Red);
		}

		void OnGestureTapped(int argsX, int argsY)
		{
			// 3 touches at the same time kill everybody :-)
			if (Input.NumTouches == 3)
				KillAll();

			if (surfaceIsValid && !positionIsSelected)
			{
				UI.Root.RemoveChild(loadingLabel);
				loadingLabel = null;
				PlaneDetectionEnabled = false;
				//hide planes:

				foreach (var node in anchorsNode.Children.ToArray())
				{
					// if surface is higher than floor - mark as Obstacle
					if (Math.Abs(node.WorldPosition.Y - LastHitTest.Value.Y) >= 0.1f)
					{
						node.CreateComponent<Obstacle>();
					}

					var model = node.GetChild("SubPlane").GetComponent<StaticModel>();
					model.Material.SetShaderParameter("MeshColor", Color.Transparent);
				}


				var music = ResourceCache.GetSound("Sounds/theme.ogg");
				music.Looped = true;
				themeSoundSource.Play(music);

				ContinuesHitTestAtCenter = false;
				var hitPos = cursorNode.Position;// - Vector3.UnitZ * 0.01f;
				positionIsSelected = true;

				navMesh = Scene.CreateComponent<DynamicNavigationMesh>();
				Scene.CreateComponent<Navigable>();

				navMesh.CellSize = 0.01f;
				navMesh.CellHeight = 0.05f;
				navMesh.DrawOffMeshConnections = true;
				navMesh.DrawNavAreas = true;
				navMesh.TileSize = 1;
				navMesh.AgentRadius = 0.1f;

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
						SpawnMutant(new Vector3(hitPos.X + 0.15f * i, hitPos.Y, hitPos.Z + 0.13f * j), "Mutant " + mutantIndex++);

				return;
			}

			if (positionIsSelected)
			{
				var hitPos = Raycast((float)argsX / Graphics.Width, (float)argsY / Graphics.Height);
				if (hitPos == null)
					return;

				cursorNode.Position = hitPos.Value + Vector3.UnitY * 0.1f;
				Vector3 pathPos = navMesh.FindNearestPoint(hitPos.Value, new Vector3(0.1f, 0.1f, 0.1f) * 5);
				Scene.GetComponent<CrowdManager>().SetCrowdTarget(pathPos, Scene);


				var sound = ResourceCache.GetSound($"Sounds/go{rand.Next(1, 6)}.wav");
				actionSoundSource.Play(sound);
				actionSoundSource.Gain = 0.4f;
			}
		}

		Random rand = new Random();
	}
}