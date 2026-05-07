using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class UserRepository
{
    // ── CREATE ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new user into the USER table.
    /// Returns the new UserId assigned by IDENTITY, or -1 on failure.
    /// </summary>
    public int AddUser(User user)
    {
        // The query asks SQL Server to return the new auto-generated ID
        // immediately after the INSERT using OUTPUT INSERTED.UserID.
        // This is the ADO.NET way to get the IDENTITY value back.
        const string query = @"
            INSERT INTO [USER] (Username, Password, Email, Phone, IsActive)
            OUTPUT INSERTED.UserID
            VALUES (@Username, @Password, @Email, @Phone, @IsActive)";

        // 'using' ensures the connection is always closed — even on exception.
        // This is equivalent to Java's try-with-resources on a Connection.
        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);

        // Parameters — NEVER build SQL strings with + concatenation.
        // Concatenation opens your project to SQL Injection attacks.
        // SqlParameter sends the value separately from the query text,
        // so the DB treats it as data only, never as executable SQL.
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@Password", user.Password);
        cmd.Parameters.AddWithValue("@Email",    user.Email);
        cmd.Parameters.AddWithValue("@Phone",    user.Phone);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);

        conn.Open();

        // ExecuteScalar() returns the single value produced by OUTPUT INSERTED.
        // It returns object? so we cast it to int.
        var result = cmd.ExecuteScalar();
        return result != null ? (int)result : -1;
    }

    // ── READ (single) ─────────────────────────────────────────────────────

    /// <summary>
    /// Finds a user by their primary key.
    /// Returns null if no user with that ID exists.
    /// </summary>
    public User? GetUserById(int userId)
    {
        const string query = @"
            SELECT UserID, Username, Password, Email, Phone, IsActive
            FROM [USER]
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();

        // ExecuteReader() streams rows back one at a time.
        // Think of SqlDataReader like Java's ResultSet.
        using var reader = cmd.ExecuteReader();

        // reader.Read() advances to the next row and returns false when
        // there are no more rows. For a single-row lookup we call it once.
        if (reader.Read())
            return MapUser(reader);   // see private helper at the bottom

        return null;   // no row found — caller must handle the null
    }

    /// <summary>
    /// Finds a user by username. Used at login.
    /// </summary>
    public User? GetUserByUsername(string username)
    {
        const string query = @"
            SELECT UserID, Username, Password, Email, Phone, IsActive
            FROM [USER]
            WHERE Username = @Username";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Username", username);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapUser(reader);

        return null;
    }

    // ── READ (collection) ─────────────────────────────────────────────────

    /// <summary>
    /// Returns all users in the system. Used by Admin views.
    /// </summary>
    public List<User> GetAllUsers()
    {
        const string query = @"
            SELECT UserID, Username, Password, Email, Phone, IsActive
            FROM [USER]
            ORDER BY Username";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var users = new List<User>();

        // For a multi-row result we keep calling reader.Read() in a while
        // loop — each call moves to the next row until there are none left.
        while (reader.Read())
            users.Add(MapUser(reader));

        return users;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the editable fields of an existing user.
    /// Returns true if exactly one row was affected.
    /// </summary>
    public bool UpdateUser(User user)
    {
        const string query = @"
            UPDATE [USER]
            SET Username = @Username,
                Email    = @Email,
                Phone    = @Phone,
                IsActive = @IsActive
            WHERE UserID = @UserId";

        // Note: Password is intentionally excluded here.
        // Passwords are changed through a dedicated ChangePassword() method
        // below so that callers are always explicit about that sensitive operation.

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@Email",    user.Email);
        cmd.Parameters.AddWithValue("@Phone",    user.Phone);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        cmd.Parameters.AddWithValue("@UserId",   user.UserId);

        conn.Open();

        // ExecuteNonQuery() is used for INSERT, UPDATE, DELETE.
        // It returns the number of rows affected — not a result set.
        return cmd.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Changes a user's password. Kept separate from UpdateUser deliberately.
    /// </summary>
    public bool ChangePassword(int userId, string newPassword)
    {
        const string query = @"
            UPDATE [USER]
            SET Password = @Password
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Password", newPassword);
        cmd.Parameters.AddWithValue("@UserId",   userId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── DELETE (soft) ─────────────────────────────────────────────────────

    /// <summary>
    /// Deactivates a user instead of deleting their row.
    /// Hard deletes break ORDER/PRESCRIPTION history (FK constraints).
    /// </summary>
    public bool DeactivateUser(int userId)
    {
        const string query = @"
            UPDATE [USER]
            SET IsActive = 0
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── ROLE MANAGEMENT ───────────────────────────────────────────────────

    /// <summary>
    /// Links a role to a user via the USER_ROLE join table.
    /// </summary>
    public bool AddRoleToUser(int userId, int roleId)
    {
        const string query = @"
            IF NOT EXISTS (
                SELECT 1 FROM USER_ROLE
                WHERE UserID = @UserId AND RoleID = @RoleId
            )
            INSERT INTO USER_ROLE (UserID, RoleID)
            VALUES (@UserId, @RoleId)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@RoleId", roleId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Removes a role link from USER_ROLE.
    /// </summary>
    public bool RemoveRoleFromUser(int userId, int roleId)
    {
        const string query = @"
            DELETE FROM USER_ROLE
            WHERE UserID = @UserId AND RoleID = @RoleId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@RoleId", roleId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Loads all roles assigned to a user and populates user.Roles.
    /// Call this when you need a fully loaded user object.
    /// </summary>
    public List<Role> GetRolesForUser(int userId)
    {
        const string query = @"
            SELECT r.RoleID, r.RoleName, r.PermissionsLevel, r.Description
            FROM ROLE r
            INNER JOIN USER_ROLE ur ON r.RoleID = ur.RoleID
            WHERE ur.UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var roles = new List<Role>();
        while (reader.Read())
            roles.Add(MapRole(reader));

        return roles;
    }

    // ── AUTHENTICATION ────────────────────────────────────────────────────

    /// <summary>
    /// Validates login credentials.
    /// Returns the full User object on success, null on failure.
    /// The service layer calls this — it never touches SQL directly.
    /// </summary>
    public User? Authenticate(string username, string password)
    {
        const string query = @"
            SELECT UserID, Username, Password, Email, Phone, IsActive
            FROM [USER]
            WHERE Username = @Username
              AND Password = @Password
              AND IsActive = 1";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@Password", password);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapUser(reader);

        return null;
    }

    // ── PRIVATE MAPPING HELPERS ───────────────────────────────────────────
    // These translate a raw DB row into a clean model object.
    // Every repository has its own Map___() methods — same idea as a
    // ResultSet mapper in Java, but written by hand (no ORM here).

    private static User MapUser(SqlDataReader reader) => new User
    {
        // reader["ColumnName"] reads by column name — safer than index.
        // Convert.ToInt32 handles the object→int cast safely.
        UserId   = Convert.ToInt32(reader["UserID"]),
        Username = reader["Username"].ToString()!,
        Password = reader["Password"].ToString()!,
        Email    = reader["Email"].ToString()!,
        Phone    = reader["Phone"].ToString()!,
        IsActive = Convert.ToBoolean(reader["IsActive"])
        // Roles list is NOT populated here — call GetRolesForUser() when needed.
    };

    private static Role MapRole(SqlDataReader reader) => new Role
    {
        RoleId           = Convert.ToInt32(reader["RoleID"]),
        RoleName         = reader["RoleName"].ToString()!,
        PermissionsLevel = Convert.ToInt32(reader["PermissionsLevel"]),
        Description      = reader["Description"].ToString()!
    };
}