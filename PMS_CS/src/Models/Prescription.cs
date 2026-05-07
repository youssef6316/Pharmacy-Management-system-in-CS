namespace PMS_CS.src.Models;

public class Prescription
{
    // ── Maps to PRESCRIPTION table columns ────────────────────────────────
    public int    PrescriptionId { get; set; }
    public string IssueDate      { get; set; } = string.Empty;
    public string Status         { get; private set; } = "Active";

    // ── Foreign keys — replace object references from Java ────────────────
    // Java stored: private String PatientName  → now an FK int
    // Java stored: private Pharmacist issuedBy → now an FK int
    public int    PatientId      { get; set; }
    public int    PharmacistId   { get; set; }

    // ── Convenience strings — populated by repository JOINs ───────────────
    // Same idea as MedicineName in OrderItem: saves a second DB round-trip
    // when Views just need to display a name next to the prescription.
    public string PatientName    { get; set; } = string.Empty;
    public string PharmacistName { get; set; } = string.Empty;

    // ── Child records — replaces Set<Item> items ──────────────────────────
    // Was a HashSet<Item> in Java (whole objects).
    // Now a List<PrescriptionItem> — each entry holds MedicineId + quantity,
    // loaded by the repository via JOIN on PRESCRIPTION_ITEM.
    public List<PrescriptionItem> Items { get; set; } = new();

    // ── Constructors ──────────────────────────────────────────────────────
    public Prescription() { }

    public Prescription(string issueDate, int patientId, int pharmacistId)
    {
        IssueDate    = issueDate;
        PatientId    = patientId;
        PharmacistId = pharmacistId;
        // PrescriptionId assigned by IDENTITY — never set manually.
        // Status defaults to "Active".
    }

    // ── Item management (translated from Add_Item / Remove_Item) ──────────
    public void AddItem(PrescriptionItem item)
    {
        // Guard against duplicate medicines on the same prescription.
        // Mirrors the uniqueness that HashSet gave you for free in Java.
        // The DB also enforces this via PRIMARY KEY (PrescriptionID, MedicineID)
        // but we check here first for a cleaner error message.
        if (!Items.Any(i => i.MedicineId == item.MedicineId))
            Items.Add(item);
    }

    public bool RemoveItem(int medicineId)
    {
        var item = Items.FirstOrDefault(i => i.MedicineId == medicineId);
        return item != null && Items.Remove(item);
    }

    // ── Status transitions ────────────────────────────────────────────────
    public void Fill()   => Status = "Filled";
    public void Expire() => Status = "Expired";
    public void Cancel() => Status = "Cancelled";

    // For repository use when restoring status from DB row.
    public void SetStatus(string status) => Status = status;

    public bool IsActive()  => Status == "Active";
    public bool IsFilled()  => Status == "Filled";
    public bool IsExpired() => Status == "Expired";

    public override string ToString() =>
        $"Prescription {{ Id={PrescriptionId}, Patient={PatientName}, " +
        $"Pharmacist={PharmacistName}, Status={Status}, " +
        $"Items={Items.Count} }}";
}