using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using ZXing;
using ZXing.Mobile;

namespace SmartHome.HoloLens
{
	public static class QrCodeReader
	{
		const int DelayBetweenScans = 100;

		/// <summary>
		/// Continuously look for a QR code
		/// NOTE: this method won't work if recording is enabled ('hey Cortana, start recording' thing).
		/// </summary>
		public static async Task<string> ReadAsync(CancellationToken token = default(CancellationToken))
		{
			var mediaCapture = new MediaCapture();
			await mediaCapture.InitializeAsync();
			await mediaCapture.AddVideoEffectAsync(new MrcVideoEffectDefinition(), MediaStreamType.Photo);

			var reader = new BarcodeReader();
			reader.Options.TryHarder = false;

			while (!token.IsCancellationRequested)
			{
				var imgFormat = ImageEncodingProperties.CreateJpeg();
				using (var ras = new InMemoryRandomAccessStream())
				{
					await mediaCapture.CapturePhotoToStreamAsync(imgFormat, ras);
					var decoder = await BitmapDecoder.CreateAsync(ras);
					using (var bmp = await decoder.GetSoftwareBitmapAsync())
					{
						Result result = await Task.Run(() =>
							{
								var source = new SoftwareBitmapLuminanceSource(bmp);
								return reader.Decode(source);
							});
						if (!string.IsNullOrEmpty(result?.Text))
							return result.Text;
					}
				}
				await Task.Delay(DelayBetweenScans);
			}
			return null;
		}
	}
	
	public class MrcVideoEffectDefinition : IVideoEffectDefinition
	{
		public string ActivatableClassId => "Windows.Media.MixedRealityCapture.MixedRealityCaptureVideoEffect";

		public IPropertySet Properties { get; }

		public MrcVideoEffectDefinition()
		{
			Properties = new PropertySet
				{
					{"HologramCompositionEnabled", false}, //remove holograms
					{"RecordingIndicatorEnabled", false},
					{"VideoStabilizationEnabled", false},
					{"VideoStabilizationBufferLength", 0},
					{"GlobalOpacityCoefficient", 0.9f},
					{"StreamType", (int)MediaStreamType.Photo}
				};
		}
	}
}
