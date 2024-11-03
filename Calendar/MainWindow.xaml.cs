using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calendar
{
    public partial class MainWindow : Window
    {
        private DateTime currentDate;

        public MainWindow()
        {
            InitializeComponent();
            currentDate = DateTime.Now;
            GenerateCalendar(currentDate);
        }

        // Method to generate the calendar grid
        private void GenerateCalendar(DateTime targetDate)
        {
            // Clear existing calendar grid content
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            // Display the current month and year
            MonthYearText.Text = targetDate.ToString("MMMM yyyy");

            // Define 7 columns for the days of the week
            for (int i = 0; i < 7; i++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Define rows (1 for day names, up to 6 for weeks)
            for (int i = 0; i < 7; i++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Get the day names based on current culture
            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;

            // Adjust if the first day of the week is Monday
            bool weekStartsOnMonday = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday;
            if (weekStartsOnMonday)
            {
                // Rotate the day names array to start from Monday
                dayNames = new string[7] { dayNames[1], dayNames[2], dayNames[3], dayNames[4], dayNames[5], dayNames[6], dayNames[0] };
            }

            // Add day names to the first row
            for (int i = 0; i < 7; i++)
            {
                TextBlock dayName = new TextBlock
                {
                    Text = dayNames[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.LightGray,
                    FontSize = 16 // Increased font size
                };
                Grid.SetRow(dayName, 0);
                Grid.SetColumn(dayName, i);
                CalendarGrid.Children.Add(dayName);
            }

            // Calculate the starting date to display
            DateTime firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            int firstDayIndex = (int)firstDayOfMonth.DayOfWeek;

            // Adjust index if the week starts on Monday
            if (weekStartsOnMonday)
            {
                firstDayIndex = firstDayIndex == 0 ? 6 : firstDayIndex - 1;
            }

            // Determine the first date to display (may be from the previous month)
            int daysToDisplayFromPrevMonth = firstDayIndex;
            DateTime firstDateToDisplay = firstDayOfMonth.AddDays(-daysToDisplayFromPrevMonth);

            // Total number of cells in the calendar grid (6 weeks x 7 days)
            int totalCells = 6 * 7;

            // Loop through each cell in the grid
            int currentRow = 1;
            int currentColumn = 0;

            for (int i = 0; i < totalCells; i++)
            {
                DateTime date = firstDateToDisplay.AddDays(i);

                Border dayBorder = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.Transparent
                };

                TextBlock dayNumber = new TextBlock
                {
                    Text = date.Day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14
                };

                // Check if the date is in the current month
                bool isCurrentMonth = date.Month == targetDate.Month;

                // Set styling based on whether the date is in the current month
                if (isCurrentMonth)
                {
                    dayNumber.Foreground = Brushes.White;

                    // Highlight today's date
                    bool isToday = date.Date == DateTime.Now.Date;
                    if (isToday)
                    {
                        dayBorder.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                    }
                }
                else
                {
                    dayNumber.Foreground = Brushes.Gray;
                    dayNumber.Text += $"\n{date.ToString("MMM")}";
                    dayNumber.FontSize = 12;
                }

                dayBorder.Child = dayNumber;

                Grid.SetRow(dayBorder, currentRow);
                Grid.SetColumn(dayBorder, currentColumn);
                CalendarGrid.Children.Add(dayBorder);

                // Move to the next cell
                currentColumn++;
                if (currentColumn > 6)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        // Event handler for the previous month button
        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(-1);
            GenerateCalendar(currentDate);
        }

        // Event handler for the next month button
        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentDate = currentDate.AddMonths(1);
            GenerateCalendar(currentDate);
        }
    }
}
