using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using Newtonsoft.Json;

namespace MyCalendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
                        labelFajr.Content = $"{result.Imsaak, 10}";
                        labelSunrise.Content = $"{result.Sunrise, 10}";
                        labelDhuhr.Content = $"{result.Noon, 10}";
                        labelSunset.Content = $"{result.Sunset, 10}";
                        labelMaghrib.Content = $"{result.Maghreb, 10}";
                        labelMidnight.Content = $"{result.Midnight, 10}";
                        labelQiblaDirection.Content = $"{result.SimultaneityOfKaaba, 10}";
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
