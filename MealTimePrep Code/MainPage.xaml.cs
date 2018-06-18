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

using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Storage;
using System.Net.Http;
using Newtonsoft.Json;

using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Text;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MealTimePrep
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        IMobileServiceTable<Users> userTable = App.MobileService.GetTable<Users>();
        MobileServiceCollection<Users, Users> usersList;

        public class Users
        {
            public string ID { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginAuthenticate();
        }

        async private void LoginAuthenticate()
        {
            try
            {                
                //search database for username and password
                usersList = await userTable
                        .Where(Users => Users.username == loginUsername.Text && Users.password == loginPassword.Password)
                        .ToCollectionAsync();

                if (loginUsername.Text != "" || loginPassword.Password != "")
                {
                    if (usersList.Count == 0)
                    {
                        var dialog = new MessageDialog("Username/Password incorrect. Please try again.");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        var dialog = new MessageDialog("Login Successful!");
                        await dialog.ShowAsync();
                        SessionUser.sessionUserID = "";
                        SessionUser.sessionUsername = "";
                        SessionUser.sessionUserID = usersList[0].ID;
                        SessionUser.sessionUsername = loginUsername.Text;
                        this.Frame.Navigate(typeof(NavigationPage), null);
                    }
                }
                else
                {
                    var dialog = new MessageDialog("Please enter username and password.");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception em)
            {
                var dialog = new MessageDialog("An Error Occured: " + em.Message);
                await dialog.ShowAsync();
            }

        }

        public static class SessionUser
        {
            public static string sessionUserID;
            public static string sessionUsername;
        }


        private void CreateAccountNav_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateAccount), null);
        }
    }
}
