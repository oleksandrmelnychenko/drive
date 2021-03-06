﻿using Drive.Client.Helpers.Localize;
using Drive.Client.Models.Arguments.IdentityAccounting.ChangePassword;
using Drive.Client.Models.EntityModels.Identity;
using Drive.Client.Resources.Resx;
using Drive.Client.Services.Identity;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Drive.Client.ViewModels.IdentityAccounting.EditProfile {
    public sealed class EditPasswordFinallyStepViewModel : StepBaseViewModel {

        private CancellationTokenSource _updatePasswordCancellationTokenSource = new CancellationTokenSource();

        private ChangePasswordArgs _changePasswordArgs;

        private readonly IIdentityService _identityService;

        /// <summary>
        ///     ctor().
        /// </summary>
        /// <param name="identityService"></param>
        public EditPasswordFinallyStepViewModel(IIdentityService identityService) {
            _identityService = identityService;

            StepTitle = PASSWORD_CONFIRM_STEP_REGISTRATION_TITLE;
            MainInputPlaceholder = PASSWORD_PLACEHOLDER_STEP_REGISTRATION;
            MainInputIconPath = PASSWORD_ICON_PATH;
            IsPasswordInput = true;
        }

        public override Task InitializeAsync(object navigationData) {
            if (navigationData is ChangePasswordArgs changePasswordArgs) {
                _changePasswordArgs = changePasswordArgs;
            }

            return base.InitializeAsync(navigationData);
        }

        public override void Dispose() {
            base.Dispose();

            ResetCancellationTokenSource(ref _updatePasswordCancellationTokenSource);
        }

        public override bool ValidateForm() {
            if (!base.ValidateForm()) return false;

            bool isValid = MainInput.Value != null && MainInput.Value.Equals(_changePasswordArgs.NewPassword);
            if (!isValid) {
                ServerError = (ResourceLoader.GetString(nameof(AppStrings.ReEnteredPasswordIncorrect)).Value);
            }

            return isValid;
        }

        protected async override void OnStepCommand() {
            if (ValidateForm()) {
                ResetCancellationTokenSource(ref _updatePasswordCancellationTokenSource);
                CancellationTokenSource cancellationTokenSource = _updatePasswordCancellationTokenSource;

                Guid busyKey = Guid.NewGuid();
                SetBusy(busyKey, true);


                try {
                    User user = await _identityService.UpdatePasswordAsync(_changePasswordArgs, _updatePasswordCancellationTokenSource.Token);

                    if (user != null) {
                        await NavigationService.InitializeAsync();
                    }
                }
                catch (Exception ex) {
                    try {
                        var error = JsonConvert.DeserializeObject<HttpRequestExceptionResult>(ex.Message);
                        ServerError = error.Message;

                        Debug.WriteLine($"ERROR:{error.Message}");
                        Debugger.Break();
                    }
                    catch (Exception) {
                        Debug.WriteLine($"ERROR EditPasswordFinallyStepViewModel.OnStepCommand():{ex.Message}");
                        Debugger.Break();
                    }
                }
                SetBusy(busyKey, false);
            }
        }
    }
}
