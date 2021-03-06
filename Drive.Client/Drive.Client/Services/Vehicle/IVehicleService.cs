﻿using Drive.Client.Models.EntityModels.Search;
using Drive.Client.Models.EntityModels.Vehicle;
using Drive.Client.Models.Identities.NavigationArgs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Drive.Client.Services.Vehicle {
    public interface IVehicleService {
        Task<VehicleDetailsByResidentFullName> GetVehicleDetailsByResidentFullNameAsync(SearchByPersonArgs searchByPersonArgs, CancellationToken cancellationToken = default(CancellationToken));

        Task<PolandVehicleDetail> GetPolandVehicleDetails(SearchByPolandNumberArgs searchByPolandNumberArgs, CancellationToken cancellationToken = default(CancellationToken));
        Task<PolandVehicleDetail> GetPolandVehicleDetailsByRequestIdAsync(string requestId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<VehicleDetail>> GetVehiclesByRequestIdAsync(long govRequestId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<PolandVehicleRequest>> GetPolandVehicleRequestsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<List<ResidentRequest>> GetUserVehicleDetailRequestsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<List<CognitiveRequest>> GetCognitiveRequestsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<DriveAuto> GetCognitiveVehicleDetailsAsync(string netId, CancellationToken token = default(CancellationToken));
    }
}
