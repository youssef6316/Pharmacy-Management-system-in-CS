using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class PatientService
{
    private readonly PatientRepository _patientRepo;

    public PatientService()
    {
        _patientRepo = new PatientRepository();
    }

    // ── PROFILE ───────────────────────────────────────────────────────────

    public Patient? GetPatientById(int userId)    => _patientRepo.GetPatientById(userId);
    public List<Patient> GetAllPatients()         => _patientRepo.GetAllPatients();

    public (bool Success, string Error) UpdatePatient(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.Address))
            return (false, "Address cannot be empty.");

        if (patient.Age is < 0 or > 150)
            return (false, "Please enter a valid age.");

        bool updated = _patientRepo.UpdatePatient(patient);
        return updated
            ? (true, string.Empty)
            : (false, "Update failed.");
    }

    // ── ALLERGY MANAGEMENT ────────────────────────────────────────────────

    public (bool Success, string Error) AddAllergy(int patientId, string allergy)
    {
        if (string.IsNullOrWhiteSpace(allergy))
            return (false, "Allergy name cannot be empty.");

        bool added = _patientRepo.AddAllergy(patientId, allergy.Trim());
        return added
            ? (true, string.Empty)
            : (false, "Allergy already exists or could not be added.");
    }

    public (bool Success, string Error) RemoveAllergy(int patientId, string allergy)
    {
        bool removed = _patientRepo.RemoveAllergy(patientId, allergy);
        return removed
            ? (true, string.Empty)
            : (false, "Allergy not found.");
    }

    public List<string> GetAllergies(int patientId) =>
        _patientRepo.GetAllergies(patientId);

    // ── BALANCE ───────────────────────────────────────────────────────────

    public (bool Success, string Error) TopUpBalance(int patientId, double amount)
    {
        if (amount <= 0)
            return (false, "Top-up amount must be greater than zero.");

        var patient = _patientRepo.GetPatientById(patientId);
        if (patient == null)
            return (false, "Patient not found.");

        patient.Credit(amount);
        bool updated = _patientRepo.UpdateBalance(patientId, patient.PatientBalance);

        return updated
            ? (true, string.Empty)
            : (false, "Balance update failed.");
    }
}