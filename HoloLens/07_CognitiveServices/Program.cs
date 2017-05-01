using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Microsoft.ProjectOxford.Vision;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Resources;
using Urho.Shapes;
using Urho.Urho2D;

namespace CognitiveServices
{
	internal class Program
	{
		[MTAThread]
		static void Main() => CoreApplication.Run(new UrhoAppViewSource<CognitiveServicesApp>());
	}


	public class CognitiveServicesApp : StereoApplication
	{
		//the key can be obtained for free here: https://www.microsoft.com/cognitive-services/en-us/computer-vision-api
		//click on "Get started for free"
		const string VisionApiKey = "YOUR KEY HERE";

		Node busyIndicatorNode;
		MediaCapture mediaCapture;
		bool inited;
		bool busy;
		bool withPreview;

		public CognitiveServicesApp(ApplicationOptions opts) : base(opts) { }

		protected override async void Start()
		{
			ResourceCache.AutoReloadResources = true;
			base.Start();

			EnableGestureTapped = true;

			busyIndicatorNode = Scene.CreateChild();
			busyIndicatorNode.SetScale(0.06f);
			busyIndicatorNode.CreateComponent<BusyIndicator>();

			mediaCapture = new MediaCapture();
			await mediaCapture.InitializeAsync();
			await mediaCapture.AddVideoEffectAsync(new MrcVideoEffectDefinition(), MediaStreamType.Photo);
			await RegisterCortanaCommands(new Dictionary<string, Action> {
					{"Describe", () => CaptureAndShowResult(false)},
					{"Read this text", () => CaptureAndShowResult(true)}, 
					{"Enable preview", () => EnablePreview(true) },
					{"Disable preview", () => EnablePreview(false) },
					{"Help", Help }
				});
			
			ShowBusyIndicator(true);
			await TextToSpeech("Welcome to the Microsoft Cognitive Services sample for HoloLens and UrhoSharp.");
			ShowBusyIndicator(false);

			inited = true;
		}

		async void Help()
		{
			await TextToSpeech("Available commands are:");
			foreach (var cortanaCommand in CortanaCommands.Keys)
				await TextToSpeech(cortanaCommand);
		}

		async void EnablePreview(bool enable)
		{
			withPreview = enable;
			await TextToSpeech("Preview mode is " + (enable ? "enabled" : "disabled"));
		}

		public override void OnGestureDoubleTapped()
		{
			CaptureAndShowResult(false);
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

		void ShowBusyIndicator(bool show)
		{
			busy = show;
			busyIndicatorNode.Position = LeftCamera.Node.WorldPosition + LeftCamera.Node.WorldDirection * 1f;
			busyIndicatorNode.GetComponent<BusyIndicator>().IsBusy = show;
		}
		
		async Task<string> CaptureAndAnalyze(bool readText = false)
		{
			var imgFormat = ImageEncodingProperties.CreateJpeg();

			//NOTE: this is how you can save a frame to the CameraRoll folder:
			//var file = await KnownFolders.CameraRoll.CreateFileAsync($"MCS_Photo{DateTime.Now:HH-mm-ss}.jpg", CreationCollisionOption.GenerateUniqueName);
			//await mediaCapture.CapturePhotoToStorageFileAsync(imgFormat, file);
			//var stream = await file.OpenStreamForReadAsync();

			// Capture a frame and put it to MemoryStream
			var memoryStream = new MemoryStream();
			using (var ras = new InMemoryRandomAccessStream())
			{
				await mediaCapture.CapturePhotoToStreamAsync(imgFormat, ras);
				ras.Seek(0);
				using (var stream = ras.AsStreamForRead())
					stream.CopyTo(memoryStream);
			}

			var imageBytes = memoryStream.ToArray();
			memoryStream.Position = 0;

			if (withPreview)
			{
				InvokeOnMain(() =>
					{
						var image = new Image();
						image.Load(new Urho.MemoryBuffer(imageBytes));

						Node child = Scene.CreateChild();
						child.Position = LeftCamera.Node.WorldPosition + LeftCamera.Node.WorldDirection * 2f;
						child.LookAt(LeftCamera.Node.WorldPosition, Vector3.Up, TransformSpace.World);

						child.Scale = new Vector3(1f, image.Height / (float)image.Width, 0.1f) / 10;
						var texture = new Texture2D();
						texture.SetData(image, true);

						var material = new Material();
						material.SetTechnique(0, CoreAssets.Techniques.Diff, 0, 0);
						material.SetTexture(TextureUnit.Diffuse, texture);

						var box = child.CreateComponent<Box>();
						box.SetMaterial(material);

						child.RunActions(new EaseBounceOut(new ScaleBy(1f, 5)));
					});
			}
			
			try
			{
				var client = new VisionServiceClient(VisionApiKey);
				if (readText)
				{
					var ocrResult = await client.RecognizeTextAsync(memoryStream, detectOrientation: false);
					var words = ocrResult.Regions.SelectMany(region => region.Lines).SelectMany(line => line.Words).Select(word => word.Text);
					return "it says: " + string.Join(" ", words);
				}
				else
				{
					// just describe the picture, you can also use cleint.AnalyzeImageAsync method to get more info
					var result = await client.DescribeAsync(memoryStream);
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