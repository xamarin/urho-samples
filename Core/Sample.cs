//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;

namespace Urho.Samples
{
	public class Sample : Application
	{
		MonoDebugHud monoDebugHud;
		UrhoConsole console;
		DebugHud debugHud;
		ResourceCache cache;
		Sprite logoSprite;
		UI ui;

		protected const float PixelSize = 0.01f;
		protected const float TouchSensitivity = 2;
		protected float Yaw { get; set; }
		protected float Pitch { get; set; }
		protected bool TouchEnabled { get; set; }
		protected Node CameraNode { get; set; }

		protected Sample (Context ctx) : base (ctx) {}
	
		protected bool IsLogoVisible
		{
			get { return logoSprite.IsVisible(); }
			set { logoSprite.SetVisible(value); }
		}

		protected override void OnSceneUpdate(float timeStep, Scene scene)
		{
			if (!TouchEnabled || CameraNode == null)
				return;

			var input = Input;
			for (uint i = 0, num = input.NumTouches; i < num; ++i)
			{
				TouchState state = input.GetTouch(i);
				if (state.TouchedElement () != null)
					continue;

				if (state.Delta.X != 0 || state.Delta.Y != 0)
				{
					var camera = CameraNode.GetComponent<Camera> ();
					if (camera == null)
						return;
					var graphics = Graphics;
					Yaw += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.X;
					Pitch += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.Y;
					CameraNode.Rotation = new Quaternion (Pitch, Yaw, 0);
				}
				else
				{
					var cursor = UI.Cursor;
					if (cursor != null && cursor.IsVisible ())
						cursor.Position = state.Position;
				}
			}
		}

		readonly Random random = new Random();
		/// Return a random float between 0.0 (inclusive) and 1.0 (exclusive.)
		protected float NextRandom() { return (float)random.NextDouble(); }
		/// Return a random float between 0.0 and range, inclusive from both ends.
		protected float NextRandom(float range) { return (float)random.NextDouble() * range; }
		/// Return a random float between min and max, inclusive from both ends.
		protected float NextRandom(float min, float max) { return (float)((random.NextDouble() * (max - min)) + min); }
		/// Return a random integer between min and max - 1.
		protected int NextRandom(int min, int max) { return random.Next(min, max); }

		/// <summary>
		/// Joystick XML layout for mobile platforms
		/// </summary>
		protected virtual string JoystickLayoutPatch => string.Empty;

		public override void Start ()
		{
			base.Start();
			var platform = Runtime.Platform;
			switch (platform)
			{
				case "Android":
				case "iOS":
					InitTouchInput ();
					break;
			}

			monoDebugHud = new MonoDebugHud(this);
			monoDebugHud.Show();

			CreateLogo ();
			SetWindowAndTitleIcon ();
			CreateConsoleAndDebugHud ();
			SubscribeToKeyDown (HandleKeyDown);
		}

		/// <summary>
		/// Move camera for 2D samples
		/// </summary>
		protected void SimpleMoveCamera2D (float timeStep)
		{
			// Do not move if the UI has a focused element (the console)
			if (UI.FocusElement != null)
				return;

			Input input = Input;

			// Movement speed as world units per second
			const float moveSpeed = 4.0f;

			// Read WASD keys and move the camera scene node to the corresponding direction if they are pressed
			if (input.GetKeyDown(Key.W))
				CameraNode.Translate(Vector3.UnitY * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown(Key.S))
				CameraNode.Translate(new Vector3(0.0f, -1.0f, 0.0f) * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown(Key.A))
				CameraNode.Translate(new Vector3(-1.0f, 0.0f, 0.0f) * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown(Key.D))
				CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep, TransformSpace.Local);

			if (input.GetKeyDown(Key.PageUp))
			{
				Camera camera = CameraNode.GetComponent<Camera>();
				camera.Zoom = (camera.Zoom * 1.01f);
			}

			if (input.GetKeyDown(Key.PageDown))
			{
				Camera camera = CameraNode.GetComponent<Camera>();
				camera.Zoom = (camera.Zoom * 0.99f);
			}
		}

		/// <summary>
		/// Move camera for 3D samples
		/// </summary>
		protected void SimpleMoveCamera3D (float timeStep)
		{
			const float mouseSensitivity = .1f;
		
			if (UI.FocusElement != null)
				return;
			var input = Input;
			const float moveSpeed = 40f;

			var mouseMove = input.MouseMove;
			Yaw += mouseSensitivity * mouseMove.X;
			Pitch += mouseSensitivity * mouseMove.Y;
			Pitch = MathHelper.Clamp(Pitch, -90, 90);

			CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);

