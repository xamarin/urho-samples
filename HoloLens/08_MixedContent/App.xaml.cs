using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Urho;
using Application = Windows.UI.Xaml.Application;


namespace MixedContent
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{		
	    public static ApplicationView ViewXaml;
	    public static ApplicationView View3D;

	    /// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			InitializeComponent();			
		}
		
	

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used when the application is launched to open a specific file, to display
		/// search results, and so forth.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{
			InitializeUhroSharp();
		}

		private void InitializeUhroSharp()
		{			            
			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
		    if (rootFrame == null)
		    {
		        rootFrame = new Frame();
		        Window.Current.Content = rootFrame;
                
                rootFrame.Navigate(typeof(MainPage));
                
                //remember Xaml view 
                ViewXaml = ApplicationView.GetForCurrentView();

                //create an urho view we can switch to
		        Create3DView();

		    }
            
		    Window.Current.Activate();
		}

	    private async void Create3DView()
	    {
            var viewSource = new UrhoAppViewSource<MutantApp>(new ApplicationOptions("Data"));
            CoreApplicationView urhoView = CoreApplication.CreateNewView(viewSource);

            View3D = null;

            await urhoView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                View3D = ApplicationView.GetForCurrentView();
                CoreWindow.GetForCurrentThread().Activate();
            });
        }
	}
}
