using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Delta.Functions;

/// <summary>
/// Health check endpoint — verifies the Azure Functions host is running.
/// GET /api/health → 200 OK
/// </summary>
public class HealthCheck
{
    [Function("Health")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]
        HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString("""{"status":"ok","service":"delta-backend"}""");
        return response;
    }
}
