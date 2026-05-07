namespace PMS_CS.src.Models;

public class User
{
    // ── Maps to USER table columns ────────────────────────────────────────
    public int    UserId      { get; set; }
    public string Username    { get; set; } = string.Empty;
    public string Password    { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
    public string Phone       { get; set; } = string.Empty;
    public bool   IsActive    { get; set; } = true;

    // ── Roles — loaded from USER_ROLE join table by repository ────────────
    /*
    Not a column in USER. The UserRepository populates this list by
    querying USER_ROLE JOIN ROLE whenever a full user profile is needed.
    It is empty by default — do not assume it is populated everywhere.
    */
    public List<Role> Roles   { get; set; } = new();

    // ── Constructors ──────────────────────────────────────────────────────
    public User() { }

    public User(string username, string password, string email, string phone)
    {
        Username = username;
        Password = password;
        Email    = email;
        Phone    = phone;
        IsActive = true;
        // UserId assigned by IDENTITY on INSERT.
    }

    // ── Role management (translated from User.java) ───────────────────────
    // These operate on the in-memory list only.
    // Persisting a role change requires calling UserRepository.AddRole() separately.
    public bool HasRole(Role role)    => Roles.Any(r => r.RoleId == role.RoleId);
    public void AddRole(Role role)    { if (!HasRole(role)) Roles.Add(role); }
    public bool RemoveRole(Role role) => Roles.RemoveAll(r => r.RoleId == role.RoleId) > 0;

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;

    public override string ToString() =>
        $"User {{ Id={UserId}, Username={Username}, Email={Email}, Active={IsActive} }}";
}