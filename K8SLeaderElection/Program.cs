using K8SLeaderElection;

var source = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var client = new K8SLeaderElectionClient();
await client.StartAsync(source.Token);