using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using PrintblocProject.Helpers;
using PrintblocProject.Model;
using PrintblocProject.Models;
using PropertyChanged;
using ReactiveUI;
using ServiceStack;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            IsPageSubmit = false;
            ErrorMessageProfilePage = new ErrorMessageViewModel();

            OpenProfilePanelCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                IsOpened = !IsOpened;
            });

            ConnectCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                ErrorMessageProfilePage.ResetDisplayErrorMessage();
                if (Password == null)
                {
                    ErrorMessageProfilePage.GetErrorMessage("Please enter your password");
                    return;
                }

                if (ConfirmPassword == null)
                {
                    ErrorMessageProfilePage.GetErrorMessage("Please enter confirm password");
                    return;
                }

                if (Password.ToLower() != ConfirmPassword.ToLower())
                {
                    ErrorMessageProfilePage.GetErrorMessage("Password and confirm password do not match");
                    return;
                }

                IsPageSubmit = true;
                var response = await AccountManager.UpdatePassword(Password, Email, TeamName);
                Console.WriteLine("response" + response);
                if (response != null)
                {
                    if (response.Error)
                    {
                        IsPageSubmit = false;
                        ErrorMessageProfilePage.GetErrorMessage("Opps! something went wrong.");
                        return;
                    }
                    else
                    {
                        IsPageSubmit = false;
                        DeviceIdManager.DeleteTeamNameFile();
                        DeviceIdManager.SaveDeviceTeamName(response.User.UserName);
                        ErrorMessageProfilePage.GetErrorMessage("Profile updated");
                        return;
                    }
                }
                else
                {
                    IsPageSubmit = false;
                    ErrorMessageProfilePage.GetErrorMessage("Opps! something went wrong.");

                }
            });
        }

        public async Task UpdateUserInformation()
        {
            PasscodeResponse user = await AccountManager.UserInformation();
            if (user != null)
            {
                if(user.Status)
                {
                    System.Diagnostics.Debug.WriteLine(user.User.UserName);
                    TeamName = user.User.UserName;
                    Email = user.User.Email;
                } else
                {
                    switch (user.Message.ToLower())
                    {
                        case "invalid token":
                            ErrorMessageProfilePage.GetErrorMessage(user.Message);
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
                            ErrorMessageProfilePage.GetErrorMessage(user.Message);
                            break;
                    }
                }
            }
        }

        public string TeamName { get; set; }
        public string Email { get; set; }
        public ICommand OpenProfilePanelCommand { get; }
        public ICommand ConnectCommand { get; }
        public bool IsOpened { get; set; }
        public bool IsPageSubmit { get; set; }

        string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
            }
        }

        string confirmPassword;
        public string ConfirmPassword
        {
            get { return confirmPassword; }
            set
            {
                confirmPassword = value;
            }
        }

        public void Close()
        {
            TeamName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            IsOpened = false;
        }

        public void Open()
        {
            IsOpened = true;
            _ = UpdateUserInformation();
        }

        public void ResetErrorCommand()
        {
            ErrorMessageProfilePage.ResetDisplayErrorMessage();
            ErrorMessageProfilePage.IsError = false;
        }

        public ErrorMessageViewModel ErrorMessageProfilePage { get; set; }
    }
}
