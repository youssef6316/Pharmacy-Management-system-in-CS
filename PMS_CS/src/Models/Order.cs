namespace PMS_CS.src.Models;

public class Order
{
    // ── Maps to ORDER table columns ───────────────────────────────────────
    public int OrderId {get; set;}
    public string OrderDate {get; set;} = string.Empty;
    public double TotalPrice {get; internal set;}   // controlled by methods below
    public string Status {get; internal set;} = "Pending";
    public int PatientId {get; set;}
    public int CashierId {get; set;}   // was string checkedBy — now an FK

    // ── Child records (loaded by repository via JOIN on ORDER_ITEM) ───────
    public List<OrderItem> OrderItems {get; set;} = new();

    // ── Constructors ──────────────────────────────────────────────────────
    public Order() { }

    public Order(string orderDate, int patientId, int cashierId)
    {
        OrderDate = orderDate;
        PatientId = patientId;
        CashierId = cashierId;
        // OrderId is assigned by IDENTITY on INSERT — never set manually.
    }

    // ── Business logic (faithfully translated from Order.java) ────────────

    public void AddItem(OrderItem item)
    {
        OrderItems.Add(item);
        TotalPrice += item.LineTotal;
    }

    public bool RemoveItem(OrderItem item)
    {
        if (OrderItems.Remove(item))
        {
            TotalPrice -= item.LineTotal;
            return true;
        }
        return false;
    }

    public void ClearOrder()
    {
        OrderItems.Clear();
        TotalPrice = 0.0;
    }

    public void RecalculateTotal()
    {
        // Call this after loading OrderItems from DB to sync TotalPrice.
        TotalPrice = OrderItems.Sum(i => i.LineTotal);
    }

    public void CompleteOrder() => Status = "Completed";
    public void CancelOrder()   => Status = "Cancelled";

    public bool IsPending()   => Status == "Pending";
    public bool IsCompleted() => Status == "Completed";
    public bool IsCancelled() => Status == "Cancelled";

    public override string ToString() =>
        $"Order {{ Id={OrderId}, Date={OrderDate}, Total={TotalPrice:C}, " +
        $"Status={Status}, Items={OrderItems.Count} }}";
}