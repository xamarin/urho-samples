using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace FaceDetection.UWP
{
	public sealed partial class MainPage : Page
	{
		MediaCapture mediaCapture;
		WriteableBitmap tempWb;
		UrhoApp urhoApp;

		public MainPage()
		{
			this.InitializeComponent();
			ApplicationView.PreferredLaunchViewSize = new Size(880, 700);
			ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
			Loaded += MainPage_Loaded;
		}

		async void MainPage_Loaded(object sender, RoutedEventArgs e)
		{
			mediaCapture = new MediaCapture();
			await mediaCapture.InitializeAsync();

			CaptureElement.Source = mediaCapture;
			await mediaCapture.StartPreviewAsync();
			urhoApp = UrhoSurface.Run<UrhoApp>();
			urhoApp.CaptureVideo(CaptureFrameAsync);
		}

		private async Task<FrameWithFaces> CaptureFrameAsync()
		{
			var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
			var width = (int)previewProperties.Width;
			var height = (int)previewProperties.Height;
			var videoFrame = new VideoFrame(BitmapPixelFormat.Rgba8, width, height);
			if (tempWb == null)
				tempWb = new WriteableBitmap(width, height);

			using (var currentFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame))
			{
				SoftwareBitmap bitmap = currentFrame.SoftwareBitmap;
				var detector = await Windows.Media.FaceAnalysis.FaceDetector.CreateAsync();
				var supportedBitmapPixelFormats = Windows.Media.FaceAnalysis.FaceDetector.GetSupportedBitmapPixelFormats();
				var convertedBitmap = SoftwareBitmap.Convert(bitmap, supportedBitmapPixelFormats.First());
				var detectedFaces = await detector.DetectFacesAsync(convertedBitmap);

				byte[] bytes;
				bitmap.CopyToBuffer(tempWb.PixelBuffer);
				using (Stream stream = tempWb.PixelBuffer.AsStream())
				using (MemoryStream memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					bytes = memoryStream.ToArray();
					return new FrameWithFaces
						{
							FrameData = bytes,
							FrameWidth = width,
							FrameHeight = height,
							Faces = detectedFaces.Select(f => 
								new Rect {X = f.FaceBox.X, Y = f.FaceBox.Y, Width = f.FaceBox.Width, Height = f.FaceBox.Height}).ToArray()
						};
				}
			}
		}

		private void RotateButton_Click(object sender, RoutedEventArgs e)
		{
			Urho.Application.InvokeOnMain(() => urhoApp.Rotate());
		}
	}
}
