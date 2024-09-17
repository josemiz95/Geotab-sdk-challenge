using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace GeotabChallenge;

public class GeotabApiConection
{
    private readonly API api;

    private GeotabApiConection(API api)
    {
        this.api = api;
    }

    public static async Task<GeotabApiConection?> Create(string user, string password, string database, string server, CancellationToken token = default)
    {
        try
        {
            var api = new API(user, password, null, database, server);
            await api.AuthenticateAsync(token);

            return new GeotabApiConection(api);
        }
        catch (InvalidUserException ex)
        {
            Console.WriteLine("Invalid user.");
            Console.WriteLine($"Exception details: {ex.Message}");
        }
        catch (DbUnavailableException ex)
        {
            Console.WriteLine("Unable to connect to database.");
            Console.WriteLine($"Exception details: {ex.Message}");
        }
        catch (OverLimitException ex)
        {
            Console.WriteLine("The user has exceeded the query limit.");
            Console.WriteLine($"Exception details: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurs while authenticating user.");
            Console.WriteLine($"Exception details: {ex.Message}");
        }

        return null;
    }

    public async Task<IEnumerable<Vehicle>> GetVehiclesDataAsync() 
    {
        try
        {
            var devices = await api.CallAsync<IList<Device>>("Get", typeof(Device));

            if (devices != null && devices.Count > 0)
            {
                var devicesIds = devices
                    .Where(d => d.Id != null)
                    .Select(x => x.Id!)
                    .ToList();

                var devicesStatusInfo = await GetDevicesStatusInfo(devicesIds);
                var statusDatas = await GetStatusData(devicesIds);

                return MapInformation(devices, devicesStatusInfo, statusDatas);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error happend while getting information, {ex}");
        }

        return new List<Vehicle>();
    }

    private async Task<IEnumerable<DeviceStatusInfo>?> GetDevicesStatusInfo(List<Id> devicesIds)
    {
        return await api.CallAsync<IList<DeviceStatusInfo>>("Get", typeof(DeviceStatusInfo), new
        {
            search = new DeviceStatusInfoSearch
            {
                DeviceSearch = new DeviceSearch
                {
                    DeviceIds = devicesIds
                }
            },
            propertySelector = new PropertySelector
            {
                Fields = new List<string>
                    {
                        nameof(DeviceStatusInfo.Latitude),
                        nameof(DeviceStatusInfo.Longitude)
                    },
                IsIncluded = true
            }
        });
    }

    private async Task<IEnumerable<StatusData>?> GetStatusData(List<Id> devicesIds)
    {
        object[] calls = devicesIds.Select(GetStatusDataCall).ToArray();

        var result = await api.MultiCallAsync(calls);

        var statusDataList = result.OfType<IList<StatusData>>().Select(s => s.First());

        return statusDataList;
    }

    private static object[] GetStatusDataCall(Id deviceId)
    {
        return [ 
            "Get", 
            typeof(StatusData),
            new {
                search = new StatusDataSearch
                {
                    DeviceSearch = new DeviceSearch(deviceId),
                    DiagnosticSearch = new DiagnosticSearch(KnownId.DiagnosticOdometerAdjustmentId),
                    FromDate = DateTime.MaxValue
                },
                propertySelector = new PropertySelector
                {
                    Fields = new List<string>
                    {
                        nameof(StatusData.Data),
                    },
                    IsIncluded = true
                }
            },
            typeof(IList<StatusData>)
        ];

    }

    private IEnumerable<Vehicle> MapInformation(
        IEnumerable<Device> devices, 
        IEnumerable<DeviceStatusInfo>? devicesStatusInfo, 
        IEnumerable<StatusData>? statusDatas)
    {
        return devices.Select(d =>
        {
            var deviceStatusInfo = devicesStatusInfo?.FirstOrDefault(statusInfo => statusInfo?.Device?.Id == d.Id);
            var statusData = statusDatas?.FirstOrDefault(statusInfo => statusInfo?.Device?.Id == d.Id);

            var goDevice = d as GoDevice;

            return new Vehicle
            {
                Id = d.Id?.ToString(),
                Vin = goDevice?.VehicleIdentificationNumber ?? "",
                Latitude = deviceStatusInfo?.Latitude ?? 0,
                Longitude = deviceStatusInfo?.Longitude ?? 0,
                Odometer = statusData?.Data ?? 0
            };
        }).ToList();
    }
}