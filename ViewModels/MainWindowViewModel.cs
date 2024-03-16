using Microsoft.Extensions.Configuration;
using PrintblocProject.Models;
using ReactiveUI;
using ServiceStack;
using Splat;
using System;
using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Collections.Generic;
using System.Text;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using PrintblocProject.Helpers;
using Avalonia.Logging;
using Avalonia.Controls;
using PrintblocProject.Model;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using System.Timers;
using Avalonia.Threading;
using static QRCoder.PayloadGenerator;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace PrintblocProject.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Timer connectivityCheckTimer;
        public MainWindowViewModel()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.isWindows = false;
                this.isLinux = true;
            }
            else
            {
                this.isWindows = true;
                this.isLinux = false;
            }

            HomeColor = "White";
            ProfileColor = "Transparent";
            SettingColor = "Transparent";
            User = new CurrentUserViewModel();
            Logger.TryGet(LogEventLevel.Fatal, LogArea.Control)?.Log(this, "Avalonia Infrastructure");

            ConfirmationViewModel = new ConfirmationViewModel();

            SignalRBackgroundService signalBack = new SignalRBackgroundService();
            signalBack.StartSignalRConnectionInBackground();

            PrinterMonitor printerHelper = new PrinterMonitor();
            printerHelper.StartMonitoringPrinters();

            SettingsViewModel = new SettingsViewModel();
            ProfileViewModel = new ProfileViewModel();
            Width(false);

            if (!deviceId.IsNullOrEmpty())
            {
                deviceSetId = $"Device Id: {deviceId}";
                IsSignedIn = true;
                IsShowingLoginPage = false;
                TeamName = retrieveTeamName;
/*                _= SettingsViewModel.UpdateUserInformation();
                _ = ProfileViewModel.UpdateUserInformation();*/
            }
            else
            {
                Width(false);
                IsShowingLoginPage = true;
            }

            ICommand proceedSplashCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                isSplash = true;
                ConfirmationViewModel.Close();
            });  

            SignOutCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                signalBack.StopSignalRConnection();
                DeviceIdManager.DeleteTeamNameFile();
                DeviceIdManager.DeleteTokenFile();
                DeviceIdManager.DeleteFile();
                IsSignedIn = false;
                SettingsViewModel.Close();
                ProfileViewModel.IsOpened = false;
                IsShowingLoginPage = true;
            });

            OpenHomeCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                HomeColor = "White";
                ProfileColor = "Transparent";
                SettingColor = "Transparent";
                IsSignedIn = true;
                SettingsViewModel.Close();
                ProfileViewModel.Close();
                TeamName = DeviceIdManager.GetDeviceTeamname();
            });

            OpenProfilePanelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                HomeColor = "Transparent";
                ProfileColor = "White";
                SettingColor = "Transparent";
                IsSignedIn = false;
                SettingsViewModel.Close();
                ProfileViewModel.Open();
            });

            OpenSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                HomeColor = "Transparent";
                ProfileColor = "Transparent";
                SettingColor = "White";
                IsSignedIn = false;
                SettingsViewModel.Open();
                ProfileViewModel.Close();
            });

            ConnectCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                User.ErrorMessageLoginPage.ResetDisplayErrorMessage();
                isLoginSubmit = false;
                if (User.TeamName == null)
                {
                    User.ErrorMessageLoginPage.GetErrorMessage("Teamname is required");
                    return;
                }

                if (User.Email == null)
                {
                    User.ErrorMessageLoginPage.GetErrorMessage("EMail address is required");
                    return;
                }

                if (User.Password == null)
                {
                    User.ErrorMessageLoginPage.GetErrorMessage("Password is required");
                    return;
                }

                isLoginSubmit = true;
                var loggedInUser = await AccountManager.LoginAccount(User.TeamName, User.Email, User.Password);
                if(loggedInUser == null)
                {
                    isLoginSubmit = false;
                    User.ErrorMessageLoginPage.GetErrorMessage("Opps something went wrong");
                    return;
                }

                if (loggedInUser.Success)
                {
                    string teamId = AuthResult.Instance.User.TeamId;
                    Device device = await AccountManager.StoreDevice(teamId);
                    if (device != null)
                    {
                        if (loggedInUser.User.EmailConfirmed)
                        {
                            IsSignedIn = true;
                            isLoginSubmit = false;
                            IsShowingLoginPage = false;
                            isSplash = true;
                            TeamName = AuthResult.Instance.User.UserName;
                            deviceSetId = $"Device Id: {device.DeviceId}";
                            this.Data = $"https://printbloc.com/crc/{device.DeviceId}";
                            _ = ProcessSplash();
                        }
                        else
                        {
                            isLoginSubmit = false;
                            User.ErrorMessageLoginPage.GetErrorMessage("Please verify your account on the web to continue using prinbloc");
                        }
                    } else
                    {
                        isLoginSubmit = false;
                        User.ErrorMessageLoginPage.GetErrorMessage("");
                    }
                }
                else
                {
                    isLoginSubmit = false;
                    User.ErrorMessageLoginPage.GetErrorMessage(loggedInUser.Errors.ToString());
                }
            });

            //Bitmap qrCodeBitmap = QrcodeV2.GenerateQRCode($"https://printbloc.com/crc/{deviceId}");
            this.Data = $"https://printbloc.com/crc/{deviceId}";
            this.PixelsPerModule = 20;
            IconScale = 15;
            IconBorder = 6;
            HasIcon = false;

            OnInternetConnectivityChanged();
            StartInternetConnectivityCheck();
        }

        private void StartInternetConnectivityCheck()
        {
            connectivityCheckTimer = new Timer();
            connectivityCheckTimer.Interval = 2000;
            connectivityCheckTimer.Elapsed += OnTimerElapsed;
            connectivityCheckTimer.AutoReset = true;
            connectivityCheckTimer.Enabled = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnInternetConnectivityChanged();
        }

        private void OnInternetConnectivityChanged()
        {
            bool isConnected = Status.Instance.Online;
            if (isConnected)
            {
                SetDeviceStatus("Online");
                SetEllipseColor("Green");
            }
            else
            {
                SetDeviceStatus("Offline");
                SetEllipseColor("Red");
            }
        }

        private void SetDeviceStatus(string text)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                deviceStatusText = $"{text}";
            });
        }


        private void SetEllipseColor(String color)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                OnlineStatus = color;
            });
        }

        public string HomeColor { get; set; }
        public string SettingColor {  get; set; }
        public string ProfileColor { get; set; }

        private string data;
        private int pixelsPerModule;
        private bool quitZones;
        private bool hasIcon;
        private Bitmap iconSource;
        private int iconScale;
        private int iconBorder;
        private Color color = Colors.Black;
        private Color spaceColor = Colors.White;
        private string deviceId = DeviceIdManager.GetDeviceId();
        private static string retrieveTeamName = DeviceIdManager.GetDeviceTeamname();
        public ICommand Command { get; set; }
        public ICommand OpenProfilePanelCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public static ReactiveCommand<object, Unit> NotifyCommand { get; set; }
        public static ReactiveCommand<object, Unit> PointerPressedCommand { get; set; }
        public bool IsShowingLoginPage { get; set; }
        public bool isLoginSubmit { get; set; }
        public bool isSplash { get; set; }
        public string Title { get; set; }
        public string SplashTitle { get; set; }
        public bool IsSignedIn { get; set; }
        public string ValidationError { get; set; }
        public ICommand SignOutCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand OpenHomeCommand { get; }
        public bool windowIsFocused { get; set; }
        public bool isWindows { get; set; }
        public bool isLinux { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }
        public ProfileViewModel ProfileViewModel { get; set; }
        public string ColumndefinitionWidth { get; set; }
        public string ColumndefinitionWidth2 { get; set; }
        public double? GridWidth { get; set; }
        public string TextHeaderMain { get; set; } = "Printbloc";
        public bool SettingsActive { get; set; }
        public bool isOpened { get; set; }
        public string TextHeaderMenuInSettings { get; set; }
        public string TeamName {  get; set; }
        public CurrentUserViewModel User { get; set; }
        public ConfirmationViewModel ConfirmationViewModel { get; set; }
        public bool KeySendMessage { get; set; }
        public string deviceSetId { get; set; }
        public string deviceStatusText { get; set; }
        public string OnlineStatus { get; set; }
        public void ResetErrorCommand()
        {
            User.ErrorMessageLoginPage.ResetDisplayErrorMessage();
            User.ErrorMessageLoginPage.IsError = false;
        }
        public enum WindowState
        {
            SignOut,
            OpenProfile,
            WindowSettings,
            HeaderMenuPopup
        }
        public bool IsFirstRun { get; set; } = true;
        public void WindowStates(WindowState state)
        {
            switch (state)
            {
                case WindowState.SignOut:
                    SettingsViewModel.Close();
                    ProfileViewModel.Close();
                    IsFirstRun = true;
                    break;
                case WindowState.OpenProfile:
                    SettingsViewModel.Close();
                    Width(SettingsViewModel.IsOpened);
                    break;
                case WindowState.WindowSettings:
                    ProfileViewModel.Close();
                    Width(SettingsViewModel.IsOpened);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        public void Width(bool isWindow)
        {
            if (!isWindow)
            {
                ColumndefinitionWidth = "*";
                ColumndefinitionWidth2 = "Auto";
                TextHeaderMain = "Printbloc";
                SettingsActive = false;
                GridWidth = 388;
            }
            else
            {
                ColumndefinitionWidth = "310";
                ColumndefinitionWidth2 = "*";
                TextHeaderMain = "Settings";
                SettingsActive = true;
                GridWidth = null;
                TextHeaderMenuInSettings = "Printbloc";
            }
        }

        public async Task ProcessSplash()
        {
            await SomeAsyncFunction("Starting App");
            await SomeAsyncFunction("Authentication Confirmed");
            await SomeAsyncFunction("Syncing printer on this device");
            await SomeAsyncFunction("devicerestart");
        }

        // Example asynchronous function
        private async Task SomeAsyncFunction(string text)
        {
            await Task.Delay(3000);
            if(text.ToLower()  == "syncing printer on this device")
            {
                SplashTitle = text;
            } else if(text.ToLower() == "devicerestart")
            {
                SplashTitle = "We're  this application in 2 seconds";
                await Task.Delay(2000);
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
                {
                    desktopApp.Shutdown();

/*                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName,
                        UseShellExecute = true
                    };

                    System.Diagnostics.Process.Start(processInfo);*/
                }
            } else
            {
                SplashTitle = text;
            }
        }

        public bool HasIcon
        {
            get => hasIcon;
            set
            {
                this.RaiseAndSetIfChanged(ref hasIcon, value);
                if (value)
                    IconSource = new Bitmap(AssetLoader.Open(new Uri("avares://Assets/bit.png")));
                else
                    IconSource = null;
            }
        }

        public string Data
        {
            get => data;
            set => this.RaiseAndSetIfChanged(ref data, value);
        }

        public int PixelsPerModule
        {
            get => pixelsPerModule;
            set => this.RaiseAndSetIfChanged(ref pixelsPerModule, value);
        }

        public bool QuitZones
        {
            get => quitZones;
            set => this.RaiseAndSetIfChanged(ref quitZones, value);
        }

        public Color Color
        {
            get => color;
            set => this.RaiseAndSetIfChanged(ref color, value);
        }

        public Color SpaceColor
        {
            get => spaceColor;
            set => this.RaiseAndSetIfChanged(ref spaceColor, value);
        }

        public Bitmap IconSource
        {
            get => iconSource;
            set => this.RaiseAndSetIfChanged(ref iconSource, value);
        }

        public int IconScale
        {
            get => iconScale;
            set => this.RaiseAndSetIfChanged(ref iconScale, value);
        }

        public int IconBorder
        {
            get => iconBorder;
            set => this.RaiseAndSetIfChanged(ref iconBorder, value);
        }
    }
}