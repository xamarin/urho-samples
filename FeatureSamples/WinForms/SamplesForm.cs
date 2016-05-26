using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urho.Desktop;

namespace Urho.Samples.WinForms
{
	public partial class SamplesForm : Form
	{
		Application currentApplication;
		SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

		public SamplesForm()
		{
			InitializeComponent();
			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
			var sampleTypes = typeof(Sample).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample))
				.ToArray();
			samplesListbox.DisplayMember = "Name";
			samplesListbox.Items.AddRange(sampleTypes);
			samplesListbox.SelectedIndex = 19; //Water by default
		}

		async void samplesListbox_SelectedIndexChanged(object sender, EventArgs e)
		{
			currentApplication?.Exit();
			currentApplication = null;
			await semaphoreSlim.WaitAsync();
			var type = (Type) samplesListbox.SelectedItem;
			if (type == null) return;
			urhoSurfacePlaceholder.Controls.Clear(); //urho will destroy previous control so we have to create a new one
			var urhoSurface = new Panel { Dock = DockStyle.Fill };
			urhoSurfacePlaceholder.Controls.Add(urhoSurface);
			await Task.Yield();//give some time for GC to cleanup everything
			currentApplication = Application.CreateInstance(type, new ApplicationOptions("Data") { ExternalWindow = urhoSurface.Handle });
			urhoSurface.Focus();
			currentApplication.Run();
			semaphoreSlim.Release();
		}
	}
}
