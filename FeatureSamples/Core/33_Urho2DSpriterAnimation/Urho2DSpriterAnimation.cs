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
	public class Urho2DSpriterAnimation : Sample
	{
		Scene scene;
		Node spriteNode;
		int animationIndex;
		static readonly string[] AnimationNames =
			{
				"idle",
				"run",
				"attack",
				"hit",
				"dead",
				"dead2",
				"dead3",
			};

		public Urho2DSpriterAnimation(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructions("Mouse click to play next animation, \nUse WASD keys to move, use PageUp PageDown keys to zoom.");
			SetupViewport();
			SubscribeToEvents();
		}

		void SubscribeToEvents()
		{
			Input.SubscribeToMouseButtonDown(args =>
				{
					AnimatedSprite2D animatedSprite = spriteNode.GetComponent<AnimatedSprite2D>();
					animationIndex = (animationIndex + 1) % 7;
					animatedSprite.SetAnimation(AnimationNames[animationIndex], LoopMode2D.ForceLooped);
				});
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
			camera.OrthoSize=graphics.Height * PixelSize;
			camera.Zoom = 1.5f * Math.Min(graphics.Width / 1280.0f, graphics.Height / 800.0f); // Set zoom according to user's resolution to ensure full visibility (initial zoom (1.5) is set for full visibility at 1280x800 resolution)

			var cache = ResourceCache;
			AnimationSet2D animationSet = cache.GetAnimationSet2D("Urho2D/imp/imp.scml");
			if (animationSet == null)
				return;

			spriteNode = scene.CreateChild("SpriterAnimation");

			AnimatedSprite2D animatedSprite = spriteNode.CreateComponent<AnimatedSprite2D>();
			animatedSprite.AnimationSet = animationSet;
			animatedSprite.SetAnimation(AnimationNames[animationIndex], LoopMode2D.Default);
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithZoomInAndOut;
	}
}
