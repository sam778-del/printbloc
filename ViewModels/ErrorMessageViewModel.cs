using PropertyChanged;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ErrorMessageViewModel
    {
        public string ErrorMsg { get; private set; }
        public bool IsError { get; set; } = false;
        public void GetErrorMessage(string message)
        {
            switch (message)
            {
                case ErrorConnection:
                    ErrorMsg = "Server is not available";
                    break;

                case ErrorAuthentication:
                    ErrorMsg = "Incorrect Login and/or Password";
                    break;

                case ErrorExpiredToken:
                    ErrorMsg = "Authentication has expired";
                    break;

                default:
                    ErrorMsg = message;
                    break;
            }
        }

        public void ResetDisplayErrorMessage()
        {
            ErrorMsg = "";
        }

        public const string ErrorConnection = "500";
        public const string ErrorAuthentication = "404";
        public const string ErrorRegistration = "400";
        public const string ErrorExpiredToken = "419";
    }
}
