using k8s.LeaderElection.ResourceLock;
using k8s.LeaderElection;
using k8s;
using Microsoft.Extensions.Logging;

namespace K8SLeaderElection;

public class K8SLeaderElectionClient {
    private const string LOCK_NAME = "leader-lock";
    private readonly ILogger<K8SLeaderElectionClient> _logger;
    private readonly KubernetesClientConfiguration _kubeCfg;

    public K8SLeaderElectionClient() {
        using var loggerFactory = LoggerFactory.Create(builder => builder
            .SetMinimumLevel(LogLevel.Trace).AddConsole()
        );
        _logger = loggerFactory.CreateLogger<K8SLeaderElectionClient>();
        _kubeCfg = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig();
        _kubeCfg.Namespace = string.IsNullOrEmpty(_kubeCfg.Namespace) ? "default" : _kubeCfg.Namespace;
        _logger.LogDebug(@$"<---k8s client config file--->{GetConfigData()}");
        SetHostName();
    }

    private string GetConfigData() {
        return typeof(KubernetesClientConfiguration).GetProperties().Aggregate("",
            (current, property) => current + $"\n{property.Name}:{property.GetValue(_kubeCfg)}");
    }

    private static void SetHostName() {
        Environment.SetEnvironmentVariable("HOSTNAME", Guid.NewGuid().ToString());
    }

    private static string? GetHostName() {
        return Environment.GetEnvironmentVariable("HOSTNAME");
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        _logger.LogDebug("Starting Leader Election..");
        var client = new Kubernetes(_kubeCfg);
        var leaderLock = new LeaseLock(client, _kubeCfg.Namespace, LOCK_NAME, GetHostName());

        var config = new LeaderElectionConfig(leaderLock)
        {
            LeaseDuration = TimeSpan.FromSeconds(15),
            RenewDeadline = TimeSpan.FromSeconds(10),
            RetryPeriod = TimeSpan.FromSeconds(2)
        };

        var leaderElector = new LeaderElector(config);
        leaderElector.OnStartedLeading += () => _logger.LogDebug("Got Leader Lock");
        leaderElector.OnNewLeader += leader => _logger.LogDebug($"Leader changed to: {leader}");
        leaderElector.OnStoppedLeading += () => _logger.LogDebug("Lost Leader Lock");

        var leaderElectorTask = Task.Run(async () => {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    await leaderElector.RunAsync(cancellationToken);
                } catch (TaskCanceledException) {
                    _logger.LogDebug("Task canceled, exiting...");
                } catch (Exception ex) {
                    _logger.LogError(ex.ToString());
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }, cancellationToken);

        await leaderElectorTask;
    }
}