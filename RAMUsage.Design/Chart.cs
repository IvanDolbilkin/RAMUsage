using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Management;

namespace RAMUsage.Design
{
    public class Chart : NotifyPropertyChanged
    {
        public RoundRobinCollection ProcessorTime { get; }
        private double _lastRAMValue;
        public double LastRAMValue
        {
            get => _lastRAMValue;
            set
            {
                _lastRAMValue = value;
                OnPropertyChanged(nameof(LastRAMValue));
            }
        }

        private async void ReadRAM()
        {
            try
            {
                while (true)
                {
                    PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");
                    ManagementClass obj1 = new ManagementClass("Win32_PhysicalMemory");
                    ManagementObjectCollection moc1 = obj1.GetInstances();
                    double total = 0;
                    foreach (ManagementObject mo1 in moc1)
                    {
                        total += Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024.0, 1);
                    }
                    moc1.Dispose();
                    obj1.Dispose();
                    double left = RamCounter.NextValue();
                    LastRAMValue = ((total - left) / total) * 100;
                    ProcessorTime.Push(LastRAMValue);
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Chart()
        {
            ProcessorTime = new RoundRobinCollection(100);
            ReadRAM();
        }
    }

    public class RoundRobinCollection : NotifyPropertyChanged
    {
        private readonly List<double> _values;
        public IReadOnlyList<double> Values => _values;

        public RoundRobinCollection(int amount)
        {
            _values = new List<double>();
            for (int i = 0; i < amount; i++)
                _values.Add(0F);
        }

        public void Push(double value)
        {
            _values.RemoveAt(0);
            _values.Add(value);
            OnPropertyChanged(nameof(Values));
        }
    }
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class PolygonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection points = new PointCollection();
            if (values.Length == 3 && values[0] is IReadOnlyList<double> dataPoints && values[1] is double width && values[2] is double height)
            {
                points.Add(new Point(0, height));
                points.Add(new Point(width, height));
                double step = width / (dataPoints.Count - 1);
                double position = width;
                for (int i = dataPoints.Count - 1; i >= 0; i--)
                {
                    points.Add(new Point(position, height - height * dataPoints[i] / 100));
                    position -= step;
                }
            }
            return points;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
    }
}
