using System.Linq;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using App.Metrics.AspNetCore.Health;
using System.Threading;
using App.Metrics.Counter;

namespace MetricsDemo
{
    public class Program
    {
        internal static IMetricsRoot Metrics { get; set; }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            Metrics = new MetricsBuilder()
                .Report.ToConsole()
                .OutputMetrics.AsPrometheusPlainText()
                .Configuration.Configure(p =>
                {
                    p.DefaultContextLabel = "Application";
                    p.GlobalTags.Add("app", "MetricsDemo");
                    p.Enabled = true;
                    p.ReportingEnabled = true;
                })
            .Build();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureMetrics(Metrics)
                .ConfigureHealth(
                    builder =>
                    {
                        builder.OutputHealth.AsPlainText();

                        builder.HealthChecks.AddCheck("Database connect", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy("Database Connection OK")));
                        builder.HealthChecks.AddHttpGetCheck("github", new Uri("https://github.com/"), TimeSpan.FromSeconds(1));

                        builder.Report.Using(new MetricHealthReporter(Metrics) { ReportInterval = TimeSpan.FromSeconds(5) });
                    })
                .UseHealth()
                .UseMetrics(
                    options =>
                    {
                        options.EndpointOptions = endpointsOptions =>
                        {
                            endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.First(p => p is MetricsPrometheusTextOutputFormatter);
                        };
                    })
                .UseStartup<Startup>();
        }

        public class MetricHealthReporter : IReportHealthStatus
        {
            private IMetricsRoot _metrics;

            public MetricHealthReporter(IMetricsRoot metrics)
            {
                _metrics = metrics;
            }

            public TimeSpan ReportInterval { get; set; }

            public Task ReportAsync(HealthOptions options, HealthStatus status, CancellationToken cancellationToken = default(CancellationToken))
            {
                foreach (var item in status.Results)
                {
                    _metrics.Measure.Counter.Increment(new CounterOptions
                    {
                        Name = "health",
                        MeasurementUnit = Unit.Calls,
                        Tags = new MetricTags(
                            new [] { "name", "status" }, 
                            new[] { item.Name, item.Check.Status.ToString() })
                    });
                }

                return Task.CompletedTask;
            }
        }
    }
}
