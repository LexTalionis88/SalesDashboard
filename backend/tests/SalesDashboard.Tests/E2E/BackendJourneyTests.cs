using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace SalesDashboard.Tests.E2E;

[TestFixture, Category("E2E"), NonParallelizable]
public sealed class BackendJourneyTests
{
    private PostgreSqlContainer postgres = null!;
    private Process? server;
    private HttpClient client = null!;
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> output = new();
    private string apiPath = "";
    private string url = "";

    [OneTimeSetUp]
    public async Task Start()
    {
        postgres = new PostgreSqlBuilder("postgres:17.6-alpine").Build();
        await postgres.StartAsync();
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "backend", "SalesDashboard.slnx"))) root = root.Parent;
        if (root is null) throw new InvalidOperationException("Repository root not found.");
        var configuration = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent!.Name;
        apiPath = Path.Combine(root.FullName, "backend", "src", "SalesDashboard.Api", "bin", configuration, "net10.0", "SalesDashboard.Api.dll");
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        url = $"http://127.0.0.1:{port}";
        client = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(15) };
        await StartServer();
    }

    private async Task StartServer()
    {
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add(apiPath);
        start.ArgumentList.Add("--urls");
        start.ArgumentList.Add(url);
        start.Environment["ConnectionStrings__Sales"] = postgres.GetConnectionString();
        start.Environment["Seed__AnchorDate"] = "2026-09-25";
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["Logging__LogLevel__Default"] = "Warning";
        server = Process.Start(start)!;
        server.OutputDataReceived += (_, e) => { if (e.Data is not null) output.Enqueue(e.Data); };
        server.ErrorDataReceived += (_, e) => { if (e.Data is not null) output.Enqueue(e.Data); };
        server.BeginOutputReadLine();
        server.BeginErrorReadLine();
        for (var i = 0; i < 120; i++)
        {
            if (server.HasExited) throw new InvalidOperationException(string.Join(Environment.NewLine, output));
            try { if ((await client.GetAsync("/api/health/ready")).IsSuccessStatusCode) return; }
            catch (HttpRequestException) { }
            await Task.Delay(250);
        }
        throw new TimeoutException(string.Join(Environment.NewLine, output));
    }

    [Test]
    public async Task User_can_change_period_and_ranking_and_restart_without_changing_data()
    {
        const string query = "/api/dashboard?from=2026-08-27&to=2026-09-25";
        var first = await client.GetStringAsync(query);
        using var dashboard = JsonDocument.Parse(first);
        Assert.That(dashboard.RootElement.GetProperty("kpis").GetProperty("salesCount").GetInt64(), Is.GreaterThan(0));
        Assert.That(dashboard.RootElement.GetProperty("kpis").GetProperty("revenue").ValueKind, Is.EqualTo(JsonValueKind.String));
        using var ranked = JsonDocument.Parse(await client.GetStringAsync(query + "&rankingBy=averageCheck"));
        Assert.That(ranked.RootElement.GetProperty("rankingBy").GetString(), Is.EqualTo("averageCheck"));
        using var empty = JsonDocument.Parse(await client.GetStringAsync("/api/dashboard?from=2000-01-01&to=2000-01-02"));
        Assert.That(empty.RootElement.GetProperty("kpis").GetProperty("bestManager").ValueKind, Is.EqualTo(JsonValueKind.Null));
        using var sales = JsonDocument.Parse(await client.GetStringAsync("/api/sales?from=2026-08-27&to=2026-09-25&limit=5"));
        Assert.That(sales.RootElement.GetProperty("items").GetArrayLength(), Is.EqualTo(5));
        Assert.That((await client.GetAsync("/api/sales?limit=101")).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using var openapi = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        Assert.That(openapi.RootElement.GetProperty("paths").TryGetProperty("/api/dashboard", out _), Is.True);
        await StopServer();
        await StartServer();
        Assert.That(await client.GetStringAsync(query), Is.EqualTo(first));
    }

    private async Task StopServer()
    {
        if (server is null) return;
        if (!server.HasExited) { server.Kill(entireProcessTree: true); await server.WaitForExitAsync(); }
        server.Dispose();
        server = null;
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        await StopServer();
        client?.Dispose();
        if (postgres is not null) await postgres.DisposeAsync();
    }
}

