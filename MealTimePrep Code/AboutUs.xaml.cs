using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MealTimePrep
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutUs : Page
    {
        public AboutUs()
        {
            this.InitializeComponent();
            DisplayUser();
        }

        private void DisplayUser()
        {
            welcomeMessage.Text = "Welcome " + MainPage.SessionUser.sessionUsername + "!";
        }

        private void BackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NavigationPage), null);
        }

        private void SignOutButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignOff), null);
        }
    }
}
