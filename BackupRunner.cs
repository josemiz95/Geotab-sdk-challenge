using GeotabChallenge.Feed;

namespace GeotabChallenge;
public class BackupRunner
{
    private readonly GeotabApiConection geoTabApi;
    private readonly IFeed feed;

    public BackupRunner(GeotabApiConection api, IFeed feed) 
    { 
        this.geoTabApi = api;
        this.feed = feed;
    }

    public async Task Run(CancellationToken token = default)
    {
        Console.WriteLine("Running backup");

        await Task.Delay(1000);
    }
}

