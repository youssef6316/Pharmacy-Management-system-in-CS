using PMS_CS.src.Models;

namespace PMS_CS.Tests.Models;

public class PatientTests
{
    // ── Allergy management ────────────────────────────────────────────────

    [Fact]
    public void AddAllergy_NewAllergy_AddsToList()
    {
        var patient = new Patient();
        patient.AddAllergy("penicillin");

        Assert.Single(patient.Allergies);
        Assert.Contains("penicillin", patient.Allergies);
    }

    [Fact]
    public void AddAllergy_Duplicate_NotAddedTwice()
    {
        var patient = new Patient();
        patient.AddAllergy("penicillin");
        patient.AddAllergy("Penicillin");   // different casing

        // Duplicate check is case-insensitive — list should still have one entry.
        Assert.Single(patient.Allergies);
    }

    [Fact]
    public void RemoveAllergy_ExistingAllergy_ReturnsTrue()
    {
        var patient = new Patient();
        patient.AddAllergy("penicillin");

        bool result = patient.RemoveAllergy("penicillin");

        Assert.True(result);
        Assert.Empty(patient.Allergies);
    }

    [Fact]
    public void RemoveAllergy_NonExistentAllergy_ReturnsFalse()
    {
        var patient = new Patient();
        bool result = patient.RemoveAllergy("penicillin");

        Assert.False(result);
    }

    [Fact]
    public void HasAllergy_ExistingAllergy_ReturnsTrue()
    {
        var patient = new Patient();
        patient.AddAllergy("aspirin");

        Assert.True(patient.HasAllergy("aspirin"));
        Assert.True(patient.HasAllergy("ASPIRIN"));   // case-insensitive
    }

    // ── Balance helpers ───────────────────────────────────────────────────

    [Fact]
    public void CanAfford_SufficientBalance_ReturnsTrue()
    {
        var patient = new Patient { PatientBalance = 100.0 };
        Assert.True(patient.CanAfford(50.0));
        Assert.True(patient.CanAfford(100.0));   // exact amount is fine
    }

    [Fact]
    public void CanAfford_InsufficientBalance_ReturnsFalse()
    {
        var patient = new Patient { PatientBalance = 30.0 };
        Assert.False(patient.CanAfford(50.0));
    }

    [Fact]
    public void Debit_ReducesBalance()
    {
        var patient = new Patient { PatientBalance = 100.0 };
        patient.Debit(40.0);

        Assert.Equal(60.0, patient.PatientBalance);
    }

    [Fact]
    public void Credit_IncreasesBalance()
    {
        var patient = new Patient { PatientBalance = 50.0 };
        patient.Credit(25.0);

        Assert.Equal(75.0, patient.PatientBalance);
    }
}