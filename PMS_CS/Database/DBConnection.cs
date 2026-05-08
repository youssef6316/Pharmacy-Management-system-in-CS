using Microsoft.Data.SqlClient;

namespace PMS_CS.Database;

public static class DBConnection
{
    // ── Change these three values to match your SSMS setup ──────────────
    private const string Server   = "YOUSSEF631";
    private const string Database = "PharmacyDB";
    // ────────────────────────────────────────────────────────────────────

    private static string ConnectionString =
        $"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}