using System;
using System.Diagnostics;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;

namespace UrhoAR
{
	public partial class MyApp : SimpleApplication
	{
		void SetupAR()
		{
			Viewport.SetClearColor(Color.Black);
		}
	}
}