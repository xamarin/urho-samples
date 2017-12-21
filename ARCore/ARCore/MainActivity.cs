using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Urho.Droid;

namespace ARCore
{
	[Activity(Label = "UrhoSharp ARCore", 
		MainLauncher = true, 
		Theme = "@android:style/Theme.NoTitleBar",
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		bool launched;
		MyGame game;
		RelativeLayout placeholder;
		UrhoSurfacePlaceholder surface;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);

			placeholder = FindViewById<RelativeLayout>(Resource.Id.UrhoSurfacePlaceHolder);

			FindViewById<Button>(Resource.Id.restartGameBtn).Click += OnRestartGameClicked;
			FindViewById<Button>(Resource.Id.restartTrackingBtn).Click += OnRestartTrackingClicked;
			FindViewById<Button>(Resource.Id.gammaCorrectionBtn).Click += OnGammaCorrectionClicked;

			LaunchUrho();
		}

		void OnGammaCorrectionClicked(object sender, EventArgs e)
		{
			Urho.Application.InvokeOnMain(() => game?.CorrectGamma());
		}

		void OnRestartTrackingClicked(object sender, EventArgs e)
		{
			game?.ArCore?.Session?.Pause();
			game?.ArCore?.Session?.Resume();
		}

		void OnRestartGameClicked(object sender, EventArgs e)
		{
			launched = false;
			if (surface != null)
			{
				surface.Stop();
				var viewGroup = surface.Parent as ViewGroup;
				viewGroup?.RemoveView(surface);
			}

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
			game = await surface.Show<MyGame>(
				new Urho.ApplicationOptions {
					ResourcePaths = new [] { "MutantData"}
				});
		}

		protected override void OnResume()
		{
			base.OnResume();
			UrhoSurface.OnResume();

			// if was resumed by the Camera Permission dialog
			LaunchUrho();
		}
		protected override void OnPause()
		{
			UrhoSurface.OnPause();
			base.OnPause();
		}

		protected override void OnDestroy()
		{
			UrhoSurface.OnDestroy();
			base.OnDestroy();
		}

		public override void OnBackPressed()
		{
			UrhoSurface.OnDestroy();
			Finish();
		}

		public override void OnLowMemory()
		{
			UrhoSurface.OnLowMemory();
			base.OnLowMemory();
		}
	}
}
