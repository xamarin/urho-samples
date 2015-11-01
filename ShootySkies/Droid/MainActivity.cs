using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Views;
using Urho.Droid;

namespace ShootySkies.Droid
{
	[Activity(Label = "ShootySkies", MainLauncher = true, 
		Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
		ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			var mLayout = new AbsoluteLayout(this);
			var surface = UrhoSurfaceViewController.CreateSurface<ShootySkiesGame>(this);
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