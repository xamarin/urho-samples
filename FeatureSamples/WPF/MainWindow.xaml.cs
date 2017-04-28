using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Urho.Desktop;

namespace Urho.Samples.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		TypeInfo selectedGameType;

		public MainWindow()
		{
			InitializeComponent();
			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
			GameTypes = typeof(Sample).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample))
				.Select((t, i) => new TypeInfo(t, $"{i + 1}. {t.Name}", ""))
				.ToArray();
			DataContext = this;
			Loaded += (s,e) => SelectedGameType = GameTypes[19]; //water
		}

		public TypeInfo[] GameTypes { get; set; }

		public TypeInfo SelectedGameType
		{
			get { return selectedGameType; }
			set { RunGame(value); selectedGameType = value; }
		}

		async void RunGame(TypeInfo value)
		{
			var app = await UrhoSurfaceCtrl.Show(value.Type, new ApplicationOptions(assetsFolder: "Data"));
			Application.InvokeOnMain(() => { /*app.DoSomeStuff();*/});
		}
	}

	public class TypeInfo
	{
		public Type Type { get; }
		public string Name { get; }
		public string Description { get; }

		public TypeInfo(Type type, string name, string description)
		{
			Type = type;
			Name = name;
			Description = description;
		}
	}
}
