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
using Urho.Gui;
using Urho.Urho2D;

namespace Urho.Samples
{
	public class Sprites : Sample
	{
		readonly Dictionary<Sprite, Vector2> spritesWithVelocities = new Dictionary<Sprite, Vector2>();
		// Number of sprites to draw
		const uint NumSprites = 100;

		public Sprites(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			CreateSprites();
		}

		protected override void OnUpdate(float timeStep)
		{
			MoveSprites(timeStep);
			base.OnUpdate(timeStep);
		}

		void CreateSprites()
		{
			var cache = ResourceCache;
			var graphics = Graphics;
			UI ui = UI;

			// Get the Urho3D fish texture
			Texture2D decalTex = cache.GetTexture2D("Textures/UrhoDecal.dds");

			for (uint i = 0; i < NumSprites; ++i)
			{
				// Create a new sprite, set it to use the texture
				Sprite sprite = new Sprite();
				sprite.Texture = decalTex;

				// The UI root element is as big as the rendering window, set random position within it
				sprite.Position = new IntVector2((int) (NextRandom()*graphics.Width), (int) (NextRandom()*graphics.Height));

				// Set sprite size & hotspot in its center
				sprite.Size = new IntVector2(128, 128);
				sprite.HotSpot = new IntVector2(64, 64);

				// Set random rotation in degrees and random scale
				sprite.Rotation = NextRandom()*360.0f;
				sprite.SetScale(NextRandom(1.0f) + 0.5f);

				// Set random color and additive blending mode
				sprite.SetColor(new Color(NextRandom(0.5f) + 0.5f, NextRandom(0.5f) + 0.5f, NextRandom(0.5f) + 0.5f));
				sprite.BlendMode = BlendMode.Add;

				// Add as a child of the root UI element
				ui.Root.AddChild(sprite);

				// Store sprites to our own container for easy movement update iteration
				spritesWithVelocities[sprite] = new Vector2(NextRandom(200.0f) - 100.0f, NextRandom(200.0f) - 100.0f);
			}
		}

		void MoveSprites(float timeStep)
		{
			var graphics = Graphics;
			int width = graphics.Width;
			int height = graphics.Height;

			// Go through all sprites

			foreach (var item in spritesWithVelocities)
			{
				var sprite = item.Key;
				var vector = item.Value;

				// Rotate
				float newRot = sprite.Rotation + timeStep * 30.0f;
				sprite.Rotation=newRot;

				var x = vector.X * timeStep + sprite.Position.X;
				var y = vector.Y * timeStep + sprite.Position.Y;

				if (x < 0.0f)
					x += width;
				if (x >= width)
					x -= width;
				if (y < 0.0f)
					y += height;
				if (y >= height)
					y -= height;

				sprite.Position = new IntVector2((int) x, (int) y);
			}
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.Hidden;
	}
}
