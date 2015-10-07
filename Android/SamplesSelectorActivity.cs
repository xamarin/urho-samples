using System;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Urho.Droid;

namespace Urho.Samples.Droid
{
	[Activity(Label = "MonoUrho Samples", MainLauncher = true, Icon = "@drawable/icon", NoHistory = true)]
	public class SamplesSelectorActivity : ListActivity
	{
		System.Type[] sampleTypes;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			var assets = Assets.List("");
			if (!assets.Contains("CoreData") || !assets.Contains("Data"))
			{
				new AlertDialog.Builder(this)
					.SetMessage("Assets are empty")
					.SetTitle("Error")
					.Show();
				return;
			}

			//Show a list of available samples (click to run):
			ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Resource.Layout.samples_list_text_view);
			sampleTypes = typeof(Sample).Assembly.GetTypes().Where(t => t.BaseType == typeof(Sample) && t != typeof(_41_ToonTown)).ToArray();
			foreach (var sample in sampleTypes)
			{
				var name = sample.Name.TrimStart('_').Replace("_", ". ");
				adapter.Add(name);
			}
			SetContentView(Resource.Layout.samples_list);
			ListAdapter = adapter;
		}
		
		protected override void OnListItemClick(Android.Widget.ListView l, Android.Views.View v, int position, long id)
		{
			/*
				In order to run a sample:
				ApplicationLauncher.Run(() => new MyGame(new Urho.Context());
				It opens a special full screen activity and runs the sample.
			*/
			ApplicationLauncher.Run(() => (Application)Activator.CreateInstance(sampleTypes[position], new Urho.Context()));
		}
	}
}

