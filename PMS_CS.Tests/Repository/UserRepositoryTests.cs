using PMS_CS.src.Models;
using PMS_CS.src.Repositories;
using PMS_CS.Tests.Helpers;
using Microsoft.Data.SqlClient;

namespace PMS_CS.Tests.Repositories;

// Inheriting TestDatabase gives us CleanAllTables() before and after each test.
public class UserRepositoryTests : TestDatabase
{
    private readonly UserRepository _repo = new();

    // ── AddUser ───────────────────────────────────────────────────────────

    [Fact]
    public void AddUser_ValidUser_ReturnsPositiveId()
    {
        var user = new User("alice", "pass123", "alice@test.com", "111");

        int id = _repo.AddUser(user);

        Assert.True(id > 0);
    }

    [Fact]
    public void AddUser_DuplicateUsername_ThrowsOrReturnsNegative()
    {
        // First insert succeeds.
        var user1 = new User("alice", "pass123", "alice@test.com", "111");
        _repo.AddUser(user1);

        // Second insert with same username violates UNIQUE constraint.
        var user2 = new User("alice", "other", "other@test.com", "222");

        // The DB enforces uniqueness — ADO.NET throws SqlException.
        Assert.Throws<SqlException>(() => _repo.AddUser(user2));
    }

    // ── GetUserById ───────────────────────────────────────────────────────

    [Fact]
    public void GetUserById_ExistingId_ReturnsCorrectUser()
    {
        int id = SeedUser("bob", "pass123", "bob@test.com");

        var user = _repo.GetUserById(id);

        Assert.NotNull(user);
        Assert.Equal("bob", user.Username);
        Assert.Equal("bob@test.com", user.Email);
    }

    [Fact]
    public void GetUserById_NonExistentId_ReturnsNull()
    {
        var user = _repo.GetUserById(99999);

        Assert.Null(user);
    }

    // ── GetUserByUsername ─────────────────────────────────────────────────

    [Fact]
    public void GetUserByUsername_ExistingUser_ReturnsUser()
    {
        SeedUser("carol", "pass123", "carol@test.com");

        var user = _repo.GetUserByUsername("carol");

        Assert.NotNull(user);
        Assert.Equal("carol@test.com", user.Email);
    }

    [Fact]
    public void GetUserByUsername_NonExistentUsername_ReturnsNull()
    {
        var user = _repo.GetUserByUsername("nobody");
        Assert.Null(user);
    }

    // ── Authenticate ──────────────────────────────────────────────────────

    [Fact]
    public void Authenticate_CorrectCredentials_ReturnsUser()
    {
        SeedUser("dan", "secret99", "dan@test.com");

        var user = _repo.Authenticate("dan", "secret99");

        Assert.NotNull(user);
        Assert.Equal("dan", user.Username);
    }

    [Fact]
    public void Authenticate_WrongPassword_ReturnsNull()
    {
        SeedUser("dan", "secret99", "dan@test.com");

        var user = _repo.Authenticate("dan", "wrongpass");

        Assert.Null(user);
    }

    [Fact]
    public void Authenticate_InactiveUser_ReturnsNull()
    {
        // Seed a user then deactivate them.
        int id = SeedUser("inactive_user", "pass123", "i@test.com");
        _repo.DeactivateUser(id);

        var user = _repo.Authenticate("inactive_user", "pass123");

        // Authenticate query filters WHERE IsActive = 1.
        Assert.Null(user);
    }

    // ── UpdateUser ────────────────────────────────────────────────────────

    [Fact]
    public void UpdateUser_ChangesEmail_PersistsToDb()
    {
        int id   = SeedUser("eve", "pass123", "eve@old.com");
        var user = _repo.GetUserById(id)!;
        user.Email = "eve@new.com";

        bool result = _repo.UpdateUser(user);

        var updated = _repo.GetUserById(id);
        Assert.True(result);
        Assert.Equal("eve@new.com", updated!.Email);
    }

    // ── Role management ───────────────────────────────────────────────────

    [Fact]
    public void AddRoleToUser_ThenGetRoles_ReturnsRole()
    {
        int userId = SeedUser("frank", "pass", "frank@test.com");

        // Seed a role directly.
        int roleId = SeedRole("Admin", 10);

        bool added = _repo.AddRoleToUser(userId, roleId);
        var roles  = _repo.GetRolesForUser(userId);

        Assert.True(added);
        Assert.Single(roles);
        Assert.Equal("Admin", roles[0].RoleName);
    }

    [Fact]
    public void AddRoleToUser_Duplicate_NotAddedTwice()
    {
        int userId = SeedUser("grace", "pass", "grace@test.com");
        int roleId = SeedRole("Cashier", 5);

        _repo.AddRoleToUser(userId, roleId);
        _repo.AddRoleToUser(userId, roleId);   // duplicate call

        var roles = _repo.GetRolesForUser(userId);
        Assert.Single(roles);
    }

    // ── Helper: seed a role row ───────────────────────────────────────────
    private int SeedRole(string name, int level)
    {
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(TestConnectionString);
        using var cmd  = new Microsoft.Data.SqlClient.SqlCommand(@"
            INSERT INTO ROLE (RoleName, PermissionsLevel, Description)
            OUTPUT INSERTED.RoleID
            VALUES (@n, @l, '')", conn);
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@l", level);
        conn.Open();
        return (int)cmd.ExecuteScalar()!;
    }
}