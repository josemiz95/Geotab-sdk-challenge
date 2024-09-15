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

}