namespace GeotabChallenge;

public class Vehicle
{
    public string? Id { get; set; }
    public string Vin { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Odometer { get; set; }

    public DateTime Timestamp { get; private set; } = DateTime.Now;
}