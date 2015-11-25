This directory contains various samples for the UrhoSharp engine and
they can be compiled for Android or iOS, or can be executed on Windows
and Mac with the published NuGet package.

The toplevel "FeatureSamples" solution showcases 40 independent UrhoSharp
features, each one showcasing a particular element of the framework and runs
on all supported platforms.   It also contains a more complete "ToonTown" sample.

The "SamplyGame" directory contains a more complete game, it is a sample
inspired by the gameplay and artwork of ShootySkies and shows a more 
complete game in action, showing how to load assets, write game code and
structure a game.   It is our first game build with this, so be kind.

Both solutions are structured to have their cross platform code written
in the `Core` directory, where we build a portable class library.   While
we have taken the approach of using Portable Class Libraries, you can 
also used Shared Projects.

The structure of each solution is this:

* `Assets`: Contains the shared assets that are used for the various
  samples.

* `Core`: Contains the various samples, one for each feature that is
  being showcased and it happens to be a Portable Class Library
  project, so it can be reused as-is across all supported platforms.

  Screenshots are provided on each directory.

* `iOS`: Contains the iOS launcher.

* `Android`: Contains the Android launcher.

* `Desktop`: Contains the Desktop launcher, works on Mac and Windows.
