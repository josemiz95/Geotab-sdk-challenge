using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;

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
        IEnumerable<Vehicle> vehicles = new List<Vehicle>();

        var devices = await api.CallAsync<IList<Device>>("Get", typeof(Device));

        if (devices != null && devices.Count > 0)
        {
            var devicesIds = devices
                .Where(d => d.Id != null)
                .Select(x => x.Id!)
                .ToList();

            var devicesStatusIndo = await GetDevicesStatusInfo(devicesIds);
            // TODO: StatusData -> .Data = odometer

            vehicles = devices.Select(d =>
            {
                var deviceStatusInfo = devicesStatusIndo?.FirstOrDefault(statusInfo => statusInfo?.Device?.Id == d.Id);
                var goDevice = d as GoDevice;

                return new Vehicle
                {
                    Id = d.Id?.ToString(),
                    Name = d.Name ?? "",
                    Vin = goDevice?.VehicleIdentificationNumber ?? "",
                    Latitude = deviceStatusInfo?.Latitude ?? 0,
                    Longitude = deviceStatusInfo?.Longitude ?? 0,
                };
            }).ToList();
        }
        return vehicles;
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
}