using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;

namespace Pogoda
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<WeatherInfo> _originalWeatherData;
        private ObservableCollection<WeatherInfo> _filteredWeatherData;


        public ObservableCollection<WeatherInfo> WeatherData
        {
            get { return _filteredWeatherData; }
            set
            {
                _filteredWeatherData = value;
                OnPropertyChanged("WeatherData");
                UpdateStatistics();
            }
        }

        private double _averageTemperature;
        public double AverageTemperature
        {
            get { return Math.Round(_averageTemperature, 2); } // Округление до десятых
            set
            {
                _averageTemperature = value;
                OnPropertyChanged("AverageTemperature");
            }
        }

        private int _maxTemperature;
        public int MaxTemperature
        {
            get { return _maxTemperature; }
            set
            {
                _maxTemperature = value;
                OnPropertyChanged("MaxTemperature");
            }
        }

        private int _minTemperature;
        public int MinTemperature
        {
            get { return _minTemperature; }
            set
            {
                _minTemperature = value;
                OnPropertyChanged("MinTemperature");
            }
        }

        private string _temperatureAnomalies;
        public string TemperatureAnomalies
        {
            get { return _temperatureAnomalies; }
            set
            {
                _temperatureAnomalies = value;
                OnPropertyChanged("TemperatureAnomalies");
            }
        }

        private string _temperatureRepetitions;
        public string TemperatureRepetitions
        {
            get { return _temperatureRepetitions; }
            set
            {
                _temperatureRepetitions = value;
                OnPropertyChanged("TemperatureRepetitions");
            }
        }
        public ObservableCollection<string> WeatherTypes { get; } = new ObservableCollection<string>
        {
            "Ясный", "Дождливый", "Туманный", "Снежный"
        };

        public ICommand AddCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand FilterCommand { get; }
        public string[] SortOptions { get; } = { "по возрастанию дней (от 1 до 30)", "по убыванию дней (от большего к меньшему)", "по возрастанию температуры", "по убыванию температуры" };
        public string[] FilterOptions { get; } = { "все", "положительная температура", "отрицательная температура", "нулевая температура", "ясный", "дождливый", "туманный", "снежный" };
        public ICommand SaveCommand { get; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _originalWeatherData = new ObservableCollection<WeatherInfo>
            {
                new WeatherInfo { Day = 1, Temperature = 1, WeatherType = "Ясный" },
                new WeatherInfo { Day = 2, Temperature = 5, WeatherType = "Дождливый" },
                new WeatherInfo { Day = 3, Temperature = 4, WeatherType = "Туманный" },
                new WeatherInfo { Day = 4, Temperature = 0, WeatherType = "Снежный" },
                new WeatherInfo { Day = 5, Temperature = 5, WeatherType = "Ясный" },
                new WeatherInfo { Day = 6, Temperature = 13, WeatherType = "Ясный" },
                new WeatherInfo { Day = 7, Temperature = -7, WeatherType = "Снежный" },
                new WeatherInfo { Day = 8, Temperature = -20, WeatherType = "Снежный" },
                new WeatherInfo { Day = 9, Temperature = 19, WeatherType = "Ясный" }
            };

            WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData);

            AddCommand = new RelayCommand(AddWeatherInfo);
            SortCommand = new RelayCommand(SortWeatherData);
            FilterCommand = new RelayCommand(FilterWeatherData);

            SaveCommand = new RelayCommand(SaveWeatherData);
        }
        private void SaveWeatherData(object parameter)
        {
            if (WeatherData == null || WeatherData.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                DefaultExt = ".txt",
                FileName = "weather_data"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(dialog.FileName))
                    {
                        foreach (var weatherInfo in WeatherData)
                        {
                            writer.WriteLine($"День: {weatherInfo.Day}, Температура: {weatherInfo.Temperature}, Тип погоды: {weatherInfo.WeatherType}");
                        }
                    }
                    MessageBox.Show("Данные успешно сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void AddWeatherInfo(object parameter)
        {
            if (int.TryParse(DayTextBox.Text, out int day) && int.TryParse(TemperatureTextBox.Text, out int temp) && WeatherTypeComboBox.SelectedItem != null)
            {
                string weatherType = ((ComboBoxItem)WeatherTypeComboBox.SelectedItem).Content.ToString();
                if (day < 1 || day > 31)
                {
                    MessageBox.Show($"День {day} не существует. Введите день от 1 до 31.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (_originalWeatherData.Any(w => w.Day == day))
                {
                    MessageBox.Show($"День {day} уже существует. Введите другой день.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _originalWeatherData.Add(new WeatherInfo { Day = day, Temperature = temp, WeatherType = weatherType });
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData);
                }
            }
        }



        private void SortWeatherData(object parameter)
        {
            var sortOption = parameter as string;
            switch (sortOption)
            {
                case "по возрастанию дней (от 1 до 30)":
                    WeatherData = new ObservableCollection<WeatherInfo>(_filteredWeatherData.OrderBy(w => w.Day));
                    break;
                case "по убыванию дней (от большего к меньшему)":
                    WeatherData = new ObservableCollection<WeatherInfo>(_filteredWeatherData.OrderByDescending(w => w.Day));
                    break;
                case "по возрастанию температуры":
                    WeatherData = new ObservableCollection<WeatherInfo>(_filteredWeatherData.OrderBy(w => w.Temperature));
                    break;
                case "по убыванию температуры":
                    WeatherData = new ObservableCollection<WeatherInfo>(_filteredWeatherData.OrderByDescending(w => w.Temperature));
                    break;
            }
        }

        private void FilterWeatherData(object parameter)
        {
            var filterOption = parameter as string;
            switch (filterOption)
            {
                case "все":
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData);
                    break;
                case "положительная температура":
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData.Where(w => w.Temperature > 0));
                    break;
                case "отрицательная температура":
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData.Where(w => w.Temperature < 0));
                    break;
                case "нулевая температура":
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData.Where(w => w.Temperature == 0));
                    break;
                case "ясный":
                case "дождливый":
                case "туманный":
                case "снежный":
                    WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData.Where(w => w.WeatherType == filterOption));
                    break;
            }
        }


        private void UpdateStatistics()
        {
            if (_filteredWeatherData.Count > 0)
            {
                AverageTemperature = _filteredWeatherData.Average(w => w.Temperature);
                MaxTemperature = _filteredWeatherData.Max(w => w.Temperature);
                MinTemperature = _filteredWeatherData.Min(w => w.Temperature);
                TemperatureAnomalies = GetTemperatureAnomalies();
                TemperatureRepetitions = GetTemperatureRepetitions();
            }
        }

        private string GetTemperatureAnomalies()
        {
            var anomalies = new StringBuilder();
            var anomaliesDrop = new StringBuilder("Аномальные спад:\n");
            var anomaliesRise = new StringBuilder("Аномальный подъем:\n");

            for (int i = 1; i < _filteredWeatherData.Count; i++)
            {
                var tempDifference = _filteredWeatherData[i].Temperature - _filteredWeatherData[i - 1].Temperature;
                if (Math.Abs(tempDifference) >= 10)
                {
                    if (tempDifference > 0)
                    {
                        anomaliesRise.AppendLine($"День {_filteredWeatherData[i].Day}: +{tempDifference} градусов");
                    }
                    else
                    {
                        anomaliesDrop.AppendLine($"День {_filteredWeatherData[i].Day}: {tempDifference} градусов");
                    }
                }
            }

            anomalies.Append(anomaliesDrop).Append("\n").Append(anomaliesRise);
            return anomalies.ToString();
        }

        private string GetTemperatureRepetitions()
        {
            var repetitions = new StringBuilder();
            var temperatureCounts = _filteredWeatherData.GroupBy(w => w.Temperature)
                .Where(group => group.Count() > 1)
                .Select(group => new
                {
                    Temperature = group.Key,
                    Count = group.Count(),
                    Days = string.Join(",", group.Select(g => g.Day))
                });

            foreach (var tempCount in temperatureCounts)
            {
                repetitions.AppendLine($"Температура {tempCount.Temperature} была {tempCount.Count} раз(а) : {tempCount.Days}");
            }

            return repetitions.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WeatherInfo
    {
        public int Day { get; set; }
        public int Temperature { get; set; }
        public string WeatherType { get; set; } 
    }


    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
        public void Execute(object parameter) => execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

