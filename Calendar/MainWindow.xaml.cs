using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Calendar
{
    public partial class MainWindow : Window
    {
        private DateTime currentDate;
        private List<Holiday> holidays = new List<Holiday>();
        private Dictionary<int, List<Holiday>> holidayCache = new Dictionary<int, List<Holiday>>();

        public MainWindow()
        {
            InitializeComponent();
            currentDate = DateTime.Now;
            GenerateCalendar(currentDate);
        }

        private async Task FetchHolidays(int year)
        {
            if (holidayCache.ContainsKey(year))
            {
                // Use cached holidays to avoid unnecessary API calls
                holidays = holidayCache[year];
                return;
            }

            string countryCode = "CA"; // Country code for Canada
            string url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{countryCode}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string json = await client.GetStringAsync(url);
                    holidays = JsonConvert.DeserializeObject<List<Holiday>>(json);
                    holidayCache[year] = holidays;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching holidays: {ex.Message}");
                    holidays = new List<Holiday>(); // Ensure holidays is not null
                }
            }
        }

        // Method to generate the calendar grid
        private async void GenerateCalendar(DateTime targetDate)
        {
            // Clear existing calendar grid content
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            // Fetch holidays for the year if not already fetched
            if (!holidayCache.ContainsKey(targetDate.Year))
            {
                await FetchHolidays(targetDate.Year);
            }
            else
            {
                holidays = holidayCache[targetDate.Year];
            }

            // Display the current month and year
            MonthYearText.Text = targetDate.ToString("MMMM yyyy");

            // Define columns and rows
            for (int i = 0; i < 7; i++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < 7; i++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Add day names to the first row
            string[] dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            bool weekStartsOnMonday = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday;

            if (weekStartsOnMonday)
            {
                dayNames = new string[7] { dayNames[1], dayNames[2], dayNames[3], dayNames[4], dayNames[5], dayNames[6], dayNames[0] };
            }

            for (int i = 0; i < 7; i++)
            {
                TextBlock dayName = new TextBlock
                {
                    Text = dayNames[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.LightGray,
                    FontSize = 16
                };
                Grid.SetRow(dayName, 0);
                Grid.SetColumn(dayName, i);
                CalendarGrid.Children.Add(dayName);
            }

            // Calculate the starting date to display
            DateTime firstDayOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1);
            int firstDayIndex = (int)firstDayOfMonth.DayOfWeek;

            if (weekStartsOnMonday)
            {
                firstDayIndex = firstDayIndex == 0 ? 6 : firstDayIndex - 1;
            }

            int daysToDisplayFromPrevMonth = firstDayIndex;
            DateTime firstDateToDisplay = firstDayOfMonth.AddDays(-daysToDisplayFromPrevMonth);

            // Total number of cells in the calendar grid
            int totalCells = 6 * 7;

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

                StackPanel dayContent = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(2)
                };

                TextBlock dayNumber = new TextBlock
                {
                    Text = date.Day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 14
                };

                // Check if the date is in the current month
                bool isCurrentMonth = date.Month == targetDate.Month;

                if (isCurrentMonth)
                {
                    dayNumber.Foreground = Brushes.White;
                }
                else
                {
                    dayNumber.Foreground = Brushes.Gray;
                    dayNumber.Text += $"\n{date.ToString("MMM")}";
                    dayNumber.FontSize = 12;
                }

                // Determine the day of the week
                DayOfWeek dayOfWeek = date.DayOfWeek;
                bool isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

                // Apply subtle blue tint to weekends
                if (isWeekend && isCurrentMonth)
                {
                    dayBorder.Background = new SolidColorBrush(Color.FromRgb(45, 50, 60)); // Subtle blue tint
                }

                // Check if the date is a holiday
                Holiday holiday = holidays.FirstOrDefault(h => h.Date.Date == date.Date);

                if (holiday != null)
                {
                    // Style the day cell for a holiday
                    dayBorder.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70)); // Slightly different gray

                    // Add the holiday name to the cell
                    TextBlock holidayName = new TextBlock
                    {
                        Text = holiday.LocalName,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.LightGray,
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    };

                    dayContent.Children.Add(dayNumber);
                    dayContent.Children.Add(holidayName);
                }
                else
                {
                    dayContent.Children.Add(dayNumber);
                }

                // Highlight today's date
                bool isToday = date.Date == DateTime.Now.Date;
                if (isToday)
                {
                    // Make the current day the most noticeable with darker purple
                    dayBorder.Background = new SolidColorBrush(Color.FromRgb(75, 0, 130)); // Darker purple color
                    dayNumber.Foreground = Brushes.White;
                }

                dayBorder.Child = dayContent;

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
