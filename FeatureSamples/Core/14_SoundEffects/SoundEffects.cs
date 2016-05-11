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
using System.Linq;
using Urho.Gui;
using Urho.Resources;
using Urho.Gui;
using Urho.Audio;

namespace Urho.Samples
{
	public class SoundEffects : Sample
	{
		Scene scene;

		readonly Dictionary<string, string> sounds = new Dictionary<string, string>
			{
				{"Fist",      "Sounds/PlayerFistHit.wav"},
				{"Explosion", "Sounds/BigExplosion.wav"},
				{"Power-up",  "Sounds/Powerup.wav"},
			};

		public SoundEffects(ApplicationOptions options = null) : base(options) { }

		protected override void Start()
		{
			base.Start();
			Input.SetMouseVisible(true, false);
			CreateUI();
		}

		Button CreateButton(int x, int y, int xSize, int ySize, string text)
		{
			UIElement root = UI.Root;
			var cache = ResourceCache;
			Font font = cache.GetFont("Fonts/Anonymous Pro.ttf");

			// Create the button and center the text onto it
			Button button = new Button();
			root.AddChild(button);
			button.SetStyleAuto(null);
			button.SetPosition(x, y);
			button.SetSize(xSize, ySize);

			Text buttonText = new Text();
			button.AddChild(buttonText);
			buttonText.SetAlignment(HorizontalAlignment.Center, VerticalAlignment.Center);
			buttonText.SetFont(font, 12);
			buttonText.Value = text;

			return button;
		}

		Slider CreateSlider(int x, int y, int xSize, int ySize, string text)
		{
			UIElement root = UI.Root;
			ResourceCache cache = ResourceCache;
			Font font = cache.GetFont("Fonts/Anonymous Pro.ttf");
			// Create text and slider below it
			Text sliderText = new Text();
			root.AddChild(sliderText);
			sliderText.SetPosition(x, y);
			sliderText.SetFont(font, 12);
			sliderText.Value = text;

			Slider slider = new Slider();
			root.AddChild(slider);
			slider.SetStyleAuto(null);
			slider.SetPosition(x, y + 20);
			slider.SetSize(xSize, ySize);
			// Use 0-1 range for controlling sound/music master volume
			slider.Range = 1.0f;

			return slider;
		}

		void CreateUI()
		{
			var cache = ResourceCache;
			scene = new Scene();
			// Create a scene which will not be actually rendered, but is used to hold SoundSource components while they play sounds

			UIElement root = UI.Root;
			XmlFile uiStyle = cache.GetXmlFile("UI/DefaultStyle.xml");
			// Set style to the UI root so that elements will inherit it
			root.SetDefaultStyle(uiStyle);

			// Create buttons for playing back sounds
			int i = 0;
			foreach (var item in sounds)
			{
				Button button = CreateButton(i++ * 140 + 20, 20, 120, 40, item.Key);
				button.SubscribeToReleased(args => {
					// Get the sound resource
					Sound sound = cache.GetSound(item.Value);
					if (sound != null)
					{
						// Create a scene node with a SoundSource component for playing the sound. The SoundSource component plays
						// non-positional audio, so its 3D position in the scene does not matter. For positional sounds the
						// SoundSource3D component would be used instead
						Node soundNode = scene.CreateChild("Sound");
						SoundSource soundSource = soundNode.CreateComponent<SoundSource>();
						soundSource.Play(sound);
						// In case we also play music, set the sound volume below maximum so that we don't clip the output
						soundSource.Gain = 0.75f;
						// Set the sound component to automatically remove its scene node from the scene when the sound is done playing
					}
				});
			}
		
			// Create buttons for playing/stopping music
			var playMusicButton = CreateButton(20, 80, 120, 40, "Play Music");
			playMusicButton.SubscribeToReleased (args => {
				if (scene.GetChild ("Music", false) != null)
					return;

				var music = cache.GetSound ("Music/Ninja Gods.ogg");
				music.Looped = true;
				Node musicNode = scene.CreateChild ("Music");
				SoundSource musicSource = musicNode.CreateComponent<SoundSource> ();
				// Set the sound type to music so that master volume control works correctly
				musicSource.SetSoundType (SoundType.Music.ToString ());
				musicSource.Play (music);
			});

			var stopMusicButton = CreateButton(160, 80, 120, 40, "Stop Music");
			stopMusicButton.SubscribeToReleased (args => scene.RemoveChild (scene.GetChild ("Music", false)));

			// Create sliders for controlling sound and music master volume
			var soundSlider = CreateSlider(20, 140, 200, 20, "Sound Volume");
			soundSlider.Value = Audio.GetMasterGain(SoundType.Effect.ToString());
			soundSlider.SubscribeToSliderChanged(args => Audio.SetMasterGain(SoundType.Effect.ToString(), args.Value));
					
			var musicSlider = CreateSlider(20, 200, 200, 20, "Music Volume");
			musicSlider.Value = Audio.GetMasterGain(SoundType.Music.ToString());
			musicSlider.SubscribeToSliderChanged (args=> Audio.SetMasterGain(SoundType.Music.ToString(), args.Value));
		}

		protected override string JoystickLayoutPatch => JoystickLayoutPatches.Hidden;
	}
}
