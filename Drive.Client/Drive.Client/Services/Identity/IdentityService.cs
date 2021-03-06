﻿using Drive.Client.Exceptions;
using Drive.Client.Helpers;
using Drive.Client.Models.Arguments.IdentityAccounting.ChangePassword;
using Drive.Client.Models.Arguments.IdentityAccounting.ForgotPassword;
using Drive.Client.Models.Arguments.IdentityAccounting.Registration;
using Drive.Client.Models.EntityModels.Identity;
using Drive.Client.Models.Identities.NavigationArgs;
using Drive.Client.Models.Medias;
using Drive.Client.Services.Navigation;
using Drive.Client.Services.RequestProvider;
using Drive.Client.Services.Signal.Announcement;
using Drive.Client.ViewModels.BottomTabViewModels;
using Drive.Client.ViewModels.BottomTabViewModels.Profile;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Drive.Client.Services.Identity {
    public class IdentityService : IIdentityService {

        private readonly IRequestProvider _requestProvider;
        private readonly INavigationService _navigationService;
        private readonly IAnnouncementSignalService _announcementHubService;

        /// <summary>
        ///     ctor().
        /// </summary>
        public IdentityService(
            IRequestProvider requestProvider,
            INavigationService navigationService,
            IAnnouncementSignalService announcementHubService) {

            _requestProvider = requestProvider;
            _navigationService = navigationService;
            _announcementHubService = announcementHubService;
        }

        public async Task<PhoneNumberAvailabilty> CheckPhoneNumberAvailabiltyAsync(string phoneNumber, CancellationToken cancellationToken) =>
            await Task.Run(async () => {
                PhoneNumberAvailabilty phoneNumberAvailabilty = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.CheckPhoneNumberEndPoint, phoneNumber);

                try {
                    phoneNumberAvailabilty = await _requestProvider.GetAsync<PhoneNumberAvailabilty>(url);
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }

                return phoneNumberAvailabilty;
            }, cancellationToken);

        public async Task<UserNameAvailability> CheckUserNameAvailabiltyAsync(string userNmae, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                UserNameAvailability userNameAvailability = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.CheckUserNameEndpoint, userNmae);

                try {
                    userNameAvailability = await _requestProvider.GetAsync<UserNameAvailability>(url);
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
                return userNameAvailability;
            }, cancellationToken);

        public async Task<AuthenticationResult> SignUpAsync(RegistrationCollectedInputsArgs collectedInputsArgs, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                AuthenticationResult authenticationResult = null;

                string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.SignUpEndPoint;

                try {
                    authenticationResult = await _requestProvider.PostAsync<AuthenticationResult, RegistrationCollectedInputsArgs>(url, collectedInputsArgs);

                    if (authenticationResult != null && authenticationResult.IsSucceed) {
                        await SetupProfileAsync(authenticationResult);
                    }
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }

                return authenticationResult;
            }, cancellationToken);


        public async Task<AuthenticationResult> SignInAsync(SignInArgs signInArgsArgs, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                AuthenticationResult authenticationResult = null;

                string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.SignInEndPoint;

                try {
                    authenticationResult = await _requestProvider.PostAsync<AuthenticationResult, SignInArgs>(url, signInArgsArgs);

                    if (authenticationResult != null && authenticationResult.IsSucceed) {
                        await SetupProfileAsync(authenticationResult);
                    }
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }

                return authenticationResult;
            }, cancellationToken);

        public async Task LogOutAsync() {
            await Task.Run(async () => {
                try {
                    string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.LogOutEndPoint;
                    string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

                    LogOutResult logOutResult = await _requestProvider.PostAsync<LogOutResult, object>(url, null, accessToken);

                    if (logOutResult != null) { }
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR LogOutAsync():{ex.Message}");
                }
            });

            try {
                BaseSingleton<GlobalSetting>.Instance.UserProfile.ClearUserProfile();
                BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();

                await StopSocketServicesAsync();

                Device.BeginInvokeOnMainThread(async () => {
                    await _navigationService.InitializeAsync(new BottomTabIndexArgs() { TargetTab = typeof(ProfileViewModel) });
                });
            }
            catch (Exception exc) {
                Crashes.TrackError(exc);
                Debugger.Break();
                throw;
            }
        }

        public async Task<ChangedProfileData> ChangePhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default(CancellationToken)) =>
           await Task.Run(async () => {
               ChangedProfileData changedProfileData = null;

               string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.ChangePhoneNumberEndPoint, phoneNumber);
               string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

               try {
                   changedProfileData = await _requestProvider.PostAsync<ChangedProfileData, object>(url, null, accessToken);

                   if (changedProfileData != null) {
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.PhoneNumber = changedProfileData.PhoneNumber;
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();
                   }
               }
               catch (ConnectivityException ex) {
                   throw ex;
               }
               catch (ServiceAuthenticationException ex) {
                   await this.LogOutAsync();
                   throw ex;
               }
               catch (HttpRequestExceptionEx ex) {
                   throw ex;
               }
               catch (Exception ex) {
                   Debug.WriteLine($"ERROR:{ex.Message}");
                   Debugger.Break();
               }
               return changedProfileData;
           }, cancellationToken);

        public async Task<ChangedProfileData> ChangeUserNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken)) =>
           await Task.Run(async () => {
               ChangedProfileData changedProfileData = null;

               string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.ChangeUserNameEndPoint, userName);
               string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

               try {
                   changedProfileData = await _requestProvider.PostAsync<ChangedProfileData, object>(url, null, accessToken);

                   if (changedProfileData != null) {
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.UserName = changedProfileData.UserName;
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();
                   }
               }
               catch (ConnectivityException ex) {
                   throw ex;
               }
               catch (ServiceAuthenticationException ex) {
                   await this.LogOutAsync();
                   throw ex;
               }
               catch (HttpRequestExceptionEx ex) {
                   throw ex;
               }
               catch (Exception ex) {
                   Debug.WriteLine($"ERROR:{ex.Message}");
                   Debugger.Break();
               }
               return changedProfileData;
           }, cancellationToken);

        public async Task<ChangedProfileData> ChangeEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                ChangedProfileData changedProfileData = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.ChangeEmailEndPoint, email);
                string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

                try {
                    changedProfileData = await _requestProvider.PostAsync<ChangedProfileData, object>(url, null, accessToken);

                    if (changedProfileData != null) {
                        BaseSingleton<GlobalSetting>.Instance.UserProfile.Email = changedProfileData.Email;
                        BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();
                    }
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (ServiceAuthenticationException ex) {
                    await LogOutAsync();
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
                return changedProfileData;
            }, cancellationToken);

        public async Task<string> UploadUserAvatarAsync(PickedImage pickedImage, CancellationToken cancellationToken = default(CancellationToken)) =>
           await Task.Run(async () => {
               string userAvatar = string.Empty;

               string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.UploadUserAvatarEndpoint;
               string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

               try {
                   User user = await _requestProvider.PostFormDataAsync<User, PickedImage>(url, pickedImage, accessToken);

                   if (user != null) {
                       userAvatar = user.AvatarUrl;
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.AvatarUrl = user.AvatarUrl;
                       BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();
                   }
               }
               catch (ConnectivityException ex) {
                   throw ex;
               }
               catch (ServiceAuthenticationException ex) {
                   await this.LogOutAsync();
                   throw ex;
               }
               catch (HttpRequestExceptionEx ex) {
                   throw ex;
               }
               catch (Exception ex) {
                   Debug.WriteLine($"ERROR:{ex.Message}");
                   Debugger.Break();
               }
               return userAvatar;
           }, cancellationToken);


        public async Task<User> UpdatePasswordAsync(ChangePasswordArgs changePasswordArgs, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                User user = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.ChangePasswordEndPoint,
                     changePasswordArgs.NewPassword, changePasswordArgs.CurrentPassword);
                string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

                try {
                    user = await _requestProvider.PostAsync<User, object>(url, null, accessToken);

                    if (user != null) {

                    }
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (ServiceAuthenticationException ex) {
                    await this.LogOutAsync();
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
                return user;
            }, cancellationToken);


        public async Task<CanChangeForgottenPassword> CanUserChangeForgottenPasswordAsync(string phoneNumber, string name, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                CanChangeForgottenPassword result = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.CanUserChangeForgottenPasswordEndPoint,
                                           phoneNumber,
                                           name);
                try {
                    result = await _requestProvider.GetAsync<CanChangeForgottenPassword>(url);
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
                return result;
            }, cancellationToken);


        public async Task<User> ForgotPasswordAsync(ForgotPasswordArgs forgotPasswordArgs, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                User user = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.ForgotPasswordEndPoint,
                                           forgotPasswordArgs.PhoneNumber, forgotPasswordArgs.UserName, forgotPasswordArgs.NewPassword);
                try {
                    user = await _requestProvider.PutAsync<User, object>(url, null);
                }
                catch (ConnectivityException ex) {
                    throw ex;
                }
                catch (HttpRequestExceptionEx ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
                return user;
            }, cancellationToken);


        public async Task<User> GetUserAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
              await Task.Run(async () => {
                  User user = null;

                  string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.GetUserEndPoint;
                  string accessToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

                  try {
                      user = await _requestProvider.GetAsync<User>(url, accessToken, cancellationToken);
                  }
                  catch (ConnectivityException ex) {
                      throw ex;
                  }
                  catch (ServiceAuthenticationException ex) {
                      await this.LogOutAsync();
                      throw ex;
                  }
                  catch (HttpRequestExceptionEx ex) {
                      throw ex;
                  }
                  catch (Exception ex) {
                      Debug.WriteLine($"ERROR:{ex.Message}");
                      Debugger.Break();
                  }
                  return user;
              }, cancellationToken);

        public Task<IsCurrentPasswordExistResponse> IsCurrentPasswordExistAsync(string password, CancellationToken cancellationToken = default(CancellationToken)) =>
              Task<IsCurrentPasswordExistResponse>.Run(async () => {
                  IsCurrentPasswordExistResponse completion = null;

                  try {
                      string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.IdentityEndpoints.IsCurrentPasswordExist, password);

                      completion = await _requestProvider.GetAsync<IsCurrentPasswordExistResponse>(url,
                          BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken,
                          cancellationToken);
                  }
                  catch (ConnectivityException ex) {
                      throw ex;
                  }
                  catch (HttpRequestExceptionEx ex) {
                      completion = JsonConvert.DeserializeObject<IsCurrentPasswordExistResponse>(ex.Message);

                      throw new InvalidOperationException(completion.Message, ex);
                  }
                  catch (Exception ex) {
                      Debug.WriteLine($"ERROR:{ex.Message}");
                      Debugger.Break();
                  }

                  return completion;
              }, cancellationToken);

        public async void StartUseUserProfile() {
            try {
                if (!string.IsNullOrEmpty(BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken)) {
                    await RestartSocketServicesAsync();
                }
                else {
                    //await LogOutAsync();
                }
            }
            catch (Exception exc) {
                Debugger.Break();
                Crashes.TrackError(exc);

                //await LogOutAsync();
            }
        }

        private Task SetupProfileAsync(AuthenticationResult authenticationResult) =>
            Task.Run(async () => {
                try {
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken = authenticationResult.AccessToken;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.RefreshToken = authenticationResult.RefreshToken;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.IsAuth = true;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.NetId = authenticationResult.User.NetId;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.PhoneNumber = authenticationResult.User.PhoneNumber;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.Email = authenticationResult.User?.Email;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.UserName = authenticationResult.User.UserName;
                    BaseSingleton<GlobalSetting>.Instance.UserProfile.AvatarUrl = authenticationResult.User.AvatarUrl;

                    BaseSingleton<GlobalSetting>.Instance.UserProfile.SaveChanges();

                    await RestartSocketServicesAsync();
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR:{ex.Message}");
                    Debugger.Break();
                }
            });

        private async Task RestartSocketServicesAsync() {
            await StopSocketServicesAsync();

            await _announcementHubService.StartAsync(BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken);
        }

        private async Task StopSocketServicesAsync() {
            await _announcementHubService.StopAsync();
        }
    }
}
