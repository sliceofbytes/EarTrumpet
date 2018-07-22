﻿using EarTrumpet.DataModel;
using EarTrumpet.UI.Controls;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using EarTrumpet.UI.Themes;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.UI.Views;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet
{
    public partial class App
    {
        private DeviceCollectionViewModel _viewModel;
        private TrayIcon _trayIcon;
        private FlyoutWindow _flyoutWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ErrorReportingService.Initialize();

            Trace.WriteLine("App Application_Startup");

            if (!SingleInstanceAppMutex.TakeExclusivity())
            {
                Trace.WriteLine("App Application_Startup TakeExclusivity failed");
                Current.Shutdown();
                return;
            }

            ((ThemeManager)Resources["ThemeManager"]).SetTheme(ThemeData.GetBrushData());

            Hosting.AddonHostService.Initialize();

            var deviceManager = DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback);

            _viewModel = new DeviceCollectionViewModel(deviceManager);
            _viewModel.Ready += MainViewModel_Ready;

            var flyoutViewModel = new FlyoutViewModel(_viewModel);
            _flyoutWindow = new FlyoutWindow(_viewModel, flyoutViewModel);
            _trayIcon = new TrayIcon(new TrayViewModel(_viewModel, () => flyoutViewModel.OpenFlyout(FlyoutShowOptions.Pointer)));

            HotkeyService.Register(SettingsService.Hotkey);
            HotkeyService.KeyPressed += (_, __) => flyoutViewModel.OpenFlyout(FlyoutShowOptions.Keyboard);

            StartupUWPDialogDisplayService.ShowIfAppropriate();

            Trace.WriteLine($"App Application_Startup Exit");
        }

        private void MainViewModel_Ready(object sender, System.EventArgs e)
        {
            Trace.WriteLine("App Application_Startup MainViewModel_Ready");
            _trayIcon.Show();

            Hosting.AddonHostService.Instance.InitializeSettings();
            Hosting.AddonHostService.Instance.OnApplicationLifecycleEvent(Extensibility.ApplicationLifecycleEvent.Startup);
        }
    }
}