/*
To be added: 
1. Delete Payment (support Refund operation)
*/
using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class PaymentRepository
{
    public int AddPayment(Payment payment)
    {
        const string query = @"
            INSERT INTO PAYMENT (Amount, PaymentDate, PaymentMethod, Status, OrderID)
            OUTPUT INSERTED.PaymentID
            VALUES (@Amount, @PaymentDate, @PaymentMethod, @Status, @OrderId)";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Amount",        payment.Amount);
        cmd.Parameters.AddWithValue("@PaymentDate",   payment.PaymentDate);
        cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
        cmd.Parameters.AddWithValue("@Status",        payment.Status);
        cmd.Parameters.AddWithValue("@OrderId",       payment.OrderId);

        conn.Open();
        var result = cmd.ExecuteScalar();
        return result != null ? (int)result : -1;
    }

    public Payment? GetPaymentById(int paymentId)
    {
        const string query = @"
            SELECT PaymentID, Amount, PaymentDate, PaymentMethod, Status, OrderID
            FROM PAYMENT
            WHERE PaymentID = @PaymentId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PaymentId", paymentId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read()) return MapPayment(reader);
        return null;
    }

    public Payment? GetPaymentByOrder(int orderId)
    {
        const string query = @"
            SELECT PaymentID, Amount, PaymentDate, PaymentMethod, Status, OrderID
            FROM PAYMENT
            WHERE OrderID = @OrderId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read()) return MapPayment(reader);
        return null;
    }

    public bool UpdateStatus(int paymentId, string status)
    {
        const string query = @"
            UPDATE PAYMENT SET Status = @Status
            WHERE PaymentID = @PaymentId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Status",    status);
        cmd.Parameters.AddWithValue("@PaymentId", paymentId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    /// <summary>
    /// Returns total income across all completed payments.
    /// Replaces Admin.totalIncome static field — computed live from DB.
    /// </summary>
    public double GetTotalIncome()
    {
        const string query = @"
            SELECT ISNULL(SUM(Amount), 0)
            FROM PAYMENT
            WHERE Status = 'Completed'";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);

        conn.Open();
        return Convert.ToDouble(cmd.ExecuteScalar());
    }

    private static Payment MapPayment(SqlDataReader reader)
    {
        var p = new Payment
        {
            PaymentId     = Convert.ToInt32(reader["PaymentID"]),
            Amount        = Convert.ToDouble(reader["Amount"]),
            PaymentDate   = reader["PaymentDate"].ToString()!,
            PaymentMethod = reader["PaymentMethod"].ToString()!,
            OrderId       = Convert.ToInt32(reader["OrderID"])
        };
        p.SetStatus(reader["Status"].ToString()!);
        return p;
    }
}