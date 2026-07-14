namespace RBS.Core.Entities.Base;

/// <summary>
/// 值对象基类（Value Object Base）
///
/// DDD 角色：值对象（Value Object）是领域驱动设计中通过属性值而非身份标识来区分的对象。
/// 与实体（Entity）不同，值对象没有唯一标识符（Id），两个值对象如果所有属性值都相同则视为相等。
/// 值对象是不可变的（Immutable），任何修改都应返回一个新实例。
///
/// 相等性比较：
/// 本基类重写了 Equals 和 GetHashCode 方法，基于 <see cref="GetEqualityComponents"/> 返回的属性集合
/// 进行按序列比较。子类只需实现 GetEqualityComponents 方法，逐一 yield return 参与比较的属性即可。
///
/// 使用场景：
/// - Money（金额）、Period（账期）、ContractStatus（合同状态）等业务概念
/// - 任何没有独立生命周期、仅用于描述实体属性的模型
///
/// 注意：若值对象包含集合类型属性，集合的相等性比较需要额外处理（本基类仅适用于无集合的简单值对象）。
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// 获取参与相等性比较的属性分量
    /// 子类需实现此方法，逐项 yield return 参与比较的属性值。
    /// 属性分量的顺序影响 Equals 的判定结果，建议保持稳定的返回顺序。
    /// </summary>
    /// <returns>参与相等性比较的属性值枚举</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// 判断当前值对象是否与另一个对象相等
    /// 基于 <see cref="GetEqualityComponents"/> 返回的所有属性分量按序逐一比较。
    /// 要求比较对象的类型与当前类型完全一致（不支持子类型比较）。
    /// </summary>
    /// <param name="obj">待比较的对象</param>
    /// <returns>如果所有分量相等则返回 true，否则返回 false</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// 获取值对象的哈希码
    /// 通过对所有属性分量的哈希码进行异或运算（XOR）聚合生成。
    /// 满足 GetHashCode 与 Equals 一致的约定：相等的值对象具有相同的哈希码。
    /// </summary>
    /// <returns>聚合后的哈希码</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
}
