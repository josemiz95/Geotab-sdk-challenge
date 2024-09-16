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

        await Task.Delay(1000);
    }
}

