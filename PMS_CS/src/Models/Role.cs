namespace PMS_CS.src.Models;

public class Role
{
    // ── Maps to ROLE table columns ────────────────────────────────────────
    // Note: Java used String ID — DB uses IDENTITY int. Much cleaner.
    public int    RoleId           { get; set; }
    public string RoleName         { get; set; } = string.Empty;
    public int    PermissionsLevel { get; set; }
    public string Description      { get; set; } = string.Empty;

    // ── Constructors ──────────────────────────────────────────────────────
    public Role() { }

    public Role(string roleName, int permissionsLevel, string description)
    {
        RoleName         = roleName;
        PermissionsLevel = permissionsLevel;
        Description      = description;
        // RoleId assigned by IDENTITY on INSERT.
    }

    // ── C# equality — replaces the broken equals() in Role.java ──────────
    // Java's equals() had a logic error: the first branch cast before
    // null-checking, which would throw a NullReferenceException if obj
    // happened to be null. This version is safe.
    public override bool Equals(object? obj)
    {
        if (obj is not Role other) return false;
        return RoleId == other.RoleId;
    }

    // In C#, overriding Equals() requires overriding GetHashCode() too.
    // The compiler will warn you if you forget. RoleId is the unique
    // identifier so it's the right hash source.
    public override int GetHashCode() => RoleId.GetHashCode();

    public override string ToString() =>
        $"Role {{ Id={RoleId}, Name={RoleName}, Level={PermissionsLevel} }}";
}