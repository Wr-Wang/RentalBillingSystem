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

    /// <summary>领域构造函数</summary>
    public Menu(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("菜单名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    // ===== 属性设置 =====
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("菜单名称不能为空", nameof(name));
        Name = name;
    }
    public void SetPermissionCode(string? code) => PermissionCode = code;
    public void SetPath(string? path) => Path = path;
    public void SetIcon(string? icon) => Icon = icon;
    public void SetParentId(Guid? parentId) => ParentId = parentId;
    public void SetSortOrder(int order) => SortOrder = order;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
