namespace PMS_CS.src.Models;

public class OrderItem
{
    // ── Maps directly to ORDER_ITEM table columns ─────────────────────────
    public int OrderId {get; set;}
    public int MedicineId {get; set;}
    public int Quantity {get; set;}
    public double UnitPrice {get; set;}

    // ── Computed — not a DB column ────────────────────────────────────────
    // Populated by the repository via a JOIN with MEDICINE.
    // Saves you from doing a second lookup just to display the name.
    public string MedicineName {get; set;} = string.Empty;

    // Calculated on the fly — never stored in DB, always derived.
    public double LineTotal => UnitPrice * Quantity;

    // ── Constructors ──────────────────────────────────────────────────────
    public OrderItem() { }

    public OrderItem(int orderId, int medicineId, int quantity, double unitPrice)
    {
        OrderId    = orderId;
        MedicineId = medicineId;
        Quantity   = quantity;
        UnitPrice  = unitPrice;
    }

    public override string ToString() =>
        $"OrderItem {{ Medicine={MedicineName}, Qty={Quantity}, " +
        $"Unit={UnitPrice:C}, Total={LineTotal:C} }}";
}