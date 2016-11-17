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
			var barcode = new ZXingBarcodeImageView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BarcodeFormat = ZXing.BarcodeFormat.QR_CODE,
				BarcodeOptions =
					{
						Width = 640,
						Height = 640,
					},
				BarcodeValue = ip,
			};
			BackgroundColor = Color.White;
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						TextColor = Color.Black,
						HorizontalTextAlignment = TextAlignment.Center,
						Text = $"Open SmartHome for HoloLens companion app and\nscan this qr code in order to be connected ({ip}):"
					},
					barcode
				}
			};

			await ScannerConnection.WaitForCompanion();
			await Navigation.PushAsync(new MainPage());
		}
	}
}
