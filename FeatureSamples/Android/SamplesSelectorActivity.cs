using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Urho.Droid;

namespace Urho.Samples.Droid
{
	[Activity(Label = "MonoUrho Samples", MainLauncher = true, Icon = "@drawable/icon")]
	public class SamplesSelectorActivity : ListActivity
	{
		System.Type[] sampleTypes;


		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			//Show a list of available samples (click to run):
			ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Resource.Layout.samples_list_text_view);
			sampleTypes = typeof(Sample).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Application)) 
				&& t != typeof(Sample) && t != typeof(PBRMaterials) && t != typeof(BasicTechniques)).ToArray();
			foreach (var sample in sampleTypes)
			{
				adapter.Add(sample.Name);
			}
			SetContentView(Resource.Layout.samples_list);
			ListAdapter = adapter;
		}
		
		protected override void OnListItemClick(Android.Widget.ListView l, Android.Views.View v, int position, long id)
		{
			var intent = new Intent(this, typeof (GameActivity));
			intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
			intent.PutExtra("Type", sampleTypes[position].AssemblyQualifiedName);
			StartActivity(intent);
		}
	}
}

