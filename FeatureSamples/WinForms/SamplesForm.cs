using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Urho.Desktop;
using Urho.Extensions.WinForms;

namespace Urho.Samples.WinForms
{
	public partial class SamplesForm : Form
	{
		Application currentApplication;
		SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
		UrhoSurface surface;

		public SamplesForm()
		{
			InitializeComponent();
			DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
			var sampleTypes = typeof(Sample).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Application)) && t != typeof(Sample))
				.ToArray();
			samplesListbox.DisplayMember = "Name";
			samplesListbox.Items.AddRange(sampleTypes);

			surface = new UrhoSurface();
			surface.Dock = DockStyle.Fill;
			urhoSurfacePlaceholder.Controls.Add(surface);

			samplesListbox.SelectedIndex = 19; //Water by default
		}

		async void samplesListbox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var type = (Type)samplesListbox.SelectedItem;
			if (type == null)
				return;
			var app = await surface.Show(type, new ApplicationOptions("Data"));
		}
	}
}
