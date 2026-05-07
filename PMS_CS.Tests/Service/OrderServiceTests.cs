using PMS_CS.src.Models;
using PMS_CS.src.Repositories;
using PMS_CS.src.Services;
using PMS_CS.Tests.Helpers;

namespace PMS_CS.Tests.Services;

public class OrderServiceTests : TestDatabase
{
    private readonly OrderService   _service        = new();
    private readonly PatientRepository _patientRepo = new();

    // ── PlaceOrder ────────────────────────────────────────────────────────

    [Fact]
    public void PlaceOrder_ValidOrder_ReturnsPositiveId()
    {
        var (patientId, cashierId, medicineId) = SeedOrderDependencies(balance: 500);

        var order = BuildOrder(patientId, cashierId, medicineId, qty: 2, price: 10.0);
        var (id, error) = _service.PlaceOrder(order);

        Assert.True(id > 0);
        Assert.Empty(error);
    }

    [Fact]
    public void PlaceOrder_DeductsPatientBalance()
    {
        var (patientId, cashierId, medicineId) = SeedOrderDependencies(balance: 500);

        var order = BuildOrder(patientId, cashierId, medicineId, qty: 2, price: 10.0);
        _service.PlaceOrder(order);

        var patient = _patientRepo.GetPatientById(patientId);
        // 500 - (2 × 50) = 400
        Assert.Equal(480.0, patient!.PatientBalance);
    }

    [Fact]
    public void PlaceOrder_DeductsStockCorrectly()
    {
        var (patientId, cashierId, medicineId) =
            SeedOrderDependencies(balance: 500, stock: 30);

        var order = BuildOrder(patientId, cashierId, medicineId, qty: 5, price: 10.0);
        _service.PlaceOrder(order);

        var medicineRepo = new MedicineRepository();
        var medicine     = medicineRepo.GetMedicineById(medicineId);
        Assert.Equal(25, medicine!.StockQuantity);
    }

    [Fact]
    public void PlaceOrder_InsufficientBalance_ReturnsError()
    {
        var (patientId, cashierId, medicineId) = SeedOrderDependencies(balance: 10);

        var order = BuildOrder(patientId, cashierId, medicineId, qty: 5, price: 50.0);
        var (id, error) = _service.PlaceOrder(order);

        Assert.Equal(-1, id);
        Assert.Contains("balance", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PlaceOrder_InsufficientStock_ReturnsError()
    {
        var (patientId, cashierId, medicineId) =
            SeedOrderDependencies(balance: 9999, stock: 2);

        var order = BuildOrder(patientId, cashierId, medicineId, qty: 10, price: 5.0);
        var (id, error) = _service.PlaceOrder(order);

        Assert.Equal(-1, id);
        Assert.Contains("stock", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PlaceOrder_EmptyItems_ReturnsError()
    {
        int patientUserId = SeedUser("pp", "pass", "pp@test.com");
        SeedPatient(patientUserId, balance: 500);
        int cashierUserId = SeedUser("cc", "pass", "cc@test.com");
        SeedEmployee(cashierUserId, "Cashier");

        var order           = new Order("2025-01-01", patientUserId, cashierUserId);
        var (id, error)     = _service.PlaceOrder(order);

        Assert.Equal(-1, id);
        Assert.Contains("empty", error, StringComparison.OrdinalIgnoreCase);
    }

    // ── CancelOrder ───────────────────────────────────────────────────────

    [Fact]
    public void CancelOrder_PendingOrder_RestoresStockAndBalance()
    {
        var (patientId, cashierId, medicineId) =
            SeedOrderDependencies(balance: 500, stock: 30);

        var order   = BuildOrder(patientId, cashierId, medicineId, qty: 5, price: 20.0);
        var (id, _) = _service.PlaceOrder(order);

        var (success, error) = _service.CancelOrder(id);

        // Stock and balance should be restored.
        var patient      = _patientRepo.GetPatientById(patientId)!;
        var medicineRepo = new MedicineRepository();
        var medicine     = medicineRepo.GetMedicineById(medicineId)!;

        Assert.True(success);
        Assert.Equal(500.0, patient.PatientBalance);
        Assert.Equal(30,    medicine.StockQuantity);
    }

    [Fact]
    public void CancelOrder_AlreadyCompleted_ReturnsError()
    {
        var (patientId, cashierId, medicineId) = SeedOrderDependencies(balance: 500);
        var order   = BuildOrder(patientId, cashierId, medicineId, qty: 1, price: 10.0);
        var (id, _) = _service.PlaceOrder(order);

        // Mark it completed first.
        var orderRepo = new OrderRepository();
        orderRepo.UpdateStatus(id, "Completed");

        var (success, error) = _service.CancelOrder(id);

        Assert.False(success);
        Assert.NotEmpty(error);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private (int patientId, int cashierId, int medicineId)
        SeedOrderDependencies(double balance = 500, int stock = 100)
    {
        // Using Guid to guarantee unique usernames across test runs.
        string pName = $"pat_{Guid.NewGuid():N}";
        string cName = $"cas_{Guid.NewGuid():N}";

        int pUserId = SeedUser(pName, "pass", $"{pName}@t.com");
        SeedPatient(pUserId, balance: balance);
        int cUserId = SeedUser(cName, "pass", $"{cName}@t.com");
        SeedEmployee(cUserId, "Cashier");
        int mId = SeedMedicine(stock: stock);

        return (pUserId, cUserId, mId);
    }

    private static Order BuildOrder(
        int patientId, int cashierId, int medicineId,
        int qty, double price)
    {
        var order = new Order("2025-01-01", patientId, cashierId);
        order.OrderItems.Add(new OrderItem
        {
            MedicineId = medicineId,
            Quantity   = qty,
            UnitPrice  = price
        });
        order.RecalculateTotal();
        return order;
    }
}