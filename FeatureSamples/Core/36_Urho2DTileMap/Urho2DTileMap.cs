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
using Urho.Urho2D;

namespace Urho.Samples
{
	public class Urho2DTileMap : Sample
	{
		Scene scene;

		public Urho2DTileMap(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd(", use PageUp PageDown keys to zoom.");
			SetupViewport();
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera2D(timeStep);
		}

		void SetupViewport()
		{
			var renderer = Renderer;
			renderer.SetViewport(0, new Viewport(Context, scene, CameraNode.GetComponent<Camera>(), null));
		}

		void CreateScene()
		{
			scene = new Scene();
			scene.CreateComponent<Octree>();

			// Create camera node
			CameraNode = scene.CreateChild("Camera");
			// Set camera's position
			CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));

			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.Orthographic = true;

			var graphics = Graphics;
			camera.OrthoSize=(float)graphics.Height * PixelSize;
			camera.Zoom = (1.0f * Math.Min((float)graphics.Width / 1280.0f, (float)graphics.Height / 800.0f)); // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.0) is set for full visibility at 1280x800 resolution)

			var cache = ResourceCache;
			// Get tmx file
			TmxFile2D tmxFile = cache.GetTmxFile2D("Urho2D/isometric_grass_and_water.tmx");
			if (tmxFile == null)
				return;

			Node tileMapNode = scene.CreateChild("TileMap");
			tileMapNode.Position = new Vector3(0.0f, 0.0f, -1.0f);

			TileMap2D tileMap = tileMapNode.CreateComponent<TileMap2D>();
			// Set animation
			tileMap.TmxFile = tmxFile;

			// Set camera's position
			TileMapInfo2D info = tileMap.Info;
			float x = info.MapWidth * 0.5f;
			float y = info.MapHeight * 0.5f;
			CameraNode.Position = new Vector3(x, y, -10.0f);
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithZoomInAndOut;
	}
}
