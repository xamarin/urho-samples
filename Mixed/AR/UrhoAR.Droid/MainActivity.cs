using Urho;
using Urho.Droid;
using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

namespace UrhoAR.Droid
{
	[Activity(Label = "UrhoAR Droid",
		MainLauncher = true,
		Theme = "@android:style/Theme.NoTitleBar",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		UrhoSurfacePlaceholder surface;
		MyApp app;
		bool launched;
		FrameLayout placeholder;

		protected override async void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			placeholder = new FrameLayout(this);
			surface = UrhoSurface.CreateSurface(this);
			placeholder.AddView(surface);
			SetContentView(placeholder);

			LaunchUrho();
		}
		async void LaunchUrho()
		{
			if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)
			{
				ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera }, 42);
				return;
			}

			if (launched)
				return;

			launched = true;
			surface = UrhoSurface.CreateSurface(this);
			placeholder.AddView(surface);
			app = await surface.Show<MyApp>(new Urho.ApplicationOptions("MyData"));
		}

		protected override void OnResume()
		{
			base.OnResume();

			if (launched)
				UrhoSurface.OnResume();

			// if was resumed by the Camera Permission dialog
			LaunchUrho();
		}

		protected override void OnPause()
		{
			if (launched)
				UrhoSurface.OnPause();
			base.OnPause();
		}

		public override void OnLowMemory()
		{
			if (launched)
				UrhoSurface.OnLowMemory();
			base.OnLowMemory();
		}

		protected override void OnDestroy()
		{
			if (launched)
				UrhoSurface.OnDestroy();
			base.OnDestroy();
		}
	}
}
