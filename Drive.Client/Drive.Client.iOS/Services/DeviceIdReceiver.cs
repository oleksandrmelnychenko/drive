﻿using Drive.Client.iOS.Services;
using Drive.Client.Models.Services.DeviceIdentifier;
using Drive.Client.Services.DeviceIdentifer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceIdentifier))]
namespace Drive.Client.iOS.Services {
    internal class DeviceIdentifier : IDeviceIdentifier {
        public string GetDeviceId() {
            return UIDevice.CurrentDevice.IdentifierForVendor.AsString();
        }

        public Task<ILocation> GetDeviceLocationAsync() =>
            Task<ILocation>.Run(async () => {
                ILocation result = null;

                try {
                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Low);
                    Location location = await Geolocation.GetLocationAsync(request);

                    if (location != null) {
                        result = new Drive.Client.iOS.Models.Services.DeviceIdentifier.Location {
                            Longitude = location.Longitude,
                            Latitude = location.Latitude,
                            TimestampUtc = location.TimestampUtc.UtcDateTime
                        };
                    }
                }
                catch (FeatureNotSupportedException fnsEx) {
                    // TODO: Handle not supported on device exception
                    Debugger.Break();
                }
                catch (PermissionException pEx) {
                    // TODO: Handle permission exception
                    Debugger.Break();
                }
                catch (Exception ex) {
                    // TODO: Unable to get location
                    Debugger.Break();
                }

                return result;
            });
    }
}