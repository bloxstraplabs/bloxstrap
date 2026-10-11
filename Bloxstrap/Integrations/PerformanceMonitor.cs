namespace Bloxstrap.Integrations
{
    public class PerformanceMonitor : IDisposable
    {
        private const int SampleSeconds = 2;

        private const int MaxStoredSamples = 90;

        private const int MaxSessions = 30;

        private readonly int _processId;

        private readonly CancellationTokenSource _cancel = new();

        private readonly object _lock = new();

        private readonly List<(double Cpu, double Gpu, double MemoryMb)> _samples = new();

        private List<PerformanceCounter> _gpuCounters = new();

        private DateTime _lastGpuScan = DateTime.MinValue;

        private DateTime _sessionStarted = DateTime.Now;

        public PerformanceMonitor(int processId)
        {
            _processId = processId;
            Task.Run(Run);
        }

        public void BeginSession()
        {
            lock (_lock)
            {
                _samples.Clear();
                _sessionStarted = DateTime.Now;
            }
        }

        private async Task Run()
        {
            TimeSpan? lastCpu = null;
            DateTime lastTime = DateTime.Now;

            while (!_cancel.IsCancellationRequested)
            {
                try
                {
                    using var process = Process.GetProcessById(_processId);

                    var cpuTime = process.TotalProcessorTime;
                    var now = DateTime.Now;

                    double cpu = 0;

                    if (lastCpu is TimeSpan previous)
                        cpu = (cpuTime - previous).TotalMilliseconds / (now - lastTime).TotalMilliseconds / Environment.ProcessorCount * 100;

                    lastCpu = cpuTime;
                    lastTime = now;

                    double memory = process.WorkingSet64 / 1024d / 1024d;
                    double gpu = ReadGpu();

                    lock (_lock)
                        _samples.Add((Math.Clamp(cpu, 0, 100), Math.Clamp(gpu, 0, 100), memory));
                }
                catch (ArgumentException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("PerformanceMonitor::Run", ex);
                }

                try
                {
                    await Task.Delay(SampleSeconds * 1000, _cancel.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private double ReadGpu()
        {
            if (!_gpuCounters.Any() && DateTime.Now - _lastGpuScan > TimeSpan.FromSeconds(30))
            {
                _lastGpuScan = DateTime.Now;

                try
                {
                    string prefix = $"pid_{_processId}_";

                    _gpuCounters = new PerformanceCounterCategory("GPU Engine")
                        .GetInstanceNames()
                        .Where(x => x.StartsWith(prefix) && x.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase))
                        .Select(x => new PerformanceCounter("GPU Engine", "Utilization Percentage", x, true))
                        .ToList();

                    foreach (var counter in _gpuCounters)
                        counter.NextValue();
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("PerformanceMonitor::ReadGpu", ex);
                }
            }

            double total = 0;

            foreach (var counter in _gpuCounters.ToList())
            {
                try
                {
                    total += counter.NextValue();
                }
                catch (InvalidOperationException)
                {
                    counter.Dispose();
                    _gpuCounters.Remove(counter);
                }
            }

            return total;
        }

        private static List<double> Downsample(List<double> values)
        {
            if (values.Count <= MaxStoredSamples)
                return values.Select(x => Math.Round(x, 1)).ToList();

            double step = values.Count / (double)MaxStoredSamples;

            return Enumerable.Range(0, MaxStoredSamples)
                .Select(i => values.Skip((int)(i * step)).Take(Math.Max((int)step, 1)).Average())
                .Select(x => Math.Round(x, 1))
                .ToList();
        }

        public SessionPerformance? EndSession(ActivityData activity, string name)
        {
            List<(double Cpu, double Gpu, double MemoryMb)> samples;

            lock (_lock)
            {
                samples = _samples.Skip(1).ToList();
                _samples.Clear();
            }

            if (samples.Count < 3)
                return null;

            var session = new SessionPerformance
            {
                UniverseId = activity.UniverseId,
                PlaceId = activity.PlaceId,
                Name = name,
                Started = _sessionStarted,
                Ended = activity.TimeLeft ?? DateTime.Now,
                AverageCpu = Math.Round(samples.Average(x => x.Cpu), 1),
                PeakCpu = Math.Round(samples.Max(x => x.Cpu), 1),
                AverageGpu = Math.Round(samples.Average(x => x.Gpu), 1),
                PeakGpu = Math.Round(samples.Max(x => x.Gpu), 1),
                AverageMemoryMb = Math.Round(samples.Average(x => x.MemoryMb)),
                PeakMemoryMb = Math.Round(samples.Max(x => x.MemoryMb)),
                CpuSamples = Downsample(samples.Select(x => x.Cpu).ToList()),
                GpuSamples = Downsample(samples.Select(x => x.Gpu).ToList())
            };

            try
            {
                var manager = new JsonManager<PerformanceLog>();
                manager.Load(false);

                manager.Prop.Sessions.Insert(0, session);

                if (manager.Prop.Sessions.Count > MaxSessions)
                    manager.Prop.Sessions.RemoveRange(MaxSessions, manager.Prop.Sessions.Count - MaxSessions);

                manager.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::EndSession", ex);
            }

            return session;
        }

        public void Dispose()
        {
            _cancel.Cancel();

            foreach (var counter in _gpuCounters)
                counter.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
