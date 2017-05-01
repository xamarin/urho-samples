UrhoSharp HoloLens Samples
=======

These samples use the [UrhoSharp.SharpReality](https://www.nuget.org/packages/UrhoSharp.SharpReality/) package 
and show how you can use Urho to create Holographic applications with HoloLens.

![Screenshot](05_Physics/Screenshots/Video2.gif) 

The above is the [Physics](https://github.com/xamarin/urho-samples/blob/master/HoloLens/05_Physics/) sample in this directory.


Quick start
=======

In order to start doing your holograms you can try our Visual Studio project template. It contains a basic scene
and a few assets to get started:
![vstemplate](https://habrastorage.org/files/dc7/595/7d9/dc75957d9f9c4e49acfeea9c6c25bd3e.gif)


Making HoloLens applications with Urho is trivial, all you have to do is this:

```
using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.SharpReality;
using Urho.Shapes;
using Windows.ApplicationModel.Core;

internal class Program
{
    [MTAThread]
    static void Main() => CoreApplication.Run(
        new UrhoAppViewSource<HelloWorldApplication>(
            new ApplicationOptions("Data")));
}

public class MyHoloApp : StereoApplication
{
    public MyHoloApp(ApplicationOptions opts) : base(opts) { }

    protected override async void Start()
    {
        // base.Start() creates a basic Scene
        base.Start();
        
        // Create a node
        boxNode = Scene.CreateChild();
        boxNode.Rotation = new Quaternion(0, 45, 0);
        boxNode.Position = new Vector3(0, 0, 2); //2 meters 
        boxNode.SetScale(0.3f); //30cm*30cm*30cm

        // Attach a StaticModel to the node:
        var model = boxNode.CreateComponent<StaticModel>();
        model.Model = CoreAssets.Models.Box;
        mode.SetMaterial(Material.FromColor(Color.Yellow));
        
        boxNode.RunActions(new RepeatForever(new RotateBy(1f, 0, 90, 0)));
    }
}
```

![Screenshot](06_CrowdNavigation/Screenshots/Video.gif) 
