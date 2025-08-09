using InfluxDB.Client;
using Microsoft.Extensions.Configuration;

namespace DetectionData;

public class DetectionDbContext : IDisposable
{
    public InfluxDBClient Client { get; }
    public string Bucket { get; }
    public string Org { get; }

    public DetectionDbContext(InfluxDbSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Url)) throw new ArgumentNullException(nameof(settings.Url));
        if (string.IsNullOrWhiteSpace(settings.Token)) throw new ArgumentNullException(nameof(settings.Token));
        if (string.IsNullOrWhiteSpace(settings.Bucket)) throw new ArgumentNullException(nameof(settings.Bucket));
        if (string.IsNullOrWhiteSpace(settings.Org)) throw new ArgumentNullException(nameof(settings.Org));

        Bucket = settings.Bucket;
        Org = settings.Org;
        Client = InfluxDBClientFactory.Create(settings.Url, settings.Token.ToCharArray());
    }

    public void Dispose() => Client.Dispose();
}
