using Urho;

public class _01_HelloWorld : Application
{
	public _01_HelloWorld(Context c) : base(c)
	{
	}

	public override void Start()
	{
		var cache = ResourceCache;
		var helloText = new Text(Context)
		{
			Value = "Hello World from Urho3D + Mono",
			HorizontalAlignment = HorizontalAlignment.HA_CENTER,
			VerticalAlignment = VerticalAlignment.VA_CENTER
		};
		helloText.SetColor(new Color(0f, 1f, 0f));
		helloText.SetFont(cache.GetFont("Fonts/Anonymous Pro.ttf"), 30);
		UI.Root.AddChild(helloText);

		var node = MakeNode();
		var refs = node.Refs();
	}

	private Node MakeNode()
	{
		var x = new Node(Context);
		var y = new Node(Context);
		x.AddChild(y, 0);
		var r1 = y.Refs();
		var r2 = y.WeakRefs();
		var rr1 = x.Refs();
		var rr2 = x.WeakRefs();
		return x;
	}
}