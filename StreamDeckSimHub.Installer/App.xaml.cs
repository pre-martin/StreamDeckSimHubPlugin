// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Reflection;
using System.Windows;
using NLog;

namespace StreamDeckSimHub.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LogManager.Setup().LoadConfigurationFromAssemblyResource(typeof(App).GetTypeInfo().Assembly);
        }
    }
}