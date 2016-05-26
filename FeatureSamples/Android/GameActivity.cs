using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Org.Libsdl.App;
using Urho.Droid;

namespace Urho.Samples.Droid
{
	[Activity(Label = "UrhoSharp",
		Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
		ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Landscape)]
	public class GameActivity : Activity
	{
		private SDLSurface surface;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			var mLayout = new AbsoluteLayout(this);
			surface = UrhoSurface.CreateSurface(this, Type.GetType(Intent.GetStringExtra("Type")), new ApplicationOptions("Data"), true);
			mLayout.AddView(surface);
			SetContentView(mLayout);
		}

		protected override void OnResume()
		{
			UrhoSurface.OnResume();
			base.OnResume();
		}

		protected override void OnPause()
		{
			UrhoSurface.OnPause();
			base.OnPause();
		}

		public override void OnLowMemory()
		{
			UrhoSurface.OnLowMemory();
			base.OnLowMemory();
		}

		protected override void OnDestroy()
		{
			UrhoSurface.OnDestroy();
			base.OnDestroy();
		}

		public override bool DispatchKeyEvent(KeyEvent e)
		{
			if (!UrhoSurface.DispatchKeyEvent(e))
				return false;
			return base.DispatchKeyEvent(e);
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			UrhoSurface.OnWindowFocusChanged(hasFocus);
			base.OnWindowFocusChanged(hasFocus);
		}
	}
}