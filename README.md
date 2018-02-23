# UrhoSharp samples

This directory contains various samples for the [UrhoSharp](http://developer.xamarin.com/guides/cross-platform/urho/) 
engine and they can be compiled for Android or iOS, or can be executed on Windows
and Mac with the published NuGet package.

# Samples

Some of the samples here include:

* FormsSamples
* FeatureSamples
* SamplyGame
* HoloLens, ARKit, ARCore
* Mixed samples

## ARKit, ARCore and HoloLens

These directories contain samples for running UrhoSharp on HoloLens, ARKit and ARCore.  
Also there is a mixed sample that shares a scene between iOS, Android and HoloLens (see Mixed/AR)

![Screenshot](ARKit/Mutant.gif)
![Screenshot](ARKit/Crowd.gif)

## FormsSamples

The 'FormsSamples' solution demonstrates how UrhoSharp can be used in Xamarin.Forms 
applications as a View element.

![Screenshot](FormsSample/Screenshots/Android.gif) ![Screenshot](FormsSample/Screenshots/Ios.gif)

## FeatureSamples

The toplevel `FeatureSamples` solution showcases 40 independent UrhoSharp
features, each one showcasing a particular element of the framework and runs
on all supported platforms. 

![Physics2D](https://habrastorage.org/files/d77/060/698/d770606980874fb6a15484d04bea6dd6.gif)
![Water](https://habrastorage.org/files/e3e/8f1/80d/e3e8f180d8b54f0989d9448c98eacd5b.png)

## SamplyGame

The `SamplyGame` directory contains a more complete game, it is a sample
inspired by the gameplay and artwork of ShootySkies and shows a more 
complete game in action, showing how to load assets, write game code and
structure a game.   It is our first game build with this, so be kind.

![Screenshot](SamplyGame/Screenshots/Video.gif)

## Structure

All solutions are structured to have their cross platform code written
in the `Core` directory, where we build a portable class library.   While
we have taken the approach of using Portable Class Libraries, you can 
also used Shared Projects.

The structure of each solution is this:

* `Assets`: Contains the shared assets that are used for the various
  samples.

* `Core`: Contains the various samples, one for each feature that is
  being showcased and it happens to be a Portable Class Library
  project, so it can be reused as-is across all supported platforms.

* `iOS`: Contains the iOS launcher.

* `Android`: Contains the Android launcher.

* `Mac`: Contains the Mac launcher (but works on Windows too).

* `WPF`: Contains the Windows launcher based on WPF.

* `WinForms`: Contains the Windows launcher based on WinForms.

# To build the samples

* Windows: Use [Visual Studio 2015](https://www.visualstudio.com/).
* Mac: Use the latest [Xamarin Studio](https://xamarin.com/studio) 
