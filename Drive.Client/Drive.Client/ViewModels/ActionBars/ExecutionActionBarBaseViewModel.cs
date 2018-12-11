﻿using Drive.Client.ViewModels.Base;
using System.Windows.Input;
using Xamarin.Forms;

namespace Drive.Client.ViewModels.ActionBars {
    public abstract class ExecutionActionBarBaseViewModel : CommonActionBarViewModel {

        public ICommand ExecuteCommand => new Command(OnExecuteCommand, () => IsEcutionAvailable);

        public ContentPageBaseViewModel RelativeContentPageBaseViewModel { get; private set; }

        private string _title;
        public string Title {
            get => _title;
            protected set => SetProperty<string>(ref _title, value);
        }

        private bool _isEcutionAvailable;
        public bool IsEcutionAvailable {
            get => _isEcutionAvailable;
            protected set => SetProperty<bool>(ref _isEcutionAvailable, value);
        }

        public virtual void ResolveExecutionAvailability(object condition) { }

        protected virtual void OnExecuteCommand() { }
    }
}
