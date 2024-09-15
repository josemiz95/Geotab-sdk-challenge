namespace GeotabChallenge.Feed;

public class FeedToCSV : IFeed
{
    private readonly string path;

    public FeedToCSV(string path)
    {
        this.path = path;
    }

    public Task FeedVehicle(Vehicle vehicle)
    {
        throw new NotImplementedException();
    }
}

