/*
To be added: 
1. Get all Prescriptions.
2. Delete Prescription.
*/
using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class PrescriptionRepository
{
    public int AddPrescription(Prescription prescription)
    {
        using var conn = DBConnection.GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            const string presQuery = @"
                INSERT INTO PRESCRIPTION (IssueDate, Status, PatientID, PharmacistID)
                OUTPUT INSERTED.PrescriptionID
                VALUES (@IssueDate, @Status, @PatientId, @PharmacistId)";

            using var presCmd = new SqlCommand(presQuery, conn, transaction);
            presCmd.Parameters.AddWithValue("@IssueDate",    prescription.IssueDate);
            presCmd.Parameters.AddWithValue("@Status",       prescription.Status);
            presCmd.Parameters.AddWithValue("@PatientId",    prescription.PatientId);
            presCmd.Parameters.AddWithValue("@PharmacistId", prescription.PharmacistId);

            var result = presCmd.ExecuteScalar();
            if (result == null) throw new Exception("Prescription INSERT returned no ID.");

            int newId = (int)result;

            const string itemQuery = @"
                INSERT INTO PRESCRIPTION_ITEM
                    (PrescriptionID, MedicineID, PrescribedQuantity)
                VALUES (@PrescriptionId, @MedicineId, @Quantity)";

            foreach (var item in prescription.Items)
            {
                using var itemCmd = new SqlCommand(itemQuery, conn, transaction);
                itemCmd.Parameters.AddWithValue("@PrescriptionId", newId);
                itemCmd.Parameters.AddWithValue("@MedicineId",     item.MedicineId);
                itemCmd.Parameters.AddWithValue("@Quantity",       item.PrescribedQuantity);
                itemCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return newId;
        }
        catch
        {
            transaction.Rollback();
            return -1;
        }
    }

    public Prescription? GetPrescriptionById(int prescriptionId)
    {
        const string query = @"
            SELECT pr.PrescriptionID, pr.IssueDate, pr.Status,
                   pr.PatientID, pr.PharmacistID,
                   u_pat.Username  AS PatientName,
                   u_pha.Username  AS PharmacistName
            FROM PRESCRIPTION pr
            INNER JOIN [USER] u_pat ON pr.PatientID    = u_pat.UserID
            INNER JOIN [USER] u_pha ON pr.PharmacistID = u_pha.UserID
            WHERE pr.PrescriptionID = @PrescriptionId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PrescriptionId", prescriptionId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (!reader.Read()) return null;

        var prescription = MapPrescription(reader);
        reader.Close();

        prescription.Items = GetPrescriptionItems(prescriptionId);
        return prescription;
    }

    public List<Prescription> GetPrescriptionsByPatient(int patientId)
    {
        const string query = @"
            SELECT pr.PrescriptionID, pr.IssueDate, pr.Status,
                   pr.PatientID, pr.PharmacistID,
                   u_pat.Username AS PatientName,
                   u_pha.Username AS PharmacistName
            FROM PRESCRIPTION pr
            INNER JOIN [USER] u_pat ON pr.PatientID    = u_pat.UserID
            INNER JOIN [USER] u_pha ON pr.PharmacistID = u_pha.UserID
            WHERE pr.PatientID = @PatientId
            ORDER BY pr.IssueDate DESC";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PatientId", patientId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var prescriptions = new List<Prescription>();
        while (reader.Read())
            prescriptions.Add(MapPrescription(reader));

        return prescriptions;
    }

    public bool UpdateStatus(int prescriptionId, string status)
    {
        const string query = @"
            UPDATE PRESCRIPTION SET Status = @Status
            WHERE PrescriptionID = @PrescriptionId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Status",         status);
        cmd.Parameters.AddWithValue("@PrescriptionId", prescriptionId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    public List<PrescriptionItem> GetPrescriptionItems(int prescriptionId)
    {
        const string query = @"
            SELECT pi.PrescriptionID, pi.MedicineID,
                   pi.PrescribedQuantity, m.Name AS MedicineName
            FROM PRESCRIPTION_ITEM pi
            INNER JOIN MEDICINE m ON pi.MedicineID = m.MedicineID
            WHERE pi.PrescriptionID = @PrescriptionId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PrescriptionId", prescriptionId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var items = new List<PrescriptionItem>();
        while (reader.Read())
        {
            items.Add(new PrescriptionItem
            {
                PrescriptionId     = Convert.ToInt32(reader["PrescriptionID"]),
                MedicineId         = Convert.ToInt32(reader["MedicineID"]),
                PrescribedQuantity = Convert.ToInt32(reader["PrescribedQuantity"]),
                MedicineName       = reader["MedicineName"].ToString()!
            });
        }
        return items;
    }

    private static Prescription MapPrescription(SqlDataReader reader)
    {
        var p = new Prescription
        {
            PrescriptionId = Convert.ToInt32(reader["PrescriptionID"]),
            IssueDate      = reader["IssueDate"].ToString()!,
            PatientId      = Convert.ToInt32(reader["PatientID"]),
            PharmacistId   = Convert.ToInt32(reader["PharmacistID"]),
            PatientName    = reader["PatientName"].ToString()!,
            PharmacistName = reader["PharmacistName"].ToString()!
        };
        p.SetStatus(reader["Status"].ToString()!);
        return p;
    }
}