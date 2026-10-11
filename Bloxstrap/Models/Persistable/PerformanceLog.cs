namespace Bloxstrap.Models.Persistable
{
    public class PerformanceLog
    {
        public List<SessionPerformance> Sessions { get; set; } = new();
    }

    public class SessionPerformance
    {
        public long UniverseId { get; set; }

        public long PlaceId { get; set; }

        public string Name { get; set; } = "";

        public DateTime Started { get; set; }

        public DateTime Ended { get; set; }

        public double AverageCpu { get; set; }

        public double PeakCpu { get; set; }

        public double AverageGpu { get; set; }

        public double PeakGpu { get; set; }

        public double AverageMemoryMb { get; set; }

        public double PeakMemoryMb { get; set; }

        public List<double> CpuSamples { get; set; } = new();

        public List<double> GpuSamples { get; set; } = new();
    }
}
