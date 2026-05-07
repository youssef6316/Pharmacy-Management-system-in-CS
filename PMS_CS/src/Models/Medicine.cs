namespace PMS_CS.src.Models;

public class Medicine
{
    // ── Core fields (map 1-to-1 with MEDICINE table columns) ─────────────
    public int MedicineId {get; set;}
    public string Name {get; set;} = string.Empty;
    public double Price {get; set;}
    public string Category {get; set;} = string.Empty;
    public string ExpiryDate {get; set;} = string.Empty;
    public int StockQuantity {get; set;}
    public string UsageInstructions {get; set;} = string.Empty;
    public bool IsRefundable {get; set;}

    // ── Multi-value fields (loaded from MED_SIDE_EFFECT / MED_HEALING_EFFECT)
    // These are NOT columns in MEDICINE — the repository populates them
    // via a second query and fills these lists.
    public List<string> SideEffects    { get; set; } = new();
    public List<string> HealingEffects { get; set; } = new();

    // ── Constructors ──────────────────────────────────────────────────────

    // Empty constructor — used by the repository when reading rows from the DB.
    // The repository sets each property one by one after reading the SqlDataReader.
    public Medicine() { }

    // Full constructor — used when creating a new medicine to INSERT into the DB.
    public Medicine(string name, double price, string category,
                    string expiryDate, int stockQuantity,
                    string usageInstructions, bool isRefundable)
    {
        Name              = name;
        Price             = price;
        Category          = category;
        ExpiryDate        = expiryDate;
        StockQuantity     = stockQuantity;
        UsageInstructions = usageInstructions;
        IsRefundable      = isRefundable;
        // MedicineId is intentionally NOT set here.
        // IDENTITY(1,1) in SQL Server assigns it automatically on INSERT.
    }

    // ── Business logic methods (kept from Item.java) ──────────────────────

    public bool IsExpired()
    {
        // Tries to parse the date string. If it's a valid date and already
        // passed, the medicine is expired.
        if (DateTime.TryParse(ExpiryDate, out DateTime expiry))
            return expiry < DateTime.Today;
        return false;
    }

    public bool IsInStock() => StockQuantity > 0;

    public bool HasSideEffect(string effect) =>
        SideEffects.Contains(effect, StringComparer.OrdinalIgnoreCase);

    // ── C# equivalent of Java's toString() ───────────────────────────────
    public override string ToString() =>
        $"Medicine {{ Id={MedicineId}, Name={Name}, Price={Price:C}, " +
        $"Stock={StockQuantity}, Refundable={IsRefundable} }}";
    
}