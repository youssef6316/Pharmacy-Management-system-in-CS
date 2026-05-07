/*
 To be added:
 1. Read Medicine by name.
 2. Remove Side/Healing effect.
 3. Update Side/Healing effect.
 */

using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class MedicineRepository
{
    // ── CREATE ────────────────────────────────────────────────────────────

    public int AddMedicine(Medicine medicine)
    {
        const string query = @"
            INSERT INTO MEDICINE
                (Name, Price, Category, ExpiryDate,
                 StockQuantity, UsageInstructions, IsRefundable)
            OUTPUT INSERTED.MedicineID
            VALUES
                (@Name, @Price, @Category, @ExpiryDate,
                 @Stock, @Usage, @IsRefundable)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Name",         medicine.Name);
        cmd.Parameters.AddWithValue("@Price",        medicine.Price);
        cmd.Parameters.AddWithValue("@Category",     medicine.Category);
        cmd.Parameters.AddWithValue("@ExpiryDate",   medicine.ExpiryDate);
        cmd.Parameters.AddWithValue("@Stock",        medicine.StockQuantity);
        cmd.Parameters.AddWithValue("@Usage",        medicine.UsageInstructions);
        cmd.Parameters.AddWithValue("@IsRefundable", medicine.IsRefundable);

        conn.Open();
        var result = cmd.ExecuteScalar();

        if (result == null) return -1;

        int newId = (int)result;

        // Insert side effects and healing effects into their own tables.
        // Each is its own row linked by MedicineID.
        foreach (var effect in medicine.SideEffects)
            AddSideEffect(newId, effect);

        foreach (var effect in medicine.HealingEffects)
            AddHealingEffect(newId, effect);

        return newId;
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public Medicine? GetMedicineById(int medicineId)
    {
        const string query = @"
            SELECT MedicineID, Name, Price, Category, ExpiryDate,
                   StockQuantity, UsageInstructions, IsRefundable
            FROM MEDICINE
            WHERE MedicineID = @MedicineId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (!reader.Read()) return null;

        var medicine = MapMedicine(reader);
        reader.Close();   // must close reader before reusing connection

        medicine.SideEffects    = GetSideEffects(medicineId);
        medicine.HealingEffects = GetHealingEffects(medicineId);

        return medicine;
    }

    public List<Medicine> GetAllMedicines()
    {
        const string query = @"
            SELECT MedicineID, Name, Price, Category, ExpiryDate,
                   StockQuantity, UsageInstructions, IsRefundable
            FROM MEDICINE
            ORDER BY Name";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var medicines = new List<Medicine>();
        while (reader.Read())
            medicines.Add(MapMedicine(reader));

        return medicines;
        // Side/healing effects loaded on-demand per medicine when needed.
    }

    public List<Medicine> GetMedicinesByCategory(string category)
    {
        const string query = @"
            SELECT MedicineID, Name, Price, Category, ExpiryDate,
                   StockQuantity, UsageInstructions, IsRefundable
            FROM MEDICINE
            WHERE Category = @Category
            ORDER BY Name";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Category", category);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var medicines = new List<Medicine>();
        while (reader.Read())
            medicines.Add(MapMedicine(reader));

        return medicines;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────

    public bool UpdateMedicine(Medicine medicine)
    {
        const string query = @"
            UPDATE MEDICINE
            SET Name              = @Name,
                Price             = @Price,
                Category          = @Category,
                ExpiryDate        = @ExpiryDate,
                StockQuantity     = @Stock,
                UsageInstructions = @Usage,
                IsRefundable      = @IsRefundable
            WHERE MedicineID = @MedicineId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Name",         medicine.Name);
        cmd.Parameters.AddWithValue("@Price",        medicine.Price);
        cmd.Parameters.AddWithValue("@Category",     medicine.Category);
        cmd.Parameters.AddWithValue("@ExpiryDate",   medicine.ExpiryDate);
        cmd.Parameters.AddWithValue("@Stock",        medicine.StockQuantity);
        cmd.Parameters.AddWithValue("@Usage",        medicine.UsageInstructions);
        cmd.Parameters.AddWithValue("@IsRefundable", medicine.IsRefundable);
        cmd.Parameters.AddWithValue("@MedicineId",   medicine.MedicineId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Adjusts stock after an order. Pass a negative delta to reduce stock.
    /// </summary>
    public bool UpdateStock(int medicineId, int delta)
    {
        const string query = @"
            UPDATE MEDICINE
            SET StockQuantity = StockQuantity + @Delta
            WHERE MedicineID = @MedicineId
              AND StockQuantity + @Delta >= 0";
        // The AND guard prevents stock going negative at the DB level.
        // If the row is not updated (returns 0), stock was insufficient.

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Delta",      delta);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── DELETE ────────────────────────────────────────────────────────────

    public bool DeleteMedicine(int medicineId)
    {
        // Side/healing effect rows deleted automatically by CASCADE,
        // but ORDER_ITEM and PRESCRIPTION_ITEM rows will block deletion
        // if they reference this medicine — correct behaviour, don't override.
        const string query = @"
            DELETE FROM MEDICINE WHERE MedicineID = @MedicineId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── SIDE / HEALING EFFECTS ────────────────────────────────────────────
    
    // ── CREATE ────────────────────────────────────────────────────────────
    public bool AddSideEffect(int medicineId, string effectName)
    {
        const string query = @"
            IF NOT EXISTS (
                SELECT 1 FROM MED_SIDE_EFFECT
                WHERE MedicineID = @MedicineId AND SideEffectName = @Effect
            )
            INSERT INTO MED_SIDE_EFFECT (MedicineID, SideEffectName)
            VALUES (@MedicineId, @Effect)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);
        cmd.Parameters.AddWithValue("@Effect",     effectName);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    public bool AddHealingEffect(int medicineId, string effectName)
    {
        const string query = @"
            IF NOT EXISTS (
                SELECT 1 FROM MED_HEALING_EFFECT
                WHERE MedicineID = @MedicineId AND HealingEffectName = @Effect
            )
            INSERT INTO MED_HEALING_EFFECT (MedicineID, HealingEffectName)
            VALUES (@MedicineId, @Effect)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);
        cmd.Parameters.AddWithValue("@Effect",     effectName);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public List<string> GetSideEffects(int medicineId)
    {
        const string query = @"
            SELECT SideEffectName FROM MED_SIDE_EFFECT
            WHERE MedicineID = @MedicineId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var effects = new List<string>();
        while (reader.Read())
            effects.Add(reader["SideEffectName"].ToString()!);
        return effects;
    }

    public List<string> GetHealingEffects(int medicineId)
    {
        const string query = @"
            SELECT HealingEffectName FROM MED_HEALING_EFFECT
            WHERE MedicineID = @MedicineId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MedicineId", medicineId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var effects = new List<string>();
        while (reader.Read())
            effects.Add(reader["HealingEffectName"].ToString()!);
        return effects;
    }
    
    // ── UPDATE ──────────────────────────────────────────────────────────────
    
    
    // ── DELETE ──────────────────────────────────────────────────────────────
    
    
    // ── PRIVATE MAPPING ───────────────────────────────────────────────────

    private static Medicine MapMedicine(SqlDataReader reader) => new Medicine
    {
        MedicineId        = Convert.ToInt32(reader["MedicineID"]),
        Name              = reader["Name"].ToString()!,
        Price             = Convert.ToDouble(reader["Price"]),
        Category          = reader["Category"].ToString()!,
        ExpiryDate        = reader["ExpiryDate"].ToString()!,
        StockQuantity     = Convert.ToInt32(reader["StockQuantity"]),
        UsageInstructions = reader["UsageInstructions"].ToString()!,
        IsRefundable      = Convert.ToBoolean(reader["IsRefundable"])
    };
}