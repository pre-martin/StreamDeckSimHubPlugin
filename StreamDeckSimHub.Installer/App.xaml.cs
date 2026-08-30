// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Reflection;
using System.Windows;
using NLog;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Must run before any code touches a type from an embedded dependency (NLog, CommunityToolkit.Mvvm, ...).
        // A static constructor is guaranteed by the CLR to run before the first use of this type, which happens
        // right at the start of the generated Main() method ("new App()") - i.e. before InitializeComponent()
        // and before the Startup event is raised.
        static App()
        {
            AssemblyLoader.Register();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LogManager.Setup().LoadConfigurationFromAssemblyResource(typeof(App).GetTypeInfo().Assembly);
        }
    }
}