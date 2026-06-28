namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

public class Menu : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? PermissionCode { get; private set; }
    public string? Path { get; private set; }
    public string? Icon { get; private set; }
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Menu() { }
    public Menu(string name) { Name = name; }
}
