//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Collections.Generic;
using Urho.Gui;
namespace Urho.Samples
{
	public class UIDrag : Sample
	{
		Dictionary<UIElement, ElementInfo> elements;

		public UIDrag(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
		
			elements = new Dictionary<UIElement, ElementInfo>();
			// Set mouse visible
		
			if (Platform != Platforms.Android && Platform != Platforms.iOS)
				Input.SetMouseVisible(true, false);

			// Create the UI content
			CreateGUI();
			CreateInstructions();
		}

		void CreateGUI()
		{
			var cache = ResourceCache;
			UI ui = UI;

			UIElement root = ui.Root;
			// Load the style sheet from xml
			root.SetDefaultStyle(cache.GetXmlFile("UI/DefaultStyle.xml"));

			for (int i = 0; i < 10; i++)
			{
				Button b = new Button();
				root.AddChild(b);
				// Reference a style from the style sheet loaded earlier:
				b.SetStyle("Button", null);
				b.SetSize(300, 100);
				b.Position = new IntVector2(50 * i, 50 * i);

				b.SubscribeToDragMove(HandleDragMove);
				b.SubscribeToDragBegin(HandleDragBegin);
				b.SubscribeToDragCancel(HandleDragCancel);
				b.SubscribeToDragEnd(HandleDragEnd);

				{
					var t = new Text();
					b.AddChild(t);
					t.SetStyle("Text", null);
					t.HorizontalAlignment = HorizontalAlignment.Center;
					t.VerticalAlignment = VerticalAlignment.Center;
					t.Name = ("Text");
				}

				{
					var t = new Text();
					b.AddChild(t);
					t.SetStyle("Text", null);
					t.Name=("Event Touch");
					t.HorizontalAlignment=HorizontalAlignment.Center;
					t.VerticalAlignment=VerticalAlignment.Bottom;
				}

				{
					var t = new Text();
					b.AddChild(t);
					t.SetStyle("Text", null);
					t.Name=("Num Touch");
					t.HorizontalAlignment=HorizontalAlignment.Center;
					t.VerticalAlignment=VerticalAlignment.Top;
				}
			}

			for (int i = 0; i< 10; i++)
			{
				var t = new Text();
				root.AddChild(t);
				t.SetStyle("Text", null);
				t.Name=("Touch "+ i);
				t.Visible = false;
			}
		}

		void CreateInstructions()
		{
			var cache = ResourceCache;
			UI ui = UI;

			// Construct new Text object, set string to display and font to use
			var instructionText = new Text();
			instructionText.Value = "Drag on the buttons to move them around.\nMulti- button drag also supported.";
			instructionText.SetFont(cache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
			ui.Root.AddChild(instructionText);

			// Position the text relative to the screen center
			instructionText.HorizontalAlignment = HorizontalAlignment.Center;
			instructionText.VerticalAlignment = VerticalAlignment.Center;
			instructionText.SetPosition(0, ui.Root.Height/4);
		}

		void HandleDragBegin(DragBeginEventArgs args)
		{
			var element = args.Element;

			int lx = args.X;
			int ly = args.Y;

			IntVector2 p = element.Position;
			elements[element] = new ElementInfo(element, p, new IntVector2(p.X - lx, p.Y - ly), args.Buttons);

			int buttons = args.Buttons;

			var t = (Text)element.GetChild("Text", false);
			t.Value = "Drag Begin Buttons: " + buttons;

			t = (Text)element.GetChild("Num Touch", false);
			t.Value = "Number of buttons: " + args.NumButtons;
		}

		void HandleDragMove(DragMoveEventArgs args)
		{
			var element = elements[args.Element];
			int buttons = args.Buttons;
			IntVector2 d = element.Delta;
			int x = args.X + d.X;
			int y = args.Y + d.Y;
			var t = (Text)element.Element.GetChild("Event Touch", false);
			t.Value = "Drag Move Buttons: " + buttons;

			if (buttons == element.Buttons)
				element.Element.Position = new IntVector2(x, y);
		}

		void HandleDragCancel(DragCancelEventArgs args)
		{
			args.Element.Position = elements[args.Element].Start;
		}

		void HandleDragEnd(DragEndEventArgs args)
		{
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			UI ui = UI;
			UIElement root = ui.Root;

			Input input = Input;

			uint n = input.NumTouches;
			for (uint i = 0; i < n; i++)
			{
				var text = (Text)root.GetChild("Touch " + i, false);
				TouchState ts = input.GetTouch(i);
				text.Value = "Touch " + ts.TouchID;

				IntVector2 pos = ts.Position;
				pos.Y -= 30;

				text.Position = (pos);
				text.Visible = true;
			}

			for (uint i = n; i < 10; i++)
			{
				var text = root.GetChild("Touch " + i, false);
				text.Visible = false;
			}
		}

		class ElementInfo
		{
			public UIElement Element { get; set; }
			public IntVector2 Start { get; set; }
			public IntVector2 Delta { get; set; }
			public int Buttons { get; set; }

			public ElementInfo(UIElement element, IntVector2 start, IntVector2 delta, int buttons)
			{
				Element = element;
				Start = start;
				Delta = delta;
				Buttons = buttons;
			}
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.Hidden;
	}
}
