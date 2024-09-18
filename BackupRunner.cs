using Geotab.Checkmate.ObjectModel;
using GeotabChallenge.Feed;

namespace GeotabChallenge;
public class BackupRunner
{
    private readonly GeotabApiConection geoTabApi;
    private readonly IFeed feed;

    public BackupRunner(GeotabApiConection geoTabApi, IFeed feed) 
    { 
        this.geoTabApi = geoTabApi;
        this.feed = feed;
    }

    public async Task Run()
    {
        var vehiclesData = await GetVehiclesDataAsync();

        if (vehiclesData == null || vehiclesData.Count() == 0)
            return;
            
        else
            Console.WriteLine($"{vehiclesData.Count()} vehicles found");

        List<Task> tasks = new List<Task>();

        foreach (var vehicle in vehiclesData)
        {
            tasks.Add(WriteData(vehicle));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<IEnumerable<Vehicle>?> GetVehiclesDataAsync()
    {
        try
        {
            var vehiclesData = await geoTabApi.GetVehiclesDataAsync();

            return vehiclesData;
        }
        catch (OverLimitException)
        {
            Console.WriteLine("The user has exceeded the query limit. This is going to be retried");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get vehicles data, {ex}");
        }

        return null;
    }

    private async Task WriteData(Vehicle vehicle)
    {
        try
        {
            await feed.FeedVehicle(vehicle);
        }
        catch (IOException)
        {
            Console.WriteLine("The file is not accessible. It might be in use by another program.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while feeding information for vehicle {vehicle.Id}, {ex.Message}");
            return;
        }
    }
}

