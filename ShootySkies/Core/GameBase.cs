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
using Urho;

namespace ShootySkies
{
	public class GameBase : Application
	{
		protected const float TouchSensitivity = 2;
		protected bool TouchEnabled { get; set; }
		protected Node CameraNode { get; set; }
		protected MonoDebugHud MonoDebugHud { get; set; }

		protected GameBase(Context ctx, ApplicationOptions options = null) : base(ctx, options) { }

		protected override void OnSceneUpdate(float timeStep, Scene scene)
		{
			if (!TouchEnabled || CameraNode == null)
				return;

			var input = Input;
			for (uint i = 0, num = input.NumTouches; i < num; ++i)
			{
				TouchState state = input.GetTouch(i);
				if (state.TouchedElement() != null)
					continue;

				if (state.Delta.X != 0 || state.Delta.Y != 0)
				{
					var camera = CameraNode.GetComponent<Camera>();
					if (camera == null)
						return;

					var graphics = Graphics;
					//Yaw += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.X;
					//Pitch += TouchSensitivity * camera.Fov / graphics.Height * state.Delta.Y;
					//CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);
				}
				else
				{
					var cursor = UI.Cursor;
					if (cursor != null && cursor.IsVisible())
						cursor.Position = state.Position;
				}
			}
		}

		void HandleKeyDown(KeyDownEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Esc:
					Engine.Exit();
					return;
			}
		}

		public override void Start()
		{
			base.Start();
			var platform = Runtime.Platform;
			if (platform == "Android" || platform == "iOS")
			{
				TouchEnabled = true;
			}

			MonoDebugHud = new MonoDebugHud(this);
			MonoDebugHud.Show();
			SubscribeToKeyDown(HandleKeyDown);
		}
	}
}
