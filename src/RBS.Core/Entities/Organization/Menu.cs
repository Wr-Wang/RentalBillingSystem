namespace RBS.Core.Entities.Organization;

using RBS.Core.Entities.Base;

/// <summary>
/// 菜单实体 — 系统导航菜单及权限控制点（AuditableEntity）
/// 菜单既是前端页面路由的配置来源，也是后端权限控制（PermissionCode）的基本单位。
/// 支持树形层级结构（ParentId）、排序、作用域隔离（公司级/系统级）
/// </summary>
public class Menu : AuditableEntity
{
    /// <summary>
    /// 菜单名称，如"合同管理"、"财务报表"，用于前端界面展示
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 权限编码（可选），用于后端接口鉴权标识，如"Contract:View"、"Finance:Export"
    /// </summary>
    public string? PermissionCode { get; private set; }

    /// <summary>
    /// 前端路由路径（可选），对应 Vue Router 的 path，如"/contracts/list"
    /// 注意：路径中禁止包含连字符（参照项目规范）
    /// </summary>
    public string? Path { get; private set; }

    /// <summary>
    /// 菜单图标样式（可选），如 "el-icon-setting"、"icon-user"，用于前端菜单图标展示
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// 父级菜单标识（可选），支持树形层级结构。为 null 时表示一级菜单
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// 排序序号，同级菜单按此值升序排列，数值越小越靠前
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 是否启用，true=启用（在导航栏中可见），false=停用（隐藏）
    /// 默认值为 true
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// 作用域：Company=公司级菜单（普通用户可见），System=系统级菜单（平台管理员可见）
    /// 默认值为 "Company"
    /// </summary>
    public string Scope { get; private set; } = "Company";  // Company / System

    /// <summary>
    /// 所属公司标识（可选），仅公司级自定义菜单时填充，表示该菜单属于特定公司
    /// </summary>
    public Guid? CompanyId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private Menu() { }

    /// <summary>
    /// 创建菜单实例。菜单名称是必填项，创建后默认处于启用状态
    /// </summary>
    /// <param name="name">菜单名称，不能为空或空白字符</param>
    /// <exception cref="ArgumentException">当菜单名称为空或仅含空白字符时抛出</exception>
    public Menu(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("菜单名称不能为空", nameof(name));
        Name = name;
        IsActive = true;
    }

    // ===== 属性设置方法 =====

    /// <summary>重命名菜单</summary>
    /// <param name="name">新的菜单名称，不能为空或空白字符</param>
    /// <exception cref="ArgumentException">当新名称为空时抛出</exception>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("菜单名称不能为空", nameof(name));
        Name = name;
    }

    /// <summary>设置权限编码</summary>
    /// <param name="code">权限编码，如 "Contract:View"</param>
    public void SetPermissionCode(string? code) => PermissionCode = code;

    /// <summary>设置前端路由路径</summary>
    /// <param name="path">路由路径，如 "/contracts/list"（禁止含连字符）</param>
    public void SetPath(string? path) => Path = path;

    /// <summary>设置菜单图标样式</summary>
    /// <param name="icon">图标类名，如 "el-icon-setting"</param>
    public void SetIcon(string? icon) => Icon = icon;

    /// <summary>设置父级菜单（构建树形层级）</summary>
    /// <param name="parentId">父菜单标识，设为 null 表示设为一级菜单</param>
    public void SetParentId(Guid? parentId) => ParentId = parentId;

    /// <summary>设置排序序号</summary>
    /// <param name="order">排序序号，同级按升序排列</param>
    public void SetSortOrder(int order) => SortOrder = order;

    /// <summary>设置作用域</summary>
    /// <param name="scope">"Company"=公司级 或 "System"=系统级</param>
    public void SetScope(string scope) => Scope = scope;

    /// <summary>设置所属公司（用于公司级自定义菜单）</summary>
    /// <param name="companyId">公司标识，设为 null 表示平台级菜单</param>
    public void SetCompanyId(Guid? companyId) => CompanyId = companyId;

    /// <summary>启用菜单，在导航中可见</summary>
    public void Activate() => IsActive = true;

    /// <summary>停用菜单，在导航中隐藏</summary>
    public void Deactivate() => IsActive = false;
}
