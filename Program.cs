using GeotabChallenge;
using GeotabChallenge.Feed;

var interval = 60000;
string? server = null;
string? database = null;
string? user = null;
string? password = null;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--u":
            user = args[++i];
            break;
        case "--p":
            password = args[++i];
            break;
        case "--d":
            database = args[++i];
            break;
        case "--s":
            server = args[++i];
            break;
    }
}

if (server == null || database == null || user == null || password == null)
{
    Console.WriteLine("Some parameters are missing.");
    Console.WriteLine();

    Console.WriteLine("Usage: dotnet run --u '<user>' --p '<password>' --d '<database>' --s '<server>'");
    Console.WriteLine("--u  The User");
    Console.WriteLine("--p  The Password");
    Console.WriteLine("--d  The Database");
    Console.WriteLine("--s  The Server");
    return;
}

var cancellationTokenSrc = new CancellationTokenSource();
var cancellationToken = cancellationTokenSrc.Token;

var geoTabApi = await GeotabApiConection.Create(user, password, database, server, cancellationToken);

if (geoTabApi == null)
{
    Console.WriteLine("Failed to connect to the API. Exiting the application.");
    return;
}

var path = Path.Combine(Environment.CurrentDirectory, "BackUps");
var feed = new FeedToCSV(path);
var backUp = new BackupRunner(geoTabApi, feed);

await Task.Run(async () =>
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            Console.WriteLine("Running backup");
            await backUp.Run();
            Console.WriteLine($"Task complete the back up will be run in {interval}ms");
            Console.WriteLine();
            await Task.Delay(interval, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }
    }
}, cancellationToken);