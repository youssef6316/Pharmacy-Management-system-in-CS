using PMS_CS.src.Models;
namespace PMS_CS.src.Models;

public class Patient
{
    // ── Foreign key back to USER table ────────────────────────────────────
    // Patient IS-A User in Java inheritance.
    // Patient HAS-A User in the DB (UserID is both PK and FK in PATIENT).
    // This property is the link between the two tables.
    public int    UserId         { get; set; }

    // ── Maps to PATIENT table columns ─────────────────────────────────────
    public float  Age            { get; set; }
    public string Address        { get; set; } = string.Empty;
    public double PatientBalance { get; set; }

    // ── Navigation property — populated on demand by repository ───────────
    // When a View needs to display username/email alongside patient data,
    // the repository JOINs USER and sets this. Null otherwise.
    public User?  UserInfo       { get; set; }

    // ── Multi-value attribute — loaded from PATIENT_ALLERGY ───────────────
    // Was Set<String> allergies in Java.
    // Uniqueness is enforced by the composite PK in PATIENT_ALLERGY.
    public List<string> Allergies { get; set; } = new();

    // Orders are NOT stored here — was List<Order> orders in Java.
    // In the DB, ORDER has a PatientID foreign key. To get a patient's
    // orders you query ORDER WHERE PatientID = this.UserId.
    // The OrderRepository handles that — Patient.cs stays lean.

    // ── Constructors ──────────────────────────────────────────────────────
    public Patient() { }

    public Patient(int userId, float age, string address, double patientBalance = 0)
    {
        UserId         = userId;
        Age            = age;
        Address        = address;
        PatientBalance = patientBalance;
    }

    // ── Allergy management (translated from Patient.java) ─────────────────
    // These operate on the in-memory list only.
    // Persisting requires PatientRepository.AddAllergy() / RemoveAllergy().
    public void AddAllergy(string allergy)
    {
        if (!Allergies.Contains(allergy, StringComparer.OrdinalIgnoreCase))
            Allergies.Add(allergy);
    }

    public bool RemoveAllergy(string allergy) =>
        Allergies.RemoveAll(a =>
            a.Equals(allergy, StringComparison.OrdinalIgnoreCase)) > 0;

    public bool HasAllergy(string allergy) =>
        Allergies.Any(a => a.Equals(allergy, StringComparison.OrdinalIgnoreCase));

    // ── Balance helpers ───────────────────────────────────────────────────
    public bool CanAfford(double amount) => PatientBalance >= amount;

    public void Debit(double amount)  => PatientBalance -= amount;
    public void Credit(double amount) => PatientBalance += amount;

    public override string ToString() =>
        $"Patient {{ UserId={UserId}, Age={Age}, Balance={PatientBalance:C}, " +
        $"Allergies={Allergies.Count} }}";
}