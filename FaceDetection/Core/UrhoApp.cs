using System;
using System.Linq;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;
using Urho.Actions;
using Urho.Repl;
using Urho.Urho2D;

namespace FaceDetection
{
	public class UrhoApp : Application
	{
		Texture2D cameraTexture;
		Node maskNode;
		Node planeNode;
		Scene scene;
		Camera camera;

		public const int VideoCapturingFPS = 0; //200, 60, 24, 0 (no limit)

		public UrhoApp(ApplicationOptions options) : base(options) {}

		public async void CaptureVideo(Func<Task<FrameWithFaces>> frameSource)
		{
			while (!Engine.Exiting && !IsDeleted)
			{
				DateTime time = DateTime.UtcNow;
				var result = await frameSource();
				InvokeOnMain(() => SetFrame(result));
				if (VideoCapturingFPS > 0)
				{
					var elapsedMs = (DateTime.UtcNow - time).TotalMilliseconds;
					var timeToWait = 1000d / VideoCapturingFPS - elapsedMs;
					if (timeToWait >= 0)
						await Task.Delay(TimeSpan.FromMilliseconds(timeToWait));
				}
			}
		}

		public async void Rotate()
		{
			if (planeNode == null)//not created yet
				return;
			var moveAction = new MoveBy(1f, new Vector3(0, 0, 5));
			var rotateAction = new RotateBy(duration: 1, deltaAngleX: 0, deltaAngleY: 90, deltaAngleZ: 20);
			//planeNode.GetComponent<StaticModel>().Model = CoreAssets.Models.Sphere;
			//planeNode.SetScale(7f);
			await planeNode.RunActionsAsync(new Urho.Actions.Parallel(moveAction, rotateAction));
			await planeNode.RunActionsAsync(new RepeatForever(new RotateBy(duration: 1, deltaAngleX: 0, deltaAngleY: 90, deltaAngleZ: 0)));
		}

		public unsafe void SetFrame(FrameWithFaces frame)
		{
			fixed (byte* bptr = frame.FrameData)
			{
				if (cameraTexture == null)
					CreateVideoTexturePlaceholder(frame.FrameWidth, frame.FrameHeight);
				cameraTexture?.SetData(0, 0, 0, frame.FrameWidth, frame.FrameHeight, bptr);
			}

			if (frame.Faces.Any())
				DrawMask(frame.FrameWidth, frame.FrameHeight, frame.Faces.First());
			else
				DrawMask(frame.FrameWidth, frame.FrameHeight, null);
		}

		void DrawMask(int frameW, int frameH, Rect? first)
		{
			if (!first.HasValue)
			{
				maskNode.SetScale(0f);
				return;
			}
			var faceBox = first.Value;

			var faceCenterX = faceBox.X + faceBox.Width / 2;
			var faceCenterY = faceBox.Y;
			//TODO: convert screen to World
			//var point = camera.ScreenToWorldPoint(new Vector3(faceCenterX / Graphics.Width, faceCenterY / Graphics.Height, 7));

			var maskX = 0f;
			var maskY = 0f;
			var halfW = frameW / 2f;
			var halfH = frameH / 2f;
			if (faceCenterX < halfW)
				maskX = -(1f - faceCenterX / halfW) * 1.3f;
			else
				maskX = (faceCenterX - halfW) / halfW;

			if (faceCenterY < halfH)
				maskY = (1f - faceCenterY / halfH);
			else
				maskY = -(faceCenterY - halfH) / halfH;

			float scale = faceBox.Width / 120f;
			maskNode.Scale = (new Vector3(1, 1, 1) / 4f) * scale;
			maskNode.Position = new Vector3(x: maskX, y: maskY - 0.12f, z: 3);
		}

		protected override void Start()
		{
			// UI text 
			var helloText = new Text(Context);
			helloText.Value = "UrhoSharp face detection";
			helloText.HorizontalAlignment = Urho.Gui.HorizontalAlignment.Center;
			helloText.VerticalAlignment = Urho.Gui.VerticalAlignment.Top;
			helloText.SetColor(new Color(r: 0f, g: 0f, b: 1f));
			helloText.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
			UI.Root.AddChild(helloText);

			// 3D scene with Octree
			scene = new Scene(Context);
			scene.CreateComponent<Octree>();

			// Mask
			maskNode = scene.CreateChild();
			maskNode.Position = new Vector3(x: 1, y: 0, z: 5);
			maskNode.Scale = new Vector3(1, 1, 1) / 3f;
			var leftEye = maskNode.CreateChild();
			var leftEyeModel = leftEye.CreateComponent<Urho.Shapes.Sphere>();
			var rightEye = maskNode.CreateChild();
			var rightEyeModel = rightEye.CreateComponent<Urho.Shapes.Sphere>();

			leftEye.Position = new Vector3(-0.6f, 0, 0);
			rightEye.Position = new Vector3(0.6f, 0, 0);

			leftEye.RunActions(new TintTo(1f, Randoms.Next(), Randoms.Next(), Randoms.Next()));
			rightEye.RunActions(new TintTo(1f, Randoms.Next(), Randoms.Next(), Randoms.Next()));

			// Light
			Node lightNode = scene.CreateChild();
			lightNode.Position = new Vector3(-2, 0, 0);
			var light = lightNode.CreateComponent<Light>();
			light.Range = 20;
			light.Brightness = 1f;

			// Camera
			Node cameraNode = scene.CreateChild();
			camera = cameraNode.CreateComponent<Camera>();

			// Viewport
			var vp = new Viewport(Context, scene, camera, null);
			Renderer.SetViewport(0, vp);
			vp.SetClearColor(Color.White);
		}

		void CreateVideoTexturePlaceholder(int width, int height)
		{
			cameraTexture = new Texture2D(this.Context);
			cameraTexture.SetNumLevels(1);
			cameraTexture.SetSize(width, height, Urho.Graphics.RGBFormat, TextureUsage.Dynamic);
			var material = new Material();
			material.SetTexture(TextureUnit.Diffuse, cameraTexture);
			material.SetTechnique(0, CoreAssets.Techniques.Diff, 0, 0);
			planeNode = scene.CreateChild();
			planeNode.Position = new Vector3(0, 0, 7);
			const float xScale = 5;
			planeNode.Scale = new Vector3(xScale, xScale * height / width, xScale);
			var planeModel = planeNode.CreateComponent<StaticModel>();
			planeModel.Model = CoreAssets.Models.Box;
			planeModel.SetMaterial(material);
		}
	}
}