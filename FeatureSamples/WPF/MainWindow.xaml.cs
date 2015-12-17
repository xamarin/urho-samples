using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Urho.Samples.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Application currentApplication;
		TypeInfo selectedGameType;
		SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

		public MainWindow()
		{
			InitializeComponent();
			UrhoEngine.Init(pathToAssets: @"../../Assets");
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
			currentApplication?.Engine.Exit();
			if (value == null) return;
			await semaphoreSlim.WaitAsync();
			//urho will destroy our Panel on Exit so let's create it for each sample
			var urhoSurface = new Panel { Dock = DockStyle.Fill };
			//TODO: capture mouse inside the control
			WindowsFormsHost.Child = urhoSurface;
			WindowsFormsHost.Focus();
			urhoSurface.Focus();
			await Task.Delay(100); //give GC some time to collect MCW from prev sample (Task.Yield won't help)
			var appOptions = new ApplicationOptions(assetsFolder: "Data")
				{
					ExternalWindow = RunInSdlWindow.IsChecked.Value ? IntPtr.Zero : urhoSurface.Handle,
					LimitFps = false, //true means "limit to 200fps"
				};
			currentApplication = Urho.Application.CreateInstance(value.Type, appOptions);
			currentApplication.Run();
			semaphoreSlim.Release();
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
