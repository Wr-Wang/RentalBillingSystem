namespace RBS.Core.Entities.SystemConfig;
using RBS.Core.Entities.Base;

/// <summary>
/// 节假日/调休配置日历（AuditableEntity）
/// 用于定义系统中的法定节假日和调休上班日，影响计费引擎的工作日/节假日判断逻辑。
/// IsWorkingDay=false 表示放假（如春节、国庆），
/// IsWorkingDay=true 表示调休上班（如周末补班）。
/// 每家公司可维护独立的节假日日历
/// </summary>
public class HolidayCalendar : AuditableEntity, IHasCompany
{
    /// <summary>
    /// 节假日日期（不含时间），标识具体的节假日或调休日
    /// </summary>
    public DateTime HolidayDate { get; private set; }

    /// <summary>
    /// 节假日名称（可选），如 "春节"、"国庆节"、"元旦调休"
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// 是否工作日。false=放假，true=调休上班。
    /// 当 IsWorkingDay=false 时，计费引擎跳过该日期；
    /// 当 IsWorkingDay=true 时，计费引擎将该日期视为工作日处理
    /// </summary>
    public bool IsWorkingDay { get; private set; }

    /// <summary>
    /// 所属公司标识，用于多租户隔离。每家公司可自定义节假日配置
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// 私有构造函数，仅用于 EF Core 反序列化，禁止直接调用
    /// </summary>
    private HolidayCalendar() { }

    /// <summary>
    /// 创建节假日配置项
    /// </summary>
    /// <param name="holidayDate">节假日日期</param>
    /// <param name="name">节假日名称（可选）</param>
    /// <param name="isWorkingDay">false=放假, true=调休上班</param>
    /// <param name="companyId">所属公司标识</param>
    public HolidayCalendar(DateTime holidayDate, string? name, bool isWorkingDay, Guid companyId)
    {
        HolidayDate = holidayDate;
        Name = name;
        IsWorkingDay = isWorkingDay;
        CompanyId = companyId;
    }

    /// <summary>设置节假日名称</summary>
    /// <param name="name">节假日名称</param>
    public void SetName(string? name) => Name = name;

    /// <summary>设置是否为工作日</summary>
    /// <param name="isWorkingDay">false=放假, true=调休上班</param>
    public void SetIsWorkingDay(bool isWorkingDay) => IsWorkingDay = isWorkingDay;
}
