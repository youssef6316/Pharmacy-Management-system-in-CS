/*
To be added: 
*/
using Microsoft.Data.SqlClient;
using PMS_CS.Database;
using PMS_CS.src.Models;

namespace PMS_CS.src.Repositories;

public class OrderRepository
{
    // ── CREATE ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts the Order header and all its OrderItems in one transaction.
    /// A transaction means: if any item INSERT fails, the whole order is
    /// rolled back — no half-written orders in the DB.
    /// This is your first introduction to transactions in ADO.NET.
    /// </summary>
    public int AddOrder(Order order)
    {
        using var conn = DBConnection.GetConnection();
        conn.Open();

        // BeginTransaction() groups everything until Commit() into
        // one atomic unit. Think of it as: all-or-nothing.
        using var transaction = conn.BeginTransaction();

        try
        {
            // Step 1: Insert the order header row.
            const string orderQuery = @"
                INSERT INTO [ORDER]
                    (OrderDate, TotalPrice, Status, PatientID, CashierID)
                OUTPUT INSERTED.OrderID
                VALUES
                    (@OrderDate, @TotalPrice, @Status, @PatientId, @CashierId)";

            using var orderCmd = new SqlCommand(orderQuery, conn, transaction);
            orderCmd.Parameters.AddWithValue("@OrderDate",  order.OrderDate);
            orderCmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
            orderCmd.Parameters.AddWithValue("@Status",     order.Status);
            orderCmd.Parameters.AddWithValue("@PatientId",  order.PatientId);
            orderCmd.Parameters.AddWithValue("@CashierId",  order.CashierId);

            var result = orderCmd.ExecuteScalar();
            if (result == null) throw new Exception("Order INSERT returned no ID.");

            int newOrderId = (int)result;

            // Step 2: Insert each item row using the new OrderID.
            const string itemQuery = @"
                INSERT INTO ORDER_ITEM (OrderID, MedicineID, Quantity, UnitPrice)
                VALUES (@OrderId, @MedicineId, @Quantity, @UnitPrice)";

            foreach (var item in order.OrderItems)
            {
                using var itemCmd = new SqlCommand(itemQuery, conn, transaction);
                itemCmd.Parameters.AddWithValue("@OrderId",    newOrderId);
                itemCmd.Parameters.AddWithValue("@MedicineId", item.MedicineId);
                itemCmd.Parameters.AddWithValue("@Quantity",   item.Quantity);
                itemCmd.Parameters.AddWithValue("@UnitPrice",  item.UnitPrice);
                itemCmd.ExecuteNonQuery();
            }

            // Step 3: Everything worked — make it permanent.
            transaction.Commit();
            return newOrderId;
        }
        catch
        {
            // Something failed — undo everything back to the start.
            transaction.Rollback();
            return -1;
        }
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public Order? GetOrderById(int orderId)
    {
        const string query = @"
            SELECT o.OrderID, o.OrderDate, o.TotalPrice, o.Status,
                   o.PatientID, o.CashierID
            FROM [ORDER] o
            WHERE o.OrderID = @OrderId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (!reader.Read()) return null;

        var order = MapOrder(reader);
        reader.Close();

        order.OrderItems = GetOrderItems(orderId);
        order.RecalculateTotal();
        return order;
    }

    public List<Order> GetOrdersByPatient(int patientId)
    {
        const string query = @"
            SELECT OrderID, OrderDate, TotalPrice, Status, PatientID, CashierID
            FROM [ORDER]
            WHERE PatientID = @PatientId
            ORDER BY OrderDate DESC";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@PatientId", patientId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var orders = new List<Order>();
        while (reader.Read())
            orders.Add(MapOrder(reader));

        return orders;
    }

    public List<Order> GetOrdersByCashier(int cashierId)
    {
        const string query = @"
            SELECT OrderID, OrderDate, TotalPrice, Status, PatientID, CashierID
            FROM [ORDER]
            WHERE CashierID = @CashierId
            ORDER BY OrderDate DESC";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CashierId", cashierId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var orders = new List<Order>();
        while (reader.Read())
            orders.Add(MapOrder(reader));

        return orders;
    }

    // ── UPDATE STATUS ─────────────────────────────────────────────────────

    public bool UpdateStatus(int orderId, string status)
    {
        const string query = @"
            UPDATE [ORDER] SET Status = @Status
            WHERE OrderID = @OrderId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Status",  status);
        cmd.Parameters.AddWithValue("@OrderId", orderId);

        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }

    // ── ORDER ITEMS ───────────────────────────────────────────────────────

    public List<OrderItem> GetOrderItems(int orderId)
    {
        // JOIN with MEDICINE to get the name in one query —
        // avoids calling GetMedicineById() once per item.
        const string query = @"
            SELECT oi.OrderID, oi.MedicineID, oi.Quantity, oi.UnitPrice,
                   m.Name AS MedicineName
            FROM ORDER_ITEM oi
            INNER JOIN MEDICINE m ON oi.MedicineID = m.MedicineID
            WHERE oi.OrderID = @OrderId";

        using var conn = DBConnection.GetConnection();
        using var cmd  = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var items = new List<OrderItem>();
        while (reader.Read())
        {
            items.Add(new OrderItem
            {
                OrderId      = Convert.ToInt32(reader["OrderID"]),
                MedicineId   = Convert.ToInt32(reader["MedicineID"]),
                Quantity     = Convert.ToInt32(reader["Quantity"]),
                UnitPrice    = Convert.ToDouble(reader["UnitPrice"]),
                MedicineName = reader["MedicineName"].ToString()!
            });
        }
        return items;
    }

    // ── PRIVATE MAPPING ───────────────────────────────────────────────────

    private static Order MapOrder(SqlDataReader reader) => new Order
    {
        OrderId   = Convert.ToInt32(reader["OrderID"]),
        OrderDate = reader["OrderDate"].ToString()!,
        TotalPrice = Convert.ToDouble(reader["TotalPrice"]),
        Status     = reader["Status"].ToString()!,
        PatientId = Convert.ToInt32(reader["PatientID"]),
        CashierId = Convert.ToInt32(reader["CashierID"])
    };
}