namespace DFApp.Web.Domain;

/// <summary>
/// 创建审计接口，只包含创建信息
/// </summary>
public interface ICreationAuditedObject : IHasCreationTime, ICreatorId
{
}
