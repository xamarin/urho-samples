using System;
using System.Diagnostics;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace UrhoAR
{
	public partial class MyApp
	{
		Node earthNode;
		Node rootNode;

		[Preserve]
		public MyApp(ApplicationOptions options) : base(options) { }

		static MyApp()
		{
			UnhandledException += (s, e) => {
				if (Debugger.IsAttached)
					Debugger.Break();
				e.Handled = true;
			};
		}

		protected override async void Start()
		{
			base.Start();

			SetupAR();

			// UI text 
			var helloText = new Text(Context);
			helloText.Value = "Hello World from UrhoSharp";
			helloText.HorizontalAlignment = HorizontalAlignment.Center;
			helloText.VerticalAlignment = VerticalAlignment.Top;
			helloText.SetColor(new Color(r: 0.5f, g: 1f, b: 1f));
			helloText.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
			UI.Root.AddChild(helloText);

			// Create a node for the Earth
			rootNode = Scene.CreateChild();
			rootNode.Position = new Vector3(0, 0, 0.6f); //60cm away
			earthNode = rootNode.CreateChild();
			earthNode.SetScale(0.5f); //25cm radius
			earthNode.Rotation = new Quaternion(0, 180, 0);

			// Create a static model component - Sphere:
			var earth = earthNode.CreateComponent<Sphere>();
			earth.SetMaterial(ResourceCache.GetMaterial("Materials/Earth.xml")); // or simply Material.FromImage("Textures/Earth.jpg")

			// Same steps for the Moon
			var moonNode = earthNode.CreateChild();
			moonNode.SetScale(0.27f); // Relative size of the Moon is 1738.1km/6378.1km
			moonNode.Position = new Vector3(1.2f, 0, 0);
			var moon = moonNode.CreateComponent<Sphere>();
			moon.SetMaterial(Material.FromImage("Textures/Moon.jpg"));

			// Clouds
			var cloudsNode = earthNode.CreateChild();
			cloudsNode.SetScale(1.02f);
			var clouds = cloudsNode.CreateComponent<Sphere>();
			var cloudsMaterial = new Material();
			cloudsMaterial.SetTexture(TextureUnit.Diffuse, ResourceCache.GetTexture2D("Textures/Earth_Clouds.jpg"));
			cloudsMaterial.SetTechnique(0, CoreAssets.Techniques.DiffAddAlpha);
			clouds.SetMaterial(cloudsMaterial);
						
			// FPS
			new MonoDebugHud(this).Show(Color.Green, 25);

			// Run a an action to spin the Earth (7 degrees per second)
			rootNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: -7, deltaAngleZ: 0)));
			// Spin clouds:
			cloudsNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 0, deltaAngleY: 1, deltaAngleZ: 0)));

			AddCity(lat: 0, lon: 0, name: "(0, 0)");
			AddCity(lat: 53.9045f, lon: 27.5615f, name: "Minsk");
			AddCity(lat: 51.5074f, lon: 0.1278f, name: "London");
			AddCity(lat: 40.7128f, lon: -74.0059f, name: "New-York");
			AddCity(lat: 37.7749f, lon: -122.4194f, name: "San Francisco");
			AddCity(lat: 39.9042f, lon: 116.4074f, name: "Beijing");
			AddCity(lat: -31.9505f, lon: 115.8605f, name: "Perth");
		}

		public void AddCity(float lat, float lon, string name)
		{
			var height = earthNode.Scale.Y / 2f;

			lat = (float)Math.PI * lat / 180f - (float)Math.PI / 2f;
			lon = (float)Math.PI * lon / 180f;

			float x = height * (float)Math.Sin(lat) * (float)Math.Cos(lon);
			float z = height * (float)Math.Sin(lat) * (float)Math.Sin(lon);
			float y = height * (float)Math.Cos(lat);

			var markerNode = rootNode.CreateChild();
			markerNode.Scale = Vector3.One * 0.03f;
			markerNode.Position = new Vector3((float)x, (float)y, (float)z);
			markerNode.CreateComponent<Sphere>();
			markerNode.RunActionsAsync(new RepeatForever(
				new TintTo(0.5f, Color.White),
				new TintTo(0.5f, Randoms.NextColor())));

			var textPos = markerNode.Position;
			textPos.Normalize();

			var textNode = markerNode.CreateChild();
			textNode.Position = textPos * 1;
			textNode.SetScale(1.5f);
			textNode.LookAt(Vector3.Zero, Vector3.Up, TransformSpace.Parent);
			var text = textNode.CreateComponent<Text3D>();
			text.SetFont(CoreAssets.Fonts.AnonymousPro, 80);
			text.EffectColor = Color.Black;
			text.TextEffect = TextEffect.Shadow;
			text.Text = name;
		}
	}
}
