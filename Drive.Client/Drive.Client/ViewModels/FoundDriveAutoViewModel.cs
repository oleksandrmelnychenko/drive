﻿using Drive.Client.Extensions;
using Drive.Client.Helpers;
using Drive.Client.Models.EntityModels;
using Drive.Client.Services.Automobile;
using Drive.Client.ViewModels.ActionBars;
using Drive.Client.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Drive.Client.ViewModels {
    public sealed class FoundDriveAutoViewModel : ContentPageBaseViewModel {

        private CancellationTokenSource _getCarsCancellationTokenSource = new CancellationTokenSource();

        private readonly IDriveAutoService _driveAutoService;

        bool _isBackButtonAvailable;
        public bool IsBackButtonAvailable {
            get { return _isBackButtonAvailable; }
            set { SetProperty(ref _isBackButtonAvailable, value); }
        }

        string _targetCarNumber;
        public string TargetCarNumber {
            get => _targetCarNumber;
            private set => SetProperty<string>(ref _targetCarNumber, value);
        }

        ObservableCollection<DriveAuto> _foundCars;
        public ObservableCollection<DriveAuto> FoundCars {
            get => _foundCars;
            private set => SetProperty(ref _foundCars, value);
        }

        DriveAuto _selectedCar;
        public DriveAuto SelectedCar {
            get { return _selectedCar; }
            set {
                if (SetProperty(ref _selectedCar, value) && value != null) {
                    NavigationService.NavigateToAsync<DriveAutoDetailsViewModel>(value);
                }
            }
        }

        /// <summary>
        ///     ctor().
        /// </summary>
        public FoundDriveAutoViewModel(IDriveAutoService driveAutoService) {
            _driveAutoService = driveAutoService;

            ActionBarViewModel = DependencyLocator.Resolve<CommonActionBarViewModel>();

            IsBackButtonAvailable = NavigationService.IsBackButtonAvailable;
        }

        public override void Dispose() {
            base.Dispose();

            ResetCancellationTokenSource(ref _getCarsCancellationTokenSource);
        }

        public override Task InitializeAsync(object navigationData) {

            if (navigationData is string) {
                TargetCarNumber = navigationData.ToString();

                if (TargetCarNumber.Length == AppConsts.LIMIT_CAR_NUMBER) {
                    GetDriveAutoDetail(TargetCarNumber);
                } else {
                    GetAllDriveAutoOrders(TargetCarNumber);
                }
            }

            return base.InitializeAsync(navigationData);
        }

       
        private async void GetDriveAutoDetail(string targetCarNumber) {
            ResetCancellationTokenSource(ref _getCarsCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getCarsCancellationTokenSource;

            Guid busyKey = Guid.NewGuid();
            SetBusy(busyKey, true);

            try {
                IEnumerable<DriveAuto> result = await _driveAutoService.GetDriveAutoByNumberAsync(targetCarNumber, cancellationTokenSource);

                if (result != null) {
                    FoundCars = result.ToObservableCollection();
                }
            }
            catch (OperationCanceledException ex) { Debug.WriteLine($"ERROR: {ex.Message}"); }
            catch (ObjectDisposedException ex) { Debug.WriteLine($"ERROR: {ex.Message}"); }

            catch (Exception ex) {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }

            SetBusy(busyKey, false);
        }

        private async void GetAllDriveAutoOrders(string targetCarNumber) {
            ResetCancellationTokenSource(ref _getCarsCancellationTokenSource);
            CancellationTokenSource cancellationTokenSource = _getCarsCancellationTokenSource;

            Guid busyKey = Guid.NewGuid();
            SetBusy(busyKey, true);

            try {
                IEnumerable<DriveAuto> result = await _driveAutoService.GetAllDriveAutosAsync(targetCarNumber, cancellationTokenSource);

                if (result != null) {
                    FoundCars = result.ToObservableCollection();
                }
            }
            catch (OperationCanceledException ex) { Debug.WriteLine($"ERROR: {ex.Message}"); }
            catch (ObjectDisposedException ex) { Debug.WriteLine($"ERROR: {ex.Message}"); }

            catch (Exception ex) {
                Debug.WriteLine($"ERROR: {ex.Message}");
            }

            SetBusy(busyKey, false);
        }
    }
}
