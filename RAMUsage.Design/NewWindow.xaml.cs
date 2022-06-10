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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Management;
using System.Dynamic;
using System.Windows.Threading;
using System.Threading;
using RAMUsage.Core;
using Microsoft.Toolkit.Uwp.Notifications;

namespace RAMUsage.Design
{
    public partial class NewWindow : Window
    {
        public int RefreshTimer { get; set; }

        public DispatcherTimer NotificationRefresh { get; set; }

        public bool CanNotify { get; set; }

        public List<ProcessData> AllProcesses { get; set; }

        public string SearchText { get; set; }

        public Thread AllProcessesInitialization { get; set; }

        public Thread MainDataRefresh { get; set; }

        public NewWindow(MainWindow main)
        {
            InitializeComponent();
            RefreshTimer = 5000;
            SearchText = "";
            CanNotify = true;
            MainDataRefresh = new Thread(UsageRefresh);
            MainDataRefresh.IsBackground = true;
            MainDataRefresh.Start();
            NotificationRefresh = new DispatcherTimer();
            NotificationRefresh.Interval = new TimeSpan(0, 2, 0);
            NotificationRefresh.Tick += new EventHandler(CanNotifyChange);
            NotificationRefresh.Start();
            InitializeFromMainWindow(main);
        }

        private void InitializeFromMainWindow(MainWindow main)
        {
            if (main.Reminder60.IsChecked)
            {
                this.Reminder60.IsChecked = true;
            }
            if (main.Reminder70.IsChecked)
            {
                this.Reminder70.IsChecked = true;
            }
            if (main.Reminder80.IsChecked)
            {
                this.Reminder80.IsChecked = true;
            }
            if (main.Reminder90.IsChecked)
            {
                this.Reminder90.IsChecked = true;
            }
            if(main.Refr5Sec.IsChecked)
            {
                this.Refr5Sec.IsChecked = true;
            }
            if (main.Refr15Sec.IsChecked)
            {
                RefreshTimer = 15000;
                this.Refr15Sec.IsChecked = true;
            }
            if (main.Refr30Sec.IsChecked)
            {
                RefreshTimer = 30000;
                this.Refr30Sec.IsChecked = true;
            }
            if (main.Refr60Sec.IsChecked)
            {
                RefreshTimer = 60000;
                this.Refr60Sec.IsChecked = true;
            }
        }

        public void CanNotifyChange(object sender, EventArgs e)
        {
            CanNotify = true;
        }

        public void UsageRefresh()
        {
            while (true)
            {
                Dispatcher.BeginInvoke(new ThreadStart(delegate { RefreshRAMUsage(); }));
                Dispatcher.BeginInvoke(new ThreadStart(delegate { Quantity.Text = $"Total processes: {Process.GetProcesses().Count()}"; }));
                Thread.Sleep(RefreshTimer);
            }
        }

        public void RefreshThread()
        {
            while (true)
            {
                ListViewInitialisation();
                Thread.Sleep(RefreshTimer);
            }
        }

        public void ListViewInitialisation()
        {
            object locker = new object();
            List<ProcessData> newProcesses = new List<ProcessData> { };
            List<Process> processes = Process.GetProcesses().ToList();

            //try
            //{
            //    lock (locker)
            //    {
            //        Parallel.For(0, processes.Count(), i =>
            //        {
            //            if( i < processes.Count())
            //                newProcesses.Add(new ProcessData(processes[i]));
            //        });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            foreach (var proc in processes)
            {
                ProcessData p = new ProcessData(proc);
                newProcesses.Add(p);
            }

            AllProcesses = newProcesses;
            Dispatcher.BeginInvoke(new ThreadStart(delegate {
                SearchSorting(false);
                ProcessListView.ItemsSource = null;
                ProcessListView.ItemsSource = AllProcesses;
            }));
        }

        private void ProcessName_Initialized(object sender, EventArgs e)
        {
            TextBlock name = sender as TextBlock;
            ProcessData p = name.DataContext as ProcessData;
            name.Text = p.Process.ProcessName;
        }

