using PMS_CS.src.Models;
using PMS_CS.src.Repositories;
using PMS_CS.Tests.Helpers;

namespace PMS_CS.Tests.Repositories;

public class OrderRepositoryTests : TestDatabase
{
    private readonly OrderRepository _repo = new();

    // ── AddOrder (transactional) ──────────────────────────────────────────

    [Fact]
    public void AddOrder_ValidOrder_ReturnsPositiveId()
    {
        int patientUserId  = SeedUser("patient1", "pass", "p@test.com");
        SeedPatient(patientUserId);
        int cashierUserId  = SeedUser("cashier1", "pass", "c@test.com");
        SeedEmployee(cashierUserId, "Cashier");
        int medicineId     = SeedMedicine();

        var order = new Order("2025-01-01", patientUserId, cashierUserId);
        order.OrderItems.Add(new OrderItem
        {
            MedicineId = medicineId, Quantity = 2, UnitPrice = 10.0
        });
        order.RecalculateTotal();

        int id = _repo.AddOrder(order);

        Assert.True(id > 0);
    }

    [Fact]
    public void AddOrder_ThenGetById_ReturnsOrderWithItems()
    {
        int patientUserId = SeedUser("patient2", "pass", "p2@test.com");
        SeedPatient(patientUserId);
        int cashierUserId = SeedUser("cashier2", "pass", "c2@test.com");
        SeedEmployee(cashierUserId, "Cashier");
        int medicineId    = SeedMedicine("Aspirin", 10.0, 100);

        var order = new Order("2025-06-01", patientUserId, cashierUserId);
        order.OrderItems.Add(new OrderItem
        {
            MedicineId = medicineId, Quantity = 3, UnitPrice = 10.0
        });
        order.RecalculateTotal();

        int id      = _repo.AddOrder(order);
        var loaded  = _repo.GetOrderById(id);

        Assert.NotNull(loaded);
        Assert.Single(loaded.OrderItems);
        Assert.Equal(30.0, loaded.TotalPrice);
        Assert.Equal("Aspirin", loaded.OrderItems[0].MedicineName);
    }

    // ── GetOrdersByPatient ────────────────────────────────────────────────

    [Fact]
    public void GetOrdersByPatient_MultipleOrders_ReturnsAll()
    {
        int patientUserId = SeedUser("patient3", "pass", "p3@test.com");
        SeedPatient(patientUserId);
        int cashierUserId = SeedUser("cashier3", "pass", "c3@test.com");
        SeedEmployee(cashierUserId, "Cashier");
        int medicineId    = SeedMedicine();

        // Place two orders for the same patient.
        for (int i = 0; i < 2; i++)
        {
            var order = new Order($"2025-01-0{i + 1}", patientUserId, cashierUserId);
            order.OrderItems.Add(new OrderItem
            {
                MedicineId = medicineId, Quantity = 1, UnitPrice = 5.0
            });
            order.RecalculateTotal();
            _repo.AddOrder(order);
        }

        var orders = _repo.GetOrdersByPatient(patientUserId);

        Assert.Equal(2, orders.Count);
    }

    // ── UpdateStatus ──────────────────────────────────────────────────────

    [Fact]
    public void UpdateStatus_ToCompleted_PersistsToDB()
    {
        int patientUserId = SeedUser("patient4", "pass", "p4@test.com");
        SeedPatient(patientUserId);
        int cashierUserId = SeedUser("cashier4", "pass", "c4@test.com");
        SeedEmployee(cashierUserId, "Cashier");
        int medicineId    = SeedMedicine();

        var order = new Order("2025-01-01", patientUserId, cashierUserId);
        order.OrderItems.Add(new OrderItem
        {
            MedicineId = medicineId, Quantity = 1, UnitPrice = 10.0
        });
        order.RecalculateTotal();
        int orderId = _repo.AddOrder(order);

        _repo.UpdateStatus(orderId, "Completed");

        var loaded = _repo.GetOrderById(orderId);
        Assert.Equal("Completed", loaded.Status);
    }
}