using System;
using Urho;
using AppKit;
using System.Linq;
using System.IO;
using Foundation;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;

namespace Urho.Samples.Cocoa
{
	public partial class ViewController : NSViewController
	{
		Type[] sampleTypes;
		int currentSampleIndex = -1;
		Application currentApp;
		ApplicationOptions options;

		public ViewController(IntPtr handle) : base(handle)
		{
		}

		public override async void ViewDidLoad()
		{
			base.ViewDidLoad();
			sampleTypes = typeof(Sample).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample))
				.ToArray();

			ButtonNext.Activated += (s, e) => 
				{
					if (++currentSampleIndex >= sampleTypes.Length)
						currentSampleIndex = 0;
					RunSample(sampleTypes[currentSampleIndex]);
				};

			ButtonPrev.Activated += (s, e) =>
				{
					if (--currentSampleIndex < 0)
						currentSampleIndex = sampleTypes.Length - 1;
					RunSample(sampleTypes[currentSampleIndex]);
				};

			var view = new UrhoSurface();
			view.Frame = UrhoSurface.Frame;
			view.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			UrhoSurface.AddSubview(view);

			string assets = "../../../../../Assets";
			string coreDataPak = "../../../CoreData.pak";
			if (!File.Exists(coreDataPak))
				File.Copy("../../../CoreData.pak", Path.Combine(assets, "CoreData.pak"));

			options = new ApplicationOptions("Data")
				{
					ExternalWindow = view.Handle,
					LimitFps = true,
					HighDpi = true,
					ResourcePrefixPaths = new string[] { assets },
				};
		}
		
		async void RunSample(Type type)
		{
			if (currentApp != null)
			{
				await currentApp.Exit();
				GC.Collect();
				await Task.Delay(10); //small workaround, will be fixed
			}
			currentApp = (Application)Activator.CreateInstance(type, options);
			await Task.Yield();
			currentApp.Run();
		}

		public override NSObject RepresentedObject
		{
			get
			{
				return base.RepresentedObject;
			}
			set
			{
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}

	public class UrhoSurface : NSView
	{
		public override async void ViewDidMoveToWindow()
		{
			base.ViewDidMoveToWindow();
			PostsFrameChangedNotifications = true;
			PostsBoundsChangedNotifications = true;
		}

		public override async void SetFrameSize(CoreGraphics.CGSize newSize)
		{
			base.SetFrameSize(newSize);
			if (Application.HasCurrent)
				NSOpenGLContext.CurrentContext?.Update();
		}
	}
}
