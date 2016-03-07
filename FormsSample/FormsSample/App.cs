using System;
using Urho;
using Urho.Forms;
using Xamarin.Forms;

namespace FormsSample
{
	public class App : Xamarin.Forms.Application
	{
		public App()
		{
			MainPage = new NavigationPage(new UrhoPage());
		}
	}

	public class UrhoPage : ContentPage
	{
		UrhoSurface urhoSurface;
		Charts urhoApp;
		Slider selectedBarSlider;

		public UrhoPage()
		{
			urhoSurface = new UrhoSurface();
			urhoSurface.VerticalOptions = LayoutOptions.FillAndExpand;

			var rotationSlider = new Slider(0, 500, 250);
			rotationSlider.ValueChanged += (s, e) => urhoApp?.Rotate((float)(e.NewValue - e.OldValue));

			selectedBarSlider = new Slider(0, 5, 2.5);
			selectedBarSlider.ValueChanged += OnValuesSliderValueChanged;

			Title = " UrhoSharp + Xamarin.Forms";
			Content = new StackLayout {
					Padding = new Thickness(0, 0, 0, 40),
					VerticalOptions = LayoutOptions.FillAndExpand,
					Children = {
						urhoSurface,
						new Label { Text = "ROTATION:"},
						rotationSlider,
						new Label { Text = "SELECTED VALUE:" },
						selectedBarSlider,
					}
				};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			urhoApp = await urhoSurface.Show<Charts>(new ApplicationOptions(assetsFolder: null) { Orientation = ApplicationOptions.OrientationType.Portrait });
			foreach (var bar in urhoApp.Bars)
				bar.Selected += OnBarSelection;
		}

		void OnValuesSliderValueChanged(object sender, ValueChangedEventArgs e)
		{
			if (urhoApp?.SelectedBar != null)
				urhoApp.SelectedBar.Value = (float)e.NewValue;
		}

		private void OnBarSelection(Bar bar)
		{
			//reset value
			selectedBarSlider.ValueChanged -= OnValuesSliderValueChanged;
			selectedBarSlider.Value = bar.Value;
			selectedBarSlider.ValueChanged += OnValuesSliderValueChanged;
		}
	}
}
