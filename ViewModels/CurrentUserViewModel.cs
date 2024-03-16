using PrintblocProject.Models;
using PropertyChanged;
using System;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class CurrentUserViewModel : ICurrentUser
    {
        public CurrentUserViewModel()
        {
            IsPassword = false;
            ErrorMessageLoginPage = new ErrorMessageViewModel();
        }

        public string DisplayName => Helpers.Helpers.NameOrLogin(TeamName, Email);

        public string Email { get; set; }

        public string TeamName { get; set; }

        public string Id { get; set; }

        string password;
        public string Password
        {
            get { return password; }
            set
            {
                IsPassword = !String.IsNullOrWhiteSpace(value);
                password = value;
            }
        }

        public bool IsPassword { get; set; }

        public ErrorMessageViewModel ErrorMessageLoginPage { get; set; }
    }
}
