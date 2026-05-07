using Microsoft.Data.SqlClient;

namespace PMS_CS.Tests.Helpers;

/// <summary>
/// Base class for all integration tests.
/// Points to PharmacyDB_Test and provides cleanup helpers so each test
/// starts with a known-clean state.
/// Change the connection string to match your machine.
/// </summary>
public abstract class TestDatabase : IDisposable
{
    // ── Test DB connection string ─────────────────────────────────────────
    // Same format as DBConnection.cs but pointing at PharmacyDB_Test.
    protected const string TestConnectionString =
        @"Server=YOUSSEF631;Database=PharmacyDB_Test;" +
        "Integrated Security=True;TrustServerCertificate=True;";

    // ── Called by every test class constructor ────────────────────────────
    protected TestDatabase()
    {
        // Override DBConnection to use the test DB.
        // We do this by patching the static field before each test run.
        OverrideConnectionString(TestConnectionString);
        CleanAllTables();
    }

    // ── Wipes all tables in dependency order after each test ──────────────
    // IDisposable.Dispose() is called automatically by xUnit after each test.
    public void Dispose()
    {
        CleanAllTables();
    }

    // ── Helpers any test can call to insert seed data ─────────────────────

    protected int SeedUser(string username = "testuser",
                           string password = "pass123",
                           string email    = "test@test.com",
                           string phone    = "0000000000")
    {
        using var conn = new SqlConnection(TestConnectionString);
        using var cmd  = new SqlCommand(@"
            INSERT INTO [USER] (Username, Password, Email, Phone, IsActive)
            OUTPUT INSERTED.UserID
            VALUES (@u, @p, @e, @ph, 1)", conn);
        cmd.Parameters.AddWithValue("@u",  username);
        cmd.Parameters.AddWithValue("@p",  password);
        cmd.Parameters.AddWithValue("@e",  email);
        cmd.Parameters.AddWithValue("@ph", phone);
        conn.Open();
        return (int)cmd.ExecuteScalar()!;
    }

    protected int SeedPatient(int userId, float age = 30,
                               string address = "123 Main St",
                               double balance = 500.0)
    {
        using var conn = new SqlConnection(TestConnectionString);
        using var cmd  = new SqlCommand(@"
            INSERT INTO PATIENT (UserID, Age, Address, PatientBalance)
            VALUES (@uid, @age, @addr, @bal)", conn);
        cmd.Parameters.AddWithValue("@uid",  userId);
        cmd.Parameters.AddWithValue("@age",  age);
        cmd.Parameters.AddWithValue("@addr", address);
        cmd.Parameters.AddWithValue("@bal",  balance);
        conn.Open();
        cmd.ExecuteNonQuery();
        return userId;
    }

    protected int SeedEmployee(int userId,
                                string jobType = "Pharmacist",
                                double salary  = 3000.0)
    {
        using var conn = new SqlConnection(TestConnectionString);
        using var cmd  = new SqlCommand(@"
            INSERT INTO EMPLOYEE (UserID, Salary, JobType)
            VALUES (@uid, @sal, @jt)", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@sal", salary);
        cmd.Parameters.AddWithValue("@jt",  jobType);
        conn.Open();
        cmd.ExecuteNonQuery();
        return userId;
    }

    protected int SeedMedicine(string name     = "Aspirin",
                                double price    = 10.0,
                                int    stock    = 100,
                                string category = "Painkiller",
                                string expiry   = "2099-01-01",
                                bool   refund   = true)
    {
        using var conn = new SqlConnection(TestConnectionString);
        using var cmd  = new SqlCommand(@"
            INSERT INTO MEDICINE
                (Name, Price, Category, ExpiryDate,
                 StockQuantity, UsageInstructions, IsRefundable)
            OUTPUT INSERTED.MedicineID
            VALUES (@n, @pr, @cat, @exp, @stk, @use, @ref)", conn);
        cmd.Parameters.AddWithValue("@n",   name);
        cmd.Parameters.AddWithValue("@pr",  price);
        cmd.Parameters.AddWithValue("@cat", category);
        cmd.Parameters.AddWithValue("@exp", expiry);
        cmd.Parameters.AddWithValue("@stk", stock);
        cmd.Parameters.AddWithValue("@use", "Take as needed.");
        cmd.Parameters.AddWithValue("@ref", refund);
        conn.Open();
        return (int)cmd.ExecuteScalar()!;
    }

    // ── Private: wipes all rows respecting FK order ───────────────────────
    private void CleanAllTables()
    {
        using var conn = new SqlConnection(TestConnectionString);
        conn.Open();

        // Delete in reverse FK dependency order.
        var tables = new[]
        {
            "PAYMENT", "ORDER_ITEM", "[ORDER]",
            "PRESCRIPTION_ITEM", "PRESCRIPTION",
            "MED_SIDE_EFFECT", "MED_HEALING_EFFECT", "MEDICINE",
            "PATIENT_ALLERGY", "PATIENT",
            "EMPLOYEE", "USER_ROLE", "ROLE", "[USER]"
        };

        foreach (var table in tables)
        {
            using var cmd = new SqlCommand($"DELETE FROM {table}", conn);
            cmd.ExecuteNonQuery();
        }
    }

    // ── Redirects DBConnection to the test database ───────────────────────
    private static void OverrideConnectionString(string connStr)
    {
        // We access the private field via reflection so we don't have to
        // change the production DBConnection.cs at all.
        // Reflection is normally discouraged in production code — it is
        // acceptable here only because this is test infrastructure.
        var field = typeof(PMS_CS.Database.DBConnection)
            .GetField("ConnectionString",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

        field?.SetValue(null, connStr);
    }
}