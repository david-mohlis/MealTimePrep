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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MealTimePrep
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GroceryList : Page
    {
        public GroceryList()
        {
            this.InitializeComponent();
            DisplayUser();
        }


        IMobileServiceTable<GroceryListItems> grocList = App.MobileService.GetTable<GroceryListItems>();
        MobileServiceCollection<GroceryListItems, GroceryListItems> items;

        public class GroceryListItems
        {
            public string Id { get; set; }
            public string username { get; set; }
            public string Text { get; set; }
            public bool Complete { get; set; }
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

        async private void AddGroceryItem_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                GroceryListItems item = new GroceryListItems
                {
                    username = MainPage.SessionUser.sessionUsername,
                    Text = groceryItem.Text,
                    Complete = false
                };
                await App.MobileService.GetTable<GroceryListItems>().InsertAsync(item);
                var dialog = new MessageDialog("Successful!");
                await dialog.ShowAsync();
            }
            catch (Exception em)
            {
                var dialog = new MessageDialog("An Error Occured: " + em.Message);
                await dialog.ShowAsync();
            }
        }

        async private void RefreshButton_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {

                items = await grocList
                    .Where(GroceryListItems => GroceryListItems.Complete == false && GroceryListItems.username == MainPage.SessionUser.sessionUsername)
                    .ToCollectionAsync();

                if (items.Count == 0)
                {
                    await new MessageDialog("No items in list").ShowAsync();
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                ListItems.ItemsSource = items;                
            }
        }
        private async void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            GroceryListItems item = cb.DataContext as GroceryListItems;
            await UpdateCheckedGrocListItem(item);
        }
        private async Task UpdateCheckedGrocListItem(GroceryListItems item)
        {
            // This code takes a freshly completed GroceryList and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await grocList.UpdateAsync(item);
            items.Remove(item);
            ListItems.Focus(Windows.UI.Xaml.FocusState.Unfocused);           
        }
    }
}
