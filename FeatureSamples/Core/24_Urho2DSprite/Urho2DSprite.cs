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

using System.Collections.Generic;
using Urho.Urho2D;

namespace Urho.Samples
{
	public class Urho2DSprite : Sample
	{
		Scene scene;
		List<NodeInfo> spriteNodes;
		const uint NumSprites = 200;

		public Urho2DSprite(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateScene();
			SimpleCreateInstructionsWithWasd("\nuse PageUp PageDown keys to zoom.");
			SetupViewport();
		}

		protected override void OnUpdate(float timeStep)
		{
			SimpleMoveCamera2D(timeStep);

			var graphics = Graphics;
			float halfWidth = graphics.Width * 0.5f * PixelSize;
			float halfHeight = graphics.Height * 0.5f * PixelSize;

			foreach (var nodeInfo in spriteNodes)
			{
				Vector3 position = nodeInfo.Node.Position;
				Vector3 moveSpeed = nodeInfo.MoveSpeed;
				Vector3 newPosition = position + moveSpeed * timeStep;
				if (newPosition.X < -halfWidth || newPosition.X > halfWidth)
				{
					newPosition.X = position.X;
					moveSpeed.X = -moveSpeed.X;
					nodeInfo.MoveSpeed = moveSpeed;
				}
				if (newPosition.Y < -halfHeight || newPosition.Y > halfHeight)
				{
					newPosition.Y = position.Y;
					moveSpeed.Y = -moveSpeed.Y;
					nodeInfo.MoveSpeed = moveSpeed;
				}

				nodeInfo.Node.Position = (newPosition);
				nodeInfo.Node.Roll(nodeInfo.RotateSpeed * timeStep, TransformSpace.Local);
			}
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
			spriteNodes = new List<NodeInfo>((int) NumSprites);

			// Create camera node
			CameraNode = scene.CreateChild("Camera");
			// Set camera's position
			CameraNode.Position = (new Vector3(0.0f, 0.0f, -10.0f));

			Camera camera = CameraNode.CreateComponent<Camera>();
			camera.Orthographic = true;


			var graphics = Graphics;
			camera.OrthoSize=(float)graphics.Height * PixelSize;

			var cache = ResourceCache;
			// Get sprite
			Sprite2D sprite = cache.GetSprite2D("Urho2D/Aster.png");
			if (sprite == null)
				return;

			float halfWidth = graphics.Width * 0.5f * PixelSize;
			float halfHeight = graphics.Height * 0.5f * PixelSize;

			for (uint i = 0; i < NumSprites; ++i)
			{
				Node spriteNode = scene.CreateChild("StaticSprite2D");
				spriteNode.Position = (new Vector3(NextRandom(-halfWidth, halfWidth), NextRandom(-halfHeight, halfHeight), 0.0f));

				StaticSprite2D staticSprite = spriteNode.CreateComponent<StaticSprite2D>();
				// Set random color
				staticSprite.Color = (new Color(NextRandom(1.0f), NextRandom(1.0f), NextRandom(1.0f), 1.0f));
				// Set blend mode
				staticSprite.BlendMode = BlendMode.Alpha;
				// Set sprite
				staticSprite.Sprite=sprite;
				// Add to sprite node vector
				spriteNodes.Add(new NodeInfo(spriteNode, new Vector3(NextRandom(-2.0f, 2.0f), NextRandom(-2.0f, 2.0f), 0.0f), NextRandom(-90.0f, 90.0f)));
			}

			// Get animation set
			AnimationSet2D animationSet = cache.GetAnimationSet2D("Urho2D/GoldIcon.scml");
			if (animationSet == null)
				return;

			var spriteNode2 = scene.CreateChild("AnimatedSprite2D");
			spriteNode2.Position = (new Vector3(0.0f, 0.0f, -1.0f));

			AnimatedSprite2D animatedSprite = spriteNode2.CreateComponent<AnimatedSprite2D>();
			// Set animation
			animatedSprite.AnimationSet = animationSet;
			animatedSprite.SetAnimation("idle", LoopMode2D.Default);
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.WithZoomInAndOut;

		class NodeInfo
		{
			public Node Node { get; set; }
			public Vector3 MoveSpeed { get; set; }
			public float RotateSpeed { get; set; }

			public NodeInfo(Node node, Vector3 moveSpeed, float rotateSpeed)
			{
				Node = node;
				MoveSpeed = moveSpeed;
				RotateSpeed = rotateSpeed;
			}
		}
	}
}
