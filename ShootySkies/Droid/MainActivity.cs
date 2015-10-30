using Android.App;
using Android.Widget;
using Android.OS;
using Urho.Droid;

namespace ShootySkies.Droid
{
	[Activity(Label = "ShootySkies.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.MyButton);
			button.Text = "Start game";

			button.Click += delegate
				{
					UrhoEngine.Init();
					UrhoSurfaceViewController.RunInActivity<ShootySkiesGame>();
				};
		}
	}
}

