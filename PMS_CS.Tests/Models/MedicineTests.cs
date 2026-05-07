using PMS_CS.src.Models;

namespace PMS_CS.Tests.Models;

// No DB needed — these test only the C# logic inside the model class.
public class MedicineTests
{
    // ── IsExpired() ───────────────────────────────────────────────────────

    [Fact]
    public void IsExpired_PastDate_ReturnsTrue()
    {
        var medicine = new Medicine { ExpiryDate = "2000-01-01" };
        Assert.True(medicine.IsExpired());
    }

    [Fact]
    public void IsExpired_FutureDate_ReturnsFalse()
    {
        var medicine = new Medicine { ExpiryDate = "2099-12-31" };
        Assert.False(medicine.IsExpired());
    }

    [Fact]
    public void IsExpired_InvalidDateString_ReturnsFalse()
    {
        // Unparseable date should not crash — returns false defensively.
        var medicine = new Medicine { ExpiryDate = "not-a-date" };
        Assert.False(medicine.IsExpired());
    }

    // ── IsInStock() ───────────────────────────────────────────────────────

    [Fact]
    public void IsInStock_PositiveQuantity_ReturnsTrue()
    {
        var medicine = new Medicine { StockQuantity = 10 };
        Assert.True(medicine.IsInStock());
    }

    [Fact]
    public void IsInStock_ZeroQuantity_ReturnsFalse()
    {
        var medicine = new Medicine { StockQuantity = 0 };
        Assert.False(medicine.IsInStock());
    }

    // ── HasSideEffect() ───────────────────────────────────────────────────

    [Fact]
    public void HasSideEffect_ExistingEffect_ReturnsTrue()
    {
        var medicine = new Medicine();
        medicine.SideEffects.Add("nausea");

        Assert.True(medicine.HasSideEffect("nausea"));
    }

    [Fact]
    public void HasSideEffect_IsCaseInsensitive()
    {
        var medicine = new Medicine();
        medicine.SideEffects.Add("Nausea");

        // Should match regardless of casing.
        Assert.True(medicine.HasSideEffect("nausea"));
        Assert.True(medicine.HasSideEffect("NAUSEA"));
    }

    [Fact]
    public void HasSideEffect_MissingEffect_ReturnsFalse()
    {
        var medicine = new Medicine();
        medicine.SideEffects.Add("nausea");

        Assert.False(medicine.HasSideEffect("dizziness"));
    }
}