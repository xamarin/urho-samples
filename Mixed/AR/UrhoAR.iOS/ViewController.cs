using System;

using UIKit;
using Urho.iOS;

namespace UrhoAR.iOS
{
	public partial class ViewController : UIViewController
	{
		UrhoSurface urhoSurface;

		public ViewController(IntPtr handle) : base(handle) { }

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();

			urhoSurface = new UrhoSurface(View.Bounds);
			View.AddSubview(urhoSurface);
			var app = await urhoSurface.Show<MyApp>(new Urho.ApplicationOptions("MyData") { 
				Orientation = Urho.ApplicationOptions.OrientationType.Portrait,
				DelayedStart = false,
			});
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}
	}
}
