using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class OrderService
{
    private readonly OrderRepository    _orderRepo;
    private readonly MedicineRepository _medicineRepo;
    private readonly PatientRepository  _patientRepo;
    private readonly InventoryService   _inventoryService;

    public OrderService()
    {
        _orderRepo        = new OrderRepository();
        _medicineRepo     = new MedicineRepository();
        _patientRepo      = new PatientRepository();
        _inventoryService = new InventoryService();
    }

    // ── PLACE ORDER ───────────────────────────────────────────────────────

    /// <summary>
    /// The main order workflow — validates stock, validates balance,
    /// writes to DB, deducts stock and balance.
    /// Returns the new OrderId or -1 with an error.
    /// </summary>
    public (int OrderId, string Error) PlaceOrder(Order order)
    {
        if (order.OrderItems.Count == 0)
            return (-1, "Cannot place an empty order.");

        // ── Step 1: Verify stock for every item before touching the DB ─────
        foreach (var item in order.OrderItems)
        {
            var medicine = _medicineRepo.GetMedicineById(item.MedicineId);
            if (medicine == null)
                return (-1, $"Medicine ID {item.MedicineId} not found.");

            if (!medicine.IsInStock() || medicine.StockQuantity < item.Quantity)
                return (-1, $"{medicine.Name} has insufficient stock " +
                            $"(requested {item.Quantity}, available {medicine.StockQuantity}).");

            // Stamp the UnitPrice from the DB — never trust the client's price.
            item.UnitPrice = medicine.Price;
        }

        // Recalculate total from verified prices.
        order.RecalculateTotal();

        // ── Step 2: Verify patient can afford the order ────────────────────
        var patient = _patientRepo.GetPatientById(order.PatientId);
        if (patient == null)
            return (-1, "Patient not found.");

        if (!patient.CanAfford(order.TotalPrice))
            return (-1, $"Insufficient balance. " +
                        $"Required: {order.TotalPrice:C}, Available: {patient.PatientBalance:C}.");

        // ── Step 3: Write order + items to DB (transactional) ─────────────
        order.OrderDate = DateTime.Today.ToString("yyyy-MM-dd");
        int newOrderId = _orderRepo.AddOrder(order);

        if (newOrderId == -1)
            return (-1, "Order could not be saved to database.");

        // ── Step 4: Deduct stock for each medicine ─────────────────────────
        foreach (var item in order.OrderItems)
            _inventoryService.DeductStock(item.MedicineId, item.Quantity);

        // ── Step 5: Deduct patient balance ────────────────────────────────
        patient.Debit(order.TotalPrice);
        _patientRepo.UpdateBalance(patient.UserId, patient.PatientBalance);

        return (newOrderId, string.Empty);
    }

    // ── CANCEL ORDER ──────────────────────────────────────────────────────

    /// <summary>
    /// Cancels a pending order and restores stock and patient balance.
    /// </summary>
    public (bool Success, string Error) CancelOrder(int orderId)
    {
        var order = _orderRepo.GetOrderById(orderId);

        if (order == null)
            return (false, "Order not found.");

        if (!order.IsPending())
            return (false, $"Cannot cancel an order with status '{order.Status}'.");
        
        // Business Rule: Prevent cancellation of completed orders
        if (order.Status == "Completed")
        {
            return (false, "Cannot cancel an order that is already completed.");
        }

        // Update status in DB first.
        bool cancelled = _orderRepo.UpdateStatus(orderId, "Cancelled");
        if (!cancelled)
            return (false, "Failed to update order status.");

        // Restore stock for each item.
        foreach (var item in order.OrderItems)
            _inventoryService.RestoreStock(item.MedicineId, item.Quantity);

        // Refund balance to patient.
        var patient = _patientRepo.GetPatientById(order.PatientId);
        if(patient != null)
        {
            patient.Credit(order.TotalPrice);
            _patientRepo.UpdateBalance(patient.UserId, patient.PatientBalance);
        }

        return (true, string.Empty);
    }

    // ── COMPLETE ORDER ────────────────────────────────────────────────────

    public (bool Success, string Error) CompleteOrder(
        int orderId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsCashier() && !requestingEmployee.IsAdmin())
            return (false, "Only cashiers or admins can complete orders.");

        var order = _orderRepo.GetOrderById(orderId);
        if (order == null)
            return (false, "Order not found.");

        if (!order.IsPending())
            return (false, $"Order status is already '{order.Status}'.");

        bool updated = _orderRepo.UpdateStatus(orderId, "Completed");
        return updated
            ? (true, string.Empty)
            : (false, "Failed to complete order.");
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public Order?        GetOrderById(int orderId)         => _orderRepo.GetOrderById(orderId);
    public List<Order>   GetOrdersByPatient(int patientId) => _orderRepo.GetOrdersByPatient(patientId);
    public List<Order>   GetOrdersByCashier(int cashierId) => _orderRepo.GetOrdersByCashier(cashierId);
}