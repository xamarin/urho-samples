
These samples use the [UrhoSharp.HoloLens](https://www.nuget.org/packages/UrhoSharp.HoloLens/) package 
and show how you can use Urho to create Holographic applications with HoloLens.

![Screenshot](Physics/Screenshots/Video2.gif) 

The above is the [Physics](https://github.com/xamarin/urho-samples/blob/master/HoloLens/Physics/) sample in this directory.

Making HoloLens applications with Urho is trivial, all you have to do is this:

```
using Urho;
using Urho.Actions;
using Urho.Holographics;
using Urho.HoloLens;
using Urho.Shapes;
using Windows.ApplicationModel.Core;

// In your AppViewSource.cs:
public IFrameworkView CreateView()
{
    return UrhoAppView.Create<MyHoloApp>("MyData"); // pass null if 
                                   // you just need the core assets
}

public class MyHoloApp : HoloApplication
{
    Node boxNode;

    public HelloWorldApplication(string pak, bool emulator) : base(pak, emulator) { }

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

![Screenshot](CrowdNavigation/Screenshots/Video.gif) 