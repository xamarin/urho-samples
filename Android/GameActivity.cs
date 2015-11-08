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
			surface = UrhoSurfaceViewController.CreateSurface(this, Type.GetType(Intent.GetStringExtra("Type")));
			mLayout.AddView(surface);
			SetContentView(mLayout);
		}

		protected override void OnResume()
		{
			UrhoSurfaceViewController.OnResume();
			base.OnResume();
		}

		protected override void OnPause()
		{
			UrhoSurfaceViewController.OnPause();
			base.OnPause();
		}

		public override void OnLowMemory()
		{
			UrhoSurfaceViewController.OnLowMemory();
			base.OnLowMemory();
		}

		protected override void OnDestroy()
		{
			UrhoSurfaceViewController.OnDestroy();
			base.OnDestroy();
		}

		public override bool DispatchKeyEvent(KeyEvent e)
		{
			if (!UrhoSurfaceViewController.DispatchKeyEvent(e))
				return false;
			return base.DispatchKeyEvent(e);
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			UrhoSurfaceViewController.OnWindowFocusChanged(hasFocus);
			base.OnWindowFocusChanged(hasFocus);
		}
	}
}