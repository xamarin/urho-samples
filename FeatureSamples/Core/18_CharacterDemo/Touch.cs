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
	public class Touch : Component
	{
		readonly float touchSensitivity;
		readonly Input input;
		bool zoom;

		public float CameraDistance { get; set; }
		public bool UseGyroscope { get; set; }

		public Touch(IntPtr handle) : base(handle) { }

		public Touch(float touchSensitivity, Input input)
		{
			this.touchSensitivity = touchSensitivity;
			this.input = input;
			CameraDistance = CharacterDemo.CameraInitialDist;
			zoom = false;
			UseGyroscope = false;
		}

		public void UpdateTouches(Controls controls)
		{
			zoom = false; // reset bool

			// Zoom in/out
			if (input.NumTouches == 2)
			{
				TouchState touch1, touch2;
				touch1 = input.GetTouch(0);
				touch2 = input.GetTouch(1);

				// Check for zoom pattern (touches moving in opposite directions and on empty space)
				if (touch1.TouchedElement != null && touch2.TouchedElement != null && ((touch1.Delta.Y > 0 && touch2.Delta.Y < 0) || (touch1.Delta.Y < 0 && touch2.Delta.Y > 0)))
					zoom = true;
				else
					zoom = false;

				if (zoom)
				{
					int sens = 0;
					// Check for zoom direction (in/out)
					if (Math.Abs(touch1.Position.Y - touch2.Position.Y) > Math.Abs(touch1.LastPosition.Y - touch2.LastPosition.Y))
						sens = -1;
					else
						sens = 1;
					CameraDistance += Math.Abs(touch1.Delta.Y - touch2.Delta.Y) * sens * touchSensitivity / 50.0f;
					CameraDistance = MathHelper.Clamp(CameraDistance, CharacterDemo.CameraMinDist, CharacterDemo.CameraMaxDist); // Restrict zoom range to [1;20]
				}
			}

			// Gyroscope (emulated by SDL through a virtual joystick)
			if (UseGyroscope && input.NumJoysticks > 0)  // numJoysticks = 1 on iOS & Android
			{
				JoystickState joystick;
				if (input.TryGetJoystickState(0, out joystick) && joystick.Axes.Size >= 2)
				{
					if (joystick.GetAxisPosition(0) < -CharacterDemo.GyroscopeThreshold)
						controls.Set(CharacterDemo.CtrlLeft, true);
					if (joystick.GetAxisPosition(0) > CharacterDemo.GyroscopeThreshold)
						controls.Set(CharacterDemo.CtrlRight, true);
					if (joystick.GetAxisPosition(1) < -CharacterDemo.GyroscopeThreshold)
						controls.Set(CharacterDemo.CtrlForward, true);
					if (joystick.GetAxisPosition(1) > CharacterDemo.GyroscopeThreshold)
						controls.Set(CharacterDemo.CtrlBack, true);
				}
			}
		}
	}
}
