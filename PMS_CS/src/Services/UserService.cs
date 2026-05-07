using PMS_CS.src.Models;
using PMS_CS.src.Repositories;

namespace PMS_CS.src.Services;

public class UserService
{
    // The service holds instances of the repositories it needs.
    // It never instantiates SqlConnection directly — that's the
    // repository's job. This is called "dependency on abstraction"
    // and it's the core principle separating these two layers.
    private readonly UserRepository    _userRepo;
    private readonly PatientRepository _patientRepo;

    public UserService()
    {
        _userRepo    = new UserRepository();
        _patientRepo = new PatientRepository();
    }

    // ── REGISTRATION ──────────────────────────────────────────────────────

    /// <summary>
    /// Registers a brand-new Patient — creates the USER row first,
    /// then the PATIENT row using the returned ID.
    /// Returns the new UserId, or -1 with an error message on failure.
    /// </summary>
    public (int UserId, string Error) RegisterPatient(
        string username, string password, string email,
        string phone, float age, string address)
    {
        // ── Validation (business rules — not SQL) ─────────────────────────
        if (string.IsNullOrWhiteSpace(username))
            return (-1, "Username cannot be empty.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (-1, "Password must be at least 6 characters.");

        if (!email.Contains('@'))
            return (-1, "Invalid email address.");

        // ── Uniqueness check — ask the DB, don't guess ────────────────────
        if (_userRepo.GetUserByUsername(username) != null)
            return (-1, "Username is already taken.");

        // ── Step 1: Insert USER row ───────────────────────────────────────
        var user = new User(username, password, email, phone);
        int userId = _userRepo.AddUser(user);

        if (userId == -1)
            return (-1, "Failed to create user account.");

        // ── Step 2: Insert PATIENT row using the returned ID ──────────────
        var patient = new Patient(userId, age, address);
        bool patientCreated = _patientRepo.AddPatient(patient);

        if (!patientCreated)
            return (-1, "User created but patient profile failed.");

        return (userId, string.Empty);
    }

    /// <summary>
    /// Registers a new Employee (Admin / Cashier / Pharmacist).
    /// Only callable when the requesting user is an Admin — enforced here.
    /// </summary>
    public (int UserId, string Error) RegisterEmployee(
        string username, string password, string email,
        string phone, string jobType, double salary,
        Employee requestingEmployee)
    {
        // ── Authorization — business rule, enforced in service ────────────
        // This is the C# equivalent of Admin.setSalary() checking the role.
        if (!requestingEmployee.IsAdmin())
            return (-1, "Only administrators can register employees.");

        if (!new[] { "Admin", "Cashier", "Pharmacist" }.Contains(jobType))
            return (-1, $"Invalid job type: {jobType}.");

        if (string.IsNullOrWhiteSpace(username))
            return (-1, "Username cannot be empty.");

        if (_userRepo.GetUserByUsername(username) != null)
            return (-1, "Username is already taken.");

        var user = new User(username, password, email, phone);
        int userId = _userRepo.AddUser(user);

        if (userId == -1)
            return (-1, "Failed to create user account.");

        // Employee row goes into EMPLOYEE table, not PATIENT.
        var employeeRepo = new EmployeeRepository();
        var employee = new Employee(userId, jobType, salary);
        bool created = employeeRepo.AddEmployee(employee);

        if (!created)
            return (-1, "User created but employee profile failed.");

        return (userId, string.Empty);
    }

    // ── AUTHENTICATION ────────────────────────────────────────────────────

    /// <summary>
    /// Validates credentials and returns the logged-in User.
    /// Returns null and an error string on failure.
    /// The View calls this on the login button click.
    /// </summary>
    public (User? User, string Error) Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (null, "Username and password are required.");

        var user = _userRepo.Authenticate(username, password);

        if (user == null)
            return (null, "Invalid username or password.");

        if (!user.IsActive)
            return (null, "This account has been deactivated.");

        // Load roles so the View can decide which screen to show next.
        user.Roles = _userRepo.GetRolesForUser(user.UserId);

        return (user, string.Empty);
    }

    // ── PROFILE MANAGEMENT ────────────────────────────────────────────────

    public (bool Success, string Error) UpdateProfile(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains('@'))
            return (false, "Invalid email address.");

        bool updated = _userRepo.UpdateUser(user);
        return updated
            ? (true, string.Empty)
            : (false, "Update failed — user may not exist.");
    }

    public (bool Success, string Error) ChangePassword(
        int userId, string currentPassword, string newPassword)
    {
        if (newPassword.Length < 6)
            return (false, "New password must be at least 6 characters.");

        // Verify the current password before allowing the change.
        var user = _userRepo.GetUserById(userId);
        if (user == null)
            return (false, "User not found.");

        if (user.Password != currentPassword)
            return (false, "Current password is incorrect.");

        bool changed = _userRepo.ChangePassword(userId, newPassword);
        return changed
            ? (true, string.Empty)
            : (false, "Password change failed.");
    }

    // ── SALARY MANAGEMENT ─────────────────────────────────────────────────
    // Translates Admin.setSalary() — now enforced through the service layer.

    public (bool Success, string Error) SetEmployeeSalary(
        int targetEmployeeId, double newSalary,
        Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin())
            return (false, "Only administrators can modify salaries.");

        if (newSalary < 0)
            return (false, "Salary cannot be negative.");

        var employeeRepo = new EmployeeRepository();
        bool updated = employeeRepo.UpdateSalary(targetEmployeeId, newSalary);

        return updated
            ? (true, string.Empty)
            : (false, "Employee not found or update failed.");
    }

    // ── ROLE MANAGEMENT ───────────────────────────────────────────────────

    public bool AssignRole(int userId, int roleId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin()) return false;
        return _userRepo.AddRoleToUser(userId, roleId);
    }

    public bool RevokeRole(int userId, int roleId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin()) return false;
        return _userRepo.RemoveRoleFromUser(userId, roleId);
    }

    // ── DEACTIVATION ──────────────────────────────────────────────────────

    public (bool Success, string Error) DeactivateUser(
        int targetUserId, Employee requestingEmployee)
    {
        if (!requestingEmployee.IsAdmin())
            return (false, "Only administrators can deactivate accounts.");

        bool deactivated = _userRepo.DeactivateUser(targetUserId);
        return deactivated
            ? (true, string.Empty)
            : (false, "User not found or already inactive.");
    }
}