namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 房东/租户
/// </summary>
public class Landlord : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Landlord() { }
    public Landlord(string name) { Name = name; }
}
