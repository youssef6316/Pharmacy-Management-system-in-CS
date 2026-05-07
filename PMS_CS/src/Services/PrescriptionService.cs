using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class PrescriptionService
{
    private readonly PrescriptionRepository _prescriptionRepo;
    private readonly InventoryService       _inventoryService;
    private readonly PatientRepository      _patientRepo;

    public PrescriptionService()
    {
        _prescriptionRepo = new PrescriptionRepository();
        _inventoryService = new InventoryService();
        _patientRepo      = new PatientRepository();
    }

    // ── ISSUE PRESCRIPTION ────────────────────────────────────────────────

    /// <summary>
    /// Issues a new prescription. Only pharmacists can do this.
    /// Validates each medicine is safe for the patient first.
    /// </summary>
    public (int PrescriptionId, string Error) IssuePrescription(
        Prescription prescription, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsPharmacist() && !requestingEmployee.IsAdmin())
            return (-1, "Only pharmacists can issue prescriptions.");

        if (prescription.Items.Count == 0)
            return (-1, "A prescription must contain at least one medicine.");

        // ── Safety check for every medicine against patient allergies ──────
        var patient = _patientRepo.GetPatientById(prescription.PatientId);
        if (patient == null)
            return (-1, "Patient not found.");

        foreach (var item in prescription.Items)
        {
            var medicine = _inventoryService.GetMedicineById(item.MedicineId);
            if (medicine == null)
                return (-1, $"Medicine ID {item.MedicineId} not found.");

            if (!_inventoryService.IsMedicineSafeForPatient(medicine, patient))
                return (-1, $"{medicine.Name} is unsafe for this patient " +
                            $"due to allergy conflict or expiry.");
        }

        prescription.IssueDate    = DateTime.Today.ToString("yyyy-MM-dd");
        prescription.PharmacistId = requestingEmployee.UserId;

        int newId = _prescriptionRepo.AddPrescription(prescription);

        return newId > 0
            ? (newId, string.Empty)
            : (-1, "Failed to save prescription to database.");
    }

    // ── STATUS TRANSITIONS ────────────────────────────────────────────────

    public (bool Success, string Error) FillPrescription(
        int prescriptionId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsPharmacist() && !requestingEmployee.IsAdmin())
            return (false, "Only pharmacists can fill prescriptions.");

        var prescription = _prescriptionRepo.GetPrescriptionById(prescriptionId);
        if (prescription == null)
            return (false, "Prescription not found.");

        if (!prescription.IsActive())
            return (false, $"Prescription status is '{prescription.Status}' — cannot fill.");

        bool updated = _prescriptionRepo.UpdateStatus(prescriptionId, "Filled");
        return updated
            ? (true, string.Empty)
            : (false, "Failed to update prescription status.");
    }

    public (bool Success, string Error) CancelPrescription(
        int prescriptionId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsPharmacist() && !requestingEmployee.IsAdmin())
            return (false, "Only pharmacists can cancel prescriptions.");

        var prescription = _prescriptionRepo.GetPrescriptionById(prescriptionId);
        if (prescription == null)
            return (false, "Prescription not found.");

        if (prescription.IsFilled())
            return (false, "Cannot cancel a prescription that has already been filled.");

        bool updated = _prescriptionRepo.UpdateStatus(prescriptionId, "Cancelled");
        return updated
            ? (true, string.Empty)
            : (false, "Failed to cancel prescription.");
    }

    // ── READ ──────────────────────────────────────────────────────────────

    public Prescription? GetPrescriptionById(int id) =>
        _prescriptionRepo.GetPrescriptionById(id);

    public List<Prescription> GetPrescriptionsByPatient(int patientId) =>
        _prescriptionRepo.GetPrescriptionsByPatient(patientId);
}