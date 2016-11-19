using System;
using System.Threading.Tasks;
using Shared;
using Urho;
using Urho.Forms;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;
using Color = Xamarin.Forms.Color;

namespace SmartHome
{
	public class MainPage : ContentPage
	{
		readonly StackLayout bulbsStack;
		readonly UrhoSurface urhoSurface;
		readonly ApartmentsDto apartments;
		readonly INetworkSerializer serializer;
		UrhoApp app;

		public MainPage(ScannerConnection connection, bool offlineMode)
		{
			serializer = new ProtobufNetworkSerializer();
			NavigationPage.SetHasNavigationBar(this, false);

			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
			grid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(4, GridUnitType.Star)});

			bulbsStack = new StackLayout();
			grid.Children.Add(bulbsStack);

			urhoSurface = new UrhoSurface
				{
					BackgroundColor = Color.Black,
					VerticalOptions = LayoutOptions.FillAndExpand
				};

			var stack = new StackLayout
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					Children = {urhoSurface}
				};

			grid.Children.Add(stack);
			Grid.SetColumn(stack, 1);
			Content = grid;

			if (!offlineMode)
			{
				apartments = new ApartmentsDto();
				connection.RegisterFor<SurfaceDto>(OnSurfaceReceived);
				connection.RegisterFor<BulbAddedDto>(OnBulbAdded);
				connection.RegisterFor<CurrentPositionDto>(OnCurrentPositionUpdated);

				Start();
			}
			else
			{
				apartments = serializer.Deserialize<ApartmentsDto>(
					(byte[])Application.Current.Properties[nameof(ApartmentsDto)]);
			}
		}

		async void Start()
		{
			while (true)
			{
				lock (apartments)
					Application.Current.Properties[nameof(ApartmentsDto)] = serializer.Serialize(apartments);
				await Application.Current.SavePropertiesAsync();
				await Task.Delay(3000);
			}
		}

		void OnBulbAdded(BulbAddedDto dto)
		{
			lock (apartments)
				apartments.Bulbs.Add(dto.Position);
			AddBulb(dto.Position);
		}

		void AddBulb(Vector3Dto position)
		{
			Urho.Application.InvokeOnMain(() => app?.AddBulb(new Vector3(position.X, position.Y, position.Z)));
			Device.BeginInvokeOnMainThread(() =>
			{
				int index = bulbsStack.Children.Count;
				Button button = new Button();
				button.FontSize = 24;
				button.TextColor = Color.Black;
				button.BackgroundColor = new Color(0.8, 0.8, 0.8);
				button.Text = "Bulb " + index;
				button.Clicked += (s, e) =>
				{
					ToggleRealDevice(index);
					Urho.Application.InvokeOnMain(() => app?.ToggleLight(index));
				};
				bulbsStack.Children.Add(button);
			});
		}

		async void ToggleRealDevice(int index)
		{
			// This code is just an example of how to work with some IoT devices, 
			// for example - LIFX bulbs using LifxHttp library https://github.com/mensly/LifxHttpNet

#if !WINDOWS_UWP
			try
			{
				// generate a new token at https://cloud.lifx.com/settings
				var client = new LifxHttp.LifxClient("your token here");
				var lights = await client.ListLights();
				if (index < lights.Count)
					await lights[index].TogglePower();
			}
			catch (Exception exc) { }
#endif
		}

		void OnCurrentPositionUpdated(CurrentPositionDto dto)
		{
			Urho.Application.InvokeOnMain(() => app?.UpdateCurrentPosition(
				new Vector3(dto.Position.X, dto.Position.Y, dto.Position.Z),
				new Vector3(dto.Direction.X, dto.Direction.Y, dto.Direction.Z)));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			StartUrhoApp();
		}

		void OnSurfaceReceived(SurfaceDto surface)
		{
			lock (apartments)
				apartments.Surfaces[surface.Id] = surface;
			Urho.Application.InvokeOnMain(() => app?.AddOrUpdateSurface(surface));
		}

		async void StartUrhoApp()
		{
			app = await urhoSurface.Show<UrhoApp>(new Urho.ApplicationOptions(assetsFolder: "Data"));
			Urho.Application.InvokeOnMain(() =>
				{
					foreach (var surface in apartments.Surfaces)
						app.AddOrUpdateSurface(surface.Value);
					foreach (var bulb in apartments.Bulbs)
						AddBulb(bulb);
				});
		}
	}
}
