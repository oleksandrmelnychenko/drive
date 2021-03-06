﻿using Drive.Client.Models.Arguments.IdentityAccounting.Registration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Drive.Client.ViewModels.IdentityAccounting.Registration {
    public sealed class PasswordRegisterStepViewModel : StepBaseViewModel {

        private RegistrationCollectedInputsArgs _collectedInputsArgs;

        /// <summary>
        ///     ctor().
        /// </summary>
        public PasswordRegisterStepViewModel() {
            StepTitle = PASSWORD_STEP_REGISTRATION_TITLE;
            MainInputPlaceholder = PASSWORD_PLACEHOLDER_STEP_REGISTRATION;
            MainInputIconPath = PASSWORD_ICON_PATH;
            IsPasswordInput = true;
        }

        public override Task InitializeAsync(object navigationData) {

            if (navigationData is RegistrationCollectedInputsArgs collectedInputsArgs) {
                _collectedInputsArgs = collectedInputsArgs;
            }

            MainInput.Value = string.Empty;

            return base.InitializeAsync(navigationData);
        }

        public override void Dispose() {
            base.Dispose();

            if (_collectedInputsArgs != null) {
                _collectedInputsArgs.Password = null;
            }
        }

        protected async override void OnStepCommand() {
            if (ValidateForm()) {
                if (_collectedInputsArgs != null) {
                    _collectedInputsArgs.Password = MainInput.Value;
                    await NavigationService.NavigateToAsync<ConfirmPasswordRegisterStepViewModel>(_collectedInputsArgs);
                } else {
                    Debugger.Break();
                    await NavigationService.GoBackAsync();
                }
            }
        }
    }
}
