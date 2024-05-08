using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyCalendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        SoundPlayer player;

        private readonly string baseUrl = "https://prayer.aviny.com/api/prayertimes/";
        private readonly Dictionary<string, int> cities = new Dictionary<string, int>
        {
            { "تهران", 1 },
            { "کرج", 9 },
            { "رشت", 16 },
            { "کبودراهنگ", 849 }
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeCityComboBox();
            string selectedCity = comboBoxCities.SelectedItem.ToString();
            int cityCode = cities[selectedCity];
            _ = getInfo(cityCode);

            labelNow.Content = DateTime.Now.ToString("HH:mm:ss");

            // Initialize and start the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.9);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Initialize sound player
            var path = "mixkit-bell-notification-933.wav";
            player = new SoundPlayer(path);
            player.Load();
        }

        // Helper method to find all child controls of a given type
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the label with the current time
            var now = DateTime.Now.ToString("HH:mm:ss");
            labelNow.Content = now;

            // Iterate through all labels in your form
            foreach (var item in FindVisualChildren<Label>(this))
            {
                if (item.Name.Contains("Time"))
                {
                    // Check if the label's content is equal to the current time
                    if (item.Content != null && item.Content.ToString().Replace(" ", "").Equals(now))
                    {
                        this.Topmost = true;
                        item.Foreground = Brushes.Red;
                        item.Background = Brushes.White;
                        player.Play();
                        this.Topmost = false;
                        break;
                    }
                    else if (item.Content != null && item.Content.ToString().Contains(now[0..5]) == false)              {
                        if (item.Foreground == Brushes.Red)
                        {
                            item.Foreground = Brushes.White;
                            item.Background = Brushes.Transparent;
                            break;
                        }
                    }
                }
            }
        }

        private void InitializeCityComboBox()
        {
            foreach (var city in cities.Keys)
            {
                comboBoxCities.Items.Add(city);
            }
            comboBoxCities.SelectedIndex = 0;
        }

        private async void comboBoxCities_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedCity = comboBoxCities.SelectedItem.ToString();
            int cityCode = cities[selectedCity];
            await getInfo(cityCode);

            comboBoxCities.Visibility = Visibility.Hidden;
            labelCity.Visibility = Visibility.Visible;
        }

        private async Task getInfo(int cityCode)
        {
            try
            {
                progressBar.Visibility = Visibility.Visible;
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{baseUrl}{cityCode}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResult = await response.Content.ReadAsStringAsync();
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<PrayerTimesResult>(jsonResult);


                        labelDate.Content = $"{result.Today.Split("-")[0], 14}";
                        labelCity.Content = $"{result.CityName}";
                        labelFajrTime.Content = $"{result.Imsaak, 10}";
                        labelSunriseTime.Content = $"{result.Sunrise, 10}";
                        labelDhuhrTime.Content = $"{result.Noon, 10}";
                        labelSunsetTime.Content = $"{result.Sunset, 10}";
                        labelMaghribTime.Content = $"{result.Maghreb, 10}";
                        labelMidnightTime.Content = $"{result.Midnight, 10}";
                        labelQiblaDirectionTime.Content = $"{result.SimultaneityOfKaaba, 10}";
                    }
                    else
                    {
                        MessageBox.Show($"Error: {response.StatusCode}");
                    }
                }
            }
            finally
            {
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Width/2 - Width/2;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                App.Current.MainWindow.DragMove();
            //else if (e.RightButton == MouseButtonState.Pressed)
        }

        private void labelCity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            comboBoxCities.Visibility = Visibility.Visible;
            labelCity.Visibility = Visibility.Hidden;
            comboBoxCities.IsDropDownOpen = true;
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void btn_mini_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Button).Foreground = Brushes.Cyan;
        }

        private void btn_mini_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Button).Foreground = Brushes.White;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }
    }


    public class PrayerTimesResult
    {
        public string Today { get; set; }
        public string CityName { get; set; }
        public string Imsaak { get; set; }
        public string Sunrise { get; set; }
        public string Noon { get; set; }
        public string Sunset { get; set; }
        public string Maghreb { get; set; }
        public string Midnight { get; set; }
        public string SimultaneityOfKaaba { get; set; }
    }
}
