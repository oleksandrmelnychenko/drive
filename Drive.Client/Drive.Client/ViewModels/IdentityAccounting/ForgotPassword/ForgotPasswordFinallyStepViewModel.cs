﻿using Drive.Client.Helpers.Localize;
using Drive.Client.Models.Arguments.IdentityAccounting.ForgotPassword;
using Drive.Client.Models.EntityModels.Identity;
using Drive.Client.Resources.Resx;
using Drive.Client.Services.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Drive.Client.ViewModels.IdentityAccounting.ForgotPassword {
    public sealed class ForgotPasswordFinallyStepViewModel : StepBaseViewModel {

        private CancellationTokenSource _forgotPasswordCancellationTokenSource = new CancellationTokenSource();

        private readonly IIdentityService _identityService;

        private ForgotPasswordArgs _forgotPasswordArgs;

        /// <summary>
        ///     ctor().
        /// </summary>
        public ForgotPasswordFinallyStepViewModel(IIdentityService identityService) {
            _identityService = identityService;

            StepTitle = PASSWORD_CONFIRM_STEP_REGISTRATION_TITLE;
            MainInputPlaceholder = PASSWORD_PLACEHOLDER_STEP_REGISTRATION;
            MainInputIconPath = PASSWORD_ICON_PATH;
            IsPasswordInput = true;
        }

        public override void Dispose() {
            base.Dispose();

            ResetCancellationTokenSource(ref _forgotPasswordCancellationTokenSource);
        }

        public override Task InitializeAsync(object navigationData) {

            if (navigationData is ForgotPasswordArgs forgotPasswordArgs) {
                _forgotPasswordArgs = forgotPasswordArgs;
            }

            return base.InitializeAsync(navigationData);
        }

        public override bool ValidateForm() {
            if (!base.ValidateForm()) return false;

            bool isValid = MainInput.Value != null && MainInput.Value.Equals(_forgotPasswordArgs.NewPassword);
            if (!isValid) {
                ServerError = (ResourceLoader.GetString(nameof(AppStrings.ReEnteredPasswordIncorrect)).Value);
            }

            return isValid;
        }

        protected async override void OnStepCommand() {
            if (ValidateForm()) {
                ResetCancellationTokenSource(ref _forgotPasswordCancellationTokenSource);
                CancellationTokenSource cancellationTokenSource = _forgotPasswordCancellationTokenSource;

                _forgotPasswordArgs.NewPassword = MainInput.Value;

                Guid busyKey = Guid.NewGuid();
                SetBusy(busyKey, true);

                try {
                    var user = await _identityService.ForgotPasswordAsync(_forgotPasswordArgs, _forgotPasswordCancellationTokenSource.Token);

                    if (user != null) {
                        await NavigationService.NavigateToAsync<MainViewModel>();
                    }
                }
                catch (Exception ex) {
                    try {
                        var error = JsonConvert.DeserializeObject<HttpRequestExceptionResult>(ex.Message);
                        ServerError = error.Message;

                        Debug.WriteLine($"ERROR:{error.Message}");
                        Debugger.Break();
                    }
                    catch (Exception exc) {
                        Debug.WriteLine($"ERROR:{exc.Message}");
                        Debugger.Break();
                    }
                }
                SetBusy(busyKey, false);
            }
        }
    }
}
