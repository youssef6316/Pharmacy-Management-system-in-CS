namespace PMS_CS.src.Models;

public class PrescriptionItem
{
    // ── Maps to PRESCRIPTION_ITEM table columns ───────────────────────────
    public int PrescriptionId     { get; set; }
    public int MedicineId         { get; set; }
    public int PrescribedQuantity { get; set; }

    // ── Populated by repository JOIN with MEDICINE ────────────────────────
    // Not a DB column — fetched so Views can display the name
    // without a second round-trip to the database.
    public string MedicineName    { get; set; } = string.Empty;

    // ── Constructors ──────────────────────────────────────────────────────
    public PrescriptionItem() { }

    public PrescriptionItem(int prescriptionId, int medicineId, int prescribedQuantity)
    {
        PrescriptionId     = prescriptionId;
        MedicineId         = medicineId;
        PrescribedQuantity = prescribedQuantity;
    }

    public override string ToString() =>
        $"PrescriptionItem {{ Medicine={MedicineName}, " +
        $"Qty={PrescribedQuantity} }}";
}