using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMPaintApp.Models
{
    public abstract class DialogViewModelBase : ObservableObject
    {
        public event Action<bool?> RequestClose = delegate { };

        protected virtual void OnRequestClose(bool? dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
    }
}
