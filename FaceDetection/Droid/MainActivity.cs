using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using Urho.Droid;
using Camera = Android.Hardware.Camera;

namespace FaceDetection.Droid
{
	[Activity(Label = "FaceDetection.Droid", MainLauncher = true,
		Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
		ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation,
		ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity, Camera.IPreviewCallback
	{
		byte[] lastFrame;
		Camera camera;
		UrhoApp urhoApp;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			var layout = new AbsoluteLayout(this);
			Urho.Application.Started += UrhoAppStarted;
			var surface = UrhoSurface.CreateSurface<UrhoApp>(this);
			layout.AddView(surface);
			SetContentView(layout);
		}

		void UrhoAppStarted()
		{
			urhoApp = Urho.Application.Current as UrhoApp;
			var camera = Camera.Open();
			camera.SetPreviewCallback(this);
			var parameters = camera.GetParameters();
			parameters.SetPreviewSize(320, 240);//TODO: check if this size is supported
			camera.SetParameters(parameters);
			camera.StartPreview();
			//camera.StartFaceDetection();
			urhoApp.CaptureVideo(OnFrameRequested);
		}

		async Task<FrameWithFaces> OnFrameRequested()
		{
			while (lastFrame == null)
				await Task.Delay(100); //wait for the first frame to appear

			return new FrameWithFaces {FrameData = lastFrame, FrameWidth = 320, FrameHeight = 240};
		}

		public void OnPreviewFrame(byte[] data, Camera camera)
		{
			//TODO: NV21 to RGB?
			lastFrame = data;
		}

		protected override void OnResume()
		{
			UrhoSurface.OnResume();
			base.OnResume();
		}

		protected override void OnPause()
		{
			UrhoSurface.OnPause();
			base.OnPause();
		}

		public override void OnLowMemory()
		{
			UrhoSurface.OnLowMemory();
			base.OnLowMemory();
		}

		protected override void OnDestroy()
		{
			UrhoSurface.OnDestroy();
			base.OnDestroy();
		}

		public override bool DispatchKeyEvent(KeyEvent e)
		{
			if (!UrhoSurface.DispatchKeyEvent(e))
				return false;
			return base.DispatchKeyEvent(e);
		}

		public override void OnWindowFocusChanged(bool hasFocus)
		{
			UrhoSurface.OnWindowFocusChanged(hasFocus);
			base.OnWindowFocusChanged(hasFocus);
		}
	}
}

