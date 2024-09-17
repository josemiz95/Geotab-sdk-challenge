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

    public async Task Run(CancellationToken token = default)
    {
        Console.WriteLine("Running backup");

        var vehiclesData = await geoTabApi.GetVehiclesDataAsync();

        if(vehiclesData == null || vehiclesData.Count() == 0)
        {
            Console.WriteLine("There is no vehicle data");
            return;
        }

        List<Task> tasks = new List<Task>();

        foreach (var vehicle in vehiclesData)
        {
            tasks.Add(feed.FeedVehicle(vehicle));
        }

        await Task.WhenAll(tasks);
    }
}

