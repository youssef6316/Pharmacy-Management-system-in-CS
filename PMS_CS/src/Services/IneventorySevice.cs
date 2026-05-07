using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class InventoryService
{
    private readonly MedicineRepository _medicineRepo;
    private readonly PatientRepository  _patientRepo;

    public InventoryService()
    {
        _medicineRepo = new MedicineRepository();
        _patientRepo  = new PatientRepository();
    }

    // ── STOCK MANAGEMENT ──────────────────────────────────────────────────

    public List<Medicine> GetAllMedicines()       => _medicineRepo.GetAllMedicines();
    public List<Medicine> GetByCategory(string c) => _medicineRepo.GetMedicinesByCategory(c);
    public Medicine? GetMedicineById(int id)      => _medicineRepo.GetMedicineById(id);

    public (bool Success, string Error) AddMedicine(
        Medicine medicine, Employee requestingEmployee)
    {
        // Replaces Admin.authorizeItem() — only admins add new medicines.
        if (!requestingEmployee.IsAdmin())
            return (false, "Only administrators can add medicines.");

        if (string.IsNullOrWhiteSpace(medicine.Name))
            return (false, "Medicine name cannot be empty.");

        if (medicine.Price < 0)
            return (false, "Price cannot be negative.");

        if (medicine.StockQuantity < 0)
            return (false, "Stock quantity cannot be negative.");

        int newId = _medicineRepo.AddMedicine(medicine);
        return newId > 0
            ? (true, string.Empty)
            : (false, "Failed to add medicine to database.");
    }

    public (bool Success, string Error) UpdateMedicine(
        Medicine medicine, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin())
            return (false, "Only administrators can edit medicines.");

        bool updated = _medicineRepo.UpdateMedicine(medicine);
        return updated
            ? (true, string.Empty)
            : (false, "Medicine not found or update failed.");
    }

    public (bool Success, string Error) RemoveMedicine(
        int medicineId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin())
            return (false, "Only administrators can remove medicines.");

        bool deleted = _medicineRepo.DeleteMedicine(medicineId);
        return deleted
            ? (true, string.Empty)
            : (false, "Medicine not found or is referenced by existing orders.");
    }

    // ── SAFETY CHECK ──────────────────────────────────────────────────────
    // Direct translation of Pharmacist.is_safe(item, patient).
    // Moved here because it's a business rule, not a property of a person.

    /// <summary>
    /// Returns true if the medicine is safe for the given patient.
    /// A medicine is unsafe if any of its side effects match the
    /// patient's known allergies AND the medicine is not yet expired.
    /// </summary>
    public bool IsMedicineSafeForPatient(Medicine medicine, Patient patient)
    {
        // Load allergies if not already populated.
        if (patient.Allergies.Count == 0)
            patient.Allergies = _patientRepo.GetAllergies(patient.UserId);

        // Load side effects if not already populated.
        if (medicine.SideEffects.Count == 0)
            medicine.SideEffects = _medicineRepo.GetSideEffects(medicine.MedicineId);

        // An expired medicine is considered unsafe regardless of allergies.
        if (medicine.IsExpired())
            return false;

        // Check for any overlap between side effects and patient allergies.
        // LINQ's Any() stops at the first match — same as your Java for-loop.
        return !medicine.SideEffects.Any(effect =>
            patient.HasAllergy(effect));
    }

    /// <summary>
    /// Returns all medicines safe for a specific patient.
    /// Useful for populating prescription item pickers in the View.
    /// </summary>
    public List<Medicine> GetSafeMedicinesForPatient(int patientId)
    {
        var patient   = _patientRepo.GetPatientById(patientId);
        if (patient == null) return new List<Medicine>();

        var medicines = _medicineRepo.GetAllMedicines();

        return medicines
            .Where(m => IsMedicineSafeForPatient(m, patient))
            .ToList();
    }

    // ── STOCK ADJUSTMENT ─────────────────────────────────────────────────

    /// <summary>
    /// Reduces stock when an order is placed. Returns false if
    /// stock is insufficient — caller must handle this before committing.
    /// </summary>
    public bool DeductStock(int medicineId, int quantity)
    {
        if (quantity <= 0) return false;
        // Negative delta reduces stock. The DB guard in UpdateStock
        // prevents it going below zero.
        return _medicineRepo.UpdateStock(medicineId, -quantity);
    }

    public bool RestoreStock(int medicineId, int quantity)
    {
        if (quantity <= 0) return false;
        return _medicineRepo.UpdateStock(medicineId, quantity);
    }

    // ── CATEGORY LISTING ─────────────────────────────────────────────────
    // Replaces Admin.validCats — now sourced live from DB.

    public List<string> GetAllCategories()
    {
        return _medicineRepo
            .GetAllMedicines()
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
}