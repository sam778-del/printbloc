using System.Windows.Input;
using PropertyChanged;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ConfirmationViewModel
    {
        public void Open(ICommand command, string confirm, string btnName)
        {
            ConfirmSelectionCommand = command;
            ConfirmationQuestion = confirm;
            ButtonName = btnName;
            IsOpened = !IsOpened;
        }

        public void Close()
        {
            IsOpened = false;
        }

        public ICommand ConfirmSelectionCommand { get; set; }
        public bool IsOpened { get; set; }
        public string ConfirmationQuestion { get; set; }
        public string ButtonName { get; set; }
    }
}
