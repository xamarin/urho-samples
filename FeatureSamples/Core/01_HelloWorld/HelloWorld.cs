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
using Urho.Gui;

namespace Urho.Samples
{
	public class HelloWorld : Application 
	{
		public HelloWorld(ApplicationOptions options = null) : base(options) {}

		protected override void Start()
		{
			var cache = ResourceCache;
			var helloText = new Text()
				{
					Value = "Hello World from UrhoSharp",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};
			helloText.SetColor(new Color(0f, 1f, 0f));
			helloText.SetFont(font: cache.GetFont("Fonts/Anonymous Pro.ttf"), size: 30);
			UI.Root.AddChild(helloText);
			
			Graphics.SetWindowIcon(cache.GetImage("Textures/UrhoIcon.png"));
			Graphics.WindowTitle = "UrhoSharp Sample";

			// Subscribe to Esc key:
			Input.SubscribeToKeyDown(args => { if (args.Key == Key.Esc) Exit(); });
		}
	}
}