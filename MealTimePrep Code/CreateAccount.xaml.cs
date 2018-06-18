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
using System.Text.RegularExpressions;

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
    public sealed partial class CreateAccount : Page
    {
        public CreateAccount()
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

        async private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            //check database to make sure username isn't taken
            usersList = await userTable
                .Where(Users => Users.username == createUsername.Text)
                .ToCollectionAsync();

            var checkUsername = createUsername.Text;
            var checkPassword = createPassword.Password;
            var checkVerifyPassword = verifyPassword.Password;
            Char[] pwLetters = checkPassword.ToCharArray();
            var regexUNItem = new Regex("^[A-Za-z0-9._]+$");
            var regexPWItem = new Regex("^[A-Za-z0-9!.$_*^]+$");
            var regexPWUpper = new Regex("^[A-Z]+$");
            var regexPWLower = new Regex("^[a-z]+$");
            var regexPWNumber = new Regex("^[0-9]+$");
            var regexPWSC = new Regex("^[!.$_*^]+$");

            if (checkUsername != "" && checkPassword != "" && verifyPassword.Password != "")
            {
                if (usersList.Count == 0)
                {
                    if (!(regexUNItem.IsMatch(checkUsername)))
                    {
                        var dialog = new MessageDialog("Username contains invalid characters.");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        if (createPassword.Password == verifyPassword.Password)
                        {
                            if (regexPWItem.IsMatch(checkPassword))
                            {
                                if (pwLetters.Length >= 8 && pwLetters.Length <= 20)
                                {
                                    int upperCount = 0;
                                    int lowerCount = 0;
                                    int numberCount = 0;
                                    int scCount = 0;

                                    for (int i = 0; i < pwLetters.Length; i++)
                                    {
                                        if (regexPWUpper.IsMatch(pwLetters[i].ToString()))
                                        {
                                            upperCount++;
                                        }
                                        if (regexPWLower.IsMatch(pwLetters[i].ToString()))
                                        {
                                            lowerCount++;
                                        }
                                        if (regexPWNumber.IsMatch(pwLetters[i].ToString()))
                                        {
                                            numberCount++;
                                        }
                                        if (regexPWSC.IsMatch(pwLetters[i].ToString()))
                                        {
                                            scCount++;
                                        }
                                    }

                                    if (upperCount <= 0 || lowerCount <= 0 || numberCount <= 0 || scCount <= 0)
                                    {
                                        var dialog1 = new MessageDialog("Password entry invalid. Please retry.");
                                        await dialog1.ShowAsync();
                                    }
                                    else
                                    {
                                        Users addUser = new Users
                                        {
                                            username = createUsername.Text,
                                            password = createPassword.Password
                                        };

                                        await App.MobileService.GetTable<Users>().InsertAsync(addUser);
                                        var dialog = new MessageDialog("Account Created Succussfully!");
                                        await dialog.ShowAsync();
                                    }

                                }
                                else
                                {
                                    var dialog2 = new MessageDialog("Password length is invalid. Please retry ");
                                    await dialog2.ShowAsync();
                                }
                            }
                            else
                            {
                                var dialog3 = new MessageDialog("Invalid password characters. Please try again.");
                                await dialog3.ShowAsync();
                            }
                        }
                        else
                        {
                            var dialog = new MessageDialog("Passwords don't match. Please try again.");
                            await dialog.ShowAsync();
                        }

                    }
                }
                else
                {
                    var dialog = new MessageDialog("Username already exists. Please choose another username.");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new MessageDialog("Please fill in all fields.");
                await dialog.ShowAsync();
            }
        }

        private void BackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }
    }
}
