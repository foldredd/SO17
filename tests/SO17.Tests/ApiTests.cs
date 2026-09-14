using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Application.Orders;
using SO17.Infrastructure.Persistence;
using Xunit;

namespace SO17.Tests;

public sealed class ApiTests(ApiFixture app) : IClassFixture<ApiFixture>
{
    private HttpClient Client => app.Client;
    private static string Suffix => Guid.NewGuid().ToString("N")[..10];
    private async Task<JsonElement> Create(string path, object body)
    {
        using var response = await ApiFixture.SendAsync(Client, HttpMethod.Post, path, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
    private Task<JsonElement> Customer() => Create("/api/customers", new { name = "Иван", surname = "Петров", phone = "+38067" + Random.Shared.NextInt64(1000000, 9999999) });
    private Task<JsonElement> Item(int quantity = 10, decimal price = 125.50m) => Create("/api/items", new { title = "Товар " + Suffix, sku = "SKU-" + Suffix, quantity, price });
    private static object Lines(JsonElement customer, JsonElement item, int quantity = 2) => new { customerId = customer.GetProperty("id").GetInt32(), lines = new[] { new { itemId = item.GetProperty("id").GetInt32(), quantity } } };

    [Fact]
    public async Task OpenApiAndDatabaseHealthAreAvailable()
    {
        Assert.Equal("Healthy", await Client.GetStringAsync("/health/ready"));
        var document = await Client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        Assert.True(document.GetProperty("paths").TryGetProperty("/api/orders", out _));
        Assert.Contains("\"Pending\"", document.GetRawText());
        Assert.Contains("\"Cancelled\"", document.GetRawText());
    }

    [Theory]
    [InlineData("/api/customers")]
    [InlineData("/api/items")]
    [InlineData("/api/orders")]
    [InlineData("/api/dashboard")]
    [InlineData("/api/auth/me")]
    public async Task AnonymousAccessIsDenied(string path)
    {
        using var anonymous = app.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task CsrfRequiredEvenForLogin()
    {
        using var anonymous = app.CreateClient();
        using var response = await anonymous.PostAsJsonAsync("/api/auth/login", new { login = app.Login, password = app.Password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("csrf_invalid", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ValidationAndMalformedJsonReturn400()
    {
        using var badItem = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/items", new { title = " ", sku = "bad", quantity = -1, price = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, badItem.StatusCode);
        using var tokenResponse = await Client.GetAsync("/api/auth/csrf");
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/customers") { Content = new StringContent("{broken", Encoding.UTF8, "application/json") };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.SendAsync(request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/orders?pageSize=1000")).StatusCode);
        using var nullLines = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/orders", new { customerId = 1, lines = new object?[] { null } });
        Assert.Equal(HttpStatusCode.BadRequest, nullLines.StatusCode);
        using var fractionalPrice = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/items", new { title = "Товар", sku = Suffix, quantity = 10, price = 1.123m });
        Assert.Equal(HttpStatusCode.BadRequest, fractionalPrice.StatusCode);
    }

    [Fact]
    public async Task CustomerCrudUniquePhoneAndStaleWriteProtection()
    {
        var customer = await Customer();
        var id = customer.GetProperty("id").GetInt32();
        var phone = customer.GetProperty("phone").GetString();
        using var duplicate = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/customers", new { name = "Иван", surname = "Петров", phone });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var update = new { name = "Пётр", surname = "Петров", phone, version = customer.GetProperty("version").GetGuid() };
        using var first = await ApiFixture.SendAsync(Client, HttpMethod.Put, "/api/customers/" + id, update);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        using var stale = await ApiFixture.SendAsync(Client, HttpMethod.Put, "/api/customers/" + id, update);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var current = await first.Content.ReadFromJsonAsync<JsonElement>();
        using var deleted = await ApiFixture.SendAsync(Client, HttpMethod.Delete, $"/api/customers/{id}?version={current.GetProperty("version").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/customers/" + id)).StatusCode);
    }

    [Fact]
    public async Task CatalogCrudAndUniqueSku()
    {
        var item = await Item();
        var id = item.GetProperty("id").GetInt32();
        var update = new { title = "Обновлённый", sku = item.GetProperty("sku").GetString()!.ToLowerInvariant(), quantity = 11, price = 99m, version = item.GetProperty("version").GetGuid() };
        using var duplicate = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/items", update);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var changed = await ApiFixture.SendAsync(Client, HttpMethod.Put, "/api/items/" + id, update);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var current = await changed.Content.ReadFromJsonAsync<JsonElement>();
        using var deleted = await ApiFixture.SendAsync(Client, HttpMethod.Delete, $"/api/items/{id}?version={current.GetProperty("version").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task OrderKeepsPriceSnapshotAndCancellationRestoresStockExactlyOnce()
    {
        var customer = await Customer(); var item = await Item();
        var order = await Create("/api/orders", Lines(customer, item));
        Assert.Equal(251m, order.GetProperty("total").GetDecimal());
        var itemId = item.GetProperty("id").GetInt32(); var id = order.GetProperty("id").GetInt32();
        var stock = await Client.GetFromJsonAsync<JsonElement>("/api/items/" + itemId);
        Assert.Equal(8, stock.GetProperty("quantity").GetInt32());
        using var changed = await ApiFixture.SendAsync(Client, HttpMethod.Put, "/api/items/" + itemId, new { title = "Другая цена", sku = stock.GetProperty("sku").GetString(), quantity = 8, price = 999m, version = stock.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var snapshot = await Client.GetFromJsonAsync<JsonElement>("/api/orders/" + id);
        Assert.Equal(251m, snapshot.GetProperty("total").GetDecimal());
        using var linkedDelete = await ApiFixture.SendAsync(Client, HttpMethod.Delete, $"/api/customers/{customer.GetProperty("id")}?version={customer.GetProperty("version").GetGuid()}");
        Assert.Equal(HttpStatusCode.Conflict, linkedDelete.StatusCode);
        var cancel = new { status = "Cancelled", version = order.GetProperty("version").GetGuid() };
        using var cancelled = await ApiFixture.SendAsync(Client, HttpMethod.Patch, $"/api/orders/{id}/status", cancel);
        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        using var twice = await ApiFixture.SendAsync(Client, HttpMethod.Patch, $"/api/orders/{id}/status", cancel);
        Assert.Equal(HttpStatusCode.Conflict, twice.StatusCode);
        stock = await Client.GetFromJsonAsync<JsonElement>("/api/items/" + itemId);
        Assert.Equal(10, stock.GetProperty("quantity").GetInt32());
    }

    [Fact]
    public async Task InvalidOrderIsAtomicAndCompletedOrderCannotBeCancelled()
    {
        var customer = await Customer(); var item = await Item(2);
        using var beforeScope = app.Services.CreateScope();
        var before = await beforeScope.ServiceProvider.GetRequiredService<AppDbContext>().Orders.CountAsync();
        using var insufficient = await ApiFixture.SendAsync(Client, HttpMethod.Post, "/api/orders", Lines(customer, item, 3));
        Assert.Equal(HttpStatusCode.Conflict, insufficient.StatusCode);
        using var afterScope = app.Services.CreateScope();
        Assert.Equal(before, await afterScope.ServiceProvider.GetRequiredService<AppDbContext>().Orders.CountAsync());
        var order = await Create("/api/orders", Lines(customer, item, 1));
        var id = order.GetProperty("id").GetInt32();
        using var skip = await ApiFixture.SendAsync(Client, HttpMethod.Patch, $"/api/orders/{id}/status", new { status = "Completed", version = order.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.Conflict, skip.StatusCode);
        foreach (var status in new[] { "Processing", "Completed" })
        {
            using var response = await ApiFixture.SendAsync(Client, HttpMethod.Patch, $"/api/orders/{id}/status", new { status, version = order.GetProperty("version").GetGuid() });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            order = await response.Content.ReadFromJsonAsync<JsonElement>();
        }
        using var cancelled = await ApiFixture.SendAsync(Client, HttpMethod.Patch, $"/api/orders/{id}/status", new { status = "Cancelled", version = order.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.Conflict, cancelled.StatusCode);
        var search = await Client.GetFromJsonAsync<JsonElement>("/api/orders?search=" + id);
        Assert.Contains(search.GetProperty("items").EnumerateArray(), x => x.GetProperty("id").GetInt32() == id);
    }

    [Fact]
    public async Task ConcurrentOrdersCannotOversellAndFailedTransactionLeavesNoOrder()
    {
        var customer = await Customer(); var item = await Item(1);
        await using var scope1 = app.Services.CreateAsyncScope();
        await using var scope2 = app.Services.CreateAsyncScope();
        var id = item.GetProperty("id").GetInt32();
        // Both units of work observe the same stock/version before either commits.
        await scope1.ServiceProvider.GetRequiredService<IItemRepository>().FindAsync(id, default);
        await scope2.ServiceProvider.GetRequiredService<IItemRepository>().FindAsync(id, default);
        var request = new CreateOrderRequest { CustomerId = customer.GetProperty("id").GetInt32(), Lines = [new() { ItemId = id, Quantity = 1 }] };
        await scope1.ServiceProvider.GetRequiredService<IOrderService>().CreateAsync(request, app.UserId, default);
        var error = await Assert.ThrowsAsync<AppException>(() => scope2.ServiceProvider.GetRequiredService<IOrderService>().CreateAsync(request, app.UserId, default));
        Assert.Equal(409, error.StatusCode);
        await using var verification = app.Services.CreateAsyncScope();
        var db = verification.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0, (await db.Items.FindAsync(id))!.Quantity);
        Assert.Equal(1, await db.OrderLines.CountAsync(x => x.ItemId == id));
    }

    [Fact]
    public async Task LoginProfilePasswordRotationLogoutAndSafeCookies()
    {
        using var first = app.CreateClient(); using var second = app.CreateClient();
        var login = "auth_" + Suffix;
        var password = "P@ssw0rd!" + Suffix;
        var registration = new { login, email = login + "@example.test", password, name = "Анна", surname = "Тест", phone = "+380671234567", invitationCode = app.Invitation };
        using var created = await ApiFixture.SendAsync(first, HttpMethod.Post, "/api/auth/register", registration);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var text = await created.Content.ReadAsStringAsync();
        Assert.DoesNotContain("hash", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(password, text);
        Assert.Contains(created.Headers.GetValues("Set-Cookie"), x => x.Contains("so17.session=") && x.Contains("httponly", StringComparison.OrdinalIgnoreCase) && x.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));
        using var logged = await ApiFixture.SendAsync(second, HttpMethod.Post, "/api/auth/login", new { login, password });
        Assert.Equal(HttpStatusCode.OK, logged.StatusCode);
        using var profile = await ApiFixture.SendAsync(first, HttpMethod.Put, "/api/auth/profile", new { name = "Мария", surname = "Тест", phone = "+380 (67) 111-22-33" });
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        using var changed = await ApiFixture.SendAsync(first, HttpMethod.Put, "/api/auth/password", new { currentPassword = password, newPassword = password + "New1!" });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await second.GetAsync("/api/auth/me")).StatusCode);
        using var logout = await ApiFixture.SendAsync(first, HttpMethod.Post, "/api/auth/logout");
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await first.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task WrongInvitationAndRepeatedBadPasswordsAreRejected()
    {
        using var client = app.CreateClient();
        var login = "lock_" + Suffix; var password = "P@ssw0rd!" + Suffix;
        using var wrong = await ApiFixture.SendAsync(client, HttpMethod.Post, "/api/auth/register", new { login, email = login + "@example.test", password, name = "Тест", surname = "Тест", phone = "+380671112233", invitationCode = "wrong" });
        Assert.Equal(HttpStatusCode.Forbidden, wrong.StatusCode);
        using var created = await ApiFixture.SendAsync(client, HttpMethod.Post, "/api/auth/register", new { login, email = login + "@example.test", password, name = "Тест", surname = "Тест", phone = "+380671112233", invitationCode = app.Invitation });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        using var anonymous = app.CreateClient();
        for (var i = 0; i < 5; i++)
        {
            using var wrongPassword = await ApiFixture.SendAsync(anonymous, HttpMethod.Post, "/api/auth/login", new { login, password = "wrong" });
            Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        }
        using var locked = await ApiFixture.SendAsync(anonymous, HttpMethod.Post, "/api/auth/login", new { login, password });
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
    }
}
