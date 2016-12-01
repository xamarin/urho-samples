using System;
using System.Collections.Generic;
using Shared;
using SmartHouse;
using Urho;

namespace SmartHome
{
	public class UrhoApp : Application
	{
		Node environmentNode;
		Node lightNode;
		Node humanNode;
		Scene scene;
		float yaw;
		float pitch;
		Node cameraNode;
		MonoDebugHud debugHud;
		Material material;
		List<Node> bulbNodes = new List<Node>();
		ScannerConnection connection;

		[Preserve]
		public UrhoApp(ApplicationOptions options) : base(options) {}

		protected override void Start()
		{
			debugHud = new MonoDebugHud(this) { FpsOnly = true };
			debugHud.Show();

			scene = new Scene();
			scene.CreateComponent<Octree>();
			var zone = scene.CreateComponent<Zone>();
			zone.AmbientColor = new Color(0.5f, 0.5f, 0.5f);

			cameraNode = scene.CreateChild();
			var camera = cameraNode.CreateComponent<Camera>();

			var viewport = new Viewport(scene, camera, null);
			// viewport.SetClearColor(Color.White);
			Renderer.SetViewport(0, viewport);

			lightNode = scene.CreateChild();
			lightNode.Position = new Vector3(0, 3, 0);
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;
			light.Brightness = 0.6f;
			light.Range = 200;

			environmentNode = scene.CreateChild();
			environmentNode.SetScale(0.1f);

			humanNode = environmentNode.CreateChild();
			humanNode.Position = new Vector3(0, -1f, 0);
			humanNode.SetScale(1f);
			var model = humanNode.CreateComponent<StaticModel>();
			model.Model = ResourceCache.GetModel("Jack.mdl");

			material = Material.FromColor(new Color(72/255f, 99/255f, 142/255f));

			yaw = -65;
			pitch = 55;
			cameraNode.Position = new Vector3(0.6f, 1.3f, -0.4f);
			cameraNode.Rotation = new Quaternion(pitch, yaw, 0);

			lightNode.SetDirection(new Vector3(-1, -1f, 0));
			InitTouchInput();
			var pointer = scene.CreateComponent<CubePointer>();
			pointer.PositionChanged += Pointer_PositionChanged;
		}

		void Pointer_PositionChanged(Vector3 position)
		{
			position /= environmentNode.Scale.X;
			connection?.Send(new PointerPositionChangedDto { Position = new Vector3Dto(position.X, position.Y, position.Z) });
		}

		public void AddOrUpdateSurface(SurfaceDto surface)
		{
			var surfaceNode = environmentNode.GetChild(surface.Id, false);

			StaticModel staticModel = null;
			bool isNew = false;
			if (surfaceNode != null)
			{
				staticModel = surfaceNode.GetComponent<StaticModel>();
			}
			else
			{
				isNew = true;
				surfaceNode = environmentNode.CreateChild(surface.Id);
				staticModel = surfaceNode.CreateComponent<StaticModel>();
			}
			
			staticModel.Model = CreateModelFromVertexData(surface);
			surfaceNode.Position = new Vector3(
				surface.BoundsCenter.X, 
				surface.BoundsCenter.Y, 
				surface.BoundsCenter.Z);
			surfaceNode.Rotation = new Quaternion(
				surface.BoundsOrientation.X, 
				surface.BoundsOrientation.Y,
				surface.BoundsOrientation.Z, 
				surface.BoundsOrientation.W);

			if (isNew)
				staticModel.SetMaterial(material.Clone(""));
		}

		unsafe Model CreateModelFromVertexData(SurfaceDto surface)
		{
			var model = new Model();
			var vertexBuffer = new VertexBuffer(Context, false);
			var indexBuffer = new IndexBuffer(Context, false);
			var geometry = new Geometry();

			vertexBuffer.Shadowed = true;
			vertexBuffer.SetSize((uint)surface.VertexData.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color, false);

			fixed (SpatialVertexDto* p = &surface.VertexData[0])
				vertexBuffer.SetData(p);

			var indexData = surface.IndexData;
			indexBuffer.Shadowed = true;
			indexBuffer.SetSize((uint)indexData.Length, false, false);
			indexBuffer.SetData(indexData);

			geometry.SetVertexBuffer(0, vertexBuffer);
			geometry.IndexBuffer = indexBuffer;
			geometry.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)indexData.Length, 0, (uint)surface.VertexData.Length, true);

			model.NumGeometries = 1;
			model.SetGeometry(0, 0, geometry);
			model.BoundingBox = new BoundingBox(new Vector3(-1.26f, -1.26f, -1.26f), new Vector3(1.26f, 1.26f, 1.26f));

			return model;
		}

		protected override void OnUpdate(float timeStep)
		{
			if (Input.NumTouches > 0)
			{
				// move
				if (Input.NumTouches == 1)
				{
					const float touchSensitivity = 2;
					TouchState state = Input.GetTouch(0);
					var camera = cameraNode.GetComponent<Camera>();
					yaw += touchSensitivity * camera.Fov / Graphics.Height * state.Delta.X;
					pitch += touchSensitivity * camera.Fov / Graphics.Height * state.Delta.Y;
					cameraNode.Rotation = new Quaternion(pitch, yaw, 0);
				}
				// multitouch zoom
				else if (Input.NumTouches == 2)
				{
					TouchState state1 = Input.GetTouch(0);
					TouchState state2 = Input.GetTouch(1);

					var distance1 = Distance(state1.Position, state2.Position);
					var distance2 = Distance(state1.LastPosition, state2.LastPosition);

					cameraNode.Translate(new Vector3(0, 0, (distance1 - distance2) / 300f));
				}
			}

			int moveSpeed = 1;
			if (Input.GetKeyDown(Key.W)) cameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.S)) cameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.A)) cameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.D)) cameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);

			base.OnUpdate(timeStep);
		}

		public void SetConnection(ScannerConnection connection) => this.connection = connection;

		public void ToggleLight(int index)
		{
			bulbNodes[index].Enabled = !bulbNodes[index].Enabled;
		}
		
		public void UpdateCurrentPosition(Vector3 position, Vector3 direction)
		{
			humanNode.Position = new Vector3(position.X, position.Y - 1f, position.Z);
			humanNode.SetDirection(new Vector3(direction.X, 0, direction.Z));
		}

		public void AddBulb(Vector3 position)
		{
			var bulbNode = environmentNode.CreateChild();
			bulbNode.Position = position - new Vector3(0, 0.2f, 0);
			var light = bulbNode.CreateComponent<Light>();
			//bulbNode.CreateComponent<Box>(); //debug
			bulbNode.SetScale(0.5f);
			bulbNode.Enabled = true;

			bulbNodes.Add(bulbNode);

			light.LightType = LightType.Point;
			light.Range = 0.5f;
			light.Color = Color.White;
			light.Brightness = 1.85f;
			light.CastShadows = true;
			light.SpecularIntensity = 2f;
		}

		void InitTouchInput()
		{
			var layout = ResourceCache.GetXmlFile("UI/ScreenJoystick2.xml");
			var screenJoystickIndex = Input.AddScreenJoystick(layout, ResourceCache.GetXmlFile("UI/DefaultStyle.xml"));
			Input.SetScreenJoystickVisible(screenJoystickIndex, false);
		}

		/// <summary>
		/// Distance between two 2D points (should be moved to IntVector2).
		/// </summary>
		float Distance(IntVector2 v1, IntVector2 v2)
		{
			return (float)Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y));
		}
	}
}
