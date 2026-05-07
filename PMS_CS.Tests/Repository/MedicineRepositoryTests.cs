using PMS_CS.src.Models;
using PMS_CS.src.Repositories;
using PMS_CS.Tests.Helpers;

namespace PMS_CS.Tests.Repositories;

public class MedicineRepositoryTests : TestDatabase
{
    private readonly MedicineRepository _repo = new();

    // ── AddMedicine ───────────────────────────────────────────────────────

    [Fact]
    public void AddMedicine_ValidMedicine_ReturnsPositiveId()
    {
        var medicine = new Medicine("Aspirin", 10.0, "Painkiller",
                                    "2099-01-01", 100, "Take as needed", true);

        int id = _repo.AddMedicine(medicine);

        Assert.True(id > 0);
    }

    [Fact]
    public void AddMedicine_WithSideEffects_PersistsToDB()
    {
        var medicine = new Medicine("Ibuprofen", 12.0, "Painkiller",
                                    "2099-01-01", 50, "Take with food", true);
        medicine.SideEffects.Add("nausea");
        medicine.SideEffects.Add("dizziness");

        int id     = _repo.AddMedicine(medicine);
        var loaded = _repo.GetMedicineById(id);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.SideEffects.Count);
        Assert.Contains("nausea",    loaded.SideEffects);
        Assert.Contains("dizziness", loaded.SideEffects);
    }

    // ── GetMedicineById ───────────────────────────────────────────────────

    [Fact]
    public void GetMedicineById_ExistingId_ReturnsCorrectMedicine()
    {
        int id     = SeedMedicine("Paracetamol", 8.0, 200);
        var loaded = _repo.GetMedicineById(id);

        Assert.NotNull(loaded);
        Assert.Equal("Paracetamol", loaded.Name);
        Assert.Equal(8.0,           loaded.Price);
        Assert.Equal(200,           loaded.StockQuantity);
    }

    [Fact]
    public void GetMedicineById_NonExistentId_ReturnsNull()
    {
        var result = _repo.GetMedicineById(99999);
        Assert.Null(result);
    }

    // ── UpdateStock ───────────────────────────────────────────────────────

    [Fact]
    public void UpdateStock_PositiveDelta_IncreasesStock()
    {
        int id = SeedMedicine(stock: 50);

        _repo.UpdateStock(id, 20);

        var medicine = _repo.GetMedicineById(id);
        Assert.Equal(70, medicine!.StockQuantity);
    }

    [Fact]
    public void UpdateStock_NegativeDelta_DecreasesStock()
    {
        int id = SeedMedicine(stock: 50);

        _repo.UpdateStock(id, -10);

        var medicine = _repo.GetMedicineById(id);
        Assert.Equal(40, medicine!.StockQuantity);
    }

    [Fact]
    public void UpdateStock_WouldGoBelowZero_ReturnsFalse()
    {
        int id = SeedMedicine(stock: 5);

        // Requesting more than available — DB guard prevents negative stock.
        bool result = _repo.UpdateStock(id, -10);

        Assert.False(result);
    }

    // ── UpdateMedicine ────────────────────────────────────────────────────

    [Fact]
    public void UpdateMedicine_ChangesPrice_PersistsToDB()
    {
        int id       = SeedMedicine("Aspirin", price: 10.0);
        var medicine = _repo.GetMedicineById(id)!;
        medicine.Price = 15.0;

        _repo.UpdateMedicine(medicine);

        var updated = _repo.GetMedicineById(id);
        Assert.Equal(15.0, updated!.Price);
    }

    // ── GetAllMedicines ───────────────────────────────────────────────────

    [Fact]
    public void GetAllMedicines_MultipleSeeded_ReturnsAll()
    {
        SeedMedicine("Aspirin");
        SeedMedicine("Ibuprofen");
        SeedMedicine("Paracetamol");

        var all = _repo.GetAllMedicines();

        Assert.Equal(3, all.Count);
    }
}