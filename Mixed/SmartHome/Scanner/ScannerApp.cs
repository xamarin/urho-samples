using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.SharpReality;
using Urho.Shapes;

namespace SmartHome.HoloLens
{
	public class ScannerApp : StereoApplication
	{
		Node environmentNode;
		SpatialCursor cursor;
		Material material;
		Node cubeNode;
		ClientConnection clientConnection;

		public ScannerApp(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();
			clientConnection = new ClientConnection();
			clientConnection.Disconnected += ClientConnection_Disconnected;
			clientConnection.RegisterForRealtimeUpdate(GetCurrentPositionDto);
			clientConnection.RegisterFor<PointerPositionChangedDto>(OnClientPointerChanged);

			Zone.AmbientColor = new Color(0.3f, 0.3f, 0.3f);
			DirectionalLight.Brightness = 0.5f;

			environmentNode = Scene.CreateChild();
			EnableGestureTapped = true;

			cubeNode = environmentNode.CreateChild();
			cubeNode.SetScale(0.2f);
			cubeNode.Position = new Vector3(1000, 1000, 1000);
			var box = cubeNode.CreateComponent<Box>();
			box.Color = Color.White;

			var moveAction = new MoveBy(0.5f, new Vector3(0, 0.005f, 0));
			cubeNode.RunActions(new RepeatForever(new RotateBy(1f, 0, 120, 0)));
			cubeNode.RunActions(new RepeatForever(moveAction, moveAction.Reverse()));

			//material = Material.FromColor(Color.Gray); //-- debug mode
			material = Material.FromColor(Color.Transparent, true);

			await RegisterCortanaCommands(new Dictionary<string, Action> {
				{ "stop spatial mapping", StopSpatialMapping}
			});

			while (!await ConnectAsync()) { }
		}

		void OnClientPointerChanged(PointerPositionChangedDto obj)
		{
			InvokeOnMain(() => cubeNode.Position = new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z));
		}

		public async Task<bool> ConnectAsync()
		{
			cursor?.Remove();
			cursor = null;

			var textNode = LeftCamera.Node.CreateChild();
			textNode.Position = new Vector3(0, 0, 1);
			textNode.SetScale(0.1f);
			var text = textNode.CreateComponent<Text3D>();
			text.Text = "Look at the QR code\nopened in Android/iOS/UWP app...";
			text.HorizontalAlignment = HorizontalAlignment.Center;
			text.VerticalAlignment = VerticalAlignment.Center;
			text.TextAlignment = HorizontalAlignment.Center;
			text.SetFont(CoreAssets.Fonts.AnonymousPro, 20);
			text.SetColor(Color.Green);

			string ipAddressString = "", ip = "";
			int port;
			while (!Utils.TryParseIpAddress(ipAddressString, out ip, out port))
			{
#if VIDEO_RECORDING //see OnGestureDoubleTapped for comments
				ipAddressString = await fakeQrCodeResultTaskSource.Task; 
#else
				ipAddressString = await QrCodeReader.ReadAsync();
#endif
			}

			InvokeOnMain(() => text.Text = "Connecting...");

			if (await clientConnection.ConnectAsync(ip, port))
			{
				InvokeOnMain(() => text.Text = "Connected!");
				await environmentNode.RunActionsAsync(new DelayTime(2));
				await StartSpatialMapping(new Vector3(100, 100, 100));
				InvokeOnMain(() =>
					{
						textNode.Remove();
						cursor = Scene.CreateComponent<SpatialCursor>();
					});
				return true;
			}
			return false;
		}

		async void ClientConnection_Disconnected()
		{
			StopSpatialMapping();
			await TextToSpeech("Disconnected");
			while (!await ConnectAsync()) { }
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			DirectionalLight.Node.SetDirection(LeftCamera.Node.Direction);
		}

		public override void OnGestureTapped()
		{
			if (!clientConnection.Connected || cursor == null)
				return;

			var pos = cursor.CursorNode.WorldPosition;
			var child = Scene.CreateChild();
			child.Scale = new Vector3(1, 1f, 1) / 10;
			child.Position = pos;
			var box = child.CreateComponent<Box>();
			box.SetMaterial(Material.FromColor(Color.Yellow, true));

			clientConnection.SendObject(new BulbAddedDto { Position = new Vector3Dto(pos.X, pos.Y, pos.Z)});
		}

		public override unsafe void OnSurfaceAddedOrUpdated(SpatialMeshInfo surface, Model generatedModel)
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
				staticModel.SetMaterial(material);
			}

			var surfaceDto = new SurfaceDto
			{
				Id = surface.SurfaceId,
				IndexData = surface.IndexData,
				BoundsCenter = new Vector3Dto(surface.BoundsCenter.X, surface.BoundsCenter.Y, surface.BoundsCenter.Z),
				BoundsOrientation = new Vector4Dto(surface.BoundsRotation.X, 
					surface.BoundsRotation.Y, surface.BoundsRotation.Z, surface.BoundsRotation.W),
				BoundsExtents = new Vector3Dto(surface.Extents.X, surface.Extents.Y, surface.Extents.Z)
			};

			var vertexData = surface.VertexData;
			surfaceDto.VertexData = new SpatialVertexDto[vertexData.Length];
			for (int i = 0; i < vertexData.Length; i++)
			{
				SpatialVertex vertexItem = vertexData[i];
				surfaceDto.VertexData[i] = *(SpatialVertexDto*)(void*)&vertexItem;
			}

			clientConnection.SendObject(surfaceDto.Id, surfaceDto);
		}

		BaseDto GetCurrentPositionDto()
		{
			var position = LeftCamera.Node.Position;
			var direction = LeftCamera.Node.Direction;
			return new CurrentPositionDto
			{
				Position = new Vector3Dto(position.X, position.Y, position.Z),
				Direction = new Vector3Dto(direction.X, direction.Y, direction.Z)
			};
		}

#if VIDEO_RECORDING
		TaskCompletionSource<string> fakeQrCodeResultTaskSource = new TaskCompletionSource<string>();
		public override void OnGestureDoubleTapped()
		{
			// Unfortunately, it's not allowed to record a video ("Hey Cortana, start recording")
			// and grab frames (in order to read a QR) at the same time - it will crash.
			// so I use a fake QR code result for the demo purposes
			// it is emulated by a double tap gesture
			Task.Run(() => fakeQrCodeResultTaskSource.TrySetResult("192.168.1.6:5206"));
		}
#endif
	}
}