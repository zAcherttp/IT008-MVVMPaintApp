using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MVVMPaintApp.Models
{

    public abstract class DialogViewModelBase(string title) : ObservableObject
    {
        public string Title { get; set; } = title ?? "Dialog";
        public event Action<MessageBoxResult> RequestClose = delegate { };

        protected virtual void CloseDialog(MessageBoxResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
    }
}
