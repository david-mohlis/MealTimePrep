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
    public sealed partial class CalendarPlanner : Page
    {
        public CalendarPlanner()
        {
            this.InitializeComponent();
            DisplayUser();
        }

        //create link to Calendar easy table in Microsoft Azure
        IMobileServiceTable<Calendar> myCalendar = App.MobileService.GetTable<Calendar>();
        MobileServiceCollection<Calendar, Calendar> calendarList;
       
        public class Calendar
        {
            public string Id { get; set; }
            public string username { get; set; }
            public string mealType { get; set; }
            public string mealName { get; set; }
            public string mealDay { get; set; }
            public Boolean isRemoved { get; set; }
        }

        //displays username in top left corner
        private void DisplayUser()
        {
            welcomeMessage.Text = "Welcome " + MainPage.SessionUser.sessionUsername + "!";
        }

        //return to navigation page
        private void BackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NavigationPage), null);
        }

        //takes users to sign off page
        private void SignOutButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignOff), null);
        }

        //collapses weekly viewing grid and displays single day viewing grid options
        private void SingleDay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SingleDayGrid.Visibility = Visibility.Visible;
            WeeklyGrid.Visibility = Visibility.Collapsed;
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;
            addCalendarForm.Visibility = Visibility.Collapsed;
            weeklyAddCalendarForm.Visibility = Visibility.Collapsed;
        }

        //collapses single day viewing grids and displays weekly viewing grid options
        private void Weekly_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SingleDayGrid.Visibility = Visibility.Collapsed;
            WeeklyGrid.Visibility = Visibility.Visible;
            weeklyAddCalendarForm.Visibility = Visibility.Collapsed;
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;
            addCalendarForm.Visibility = Visibility.Collapsed;
        }

        //display meal planning form for weekly calendar
        private void WeeklyAddMeal_Clicked(object sender, RoutedEventArgs e)
        {
            WeeklyGrid.Visibility = Visibility.Collapsed;
            weeklyAddCalendarForm.Visibility = Visibility.Visible;

            WeeklySelectMealName();
        }

        //Adds meals from weekly calendar
        async private void AddWeeklyMeal_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (addWeeklyMealName.Text != "" && weeklyMealDaySelection.SelectedValue.ToString() != "" && weeklyMealTypeSelection.SelectedValue.ToString() != "")
                {


                    Calendar addCalendarItem = new Calendar
                    {
                        username = MainPage.SessionUser.sessionUsername,
                        mealType = hiddenWeeklyMealTypeSelection.Text,
                        mealDay = hiddenWeeklyMealDaySelection.Text,
                        mealName = addWeeklyMealName.Text,
                        isRemoved = false
                    };
                    await App.MobileService.GetTable<Calendar>().InsertAsync(addCalendarItem);
                    var dialog = new MessageDialog("Calendar item added successfully!");
                    await dialog.ShowAsync();

                    weeklyMealTypeSelection.SelectedValue = "";
                    weeklyMealDaySelection.SelectedValue = "";
                    addWeeklyMealName.Text = "";

                    weeklyAddCalendarForm.Visibility = Visibility.Collapsed;
                    WeeklyGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    var dialog = new MessageDialog("Please enter all required information.");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("An Error Occured: " + ex.Message);
                await dialog.ShowAsync();
            }
        }     

        //Shows add recipe screen from single day view
        private void AddCalendarItem_Clicked(object sender, RoutedEventArgs e)
        {
            SingleDayGrid.Visibility = Visibility.Collapsed;
            addCalendarForm.Visibility = Visibility.Visible;

            SingleSelectMealName();
        }

        //Refreshes single day calendar meal planning
        async private void RefreshCalendar_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                if (hiddenMealDaySelection.Text != "")
                {
                    calendarList = await myCalendar
                        .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == hiddenMealDaySelection.Text && Calendar.isRemoved == false)
                        .ToCollectionAsync();
                }
                else
                {
                    await new MessageDialog("Day not selected. Cannot load items.").ShowAsync();
                    return;
                }
            }
            catch(MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
                if (calendarList.Count == 0)
                {
                    if (hiddenMealDaySelection.Text == "Sunday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SundayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SundayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SundayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SundaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Monday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        MondayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        MondayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        MondayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        MondaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Tuesday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        TuesdayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        TuesdayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        TuesdayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        TuesdaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Wednesday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        WednesdayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        WednesdayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        WednesdayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        WednesdaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Thursday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        ThursdayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        ThursdayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        ThursdayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        ThursdaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Friday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        FridayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        FridayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        FridayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        FridaySnack.ItemsSource = calendarList;
                    }

                    if (hiddenMealDaySelection.Text == "Saturday")
                    {
                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SaturdayBreakfast.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SaturdayLunch.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SaturdayDinner.ItemsSource = calendarList;

                        calendarList = await myCalendar
                            .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                            .ToCollectionAsync();
                        SaturdaySnack.ItemsSource = calendarList;
                    }

                    var dialog = new MessageDialog("No meals saved for " + hiddenMealDaySelection.Text + ".");
                    await dialog.ShowAsync();
                }
                else
                {
                    for (int i = 0; i < calendarList.Count; i++)
                    {
                        if (hiddenMealDaySelection.Text == "Sunday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SundayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SundayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SundayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SundaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Monday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            MondayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            MondayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            MondayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            MondaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Tuesday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            TuesdayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            TuesdayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            TuesdayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            TuesdaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Wednesday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            WednesdayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            WednesdayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            WednesdayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            WednesdaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Thursday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            ThursdayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            ThursdayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            ThursdayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            ThursdaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Friday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            FridayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            FridayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            FridayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            FridaySnack.ItemsSource = calendarList;
                        }

                        if (hiddenMealDaySelection.Text == "Saturday" && calendarList[i].isRemoved == false)
                        {
                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SaturdayBreakfast.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SaturdayLunch.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SaturdayDinner.ItemsSource = calendarList;

                            calendarList = await myCalendar
                                .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                                .ToCollectionAsync();
                            SaturdaySnack.ItemsSource = calendarList;
                        }
                    }
                }

                await new MessageDialog("Calendar List Refreshed").ShowAsync();
            }         
        }
        //Refreshes weekly calendar meal planning
        async private void RefreshWeeklyCalendar_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.isRemoved == false)
                    .ToCollectionAsync();

                if (calendarList.Count == 0)
                {
                    var dialog = new MessageDialog("No meals saved for the week.");
                    await dialog.ShowAsync();
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
                calendarList = await myCalendar
                       .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                       .ToCollectionAsync();
                WeeklySundayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySundayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySundayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Sunday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySundaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyMondayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyMondayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyMondayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Monday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyMondaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyTuesdayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyTuesdayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyTuesdayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Tuesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyTuesdaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyWednesdayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyWednesdayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyWednesdayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Wednesday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyWednesdaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyThursdayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyThursdayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyThursdayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Thursday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyThursdaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyFridayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyFridayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyFridayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Friday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklyFridaySnack.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Breakfast" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySaturdayBreakfast.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Lunch" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySaturdayLunch.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Dinner" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySaturdayDinner.ItemsSource = calendarList;

                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == "Saturday" && Calendar.mealType == "Snack" && Calendar.isRemoved == false)
                    .ToCollectionAsync();
                WeeklySaturdaySnack.ItemsSource = calendarList;

                await new MessageDialog("Calendar List Refreshed").ShowAsync();
            }
        }

        //Displays single day grid for meal planner viewing
        private void Sunday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Visible;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = SundaySelected.Content.ToString();
        }
        private void Monday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Visible;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = MondaySelected.Content.ToString();
        }
        private void Tuesday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Visible;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = TuesdaySelected.Content.ToString();
        }
        private void Wednesday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Visible;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = WednesdaySelected.Content.ToString();
        }
        private void Thursday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Visible;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = ThursdaySelected.Content.ToString();
        }
        private void Friday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Visible;
            SaturdayGrid.Visibility = Visibility.Collapsed;

            hiddenMealDaySelection.Text = FridaySelected.Content.ToString();
        }
        private void Saturday_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SundayGrid.Visibility = Visibility.Collapsed;
            MondayGrid.Visibility = Visibility.Collapsed;
            TuesdayGrid.Visibility = Visibility.Collapsed;
            WednesdayGrid.Visibility = Visibility.Collapsed;
            ThursdayGrid.Visibility = Visibility.Collapsed;
            FridayGrid.Visibility = Visibility.Collapsed;
            SaturdayGrid.Visibility = Visibility.Visible;

            hiddenMealDaySelection.Text = SaturdaySelected.Content.ToString();
        }

        //Adds meal to database with selected day entered
        async private void AddMeal_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if(addMealName.Text != "" && mealDaySelection.SelectedValue.ToString() != "" && mealTypeSelection.SelectedValue.ToString() != "")
                {


                    Calendar addCalendarItem = new Calendar
                    {
                        username = MainPage.SessionUser.sessionUsername,
                        mealType = hiddenMealTypeSelection.Text,
                        mealDay = hiddenMealDaySelection.Text,
                        mealName = addMealName.Text,
                        isRemoved = false
                    };
                    await App.MobileService.GetTable<Calendar>().InsertAsync(addCalendarItem);
                    var dialog = new MessageDialog("Calendar item added successfully!");
                    await dialog.ShowAsync();

                    mealTypeSelection.SelectedValue = "";
                    mealDaySelection.SelectedValue = "";
                    addMealName.Text = "";

                    addCalendarForm.Visibility = Visibility.Collapsed;
                    SingleDayGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    var dialog = new MessageDialog("Please enter all required information.");
                    await dialog.ShowAsync();
                }
            }
            catch(Exception ex)
            {
                var dialog = new MessageDialog("An Error Occured: " + ex.Message);
                await dialog.ShowAsync();
            }
        }        

        //Obtain combobox values to hidden textblocks in calendarplanner.xaml
        private void Breakfast_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealTypeSelection.Text = mealTypeBreakfast.Content.ToString();
        }

        private void Lunch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealTypeSelection.Text = mealTypeLunch.Content.ToString();

        }

        private void Dinner_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealTypeSelection.Text = mealTypeDinner.Content.ToString();
        }

        private void Snack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealTypeSelection.Text = mealTypeSnack.Content.ToString();
        }

        private void SundayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDaySunday.Content.ToString();
        }

        private void MondayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDayMonday.Content.ToString();
        }

        private void TuesdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDayTuesday.Content.ToString();
        }

        private void WednesdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDayWednesday.Content.ToString();
        }

        private void ThursdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDayThursday.Content.ToString();
        }

        private void FridayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDayFriday.Content.ToString();
        }

        private void SaturdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenMealDaySelection.Text = mealDaySaturday.Content.ToString();
        }

        private void WeeklySundayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDaySunday.Content.ToString();
        }

        private void WeeklyMondayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDayMonday.Content.ToString();
        }

        private void WeeklyTuesdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDayTuesday.Content.ToString();
        }

        private void WeeklyWednesdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDayWednesday.Content.ToString();
        }

        private void WeeklyThursdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDayThursday.Content.ToString();
        }

        private void WeeklyFridayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDayFriday.Content.ToString();
        }

        private void WeeklySaturdayCombobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealDaySelection.Text = weeklyMealDaySaturday.Content.ToString();
        }

        private void WeeklyBreakfast_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealTypeSelection.Text = weeklyMealTypeBreakfast.Content.ToString();
        }

        private void WeeklyLunch_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealTypeSelection.Text = weeklyMealTypeLunch.Content.ToString();
        }

        private void WeeklyDinner_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealTypeSelection.Text = weeklyMealTypeDinner.Content.ToString();
        }

        private void WeeklySnack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            hiddenWeeklyMealTypeSelection.Text = weeklyMealTypeSnack.Content.ToString();
        }


        IMobileServiceTable<Recipes.RecipesList> recipesList = App.MobileService.GetTable<Recipes.RecipesList>();
        MobileServiceCollection<Recipes.RecipesList, Recipes.RecipesList> singleRecipe;
        //Build list of selectable recipies for users to select from on weekly view grid
        async private void WeeklySelectMealName()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                singleRecipe = await recipesList
                    .Where(RecipesList => RecipesList.isChecked == true || RecipesList.userID == MainPage.SessionUser.sessionUsername)
                    .OrderBy(RecipesList => RecipesList.recipeName)
                    .ToCollectionAsync();

                if (singleRecipe.Count != 0)
                {
                    addWeeklyRecipe.Items.Clear();
                    for(int i = 0; i < singleRecipe.Count; i++)
                    {
                        addWeeklyRecipe.Items.Add(singleRecipe[i].recipeName);
                    }
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }

        //add global and personal recipe name to single day grid view
        async private void SingleSelectMealName()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                singleRecipe = await recipesList
                    .Where(RecipesList => RecipesList.isChecked == true || RecipesList.userID == MainPage.SessionUser.sessionUsername)
                    .OrderBy(RecipesList => RecipesList.recipeName)
                    .ToCollectionAsync();

                if (singleRecipe.Count != 0)
                {
                    singleSelectMeal.Items.Clear();
                    for (int i = 0; i < singleRecipe.Count; i++)
                    {
                        singleSelectMeal.Items.Add(singleRecipe[i].recipeName);
                    }
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }

        //Populate selected weekly meal name to textbox
        async private void WeeklySelectMealName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception;
            try
            {
                if(addWeeklyRecipe.SelectedValue != null)
                {
                    singleRecipe = await recipesList
                        .Where(RecipesList => RecipesList.recipeName == addWeeklyRecipe.SelectedItem.ToString())
                        .ToCollectionAsync();

                    addWeeklyMealName.Text = singleRecipe[0].recipeName;
                }
            }
            catch(MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }

        //Populate selected single meal name to textbox
        private void SingleSelectMealName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(singleSelectMeal.SelectedValue != null)
            {
                addMealName.Text = singleSelectMeal.SelectedItem.ToString();
            }
        }

        //clear single day calendar meal
        async private void SingleDayClear_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                if (hiddenMealDaySelection.Text != "")
                {
                    calendarList = await myCalendar
                        .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.mealDay == hiddenMealDaySelection.Text && Calendar.isRemoved == false)
                        .ToCollectionAsync();
                    if (calendarList.Count != 0)
                    {
                        for (int i = 0; i < calendarList.Count; i++)
                        {
                            string tempId = calendarList[i].Id;
                            string tempUsername = calendarList[i].username;
                            string tempMealType = calendarList[i].mealType;
                            string tempMealName = calendarList[i].mealName;
                            string tempMealDay = calendarList[i].mealDay;

                            Calendar updateCalendar = new Calendar()
                            {
                                Id = tempId,
                                username = tempUsername,
                                mealType = tempMealType,
                                mealName = tempMealName,
                                mealDay = tempMealDay,
                                isRemoved = true
                            };

                            await myCalendar.UpdateAsync(updateCalendar);
                        }

                        await new MessageDialog(hiddenMealDaySelection.Text + " meals have been removed.").ShowAsync();
                    }
                    else
                    {
                        var dialog = new MessageDialog("No meals saved for " + hiddenMealDaySelection.Text + ".");
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    await new MessageDialog("Day not selected. Cannot delete items.").ShowAsync();                   
                }
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }            
        }

        //clear weekly calendar meals
        async private void WeeklyDayClear_Clicked(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                calendarList = await myCalendar
                    .Where(Calendar => Calendar.username == MainPage.SessionUser.sessionUsername && Calendar.isRemoved == false)
                    .ToCollectionAsync();

                int listItems = calendarList.Count;

                if (listItems != 0)
                {
                    for (int i = 0; i < listItems; i++)
                    {
                        string tempId = calendarList[i].Id;
                        string tempUsername = calendarList[i].username;
                        string tempMealType = calendarList[i].mealType;
                        string tempMealName = calendarList[i].mealName;
                        string tempMealDay = calendarList[i].mealDay;

                        Calendar updateCalendar = new Calendar()
                        {
                            Id = tempId,
                            username = tempUsername,
                            mealType = tempMealType,
                            mealName = tempMealName,
                            mealDay = tempMealDay,
                            isRemoved = true
                        };

                        await myCalendar.UpdateAsync(updateCalendar);

                    }

                    await new MessageDialog("All meals have been removed.").ShowAsync();                    
                }
                else
                {
                    var dialog = new MessageDialog("No meals saved for the week");
                    await dialog.ShowAsync();
                }               
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                exception = ex;
            }
        }        
    }
}
