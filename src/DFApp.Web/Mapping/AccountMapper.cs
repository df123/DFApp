using System;
using DFApp.Account;
using DFApp.Web.DTOs.Account;
using Riok.Mapperly.Abstractions;
using UserEntity = DFApp.Account.User;
using UserDtoType = DFApp.Web.DTOs.Account.UserDto;
using CreateUserDtoType = DFApp.Web.DTOs.Account.CreateUserDto;
using UpdateUserDtoType = DFApp.Web.DTOs.Account.UpdateUserDto;

namespace DFApp.Web.Mapping;

/// <summary>
/// 账户模块映射器
/// </summary>
[Mapper]
public partial class AccountMapper
{
    /// <summary>
    /// User → UserDto
    /// </summary>
    public partial UserDtoType MapToDto(UserEntity entity);

    /// <summary>
    /// CreateUserDto → User
    /// 忽略密码哈希和审计字段
    /// </summary>
    [MapperIgnoreTarget(nameof(UserEntity.PasswordHash))]
    [MapperIgnoreTarget(nameof(UserEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(UserEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(UserEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(UserEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(UserEntity.LastModifierId))]
    public partial UserEntity MapToEntity(CreateUserDtoType dto);

    /// <summary>
    /// UpdateUserDto → User
    /// 忽略密码哈希和审计字段
    /// </summary>
    [MapperIgnoreTarget(nameof(UserEntity.PasswordHash))]
    [MapperIgnoreTarget(nameof(UserEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(UserEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(UserEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(UserEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(UserEntity.LastModifierId))]
    public partial UserEntity MapToEntity(UpdateUserDtoType dto);
}
