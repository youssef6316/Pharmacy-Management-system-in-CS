namespace PMS_CS.src.Models;

public class Payment
{
    // ── Maps to PAYMENT table columns ─────────────────────────────────────
    public int    PaymentId     { get; set; }
    public double Amount        { get; set; }
    public string PaymentDate   { get; set; } = string.Empty;  // was "payday"
    public string PaymentMethod { get; set; } = string.Empty;  // was "Paymethod"
    public string Status        { get; private set; } = "Pending";

    // ── Foreign key — replaces "private Order order" ──────────────────────
    // Java stored the whole Order object inside Payment.
    // C# stores only the ID. If you need order details, the repository
    // fetches them with a JOIN — you never nest objects that live in the DB.
    public int    OrderId       { get; set; }

    // ── Optional navigation property ──────────────────────────────────────
    // Populated by the repository only when explicitly needed (e.g. the
    // PaymentView wants to display order details alongside payment info).
    // It is null by default — do NOT rely on it being set everywhere.
    public Order? Order         { get; set; }

    // ── Constructors ──────────────────────────────────────────────────────
    public Payment() { }

    public Payment(double amount, string paymentDate,
                   string paymentMethod, int orderId)
    {
        Amount        = amount;
        PaymentDate   = paymentDate;
        PaymentMethod = paymentMethod;
        OrderId       = orderId;
        // PaymentId assigned by IDENTITY on INSERT — never set manually.
        // Status defaults to "Pending" — matches your Java constructor.
    }

    // ── Status transitions (mirrors your Java setStatus pattern) ──────────
    // Using explicit methods instead of a raw setter forces callers to be
    // intentional about what state they are moving to — same logic you had
    // in Order.java with completeOrder() / cancelOrder().
    public void Complete() => Status = "Completed";
    public void Cancel()   => Status = "Cancelled";
    public void Pend()     => Status = "Pending";

    // Exposed for the repository to restore status when reading from DB.
    public void SetStatus(string status) => Status = status;

    public bool IsPending()   => Status == "Pending";
    public bool IsCompleted() => Status == "Completed";

    public override string ToString() =>
        $"Payment {{ Id={PaymentId}, Amount={Amount:C}, " +
        $"Date={PaymentDate}, Method={PaymentMethod}, Status={Status} }}";
}