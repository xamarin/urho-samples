The main code lives in `Program.cs`

This sample shows both how to create a scene in the Hololens world with Urho, it creates a mutant
and places it on the screen which you can control via a series of Cortana voice commands.   

Most of the commands trigger an animation on the model, these are the available verbs:

* Animation commands: idle, kill, hip-hop, jump, jump attack, kick, punch, run, swipe, walk.
* Display commands: bigger, smaller, increase the brightness, decrease the brightness

All you have to do is speak those commands and the mutant will react accordingly.

By using the `UrhoAppView` your urho camera view is directly connected to the HoloLens camera, so
all you need to do is place [Nodes](https://developer.xamarin.com/api/type/Urho.Node/) on the 
[Scene](https://developer.xamarin.com/api/type/Urho.Scene/) and it will be displayed in the user's
world.

The mutant's node gets an [AnimatedModel](https://developer.xamarin.com/api/type/Urho.AnimatedModel/)
and we load the `Mutant.mdl` file which contains the model with their named animations, which we trigger
when the commands are issued.

![Screenshot](Screenshots/Video.gif) 
