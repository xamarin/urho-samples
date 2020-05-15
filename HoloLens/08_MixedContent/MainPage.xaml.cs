using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace MixedContent
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{		
	    public MainPage()
	    {
	        InitializeComponent();
	        NavigationCacheMode = NavigationCacheMode.Required;
	    }

        private async void OnSwitch2UrhoButton_Clicked(object sender, RoutedEventArgs e)
        {
            await ApplicationViewSwitcher.SwitchAsync(App.View3D.Id);
        }	  
    }
}
