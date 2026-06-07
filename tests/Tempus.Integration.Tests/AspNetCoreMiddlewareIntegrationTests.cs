using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tempus.AspNetCore.Context;
using Tempus.AspNetCore.DependencyInjection;
using Tempus.AspNetCore.Extensions;
using Tempus.Core.DependencyInjection;

namespace Tempus.Integration.Tests;

public class AspNetCoreMiddlewareIntegrationTests : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly HttpClient _client;

    public AspNetCoreMiddlewareIntegrationTests()
    {
        _host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddLogging(lb => lb.ClearProviders());
                    services.AddTempus();
                    services.AddTempusAspNetCore(o => o.FallbackTimeZone = "Etc/UTC");
                    services.AddRouting();
                });
                webHost.Configure(app =>
                {
                    app.UseTempusTimezone();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/tz", httpCtx =>
                        {
                            var ctx = httpCtx.RequestServices.GetRequiredService<ITempusUserContext>();
                            return httpCtx.Response.WriteAsync(ctx.TimeZoneId);
                        });
                    });
                });
            })
            .Build();

        _host.Start();
        _client = _host.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
    }

    [Fact]
    public async Task XTimezoneHeader_ResolvesCorrectly()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/tz");
        request.Headers.Add("X-Timezone", "America/New_York");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("America/New_York");
    }

    [Fact]
    public async Task QueryParam_ResolvesCorrectly()
    {
        var response = await _client.GetAsync("/tz?tz=Europe/London");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Europe/London");
    }

    [Fact]
    public async Task NoTimezone_FallsBackToConfiguredDefault()
    {
        var response = await _client.GetAsync("/tz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Etc/UTC");
    }

    [Fact]
    public async Task InvalidTimezone_DoesNotReturn5xx()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/tz");
        request.Headers.Add("X-Timezone", "Not/A/Valid/Zone");

        var response = await _client.SendAsync(request);

        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    public async Task Header_TakesPrecedenceOver_QueryParam()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/tz?tz=Europe/London");
        request.Headers.Add("X-Timezone", "Asia/Tokyo");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Asia/Tokyo");
    }
}
