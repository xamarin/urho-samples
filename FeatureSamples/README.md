This directory contains the various samples that showcase the features of UrhoSharp, they
are ports of the upstream Urho samples.

The code is organized as follows:

* `Core/` - contains the sample code, with one directory per sample, illustrating a particular feature of the Urho engine.
* `Assets/` - contains the assets used by the sample code.

Each platform has special launchers, each of these directories contains the initialization 
sequence and integration of Urho with a particular operating system.   The included ones are:

* `Android/` - Integration into Android
* `Desktop/` - Runs on Windows or Mac, launching from a .NET console application
* `Mac/` - Integration with Cocoa, using Xamarin.Mac APIs
* `UWP/` - Integration with Windows's UWP
* `WPF/` - Integration with Windows's WPF
* `Winforms/` - Integration with Window's Windows.Forms
* `iOS` - Integration with iOS

