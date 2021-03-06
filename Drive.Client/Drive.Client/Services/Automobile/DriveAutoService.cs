﻿using Drive.Client.Exceptions;
using Drive.Client.Helpers;
using Drive.Client.Models.EntityModels;
using Drive.Client.Models.EntityModels.Cognitive;
using Drive.Client.Models.EntityModels.Search;
using Drive.Client.Services.RequestProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Drive.Client.Services.Automobile {
    public class DriveAutoService : IDriveAutoService {

        private readonly IRequestProvider _requestProvider;

        /// <summary>
        ///     ctor().
        /// </summary>
        /// <param name="requestProvider"></param>
        public DriveAutoService(IRequestProvider requestProvider) {
            _requestProvider = requestProvider;
        }

        /// <summary>
        /// Get all drive autos.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<List<DriveAuto>> GetAllDriveAutosAsync(string value, CancellationToken cancellationToken = default(CancellationToken)) =>
             await Task.Run(async () => {
                 List<DriveAuto> driveAutos = null;

                 string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.CarInfoEndPoints.GetAllCarInfoEndPoint, value);

                 try {
                     driveAutos = await _requestProvider.GetAsync<List<DriveAuto>>(url);
                 }
                 catch (ConnectivityException exc) {
                     throw exc;
                 }
                 catch (Exception ex) {
                     Debug.WriteLine($"ERROR: {ex.Message}");
                     throw new Exception(ex.Message);
                 }
                 return driveAutos;
             }, cancellationToken);

        /// <summary>
        /// Get car numbers autocomplete.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<List<DriveAutoSearch>> GetCarNumbersAutocompleteAsync(string value, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                List<DriveAutoSearch> carNumbers = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.CarInfoEndPoints.AutoCompleteEndpoint, value);

                try {
                    carNumbers = await _requestProvider.GetAsync<List<DriveAutoSearch>>(url);
                }
                catch (ConnectivityException exc) {
                    throw exc;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR: {ex.Message}");
                    throw new Exception(ex.Message);
                }
                return carNumbers;
            }, cancellationToken);
        
        /// <summary>
        /// Get drive auto by full match number.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public async Task<List<DriveAuto>> GetDriveAutoByNumberAsync(string number, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                List<DriveAuto> carInfos = null;

                string url = string.Format(BaseSingleton<GlobalSetting>.Instance.RestEndpoints.CarInfoEndPoints.GetByNumberEndPoint, number);

                try {
                    carInfos = await _requestProvider.GetAsync<List<DriveAuto>>(url);
                }
                catch (ConnectivityException exc) {
                    throw exc;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR: {ex.Message}");
                    throw new Exception(ex.Message);
                }
                return carInfos;
            }, cancellationToken);

        public async Task<List<DriveAuto>> SearchDriveAutoByCognitiveAsync(FormDataContent formDataContent, CancellationToken cancellationToken = default(CancellationToken)) =>
            await Task.Run(async () => {
                List<DriveAuto> driveAutos = null;

                string url = BaseSingleton<GlobalSetting>.Instance.RestEndpoints.CarInfoEndPoints.SearchByCognitiveEndPoint;
                string accesToken = BaseSingleton<GlobalSetting>.Instance.UserProfile.AccesToken;

                try {
                    driveAutos = await _requestProvider.PostComplexFormDataAsync<List<DriveAuto>, FormDataContent>(url, formDataContent, accesToken);
                }
                catch (ConnectivityException exc) {
                    throw exc;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"ERROR: {ex.Message}");
                    throw new Exception(ex.Message);
                }
                return driveAutos;
            }, cancellationToken);
    }
}
