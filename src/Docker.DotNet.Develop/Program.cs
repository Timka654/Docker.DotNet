using Docker.DotNet.Models;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        RestartPolicy restartPolicy = new()
        {
            Name = RestartPolicyKind.OnFailure,
            MaximumRetryCount = 5
        };


        Docker.DotNet.JsonSerializer js = new Docker.DotNet.JsonSerializer();

        var t = Encoding.UTF8.GetString(js.SerializeObject(restartPolicy));

    }
}