using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand FilterCommand { get; }
        public string[] SortOptions { get; } = { "по возрастанию дней (от 1 до 30)", "по убыванию дней (от большего к меньшему)", "по возрастанию температуры", "по убыванию температуры" };
        public string[] FilterOptions { get; } = { "все", "положительная температура", "отрицательная температура", "нулевая температура" };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _originalWeatherData = new ObservableCollection<WeatherInfo>
            {
                new WeatherInfo { Day = 1, Temperature = 1 },
                new WeatherInfo { Day = 2, Temperature = 5 },
                new WeatherInfo { Day = 3, Temperature = 4 },
                new WeatherInfo { Day = 4, Temperature = -15 }
            };

            WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData);

            AddCommand = new RelayCommand(AddWeatherInfo);
            SortCommand = new RelayCommand(SortWeatherData);
            FilterCommand = new RelayCommand(FilterWeatherData);
        }

        private void AddWeatherInfo(object parameter)
        {
            if (int.TryParse(DayTextBox.Text, out int day) && int.TryParse(TemperatureTextBox.Text, out int temp))
            {
                _originalWeatherData.Add(new WeatherInfo { Day = day, Temperature = temp });
                WeatherData = new ObservableCollection<WeatherInfo>(_originalWeatherData);
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
            }
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
