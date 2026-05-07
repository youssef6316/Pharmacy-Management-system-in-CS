using PMS_CS.src.Models;

namespace PMS_CS.Tests.Models;

public class OrderTests
{
    // ── Helper: builds a test OrderItem ───────────────────────────────────
    private static OrderItem MakeItem(int medicineId, int qty, double unitPrice) =>
        new OrderItem { MedicineId = medicineId, Quantity = qty, UnitPrice = unitPrice };

    // ── AddItem / total ───────────────────────────────────────────────────

    [Fact]
    public void AddItem_UpdatesTotalPrice()
    {
        var order = new Order();
        order.AddItem(MakeItem(1, 2, 10.0));   // 2 × 10 = 20
        order.AddItem(MakeItem(2, 1, 15.0));   // 1 × 15 = 15

        Assert.Equal(35.0, order.TotalPrice);
    }

    [Fact]
    public void RemoveItem_UpdatesTotalPrice()
    {
        var order = new Order();
        var item  = MakeItem(1, 2, 10.0);
        order.AddItem(item);
        order.AddItem(MakeItem(2, 1, 15.0));

        order.RemoveItem(item);

        Assert.Equal(15.0, order.TotalPrice);
    }

    [Fact]
    public void RemoveItem_NonExistentItem_ReturnsFalse()
    {
        var order = new Order();
        var item  = MakeItem(1, 1, 10.0);

        bool result = order.RemoveItem(item);

        Assert.False(result);
        Assert.Equal(0.0, order.TotalPrice);
    }

    [Fact]
    public void ClearOrder_ResetsItemsAndTotal()
    {
        var order = new Order();
        order.AddItem(MakeItem(1, 3, 20.0));
        order.ClearOrder();

        Assert.Empty(order.OrderItems);
        Assert.Equal(0.0, order.TotalPrice);
    }

    [Fact]
    public void RecalculateTotal_SumsAllLineItems()
    {
        var order = new Order();
        // Directly add to the list to simulate a repository load.
        order.OrderItems.Add(MakeItem(1, 2, 5.0));    // 10
        order.OrderItems.Add(MakeItem(2, 3, 4.0));    // 12

        order.RecalculateTotal();

        Assert.Equal(22.0, order.TotalPrice);
    }

    // ── Status transitions ────────────────────────────────────────────────

    [Fact]
    public void NewOrder_StatusIsPending()
    {
        var order = new Order();
        Assert.True(order.IsPending());
    }

    [Fact]
    public void CompleteOrder_ChangesStatusToCompleted()
    {
        var order = new Order();
        order.CompleteOrder();

        Assert.True(order.IsCompleted());
        Assert.False(order.IsPending());
    }

    [Fact]
    public void CancelOrder_ChangesStatusToCancelled()
    {
        var order = new Order();
        order.CancelOrder();

        Assert.True(order.IsCancelled());
    }
}