using PMS_CS.src.Models;

namespace PMS_CS.Tests.Models;

public class PrescriptionTests
{
    private static PrescriptionItem MakeItem(int medicineId, int qty) =>
        new PrescriptionItem { MedicineId = medicineId, PrescribedQuantity = qty };

    // ── AddItem duplicate guard ───────────────────────────────────────────

    [Fact]
    public void AddItem_NewMedicine_AddsSuccessfully()
    {
        var prescription = new Prescription();
        prescription.AddItem(MakeItem(1, 2));

        Assert.Single(prescription.Items);
    }

    [Fact]
    public void AddItem_DuplicateMedicine_NotAddedTwice()
    {
        var prescription = new Prescription();
        prescription.AddItem(MakeItem(1, 2));
        prescription.AddItem(MakeItem(1, 5));   // same MedicineId

        // Guard in AddItem() prevents duplicates.
        Assert.Single(prescription.Items);
    }

    [Fact]
    public void RemoveItem_ExistingMedicine_ReturnsTrue()
    {
        var prescription = new Prescription();
        prescription.AddItem(MakeItem(1, 2));

        bool result = prescription.RemoveItem(1);

        Assert.True(result);
        Assert.Empty(prescription.Items);
    }

    // ── Status transitions ────────────────────────────────────────────────

    [Fact]
    public void NewPrescription_StatusIsActive()
    {
        var prescription = new Prescription();
        Assert.True(prescription.IsActive());
    }

    [Fact]
    public void Fill_ChangesStatusToFilled()
    {
        var prescription = new Prescription();
        prescription.Fill();

        Assert.True(prescription.IsFilled());
        Assert.False(prescription.IsActive());
    }

    [Fact]
    public void Expire_ChangesStatusToExpired()
    {
        var prescription = new Prescription();
        prescription.Expire();

        Assert.True(prescription.IsExpired());
    }
}