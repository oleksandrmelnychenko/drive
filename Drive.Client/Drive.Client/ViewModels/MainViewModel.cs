﻿using Drive.Client.Helpers;
using Drive.Client.Models.Arguments.Notifications;
using Drive.Client.Models.Identities.Device;
using Drive.Client.Models.Identities.NavigationArgs;
using Drive.Client.Services.DeviceUtil;
using Drive.Client.Services.Identity;
using Drive.Client.Services.Notifications;
using Drive.Client.ViewModels.Base;
using Drive.Client.ViewModels.BottomTabViewModels.Calculator;
using Drive.Client.ViewModels.BottomTabViewModels.Home;
using Drive.Client.ViewModels.BottomTabViewModels.PostBuilder;
using Drive.Client.ViewModels.BottomTabViewModels.Profile;
using Drive.Client.ViewModels.BottomTabViewModels.Search;
using Drive.Client.ViewModels.Popups;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Drive.Client.ViewModels {
    public sealed class MainViewModel : ContentPageBaseViewModel {

        private readonly IDeviceUtilService _deviceUtilService;
        private readonly INotificationService _notificationService;
        private readonly IIdentityService _identityService;

        private CancellationTokenSource _registerClientDeviceInfoCancellationTokenSource = new CancellationTokenSource();

        UpdateAppVersionPopupViewModel _updateAppVersionPopupViewModel;
        public UpdateAppVersionPopupViewModel UpdateAppVersionPopupViewModel {
            get => _updateAppVersionPopupViewModel;
            private set {
                _updateAppVersionPopupViewModel?.Dispose();
                SetProperty(ref _updateAppVersionPopupViewModel, value);
            }
        }

        public MainViewModel(
            IDeviceUtilService deviceUtilService,
            INotificationService notificationService,
            IIdentityService identityService) {

            _deviceUtilService = deviceUtilService;
            _notificationService = notificationService;
            _identityService = identityService;

            List<IBottomBarTab> bottomBarTabs = new List<IBottomBarTab>() {
                DependencyLocator.Resolve<HomeViewModel>(),
                DependencyLocator.Resolve<SearchViewModel>(),
                DependencyLocator.Resolve<CalculatorViewModel>(),
                DependencyLocator.Resolve<ProfileViewModel>()
            };

            if (!string.IsNullOrEmpty(BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken)) {
                bottomBarTabs.Insert(2, DependencyLocator.Resolve<PostBuilderViewModel>());
            }

            BottomBarItems = bottomBarTabs;
            BottomBarItems.ForEach(bottomBarTab => bottomBarTab.InitializeAsync(this));

            _identityService.StartUseUserProfile();

            if (string.IsNullOrEmpty(BaseSingleton<GlobalSetting>.Instance.MessagingDeviceToken)) {
                MessagingCenter.Subscribe<object>(this, "device_token", (sender) => {
                    MessagingCenter.Unsubscribe<object>(this, "device_token");

                    RegisterClientDeviceInfo();
                });
            }
            else {
                RegisterClientDeviceInfo();
            }

            SelectedBottomItemIndex = 1;

            UpdateAppVersionPopupViewModel = DependencyLocator.Resolve<UpdateAppVersionPopupViewModel>();
            UpdateAppVersionPopupViewModel.InitializeAsync(this);
        }

        public override void Dispose() {
            base.Dispose();

            BottomBarItems?.ForEach(bottomBarItem => bottomBarItem?.Dispose());
            UpdateAppVersionPopupViewModel?.Dispose();
            ResetCancellationTokenSource(ref _registerClientDeviceInfoCancellationTokenSource);
        }

        public override Task InitializeAsync(object navigationData) {
            UpdateAppVersionPopupViewModel.InitializeAsync(this);

            if (navigationData is BottomTabIndexArgs bottomTabIndexArgs) {
                try {
                    SelectedBottomItemIndex =
                        BottomBarItems.IndexOf(BottomBarItems?.FirstOrDefault(barItem => barItem.GetType().Equals(bottomTabIndexArgs.TargetTab)));
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERRROR:{ex.Message}");
                    Debugger.Break();
                }
            }

            BottomBarItems?.ForEach(bottomBarItem => bottomBarItem.InitializeAsync(navigationData));

            return base.InitializeAsync(navigationData);
        }

        protected override void OnSubscribeOnAppEvents() {
            base.OnSubscribeOnAppEvents();

            _notificationService.ReceivedResidentVehicleDetailInfo += OnNotificationServiceReceivedResidentVehicleDetailInfo;
            _notificationService.TryToResolveLastReceivedNotification();
        }

        protected override void OnUnsubscribeFromAppEvents() {
            base.OnUnsubscribeFromAppEvents();

            _notificationService.ReceivedResidentVehicleDetailInfo -= OnNotificationServiceReceivedResidentVehicleDetailInfo;
        }

        private async void RegisterClientDeviceInfo() {
            try {
                if (BaseSingleton<GlobalSetting>.Instance.UserProfile.IsAuth) {
                    ResetCancellationTokenSource(ref _registerClientDeviceInfoCancellationTokenSource);
                    CancellationTokenSource cancellationTokenSource = _registerClientDeviceInfoCancellationTokenSource;

                    ClientHardware clientHardware = await _deviceUtilService.GetDeviceInfoAsync(cancellationTokenSource);

                    bool deviceRegistrationCompletion = await _deviceUtilService.RegisterClientDeviceInfoAsync(clientHardware, cancellationTokenSource);
                    if (!deviceRegistrationCompletion) {
                        UpdateAppVersionPopupViewModel.ShowPopupCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERRROR:{ex.Message}");
                Debugger.Break();
            }
        }

        private void OnNotificationServiceReceivedResidentVehicleDetailInfo(object sender, ReceivedResidentVehicleDetailInfoArgs args) {
            try {
                IBottomBarTab profileTab = BottomBarItems.FirstOrDefault(tab => tab is ProfileViewModel);
                profileTab.InitializeAsync(args);

                SelectedBottomItemIndex = BottomBarItems.IndexOf(profileTab);
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERRROR:{ex.Message}");
                Debugger.Break();
            }
        }
    }
}
