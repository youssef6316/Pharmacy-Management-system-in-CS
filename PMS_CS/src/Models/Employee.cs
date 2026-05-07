namespace PMS_CS.src.Models;

public class Employee
{
    // ── Foreign key back to USER table ────────────────────────────────────
    public int    UserId  { get; set; }

    // ── Maps to EMPLOYEE table columns ────────────────────────────────────
    public double Salary  { get; set; }

    // JobType replaces three Java subclasses.
    // Valid values: "Admin" | "Cashier" | "Pharmacist"
    // Enforced at the service layer — not an enum here so that
    // new roles can be added in the DB without recompiling.
    public string JobType { get; set; } = string.Empty;

    // ── Navigation property — populated on demand by repository ───────────
    public User?  UserInfo { get; set; }

    // ── Constructors ──────────────────────────────────────────────────────
    public Employee() { }

    public Employee(int userId, string jobType, double salary = 0)
    {
        UserId  = userId;
        JobType = jobType;
        Salary  = salary;
    }

    // ── Convenience type-check properties ─────────────────────────────────
    // Replaces the instanceof checks scattered across Admin.java.
    // The service layer uses these instead of string comparisons everywhere.
    public bool IsAdmin()      => JobType == "Admin";
    public bool IsCashier()    => JobType == "Cashier";
    public bool IsPharmacist() => JobType == "Pharmacist";
    
    // ── Salary helpers ────────────────────────────────────────────────────
    // calculateAnnualSalary() lived in Pharmacist.java — it applies to
    // any employee, so it belongs on the shared model.
    public double AnnualSalary() => Salary * 12;

    public override string ToString() =>
        $"Employee {{ UserId={UserId}, JobType={JobType}, Salary={Salary:C} }}";
}