			if (input.GetKeyDown (Key.W))
				CameraNode.Translate (new Vector3(0,0,1) * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown (Key.S))
				CameraNode.Translate (new Vector3(0,0,-1) * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown (Key.A))
				CameraNode.Translate (new Vector3(-1,0,0) * moveSpeed * timeStep, TransformSpace.Local);
			if (input.GetKeyDown (Key.D))
				CameraNode.Translate (new Vector3(1,0,0) * moveSpeed * timeStep, TransformSpace.Local);
		}

		protected void SimpleCreateInstructionsWithWasd (string extra = "")
		{
			SimpleCreateInstructions("Use WASD keys and mouse/touch to move" + extra);
		}
	
		protected void SimpleCreateInstructions(string text = "")
		{
			var textElement = new Text(Context)
				{
					Value = text,
					HorizontalAlignment = HorizontalAlignment.HA_CENTER,
					VerticalAlignment = VerticalAlignment.VA_CENTER
				};
			textElement.SetFont(ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
			UI.Root.AddChild(textElement);
		}

		void CreateLogo()
		{
			cache = ResourceCache;
			var logoTexture = cache.GetTexture2D("Textures/LogoLarge.png");

			if (logoTexture == null)
				return;

			ui = UI;
			logoSprite = ui.Root.CreateSprite();
			logoSprite.Texture = logoTexture;
			int w = logoTexture.Width;
			int h = logoTexture.Height;
			logoSprite.SetScale(256.0f / w);
			logoSprite.SetSize(w, h);
			logoSprite.SetHotSpot(0, h);
			logoSprite.SetAlignment(HorizontalAlignment.HA_LEFT, VerticalAlignment.VA_BOTTOM);
			logoSprite.Opacity = 0.75f;
			logoSprite.Priority = -100;
		}

		void SetWindowAndTitleIcon()
		{
			var icon = cache.GetImage("Textures/UrhoIcon.png");
			Graphics.SetWindowIcon(icon);
			Graphics.WindowTitle = "UrhoSharp Sample";
		}

		void CreateConsoleAndDebugHud()
		{
			var xml = cache.GetXmlFile("UI/DefaultStyle.xml");
			console = Engine.CreateConsole();
			console.DefaultStyle = xml;
			console.Background.Opacity = 0.8f;

			debugHud = Engine.CreateDebugHud();
			debugHud.DefaultStyle = xml;
		}

		void HandleKeyDown(KeyDownEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Esc:
					Engine.Exit();
					return;
				case Key.F1:
					console.Toggle();
					return;
				case Key.F2:
					debugHud.ToggleAll();
					return;

				// GC tests
				case Key.N0:
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
					break;
			}

			if (UI.FocusElement == null)
				return;

			var renderer = Renderer;
			switch (e.Key)
			{
				case Key.N1:
					var quality = renderer.TextureQuality;
					++quality;
					if (quality > 2)
						quality = 0;
					renderer.TextureQuality = quality;
					break;

				case Key.N2:
					var mquality = renderer.MaterialQuality;
					++mquality;
					if (mquality > 2)
						mquality = 0;
					renderer.MaterialQuality = mquality;
					break;

				case Key.N3:
					renderer.SpecularLighting = !renderer.SpecularLighting;
					break;

				case Key.N4:
					renderer.DrawShadows = !renderer.DrawShadows;
					break;

				case Key.N5:
					var shadowMapSize = renderer.ShadowMapSize;
					shadowMapSize *= 2;
					if (shadowMapSize > 2048)
						shadowMapSize = 512;
					renderer.ShadowMapSize = shadowMapSize;
					break;

				// shadow depth and filtering quality
				case Key.N6:
					var q = renderer.ShadowQuality;
					q++;
					if (q > 3)
						q = 0;
					renderer.ShadowQuality = q;
					break;

				// occlusion culling
				case Key.N7:
					var o = !(renderer.MaxOccluderTriangles > 0);
					renderer.MaxOccluderTriangles = o ? 5000 : 0;
					break;

				// instancing
				case Key.N8:
					renderer.DynamicInstancing = !renderer.DynamicInstancing;
					break;
			}
		}

		void InitTouchInput()
		{
			TouchEnabled = true;
			var layout = ResourceCache.GetXmlFile("UI/ScreenJoystick_Samples.xml");
			if (!string.IsNullOrEmpty(JoystickLayoutPatch))
			{
				XMLFile patchXmlFile = new XMLFile(Context);
				patchXmlFile.FromString(JoystickLayoutPatch);
				layout.Patch(patchXmlFile);
			}
			var screenJoystickIndex = Input.AddScreenJoystick(layout, ResourceCache.GetXmlFile("UI/DefaultStyle.xml"));
			Input.SetScreenJoystickVisible(screenJoystickIndex, true);
		}
	}
}
