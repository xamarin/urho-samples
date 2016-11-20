using System;
using System.Threading.Tasks;
using Shared;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace SmartHome
{
	public class QrPage : ContentPage
	{
		public QrPage()
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Initialize();
		}

		async void Initialize()
		{
			var ip = await ScannerConnection.GetLocalIp() ?? "ERROR";

			Button offlineModeBtn = new Button
				{
					Text = "Offline mode",
					BackgroundColor = new Color(0.8f, 0.8f, 0.8f)
				};
			offlineModeBtn.Clicked += OnOfflineModeClicked;

			var qrSize = 320;
			var barcode = new ZXingBarcodeImageView
			{
				WidthRequest = qrSize,
				HeightRequest = qrSize,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BarcodeFormat = ZXing.BarcodeFormat.QR_CODE,
				BarcodeOptions =
					{
						Width = qrSize,
						Height = qrSize,
					},
				BarcodeValue = ip,
			};
			BackgroundColor = Color.White;
			var stack = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						TextColor = Color.Black,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = $"Open SmartHome for HoloLens companion app and\nscan this qr code in order to be connected ({ip}):"
					},
					barcode,
				}
			};

			Content = stack;
			if (Application.Current.Properties.ContainsKey(nameof(SpaceDto)))
			{
				stack.Children.Add(offlineModeBtn);
			}

			var connection = new ScannerConnection();
			await connection.WaitForCompanion();
			await Navigation.PushAsync(new MainPage(connection, false));
		}

		async void OnOfflineModeClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new MainPage(null, true));
		}
	}
}
