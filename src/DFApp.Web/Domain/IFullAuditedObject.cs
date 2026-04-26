namespace DFApp.Web.Domain;

/// <summary>
/// 完整审计接口，包含创建、修改和删除信息
/// </summary>
public interface IFullAuditedObject : IAuditedObject, ISoftDelete, IHasDeletionTime, IDeleterId
{
}
