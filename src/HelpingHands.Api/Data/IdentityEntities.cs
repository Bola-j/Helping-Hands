namespace HelpingHands.Api.Data;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = "";
    public string NormalizedUserName { get; set; } = "";
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public string? PasswordHash { get; set; }
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public string DisplayName { get; set; } = "";
    public string AccountStatus { get; set; } = "Active";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AppRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

public sealed class AppUserRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
}
