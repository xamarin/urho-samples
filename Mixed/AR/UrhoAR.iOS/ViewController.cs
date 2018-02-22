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
			var app = await urhoSurface.Show<MyApp>(new Urho.ApplicationOptions("MyData"));
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
		}
	}
}
