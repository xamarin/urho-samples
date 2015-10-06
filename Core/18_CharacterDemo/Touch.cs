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

		public Touch(Context ctx, float touchSensitivity, Input input) : base(ctx)
		{
			this.touchSensitivity = touchSensitivity;
			this.input = input;
			CameraDistance = _18_CharacterDemo.CameraInitialDist;
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
				if (touch1.TouchedElement() != null && touch2.TouchedElement() != null && ((touch1.Delta.Y > 0 && touch2.Delta.Y < 0) || (touch1.Delta.Y < 0 && touch2.Delta.Y > 0)))
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
					CameraDistance = MathHelper.Clamp(CameraDistance, _18_CharacterDemo.CameraMinDist, _18_CharacterDemo.CameraMaxDist); // Restrict zoom range to [1;20]
				}
			}

			// Gyroscope (emulated by SDL through a virtual joystick)
			if (UseGyroscope && input.NumJoysticks > 0)  // numJoysticks = 1 on iOS & Android
			{
				JoystickState joystick;
				if (input.TryGetJoystickState(0, out joystick) && joystick.Axes.Size >= 2)
				{
					if (joystick.GetAxisPosition(0) < -_18_CharacterDemo.GyroscopeThreshold)
						controls.Set(_18_CharacterDemo.CtrlLeft, true);
					if (joystick.GetAxisPosition(0) > _18_CharacterDemo.GyroscopeThreshold)
						controls.Set(_18_CharacterDemo.CtrlRight, true);
					if (joystick.GetAxisPosition(1) < -_18_CharacterDemo.GyroscopeThreshold)
						controls.Set(_18_CharacterDemo.CtrlForward, true);
					if (joystick.GetAxisPosition(1) > _18_CharacterDemo.GyroscopeThreshold)
						controls.Set(_18_CharacterDemo.CtrlBack, true);
				}
			}
		}
	}
}
