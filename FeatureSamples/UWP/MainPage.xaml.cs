using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Urho.Samples.UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		Application currentApplication;
		TypeInfo selectedGameType;

		public MainPage()
		{
			InitializeComponent();
			GameTypes = typeof(Sample).GetTypeInfo().Assembly.GetTypes()
				.Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Application)) && t != typeof(Sample))
				.Select((t, i) => new TypeInfo(t, $"{i + 1}. {t.Name}", ""))
				.ToArray();
			DataContext = this;
			Loaded += (s, e) => SelectedGameType = GameTypes[0]; //water
		}

		public TypeInfo[] GameTypes { get; set; }

		public TypeInfo SelectedGameType
		{
			get { return selectedGameType; }
			set { RunGame(value); selectedGameType = value; }
		}

		public void RunGame(TypeInfo value)
		{
			currentApplication?.Exit();
			//at this moment, UWP supports assets only in pak files (see PackageTool)
			currentApplication = UrhoSurface.Run(value.Type, "Data.pak");
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
