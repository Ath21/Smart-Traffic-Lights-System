using InfluxDB.Client;
using Microsoft.Extensions.Configuration;

namespace DetectionData;

public class DetectionDbContext : IDisposable
{
    public InfluxDBClient Client { get; }
    public string Bucket { get;  }
    public string Org { get; }

    public DetectionDbContext(IConfiguration configuration)
    {
        var url = configuration["InfluxDb:Url"] ?? throw new ArgumentNullException("InfluxDb:Url not configured");
        var token = configuration["InfluxDb:Token"] ?? throw new ArgumentNullException("InfluxDb:Token not configured");
        Bucket = configuration["InfluxDb:Bucket"] ?? throw new ArgumentNullException("InfluxDb:Bucket not configured");
        Org = configuration["InfluxDb:Org"] ?? throw new ArgumentNullException("InfluxDb:Org not configured");

        Client = InfluxDBClientFactory.Create(url, token.ToCharArray());
    }

    public void Dispose() => Client.Dispose();
}
