namespace DFApp.Web.Domain;

/// <summary>
/// 审计接口，包含创建和修改信息
/// </summary>
public interface IAuditedObject : IHasCreationTime, ICreatorId, IHasModificationTime, IModifierId
{
}