        private void ProcessStatus_Initialized(object sender, EventArgs e)
        {
            TextBlock status = sender as TextBlock;
            ProcessData p = status.DataContext as ProcessData;
            if (p.IsResponding)
                status.Text = "Responding";
            else
                status.Text = "Not Responding";
        }

        private void ProcessID_Initialized(object sender, EventArgs e)
        {
            TextBlock id = sender as TextBlock;
            ProcessData p = id.DataContext as ProcessData;
            id.Text = p.Process.Id.ToString();
        }

        private void ProcessMemory_Initialized(object sender, EventArgs e)
        {
            TextBlock memory = sender as TextBlock;
            ProcessData p = memory.DataContext as ProcessData;
            memory.Text = $"{(p.Process.PrivateMemorySize64 / 1024 / 1024).ToString()} Mb";
        }

        private void ProcessDescription_Initialized(object sender, EventArgs e)
        {
            TextBlock description = sender as TextBlock;
            ProcessData p = description.DataContext as ProcessData;
            description.Text = p.Details;
        }

        private void RefreshRAMUsage()
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
            double UsedRAMInPercents = Math.Round((total - RamCounter.NextValue()) / total * 100);
            if (CanNotify)
                Notification(UsedRAMInPercents);
            RAMUsage.Text = $"RAM: {UsedRAMInPercents}%";
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchSorting(true);
        }

        private void SearchSorting(bool isClick)
        {
            try
            {
                int count = 0;
                for (int i = 0; i < AllProcesses.Count; i++)
                {
                    ProcessData pr = AllProcesses[i];
                    string currName = pr.Process.ProcessName;
                    if (pr.Type == "App" && !App.IsChecked)
                    {
                        AllProcesses.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else if (pr.Type == "Background Process" && !BackProc.IsChecked)
                    {
                        AllProcesses.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else if (pr.Type == "Windows Process" && !WindowsProc.IsChecked)
                    {
                        AllProcesses.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (currName.ToLower().Contains(SearchText.ToLower()))
                    {
                        AllProcesses.RemoveAt(i);
                        AllProcesses.Insert(count, pr);
                        count++;
                    }
                }
                if (isClick)
                    MessageBox.Show($"{count} processes found containing this text");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Refr5Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 5000;
            Refr5Sec.IsChecked = true;
            Refr15Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr15Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 15000;
            Refr15Sec.IsChecked = true;
            Refr5Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr30Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 30000;
            Refr30Sec.IsChecked = true;
            Refr5Sec.IsChecked = false;
            Refr15Sec.IsChecked = false;
            Refr60Sec.IsChecked = false;
        }

        private void Refr60Sec_Click(object sender, RoutedEventArgs e)
        {
            RefreshTimer = 60000;
            Refr60Sec.IsChecked = true;
            Refr5Sec.IsChecked = false;
            Refr15Sec.IsChecked = false;
            Refr30Sec.IsChecked = false;
        }

        private void ProcessType_Initialized(object sender, EventArgs e)
        {
            TextBlock type = sender as TextBlock;
            ProcessData p = type.DataContext as ProcessData;
            type.Text = p.Type;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GetBack_Click(object sender, RoutedEventArgs e)
        {
            Window main = new MainWindow(this);
            main.Show();
            this.Close();
        }

        private void ProcessNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = ProcessNameSearch.Text;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AllProcessesInitialization.Abort();
            MainDataRefresh.Abort();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ProcessData currProcess = ProcessListView.SelectedItem as ProcessData;
            try
            {
                if (currProcess.Process != null)
                {
                    currProcess.Process.Kill();
                    //Thread.Sleep(1000);
                    //ListViewInitialisation();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Not enough rights to delete the process!");
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            ProcessData currProcess = ProcessListView.SelectedItem as ProcessData;
            Process.Start($"https://www.processlibrary.com/en/search?q={currProcess.Process.ProcessName}");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AllProcessesInitialization = new Thread(RefreshThread);
            AllProcessesInitialization.IsBackground = true;
            AllProcessesInitialization.Start();
        }

        private void Chart_Click(object sender, RoutedEventArgs e)
        {
            Window chart = new ChartWindow();
            chart.Show();
        }
    }
}
