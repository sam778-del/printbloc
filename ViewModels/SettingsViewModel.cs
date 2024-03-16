using PropertyChanged;
using ReactiveUI;
using System.Reactive;
using System;
using System.Windows.Input;
using Avalonia.Controls;
using PrintblocProject.Helpers;
using PrintblocProject.Model;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsViewModel
    {
        public SettingsViewModel() {
            IsNewPin = false;
            IsCurrentPin = false;
            ErrorMessagePinPage = new ErrorMessageViewModel();
            IsPageSubmit = false;

            OpenSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                IsOpened = !IsOpened;
            });

            ConnectCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                ErrorMessagePinPage.ResetDisplayErrorMessage();

                if(IsEnabled)
                {
                    if (NewPin == null)
                    {
                        ErrorMessagePinPage.GetErrorMessage("Please enter your pincode");
                        return;
                    }

                    if (CurrentPin == null)
                    {
                        ErrorMessagePinPage.GetErrorMessage("Please enter confirm pincode");
                        return;
                    }

                    if (NewPin.ToLower() != CurrentPin.ToLower())
                    {
                        ErrorMessagePinPage.GetErrorMessage("Pincode and confirm pincode do not match");
                        return;
                    }
                }

                IsPageSubmit = true;
                string deviceId = DeviceIdManager.GetDeviceId();
                PasscodeResponse pinCode = await AccountManager.EnablePinCode(deviceId, NewPin, IsEnabled);
                if (pinCode != null)
                {
                    if (pinCode.Status)
                    {
                        IsPageSubmit = false;
                        ErrorMessagePinPage.GetErrorMessage(pinCode.Message);
                    }
                    else
                    {
                        IsPageSubmit = false;
                        ErrorMessagePinPage.GetErrorMessage(pinCode.Message);
                    }
                }
                else
                {
                    IsPageSubmit = false;
                    ErrorMessagePinPage.GetErrorMessage("Something went wrong");
                    return;
                }
            });

            SwithButton = ReactiveCommand.CreateFromTask(async () =>
            {
               IsEnabled = false;
            });

            SwithButtonDisable = ReactiveCommand.CreateFromTask(async () =>
            {
                IsEnabled = true;
            });
        }

        public void Close()
        {
            NewPin = string.Empty;
            CurrentPin = string.Empty;
            IsOpened = false;
        }

        public async Task UpdateUserInformation()
        {
            PasscodeResponse user = await AccountManager.UserInformation();
            if (user != null)
            {
                if(user.Status)
                {
                    PasscodeResponse checkDevice = await AccountManager.CheckPinCode();
                    if (checkDevice != null)
                    {
                        if (checkDevice.Error)
                        {
                            IsEnabled = false;
                        }
                        else
                        {
                            if (checkDevice.Device.IsPasscode)
                            {
                                IsEnabled = true;
                            }
                            else
                            {
                                IsEnabled = false;
                            }
                        }
                    }
                    else
                    {
                        IsEnabled = false;
                    }
                } else
                {
                    switch (user.Message.ToLower())
                    {
                        case "invalid token":
                            ErrorMessagePinPage.GetErrorMessage(user.Message);
                            DeviceIdManager.DeleteTeamNameFile();
                            DeviceIdManager.DeleteTokenFile();
                            DeviceIdManager.DeleteFile();
                            await Task.Delay(1500);
                            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
                            {
                                desktopApp.Shutdown();

                                var processInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName,
                                    UseShellExecute = true
                                };

                                System.Diagnostics.Process.Start(processInfo);
                            }
                            break;
                        default:
                            ErrorMessagePinPage.GetErrorMessage(user.Message);
                            break;
                    }
                }
            }
            else
            {
                IsEnabled = false;
            }
        }

        public void Open()
        {
            IsOpened = true;
            _ = UpdateUserInformation();
        }

        public ICommand ConnectCommand { get; }
        public ICommand SwithButton { get; }
        public ICommand SwithButtonDisable { get; }
        public ICommand ContextMenuCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public bool IsOpened { get; set; }
        public bool IsPageSubmit { get; set; }

        public void ResetErrorCommand()
        {
            ErrorMessagePinPage.ResetDisplayErrorMessage();
            ErrorMessagePinPage.IsError = false;
        }

        string newpin;
        public string NewPin
        {
            get { return newpin; }
            set
            {
                IsNewPin = !String.IsNullOrWhiteSpace(value);
                newpin = value;
            }
        }

        string currentpin;
        public string CurrentPin
        {
            get { return currentpin; }
            set
            {
                IsCurrentPin = !String.IsNullOrWhiteSpace(value);
                currentpin = value;
            }
        }

        public bool IsNewPin { get; set; }
        public bool IsCurrentPin { get; set; }
        public bool IsEnabled { get; set; }

        public ErrorMessageViewModel ErrorMessagePinPage { get; set; }
    }
}
