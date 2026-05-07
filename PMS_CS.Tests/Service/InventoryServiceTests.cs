using PMS_CS.src.Models;
using PMS_CS.src.Services;
using PMS_CS.Tests.Helpers;

namespace PMS_CS.Tests.Services;

public class InventoryServiceTests : TestDatabase
{
    private readonly InventoryService _service = new();

    // ── IsMedicineSafeForPatient ──────────────────────────────────────────

    [Fact]
    public void IsMedicineSafeForPatient_NoAllergyConflict_ReturnsTrue()
    {
        var medicine = new Medicine
        {
            MedicineId = 1,
            ExpiryDate = "2099-01-01",
            SideEffects = new List<string> { "nausea" }
        };

        var patient = new Patient();
        patient.AddAllergy("penicillin");   // different from side effects

        bool safe = _service.IsMedicineSafeForPatient(medicine, patient);

        Assert.True(safe);
    }

    [Fact]
    public void IsMedicineSafeForPatient_AllergyConflict_ReturnsFalse()
    {
        var medicine = new Medicine
        {
            MedicineId = 1,
            ExpiryDate = "2099-01-01",
            SideEffects = new List<string> { "nausea", "penicillin-reaction" }
        };

        var patient = new Patient();
        patient.AddAllergy("penicillin-reaction");

        bool safe = _service.IsMedicineSafeForPatient(medicine, patient);

        Assert.False(safe);
    }

    [Fact]
    public void IsMedicineSafeForPatient_ExpiredMedicine_ReturnsFalse()
    {
        var medicine = new Medicine
        {
            MedicineId  = 1,
            ExpiryDate  = "2000-01-01",   // past date
            SideEffects = new List<string>()
        };

        var patient = new Patient();   // no allergies

        bool safe = _service.IsMedicineSafeForPatient(medicine, patient);

        // Expired is always unsafe regardless of allergies.
        Assert.False(safe);
    }

    // ── DeductStock / RestoreStock ────────────────────────────────────────

    [Fact]
    public void DeductStock_SufficientStock_ReturnsTrue()
    {
        int id = SeedMedicine(stock: 50);

        bool result = _service.DeductStock(id, 10);

        Assert.True(result);
    }

    [Fact]
    public void DeductStock_InsufficientStock_ReturnsFalse()
    {
        int id = SeedMedicine(stock: 5);

        bool result = _service.DeductStock(id, 20);

        Assert.False(result);
    }

    [Fact]
    public void RestoreStock_AfterDeduction_RestoresCorrectly()
    {
        int id = SeedMedicine(stock: 50);
        _service.DeductStock(id, 10);

        _service.RestoreStock(id, 10);

        var medicineRepo = new PMS_CS.src.Repositories.MedicineRepository();
        var medicine     = medicineRepo.GetMedicineById(id);
        Assert.Equal(50, medicine!.StockQuantity);
    }
}