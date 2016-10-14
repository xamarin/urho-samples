// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Urho.Samples.Cocoa
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton ButtonNext { get; set; }

		[Outlet]
		AppKit.NSButton ButtonPrev { get; set; }

		[Outlet]
		AppKit.NSTableView SamplesTable { get; set; }

		[Outlet]
		AppKit.NSView UrhoSurface { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SamplesTable != null) {
				SamplesTable.Dispose ();
				SamplesTable = null;
			}

			if (ButtonPrev != null) {
				ButtonPrev.Dispose ();
				ButtonPrev = null;
			}

			if (UrhoSurface != null) {
				UrhoSurface.Dispose ();
				UrhoSurface = null;
			}

			if (ButtonNext != null) {
				ButtonNext.Dispose ();
				ButtonNext = null;
			}
		}
	}
}
