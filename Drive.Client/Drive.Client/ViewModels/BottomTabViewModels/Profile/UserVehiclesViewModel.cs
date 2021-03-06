﻿using Drive.Client.Extensions;
using Drive.Client.Factories.Vehicle;
using Drive.Client.Helpers;
using Drive.Client.Helpers.Localize;
using Drive.Client.Models.Arguments.BottomtabSwitcher;
using Drive.Client.Models.Arguments.Notifications;
using Drive.Client.Models.DataItems.Vehicle;
using Drive.Client.Models.EntityModels.Search;
using Drive.Client.Models.EntityModels.Vehicle;
using Drive.Client.Models.Identities.NavigationArgs;
using Drive.Client.Resources.Resx;
using Drive.Client.Services.Vehicle;
using Drive.Client.ViewModels.Base;
using Drive.Client.ViewModels.IdentityAccounting.Registration;
using Drive.Client.Views.BottomTabViews.Bookmark;
using Drive.Client.Views.BottomTabViews.Profile;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Drive.Client.ViewModels.BottomTabViewModels.Profile {
    public sealed class UserVehiclesViewModel : NestedViewModel, IVisualFiguring, ISwitchTab {

        private CancellationTokenSource _getUserVehicleDetailRequestsCancellationTokenSource = new CancellationTokenSource();

        private CancellationTokenSource _getCognitiveVehicleDetailsCancellationTokenSource = new CancellationTokenSource();

        private CancellationTokenSource _getPolandVehicleInfoCancellationTokenSource = new CancellationTokenSource();

        private CancellationTokenSource _getRequestAsyncCancellationTokenSource = new CancellationTokenSource();

        private CancellationTokenSource _getVehiclesCancellationTokenSource = new CancellationTokenSource();

        private readonly IVehicleFactory _vehicleFactory;

        private readonly IVehicleService _vehicleService;

        private ReceivedResidentVehicleDetailInfoArgs _lastNotificationRequest;

        private bool _isGettingRequestsTEMPORARY = false;

        public Type RelativeViewType => typeof(UserVehiclesView);

        StringResource _tabHeader;
        public StringResource TabHeader {
            get => _tabHeader;
            private set => SetProperty(ref _tabHeader, value);
        }

        bool _visibilityClosedView;
        public bool VisibilityClosedView {
            get { return _visibilityClosedView; }
            set { SetProperty(ref _visibilityClosedView, value); }
        }

        ObservableCollection<BaseRequestDataItem> _userRequests;
        public ObservableCollection<BaseRequestDataItem> UserRequests {
            get { return _userRequests; }
            set {
                _userRequests?.ForEach(r => r.Dispose());
                SetProperty(ref _userRequests, value);
            }
        }

        BaseRequestDataItem _selectedRequest;
        public BaseRequestDataItem SelectedRequest {
            get { return _selectedRequest; }
            set {
                if (SetProperty(ref _selectedRequest, value) && value != null) {
                    if (value is ResidentRequestDataItem residentRequestDataItem) {
                        if (residentRequestDataItem.ResidentRequest.VehicleCount > 0) {
                            GetVehicles(residentRequestDataItem);
                        }
                    } else if (value is PolandRequestDataItem polandRequestDataItem) {
                        if (polandRequestDataItem.PolandVehicleRequest.IsParsed) {
                            OnPolandRequestDataItem(polandRequestDataItem);
                        }
                    } else if (value.HistoryType == RequestType.CognitiveImageData) {
                        GetCognitiveDriveInfo(value);
                    }
                }
            }
        }

        public ICommand SignInCommand => new Command(async () => await NavigationService.NavigateToAsync<SignInPhoneNumberStepViewModel>());

        public ICommand SignUpCommand => new Command(async () => await NavigationService.NavigateToAsync<PhoneNumberRegisterStepViewModel>());

        /// <summary>
        ///     ctor().
        /// </summary>
        public UserVehiclesViewModel(IVehicleService vehicleService, IVehicleFactory vehicleFactory) {
            _vehicleFactory = vehicleFactory;
            _vehicleService = vehicleService;
        }

        public override Task InitializeAsync(object navigationData) {
            if (navigationData is SelectedBottomBarTabArgs) {
                GetRequests();
            }
            if (navigationData is ReceivedResidentVehicleDetailInfoArgs vehicleDetailInfoArgs) {
                _lastNotificationRequest = vehicleDetailInfoArgs;
            }

            UpdateView();

            return base.InitializeAsync(navigationData);
        }

        public override void Dispose() {
            base.Dispose();

            UserRequests?.ForEach(r => r.Dispose());
            UserRequests?.Clear();
            ResetCancellationTokenSource(ref _getUserVehicleDetailRequestsCancellationTokenSource);
            ResetCancellationTokenSource(ref _getCognitiveVehicleDetailsCancellationTokenSource);
            ResetCancellationTokenSource(ref _getPolandVehicleInfoCancellationTokenSource);
            ResetCancellationTokenSource(ref _getRequestAsyncCancellationTokenSource);
            ResetCancellationTokenSource(ref _getVehiclesCancellationTokenSource);
        }

        protected override void ResolveStringResources() {
            base.ResolveStringResources();

            TabHeader = ResourceLoader.GetString(nameof(AppStrings.HistoryRequestsUpperCase));
        }

        private async void GetVehicles(ResidentRequestDataItem residentRequestDataItem) {
            IEnumerable<VehicleDetail> foundVehicles = await GetVehiclesByRequestIdAsync(residentRequestDataItem.ResidentRequest.GovRequestId);

            if (foundVehicles != null && foundVehicles.Any()) {
                VehicleArgs vehicleArgs = new VehicleArgs {
                    ResidentRequestDataItem = residentRequestDataItem,
                    VehicleDetails = foundVehicles
                };

                await NavigationService.NavigateToAsync<VehicleDetailViewModel>(vehicleArgs);
            }
        }

        private async void GetCognitiveDriveInfo(BaseRequestDataItem value) {
            Guid busyKey = Guid.NewGuid();
            UpdateBusyVisualState(busyKey, true);

            ResetCancellationTokenSource(ref _getCognitiveVehicleDetailsCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getCognitiveVehicleDetailsCancellationTokenSource;

            CognitiveRequestDataItem cognitiveRequestDataItem = value as CognitiveRequestDataItem;

            try {
                DriveAuto cognitiveVehicleDetails = await _vehicleService.GetCognitiveVehicleDetailsAsync(cognitiveRequestDataItem.CognitiveRequest.NetId, _getCognitiveVehicleDetailsCancellationTokenSource.Token);

                if (cognitiveVehicleDetails != null) {
                    UpdateBusyVisualState(busyKey, false);
                    await NavigationService.NavigateToAsync<DriveAutoDetailsViewModel>(cognitiveVehicleDetails);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception exc) {
                UpdateBusyVisualState(busyKey, false);

                Debug.WriteLine($"ERROR: {exc.Message}");
                Debugger.Break();
            }
            UpdateBusyVisualState(busyKey, false);
        }

        private async void OnPolandRequestDataItem(PolandRequestDataItem selectedPolandRequestDataItem) {
            Guid busyKey = Guid.NewGuid();
            UpdateBusyVisualState(busyKey, true);

            ResetCancellationTokenSource(ref _getPolandVehicleInfoCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getPolandVehicleInfoCancellationTokenSource;

            try {
                PolandVehicleDetail polandVehicleDetail = await _vehicleService.GetPolandVehicleDetailsByRequestIdAsync(selectedPolandRequestDataItem.PolandVehicleRequest.RequestId, cancellationTokenSource.Token);

                if (polandVehicleDetail != null) {
                    UpdateBusyVisualState(busyKey, false);
                    await NavigationService.NavigateToAsync<PolandDriveAutoDetailsViewModel>(polandVehicleDetail);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception exc) {
                UpdateBusyVisualState(busyKey, false);

                Debug.WriteLine($"ERROR: {exc.Message}");
                Debugger.Break();
            }
            UpdateBusyVisualState(busyKey, false);
        }

        private async Task<IEnumerable<VehicleDetail>> GetVehiclesByRequestIdAsync(long govRequestId) {
            ResetCancellationTokenSource(ref _getVehiclesCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getVehiclesCancellationTokenSource;

            IEnumerable<VehicleDetail> result = null;

            Guid busyKey = Guid.NewGuid();
            UpdateBusyVisualState(busyKey, true);

            try {
                result = await _vehicleService.GetVehiclesByRequestIdAsync(govRequestId, cancellationTokenSource.Token);
            }
            catch (Exception ex) {
                Debug.WriteLine($"ERROR: {ex.Message}");
                Debugger.Break();
            }

            UpdateBusyVisualState(busyKey, false);

            return result;
        }

        private void UpdateView() {
            VisibilityClosedView = !BaseSingleton<GlobalSetting>.Instance.UserProfile.IsAuth;
        }

        private async void GetRequests() {

            if (!_isGettingRequestsTEMPORARY) {
                _isGettingRequestsTEMPORARY = true;

                Guid busyKey = Guid.NewGuid();
                UpdateBusyVisualState(busyKey, true);

                try {
                    if (BaseSingleton<GlobalSetting>.Instance.UserProfile.IsAuth) {

                        UserRequests = (await GetRequestAsync()).ToObservableCollection();

                        if (_lastNotificationRequest != null) {

                            if (long.TryParse(_lastNotificationRequest.RecidentVehicleNotification.Data, out long govRequestId)) {

                                ResidentRequestDataItem requestDataItem = UserRequests?.OfType<ResidentRequestDataItem>().FirstOrDefault<ResidentRequestDataItem>(residentRequestItem => residentRequestItem.ResidentRequest.GovRequestId == govRequestId);

                                if (requestDataItem != null) {
                                    UpdateBusyVisualState(busyKey, false);
                                    GetVehicles(requestDataItem);
                                }
                            }

                            _lastNotificationRequest = null;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR: {ex.Message}");
                }

                _isGettingRequestsTEMPORARY = false;
                UpdateBusyVisualState(busyKey, false);
            }

            UpdateView();
        }

        public void ClearAfterTabTap() {
            Dispose();
        }

        public void TabClicked() {
            GetRequests();
        }

        private async Task<List<BaseRequestDataItem>> GetRequestAsync() {
            ResetCancellationTokenSource(ref _getRequestAsyncCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getRequestAsyncCancellationTokenSource;

            List<BaseRequestDataItem> createdItems = new List<BaseRequestDataItem>();

            try {
                Task[] tasks = new Task[3];

                tasks[0] = Task.Run(async () => {
                    List<ResidentRequest> userRequests = await _vehicleService.GetUserVehicleDetailRequestsAsync(cancellationTokenSource.Token);
                    if (userRequests != null) {
                        createdItems.AddRange(_vehicleFactory.BuildResidentRequestItems(userRequests, ResourceLoader));
                    }
                });

                tasks[1] = Task.Run(async () => {
                    List<PolandVehicleRequest> polandVehicleRequests = await _vehicleService.GetPolandVehicleRequestsAsync(cancellationTokenSource.Token);
                    if (polandVehicleRequests != null) {
                        createdItems.AddRange(_vehicleFactory.BuildPolandRequestItems(polandVehicleRequests, ResourceLoader));
                    }
                });

                tasks[2] = Task.Run(async () => {
                    List<CognitiveRequest> cognitiveRequests = await _vehicleService.GetCognitiveRequestsAsync(cancellationTokenSource.Token);
                    if (cognitiveRequests != null) {
                        createdItems.AddRange(_vehicleFactory.BuildCognitiveRequestItems(cognitiveRequests, ResourceLoader));
                    }
                });

                await Task.Factory.ContinueWhenAll(tasks, completedTasks => {
                    createdItems = createdItems.OrderByDescending(x => x.Created).ToList();
                });
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception exc) {
                Debug.WriteLine($"ERROR: {exc.Message}");

                createdItems = new List<BaseRequestDataItem>();
            }

            return createdItems;
        }
    }
}
