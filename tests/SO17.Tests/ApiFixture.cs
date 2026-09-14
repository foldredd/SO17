using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SO17.Infrastructure.Identity;
using SO17.Infrastructure.Persistence;
using Xunit;

namespace SO17.Tests;

public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public string Invitation { get; } = Guid.NewGuid().ToString("N");
    public string Password { get; } = "T3st!" + Guid.NewGuid().ToString("N");
    public string Login { get; } = "tests_" + Guid.NewGuid().ToString("N");
    public string DatabaseName { get; } = "SO17_Tests_" + Guid.NewGuid().ToString("N");
    public string UserId { get; private set; } = "";
    public HttpClient Client { get; private set; } = null!;
    public string ConnectionString => new SqlConnectionStringBuilder(
        Environment.GetEnvironmentVariable("SO17_TEST_SQL_CONNECTION") ??
        "Server=localhost\\SQLEXPRESS;Integrated Security=true;Encrypt=true;TrustServerCertificate=true")
    { InitialCatalog = DatabaseName }.ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(ConnectionString));
            services.Configure<RegistrationOptions>(options => options.InvitationCode = Invitation);
        });
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var response = await SendAsync(Client, HttpMethod.Post, "/api/auth/register", new
        { login = Login, email = Login + "@example.test", password = Password, name = "Тест", surname = "Сотрудник", phone = "+380671112233", invitationCode = Invitation });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        UserId = user.GetProperty("id").GetString()!;
    }

    public static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, object? body = null)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("token").GetString());
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Client.Dispose();
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // The generated test database is the only allowed cleanup target.
        Assert.Equal(DatabaseName, db.Database.GetDbConnection().Database);
        Assert.StartsWith("SO17_Tests_", DatabaseName);
        await db.Database.EnsureDeletedAsync();
        await DisposeAsync();
    }
}
