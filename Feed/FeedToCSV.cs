﻿namespace GeotabChallenge.Feed;

public class FeedToCSV : IFeed
{
    private readonly string path;

    public FeedToCSV(string path)
    {
        this.path = path;
    }

    public async Task FeedVehicle(Vehicle vehicle)
    {
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var csvPath = Path.Combine(path, $"{vehicle.Id}.csv");
            bool fileExists = File.Exists(csvPath);

            using (var streamWriter = new StreamWriter(csvPath, true))
            {
                if (!fileExists)
                    await streamWriter.WriteLineAsync("Id,Timestamp,Vin,Latitude,Longitude,Odometer");

                await streamWriter.WriteLineAsync(GetCsvLine(vehicle));
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while feeding information for vehicle {vehicle.Id}, {ex.Message}");
            return;
        }
    }

    private string GetCsvLine(Vehicle vehicle)
    {
        var cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
        var latitude = vehicle.Latitude.ToString(cultureInfo);
        var longitude = vehicle.Longitude.ToString(cultureInfo);
        var odometer = vehicle.Odometer.ToString(cultureInfo);

        return $"{vehicle.Id},{vehicle.Timestamp},{vehicle.Vin},{latitude},{longitude},{odometer}";
    }
}

