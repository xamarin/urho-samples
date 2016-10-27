using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Microsoft.ProjectOxford.Vision;
using Urho;
using Urho.HoloLens;

namespace CognitiveServices
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<HelloWorldApplication>());
	}


	public class HelloWorldApplication : HoloApplication
	{
		//the key can be obtained for free here: https://www.microsoft.com/cognitive-services/en-us/computer-vision-api
		//click on "Get started for free"
		const string VisionApiKey = "YOUR KEY HERE";

		SpeechSynthesizer synthesizer;
		Node busyIndicatorNode;
		MediaCapture mediaCapture;
		bool inited;
		bool busy;

		public HelloWorldApplication(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			base.Start();

			busyIndicatorNode = Scene.CreateChild();
			busyIndicatorNode.SetScale(0.06f);
			busyIndicatorNode.CreateComponent<BusyIndicator>();

			mediaCapture = new MediaCapture();
			await mediaCapture.InitializeAsync();
			await mediaCapture.AddVideoEffectAsync(new MrcVideoEffectDefinition(), MediaStreamType.Photo);
			await RegisterCortanaCommands(new Dictionary<string, Action> {
					{"Describe", () => CaptureAndShowResult(false)}, // describe picture
					{"Read this text", () => CaptureAndShowResult(true)}, // we can use Bing Transalte if needed
				});

			ShowBusyIndicator(true);
			synthesizer = new SpeechSynthesizer();
			await TextToSpeech("Welcome to the Microsoft Cognition Services sample for HoloLens.");
			ShowBusyIndicator(false);

			inited = true;
		}

		async void CaptureAndShowResult(bool readText)
		{
			if (!inited || busy)
				return;
			
			ShowBusyIndicator(true);
			var desc = await CaptureAndAnalyze(readText);
			InvokeOnMain(() => ShowBusyIndicator(false));
			await TextToSpeech(desc);
		}

		async Task TextToSpeech(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			var tcs = new TaskCompletionSource<bool>();
			var stream = await synthesizer.SynthesizeTextToStreamAsync(text);
			var player = BackgroundMediaPlayer.Current;
			TypedEventHandler<MediaPlayer, object> mediaEndedHandler = null;
			mediaEndedHandler = (s, e) =>
				{
					tcs.TrySetResult(true);
					//subscribe once.
					player.MediaEnded -= mediaEndedHandler;
				};
			player.SetStreamSource(stream);
			player.MediaEnded += mediaEndedHandler;
			player.Play();
			await tcs.Task;
		}

		void ShowBusyIndicator(bool show)
		{
			busy = show;
			busyIndicatorNode.Position = LeftCamera.Node.WorldPosition + LeftCamera.Node.WorldDirection * 1f;
			//busyIndicatorNode.LookAt(LeftCamera.Node.WorldPosition, Vector3.UnitY, TransformSpace.World);
			busyIndicatorNode.GetComponent<BusyIndicator>().IsBusy = show;
		}
		
		async Task<string> CaptureAndAnalyze(bool readText = false)
		{
			var imgFormat = ImageEncodingProperties.CreateJpeg();
			var file = await KnownFolders.CameraRoll.CreateFileAsync($"MCS_Photo{DateTime.Now:HH-mm-ss}.jpg", CreationCollisionOption.GenerateUniqueName);
			await mediaCapture.CapturePhotoToStorageFileAsync(imgFormat, file);

			var stream = await file.OpenStreamForReadAsync();
			try
			{
				var client = new VisionServiceClient(VisionApiKey);
				if (readText)
				{
					var ocrResult = await client.RecognizeTextAsync(stream, detectOrientation: false);
					var words = ocrResult.Regions.SelectMany(region => region.Lines).SelectMany(line => line.Words).Select(word => word.Text);
					return "it says: " + string.Join(" ", words);
				}
				else
				{
					// just describe the picture, you can also use cleint.AnalyzeImageAsync method to get more info
					var result = await client.DescribeAsync(stream);
					return result?.Description?.Captions?.FirstOrDefault()?.Text;
				}
			}
			catch (ClientException exc)
			{
				return exc?.Error?.Message ?? "Failed";
			}
			catch (Exception exc)
			{
				return "Failed";
			}
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
					{"HologramCompositionEnabled", false},
					{"RecordingIndicatorEnabled", false},
					{"VideoStabilizationEnabled", false},
					{"VideoStabilizationBufferLength", 0},
					{"GlobalOpacityCoefficient", 0.9f},
					{"StreamType", (int)MediaStreamType.Photo}
				};
		}
	}
}