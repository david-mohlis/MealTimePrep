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
    public sealed partial class Recipes : Page
    {
        public Recipes()
        {
            this.InitializeComponent();
            DisplayUser();
        }

        IMobileServiceTable<RecipesList> recipesList = App.MobileService.GetTable<RecipesList>();
        MobileServiceCollection<RecipesList, RecipesList> singleRecipe;

        public class RecipesList
        {
            public string Id { get; set; }
            public string userID { get; set; }
            public string recipeName { get; set; }
            public bool isChecked { get; set; }
            public string ingredient1 { get; set; }
            public string ingredient2 { get; set; }
            public string ingredient3 { get; set; }
            public string ingredient4 { get; set; }
            public string ingredient5 { get; set; }
            public string ingredient6 { get; set; }
            public string ingredient7 { get; set; }
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

        private void AddRecipe_Clicked(object sender, RoutedEventArgs e)
        {
            AddRecipeGrid.Visibility = Visibility.Visible;
            EditRecipeGrid.Visibility = Visibility.Collapsed;
            ViewRecipeGridHeader.Visibility = Visibility.Collapsed;

            ClearAdd();
        }

        async private void AddRecipeButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (addRecipeName.Text != "")
                {
                    RecipesList recipesList = new RecipesList
                    {
                        recipeName = addRecipeName.Text,
                        userID = MainPage.SessionUser.sessionUsername,
                        isChecked = (bool)recipeShareable.IsChecked,
                        ingredient1 = addingredient1.Text,
                        ingredient2 = addingredient2.Text,
                        ingredient3 = addingredient3.Text,
                        ingredient4 = addingredient4.Text,
                        ingredient5 = addingredient5.Text,
                        ingredient6 = addingredient6.Text,
                        ingredient7 = addingredient7.Text,
                    };

                    await App.MobileService.GetTable<RecipesList>().InsertAsync(recipesList);
                    var dialog = new MessageDialog("Recipe added successfully!");
                    await dialog.ShowAsync();

                    ClearAdd();
                }
                else
                {
                    var dialog = new MessageDialog("Please enter recipe name.");
                    await dialog.ShowAsync();               
                }
            }
            catch(Exception ex)
            {
                var dialog = new MessageDialog("An Error Occured: " + ex.Message);
                await dialog.ShowAsync();
            }
        }

        private void ClearAdd()
        {
            addRecipeName.Text = "";
            recipeShareable.IsChecked = false;
            addingredient1.Text = "";
            addingredient2.Text = "";
            addingredient3.Text = "";
            addingredient4.Text = "";
            addingredient5.Text = "";
            addingredient6.Text = "";
            addingredient7.Text = "";
        }

        async private void EditRecipe_Clicked(object sender, RoutedEventArgs e)
        {
            AddRecipeGrid.Visibility = Visibility.Collapsed;
            EditRecipeGrid.Visibility = Visibility.Visible;
            ViewRecipeGridHeader.Visibility = Visibility.Collapsed;

            ClearUpdate();

            MobileServiceInvalidOperationException exception = null;
            try
            {
                singleRecipe = await recipesList
                    .Where(RecipesList => RecipesList.userID == MainPage.SessionUser.sessionUsername)
                    .OrderBy(RecipesList => RecipesList.recipeName)
                    .ToCollectionAsync();

                if (singleRecipe.Count == 0)
                {
                    await new MessageDialog("No Recipes Available to Update").ShowAsync();
                }
                else
                {
                    editRecipeName.Items.Clear();
                    for (int i = 0; i < singleRecipe.Count; i++)
                    {
                        editRecipeName.Items.Add(singleRecipe[i].recipeName);
                    }
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }

        async private void LoadEditRecipe_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception1 = null;
            try
            {
                if (editRecipeName.SelectedValue != null)
                {
                    singleRecipe = await recipesList
                        .Where(RecipesList => RecipesList.recipeName == editRecipeName.SelectedItem.ToString())
                        .ToCollectionAsync();

                    editHiddenID.Text = singleRecipe[0].Id;
                    editHiddenRecipeName.Text = singleRecipe[0].recipeName;
                    editRecipeShareable.IsChecked = singleRecipe[0].isChecked;
                    editingredient1.Text = singleRecipe[0].ingredient1;
                    editingredient2.Text = singleRecipe[0].ingredient2;
                    editingredient3.Text = singleRecipe[0].ingredient3;
                    editingredient4.Text = singleRecipe[0].ingredient4;
                    editingredient5.Text = singleRecipe[0].ingredient5;
                    editingredient6.Text = singleRecipe[0].ingredient6;
                    editingredient7.Text = singleRecipe[0].ingredient7;
                }
                else
                {
                    await new MessageDialog("Recipe Name not selected.").ShowAsync();
                }
            }
            catch (MobileServiceInvalidOperationException e1)
            {
                exception1 = e1;
            }
        }

        async private void EditRecipeButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editRecipeName.SelectedValue != null)
                {
                    RecipesList editRecipe = new RecipesList
                    {
                        Id = editHiddenID.Text,
                        userID = MainPage.SessionUser.sessionUsername,
                        recipeName = editHiddenRecipeName.Text,
                        isChecked = (bool)editRecipeShareable.IsChecked,
                        ingredient1 = editingredient1.Text,
                        ingredient2 = editingredient2.Text,
                        ingredient3 = editingredient3.Text,
                        ingredient4 = editingredient4.Text,
                        ingredient5 = editingredient5.Text,
                        ingredient6 = editingredient6.Text,
                        ingredient7 = editingredient7.Text
                    };
                    await recipesList.UpdateAsync(editRecipe);
                    var dialog = new MessageDialog("Recipe edited successfully!");
                    await dialog.ShowAsync();

                    ClearUpdate();
                }
                else
                {
                    var dialog = new MessageDialog("Recipe not selected for edit.");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("An Error Occured: " + ex.Message);
                await dialog.ShowAsync();
            }
        }
        private void ClearUpdate()
        {
            editRecipeShareable.IsChecked = false;
            editingredient1.Text = "";
            editingredient2.Text = "";
            editingredient3.Text = "";
            editingredient4.Text = "";
            editingredient5.Text = "";
            editingredient6.Text = "";
            editingredient7.Text = "";
        }

        private void ViewRecipes_Clicked(object sender, RoutedEventArgs e)
        {
            AddRecipeGrid.Visibility = Visibility.Collapsed;
            EditRecipeGrid.Visibility = Visibility.Collapsed;
            ViewRecipeGridHeader.Visibility = Visibility.Visible;

            //Clear grid contents
            RemoveViewRecipeGrids();
            foreach (UIElement element in ViewRecipeGrid.Children)
            {
                TextBox textbox = element as TextBox;
                if (textbox != null)
                {
                    textbox.Text = String.Empty;
                }
            }
        }

        async private void ViewPersonal_Clicked(object sender, RoutedEventArgs e)
        {
            //clear grid contents
            RemoveViewRecipeGrids();
            foreach (UIElement element in ViewRecipeGrid.Children)
            {
                TextBox textbox = element as TextBox;
                if (textbox != null)
                {
                    textbox.Text = String.Empty;
                }
            }

            //Create Personal List
            MobileServiceInvalidOperationException exception = null;
            try
            {
                ViewRecipeGrid.Visibility = Visibility.Visible;

                singleRecipe = await recipesList
                    .Where(RecipesList => RecipesList.userID == MainPage.SessionUser.sessionUsername)
                    .ToCollectionAsync();

                //Create Header
                ViewRecipeGrid.RowDefinitions.Add(new RowDefinition());
                ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());

                TextBox header1 = new TextBox();
                Grid.SetColumn(header1, 0);
                Grid.SetRow(header1, 0);
                ViewRecipeGrid.Children.Add(header1);
                header1.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                header1.BorderThickness = new Thickness(2);
                header1.IsReadOnly = true;
                header1.Text = "Recipe Name";

                TextBox header2 = new TextBox();
                Grid.SetColumn(header2, 1);
                Grid.SetRow(header2, 0);
                ViewRecipeGrid.Children.Add(header2);
                header2.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                header2.BorderThickness = new Thickness(2);
                header2.IsReadOnly = true;
                header2.Text = "Ingredients";

                for (int i = 0; i < singleRecipe.Count; i++)
                {
                    ViewRecipeGrid.RowDefinitions.Add(new RowDefinition());
                    ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    TextBox results1 = new TextBox();
                    Grid.SetColumn(results1, 0);
                    Grid.SetRow(results1, i + 1);
                    ViewRecipeGrid.Children.Add(results1);
                    results1.BorderThickness = new Thickness(2);
                    results1.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                    results1.IsReadOnly = true;
                    results1.Text = singleRecipe[i].recipeName;

                    TextBox results2 = new TextBox();
                    Grid.SetColumn(results2, 1);
                    Grid.SetRow(results2, i + 1);
                    ViewRecipeGrid.Children.Add(results2);
                    results2.BorderThickness = new Thickness(2);
                    results2.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                    results2.IsReadOnly = true;
                    results2.Name = "resultstxtbox1" + i;
                    results2.Text = singleRecipe[i].ingredient1;

                    if (singleRecipe[i].ingredient2 != "")
                    {
                        results2.Text += "; " + singleRecipe[i].ingredient2;

                        if (singleRecipe[i].ingredient3 != "")
                        {
                            results2.Text += "; " + singleRecipe[i].ingredient3;

                            if (singleRecipe[i].ingredient4 != "")
                            {
                                results2.Text += "; " + singleRecipe[i].ingredient4;

                                if (singleRecipe[i].ingredient5 != "")
                                {
                                    results2.Text += "; " + singleRecipe[i].ingredient5;

                                    if (singleRecipe[i].ingredient6 != "")
                                    {
                                        results2.Text += "; " + singleRecipe[i].ingredient6;

                                        if (singleRecipe[i].ingredient7 != "")
                                        {
                                            results2.Text += "; " + singleRecipe[i].ingredient7;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }

        }

        async private void ViewGlobal_Clicked(object sender, RoutedEventArgs e)
        {
            //clear grid contents
            RemoveViewRecipeGrids();
            foreach (UIElement element in ViewRecipeGrid.Children)
            {
                TextBox textbox = element as TextBox;
                if (textbox != null)
                {
                    textbox.Text = String.Empty;
                }
            }

            //Create Global List
            MobileServiceInvalidOperationException exception = null;
            try
            {
                ViewRecipeGrid.Visibility = Visibility.Visible;

                singleRecipe = await recipesList
                    .Where(RecipesList => RecipesList.isChecked == true)
                    .ToCollectionAsync();

                //Create Header
                ViewRecipeGrid.RowDefinitions.Add(new RowDefinition());
                ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());

                TextBox header1 = new TextBox();
                Grid.SetColumn(header1, 0);
                Grid.SetRow(header1, 0);
                ViewRecipeGrid.Children.Add(header1);
                header1.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                header1.BorderThickness = new Thickness(2);
                header1.IsReadOnly = true;
                header1.Text = "Recipe Name";

                TextBox header2 = new TextBox();
                Grid.SetColumn(header2, 1);
                Grid.SetRow(header2, 0);
                ViewRecipeGrid.Children.Add(header2);
                header2.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                header2.BorderThickness = new Thickness(2);
                header2.IsReadOnly = true;
                header2.Text = "Ingredients";

                for (int i = 0; i < singleRecipe.Count; i++)
                {
                    ViewRecipeGrid.RowDefinitions.Add(new RowDefinition());
                    ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    ViewRecipeGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    TextBox results1 = new TextBox();
                    Grid.SetColumn(results1, 0);
                    Grid.SetRow(results1, i + 1);
                    ViewRecipeGrid.Children.Add(results1);
                    results1.BorderThickness = new Thickness(2);
                    results1.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                    results1.IsReadOnly = true;
                    results1.Text = singleRecipe[i].recipeName;

                    TextBox results2 = new TextBox();
                    Grid.SetColumn(results2, 1);
                    Grid.SetRow(results2, i + 1);
                    ViewRecipeGrid.Children.Add(results2);
                    results2.BorderThickness = new Thickness(2);
                    results2.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black);
                    results2.IsReadOnly = true;
                    results2.Name = "resultstxtbox1" + i;
                    results2.Text = singleRecipe[i].ingredient1;

                    if (singleRecipe[i].ingredient2 != "")
                    {
                        results2.Text += "; " + singleRecipe[i].ingredient2;

                        if (singleRecipe[i].ingredient3 != "")
                        {
                            results2.Text += "; " + singleRecipe[i].ingredient3;

                            if (singleRecipe[i].ingredient4 != "")
                            {
                                results2.Text += "; " + singleRecipe[i].ingredient4;

                                if(singleRecipe[i].ingredient5 != "")
                                {
                                    results2.Text += "; " + singleRecipe[i].ingredient5;

                                    if(singleRecipe[i].ingredient6 != "")
                                    {
                                        results2.Text += "; " + singleRecipe[i].ingredient6;

                                        if(singleRecipe[i].ingredient7 != "")
                                        {
                                            results2.Text += "; " + singleRecipe[i].ingredient7;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }
        private void RemoveViewRecipeGrids()
        {
            ViewRecipeGrid.ColumnDefinitions.Clear();
            ViewRecipeGrid.RowDefinitions.Clear();
        }


    }
}
