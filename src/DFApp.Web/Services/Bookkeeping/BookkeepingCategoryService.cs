using System.Threading.Tasks;
using DFApp.Bookkeeping;
using DFApp.Web.Data;
using DFApp.Web.Data.Bookkeeping;
using DFApp.Web.DTOs.Bookkeeping;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.Bookkeeping;

/// <summary>
/// 记账分类服务
/// </summary>
public class BookkeepingCategoryService : CrudServiceBase<BookkeepingCategory, long, BookkeepingCategoryDto, CreateUpdateBookkeepingCategoryDto, CreateUpdateBookkeepingCategoryDto>
{
    private readonly IBookkeepingExpenditureRepository _bookkeepingExpenditureRepository;
    private readonly BookkeepingMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="bookkeepingExpenditureRepository">记账支出仓储接口</param>
    public BookkeepingCategoryService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<BookkeepingCategory, long> repository,
        IBookkeepingExpenditureRepository bookkeepingExpenditureRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _bookkeepingExpenditureRepository = bookkeepingExpenditureRepository;
    }

    /// <summary>
    /// 创建记账分类
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>记账分类 DTO</returns>
    public override async Task<BookkeepingCategoryDto> CreateAsync(CreateUpdateBookkeepingCategoryDto input)
    {
        // 检查是否已存在相同分类
        var exists = await Repository.AnyAsync(x => x.Category == input.Category);
        if (exists)
        {
            throw new BusinessException("类型已经存在无需添加");
        }

        return await base.CreateAsync(input);
    }

    /// <summary>
    /// 删除记账分类
    /// </summary>
    /// <param name="id">主键 ID</param>
    public override async Task DeleteAsync(long id)
    {
        // 检查是否有支出记录
        if (await _bookkeepingExpenditureRepository.AnyAsync(x => x.CategoryId == id))
        {
            throw new BusinessException("不能删除此类型，因为此类型有开支记录");
        }

        await base.DeleteAsync(id);
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">记账分类实体</param>
    /// <returns>记账分类 DTO</returns>
    protected override BookkeepingCategoryDto MapToGetOutputDto(BookkeepingCategory entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>记账分类实体</returns>
    protected override BookkeepingCategory MapToEntity(CreateUpdateBookkeepingCategoryDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">记账分类实体</param>
    protected override void MapToEntity(CreateUpdateBookkeepingCategoryDto input, BookkeepingCategory entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.Category = mapped.Category;
    }
}
