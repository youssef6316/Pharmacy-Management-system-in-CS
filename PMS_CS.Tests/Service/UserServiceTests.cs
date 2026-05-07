using PMS_CS.src.Services;
using PMS_CS.Tests.Helpers;

namespace PMS_CS.Tests.Services;

public class UserServiceTests : TestDatabase
{
    private readonly UserService _service = new();

    // ── RegisterPatient ───────────────────────────────────────────────────

    [Fact]
    public void RegisterPatient_ValidInput_ReturnsPositiveId()
    {
        var (id, error) = _service.RegisterPatient(
            "newpatient", "pass123", "np@test.com", "0501234567", 25, "Cairo");

        Assert.True(id > 0);
        Assert.Empty(error);
    }

    [Fact]
    public void RegisterPatient_DuplicateUsername_ReturnsError()
    {
        _service.RegisterPatient("alice", "pass123", "a@test.com", "111", 30, "Cairo");

        var (id, error) = _service.RegisterPatient(
            "alice", "passcode123", "b@test.com", "222", 22, "Cairo");

        Assert.Equal(-1, id);
        Assert.Contains("taken", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RegisterPatient_ShortPassword_ReturnsError()
    {
        var (id, error) = _service.RegisterPatient(
            "bob", "123", "bob@test.com", "111", 30, "Cairo");

        Assert.Equal(-1, id);
        Assert.Contains("6 characters", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RegisterPatient_InvalidEmail_ReturnsError()
    {
        var (id, error) = _service.RegisterPatient(
            "carol", "pass123", "not-an-email", "111", 30, "Cairo");

        Assert.Equal(-1, id);
        Assert.Contains("email", error, StringComparison.OrdinalIgnoreCase);
    }

    // ── Login ─────────────────────────────────────────────────────────────

    [Fact]
    public void Login_CorrectCredentials_ReturnsUser()
    {
        _service.RegisterPatient("dan", "pass123", "dan@test.com", "111", 28, "Cairo");

        var (user, error) = _service.Login("dan", "pass123");

        Assert.NotNull(user);
        Assert.Empty(error);
        Assert.Equal("dan", user.Username);
    }

    [Fact]
    public void Login_WrongPassword_ReturnsError()
    {
        _service.RegisterPatient("eve", "pass123", "eve@test.com", "111", 28, "Cairo");

        var (user, error) = _service.Login("eve", "wrongpass");

        Assert.Null(user);
        Assert.NotEmpty(error);
    }

    [Fact]
    public void Login_EmptyFields_ReturnsError()
    {
        var (user, error) = _service.Login("", "");

        Assert.Null(user);
        Assert.NotEmpty(error);
    }
}