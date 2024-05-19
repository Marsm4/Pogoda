using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Pogoda
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private WeatherInfo _weatherInfo;
        private ObservableCollection<string> _weatherTypes;
        public Edit(WeatherInfo weatherInfo, ObservableCollection<string> weatherTypes)
        {
            InitializeComponent();

            _weatherInfo = weatherInfo;
            _weatherTypes = weatherTypes;

            // Очищаем все элементы в ComboBox
            WeatherTypeComboBox.Items.Clear();

            // Затем устанавливаем источник данных
            WeatherTypeComboBox.ItemsSource = _weatherTypes;

            // Устанавливаем выбранный элемент в ComboBox на основе значения _weatherInfo.WeatherType
            WeatherTypeComboBox.SelectedItem = _weatherInfo.WeatherType;

            // Заполняем поля информацией о погоде для редактирования
            DayTextBox.Text = _weatherInfo.Day.ToString();
            TemperatureTextBox.Text = _weatherInfo.Temperature.ToString();
        }
    
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверьте и сохраните измененную информацию о погоде
            if (int.TryParse(DayTextBox.Text, out int day) && int.TryParse(TemperatureTextBox.Text, out int temperature))
            {
                _weatherInfo.Day = day;
                _weatherInfo.Temperature = temperature;
                _weatherInfo.WeatherType = WeatherTypeComboBox.SelectedItem?.ToString();

                // Закройте окно редактирования
                Close();
            }
            else
            {
                MessageBox.Show("Некорректные данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Просто закройте окно редактирования без сохранения изменений
            Close();
        }
    }
}
 