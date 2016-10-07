using System;
using Urho;
using Urho.Actions;
using Urho.Resources;
using Urho.Shapes;

namespace CognitiveServices
{
	public class BusyIndicator : Component
	{
		bool isBusy;

		public BusyIndicator() { }

		public BusyIndicator(IntPtr ptr) : base(ptr) { }

		public bool IsBusy
		{
			get { return isBusy; }
			set { Show(isBusy = value); }
		}

		async void Show(bool show)
		{
			var node = Node.GetChild("BusyIndicatorParts", false);
			if (node != null && !node.IsDeleted)
				node.Remove();

			if (!show || IsDeleted || Node == null)
				return;

			node = Node.CreateChild("BusyIndicatorParts");

			var torus = node.CreateComponent<Torus>();
			torus.Color = Color.Cyan;
			node.Rotation = new Quaternion(90, 0, 0);
			node.RunActions(new RepeatForever(
				new EaseBounceOut(new RotateBy(0.9f, 180, 0, 0)),
				new EaseBackInOut(new RotateBy(0.9f, 0, 0, 180))));
		}
	}
}
