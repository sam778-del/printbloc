﻿using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PrintblocProject.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MenuItemObject
    {
        public ICommand Command { get; set; }
        public string Content { get; set; }
    }
}
