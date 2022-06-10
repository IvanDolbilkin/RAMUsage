using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;

namespace RAMUsage.Design
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }


        protected override void OnStartup(StartupEventArgs e)
        {
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                ValueSet userInput = toastArgs.UserInput;


                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (MainWindow.WindowState == WindowState.Minimized)
                    {
                        MainWindow.WindowState = WindowState.Normal;
                        MainWindow.Activate();
                    }
                    else
                        MainWindow.Activate();
                });
            };

            base.OnStartup(e);
        }
    }
}
