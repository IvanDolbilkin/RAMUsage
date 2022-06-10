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
using System.Diagnostics;
using System.Management;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;
using System.Windows.Threading;

namespace RAMUsage.Design
{
    public partial class MainWindow : Window
    {
        public int RefreshTimer { get; set; }

        public Thread RefreshData { get; set; }

        public DispatcherTimer NotificationRefresh { get; set; }

        public bool CanNotify { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RefreshTimer = 5000;
            CanNotify = true;
            RefreshData = new Thread(RefreshRAMUsage);
            RefreshData.IsBackground = true;
            RefreshData.Start();
            NotificationRefresh = new DispatcherTimer();
            NotificationRefresh.Interval = new TimeSpan(0, 2, 0);
            NotificationRefresh.Tick += new EventHandler(CanNotifyChange);
            NotificationRefresh.Start();
        }

        public MainWindow(NewWindow window)
        {
            InitializeComponent();
            RefreshTimer = 5000;
            CanNotify = true;
            RefreshData = new Thread(RefreshRAMUsage);
            RefreshData.IsBackground = true;
            RefreshData.Start();
            NotificationRefresh = new DispatcherTimer();
            NotificationRefresh.Interval = new TimeSpan(0, 2, 0);
            NotificationRefresh.Tick += new EventHandler(CanNotifyChange);
            NotificationRefresh.Start();
            InitializeFromNewWindow(window);
        }

        private void InitializeFromNewWindow(NewWindow window)
        {
            if (window.Reminder60.IsChecked)
            {
                this.Reminder60.IsChecked = true;
            }
            if (window.Reminder70.IsChecked)
            {
                this.Reminder70.IsChecked = true;
            }
            if (window.Reminder80.IsChecked)
            {
                this.Reminder80.IsChecked = true;
            }
            if (window.Reminder90.IsChecked)
            {
                this.Reminder90.IsChecked = true;
            }
            if (window.Refr5Sec.IsChecked)
            {
                this.Refr5Sec.IsChecked = true;
            }
            if (window.Refr15Sec.IsChecked)
            {
                this.Refr5Sec.IsChecked = false;
                this.Refr15Sec.IsChecked = true;
            }
            if (window.Refr30Sec.IsChecked)
            {
                this.Refr5Sec.IsChecked = false;
                this.Refr30Sec.IsChecked = true;
            }
            if (window.Refr60Sec.IsChecked)
            {
                this.Refr5Sec.IsChecked = false;
                this.Refr60Sec.IsChecked = true;
            }
        }

        public void CanNotifyChange(object sender, EventArgs e)
        {
            CanNotify = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window chart = new ChartWindow();
            chart.Show();
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            Window details = new NewWindow(this);
            details.Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Refr5Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 5000;
            Refr15Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr15Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 15000;
            Refr5Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr30Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 30000;
            Refr5Sec.IsChecked = false;
            Refr15Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr60Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 60000;
            Refr5Sec.IsChecked = false;
            Refr15Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Window details = new NewWindow(this);
            details.Show();
            this.Close();
        }

        private void RefreshRAMUsage()
        {
            while (true)
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { RAMUsage(); }));
                Thread.Sleep(RefreshTimer);
            }
        }

        private void RAMUsage()
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
            float left = RamCounter.NextValue();
            UsedMemory.Text = (total - left).ToString() + " Mb";
            RemainMemory.Text = left.ToString() + " Mb";
            TotalMemory.Text = total.ToString() + " Mb";
            double UsedRAMInPercents = Math.Round((total - RamCounter.NextValue()) / total * 100);
            if (CanNotify)
                Notification(UsedRAMInPercents);
            UsedInPercent.Text = $"{UsedRAMInPercents}%";
        }

        public void Notification(double used)
        {
            if (used >= 90 && Reminder90.IsChecked)
            {
                new ToastContentBuilder()
    .AddText("Your RAM usage is over 90%")
    .AddText("Check which processes use too much")
    .Show();
                CanNotify = false;
            }
            else if (used >= 80 && Reminder80.IsChecked)
            {
                new ToastContentBuilder()
    .AddText("Your RAM usage is over 80%")
    .AddText("Check which processes use too much")
    .Show();
                CanNotify = false;
            }
            else if (used >= 70 && Reminder70.IsChecked)
            {
                new ToastContentBuilder()
    .AddText("Your RAM usage is over 70%")
    .AddText("Check which processes use too much")
    .Show();
                CanNotify = false;
            }
            else if (used >= 60 && Reminder60.IsChecked)
            {
                new ToastContentBuilder()
    .AddText("Your RAM usage is over 60%")
    .AddText("Check which processes use too much")
    .Show();
                CanNotify = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RefreshData.Abort();
        }

        private void Chart_Click(object sender, RoutedEventArgs e)
        {
            Window chart = new ChartWindow();
            chart.Show();
        }
    }
}
