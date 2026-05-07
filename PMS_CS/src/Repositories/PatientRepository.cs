/*
To be added: 
1. Delete Patient and the Patient's tuple from User table as well.
*/
using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class PatientRepository
{
    // ── CREATE ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a PATIENT row. The UserId must already exist in USER.
    /// Call UserRepository.AddUser() first, then this.
    /// </summary>
    public bool AddPatient(Patient patient)
    {
        const string query = @"
            INSERT INTO PATIENT (UserID, Age, Address, PatientBalance)
            VALUES (@UserId, @Age, @Address, @Balance)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId",  patient.UserId);
        cmd.Parameters.AddWithValue("@Age",     patient.Age);
        cmd.Parameters.AddWithValue("@Address", patient.Address);
        cmd.Parameters.AddWithValue("@Balance", patient.PatientBalance);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public Patient? GetPatientById(int userId)
    {
        // JOIN with USER in one query so we get name/email too.
        // This populates patient.UserInfo — the navigation property.
        const string query = @"
            SELECT p.UserID, p.Age, p.Address, p.PatientBalance,
                   u.Username, u.Email, u.Phone, u.IsActive
            FROM PATIENT p
            INNER JOIN [USER] u ON p.UserID = u.UserID
            WHERE p.UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var patient = MapPatient(reader);
            // Load the allergies in a second query — reader is still open
            // so we must close it first before reusing the connection.
            // We do this after the using block by loading separately.
            patient.Allergies = GetAllergies(userId);
            return patient;
        }
        return null;
    }

    public List<Patient> GetAllPatients()
    {
        const string query = @"
            SELECT p.UserID, p.Age, p.Address, p.PatientBalance,
                   u.Username, u.Email, u.Phone, u.IsActive
            FROM PATIENT p
            INNER JOIN [USER] u ON p.UserID = u.UserID
            ORDER BY u.Username";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var patients = new List<Patient>();
        while (reader.Read())
            patients.Add(MapPatient(reader));

        return patients;
        // Note: allergies are not bulk-loaded here for performance.
        // Call GetAllergies(patientId) individually when a single
        // patient's full profile is needed.
    }

    // ── UPDATE ────────────────────────────────────────────────────────────

    public bool UpdatePatient(Patient patient)
    {
        const string query = @"
            UPDATE PATIENT
            SET Age            = @Age,
                Address        = @Address,
                PatientBalance = @Balance
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Age",     patient.Age);
        cmd.Parameters.AddWithValue("@Address", patient.Address);
        cmd.Parameters.AddWithValue("@Balance", patient.PatientBalance);
        cmd.Parameters.AddWithValue("@UserId",  patient.UserId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool UpdateBalance(int userId, double newBalance)
    {
        const string query = @"
            UPDATE PATIENT
            SET PatientBalance = @Balance
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Balance", newBalance);
        cmd.Parameters.AddWithValue("@UserId",  userId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── ALLERGY MANAGEMENT ────────────────────────────────────────────────

    public List<string> GetAllergies(int userId)
    {
        const string query = @"
            SELECT AllergyName
            FROM PATIENT_ALLERGY
            WHERE UserID = @UserId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var allergies = new List<string>();
        while (reader.Read())
            allergies.Add(reader["AllergyName"].ToString()!);

        return allergies;
    }

    public bool AddAllergy(int userId, string allergyName)
    {
        const string query = @"
            IF NOT EXISTS (
                SELECT 1 FROM PATIENT_ALLERGY
                WHERE UserID = @UserId AND AllergyName = @Allergy
            )
            INSERT INTO PATIENT_ALLERGY (UserID, AllergyName)
            VALUES (@UserId, @Allergy)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId",  userId);
        cmd.Parameters.AddWithValue("@Allergy", allergyName);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool RemoveAllergy(int userId, string allergyName)
    {
        const string query = @"
            DELETE FROM PATIENT_ALLERGY
            WHERE UserID = @UserId AND AllergyName = @Allergy";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId",  userId);
        cmd.Parameters.AddWithValue("@Allergy", allergyName);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── PRIVATE MAPPING ───────────────────────────────────────────────────

    private static Patient MapPatient(SqlDataReader reader)
    {
        var patient = new Patient
        {
            UserId         = Convert.ToInt32(reader["UserID"]),
            Age            = Convert.ToSingle(reader["Age"]),
            Address        = reader["Address"].ToString()!,
            PatientBalance = Convert.ToDouble(reader["PatientBalance"])
        };

        // Populate the navigation property from the JOIN columns.
        patient.UserInfo = new User
        {
            UserId   = patient.UserId,
            Username = reader["Username"].ToString()!,
            Email    = reader["Email"].ToString()!,
            Phone    = reader["Phone"].ToString()!,
            IsActive = Convert.ToBoolean(reader["IsActive"])
        };

        return patient;
    }
}