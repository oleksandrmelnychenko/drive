﻿using Drive.Client.Helpers.Localize;
using Drive.Client.Models.DataItems.Vehicle;
using Drive.Client.Models.EntityModels.Search;
using Drive.Client.Resources.Resx;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Drive.Client.Factories.Vehicle {
    class VehicleFactory : IVehicleFactory {
        public List<ResidentRequestDataItem> BuildItems(IEnumerable<ResidentRequest> residentRequests) {
            List<ResidentRequestDataItem> residentRequestDataItems = new List<ResidentRequestDataItem>();

            foreach (var request in residentRequests) {
                ResidentRequestDataItem residentRequestDataItem = new ResidentRequestDataItem {
                    ResidentRequest = request,
                    Created = request.Created,
                    Status = GetLocalizeStatus(request.Status),
                    CountOutput = GetOutputValue(request.VehicleCount),
                    StatusColor = (request.Status == Status.Finished) ? (Color)App.Current.Resources["StatusFinishedColor"] : (Color)App.Current.Resources["StatusProcessingColor"]
                };
                residentRequestDataItem.InitializeAsync(null);
                residentRequestDataItems.Add(residentRequestDataItem);
            }

            return residentRequestDataItems;
        }

        private StringResource GetLocalizeStatus(Status status) {
            StringResource resource = (status == Status.Finished) ? ResourceLoader.Instance.GetString(nameof(AppStrings.FinishedUpperCase))
                : ResourceLoader.Instance.GetString(nameof(AppStrings.ProcessingUpperCase));
            return resource;
        }

        private StringResource GetOutputValue(long count) {
            StringResource resource = count.Equals(0) || count > 4 ?
                ResourceLoader.Instance.GetString(nameof(AppStrings.VehiclesUpperCase))
                : count.Equals(1) ? ResourceLoader.Instance.GetString(nameof(AppStrings.VehicleUpperCase))
                : ResourceLoader.Instance.GetString(nameof(AppStrings.VehiclesSecondTypeUpperCase));
            return resource;
        }
    }
}