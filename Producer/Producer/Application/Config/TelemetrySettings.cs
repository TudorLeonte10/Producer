namespace Producer.Application.Config
{
    public class TelemetrySettings
    {
        public double BaseSpeed { get; set; } = 70.0;
        public double SpeedVariation { get; set; } = 15.0;
        public double FuelConsumptionRate { get; set; } = 0.01;
        public double TempBase { get; set; } = 90.0;
        public double TempVariation { get; set; } = 10.0;
        public int RecordIntervalSeconds { get; set; } = 60;
    }
}
