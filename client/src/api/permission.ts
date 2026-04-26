import { http } from "@/utils/http/index";

export interface PermissionDto {
  name: string;
  displayName: string;
  parentName?: string;
  isGranted: boolean;
  allowedProviders: any[];
  grantedProviders: Array<{
    providerName: string;
    providerKey: string;
  }>;
}

export interface PermissionGroupDto {
  name: string;
  displayName: string;
  displayNameKey: string;
  displayNameResource: string;
  permissions: PermissionDto[];
}

export interface PermissionsResultDto {
  entityDisplayName: string;
  groups: PermissionGroupDto[];
}

class PermissionApi {
  private baseUrl = "/api/app/permission-grant-management";

  /**
   * 获取权限列表
   * @param providerType 授予目标类型 ("Role" 或 "User")
   * @param providerKey 提供者标识（角色名称或用户ID）
   */
  async getPermissions(
    providerType: string,
    providerKey: string
  ): Promise<PermissionsResultDto> {
    return http.get(this.baseUrl, {
      params: {
        providerType,
        providerKey
      }
    });
  }

  /**
   * 全量更新权限
   * @param providerType 授予目标类型 ("Role" 或 "User")
   * @param providerKey 提供者标识（角色名称或用户ID）
   * @param permissionNames 要设置的权限名称列表（仅包含需要授予的权限）
   */
  async updatePermissions(
    providerType: string,
    providerKey: string,
    permissionNames: string[]
  ): Promise<void> {
    return http.put(this.baseUrl, {
      data: {
        providerType,
        providerKey,
        permissionNames
      }
    });
  }

  /**
   * 获取用户权限
   * @param userId 用户ID
   */
  async getUserPermissions(userId: string): Promise<PermissionsResultDto> {
    return this.getPermissions("User", userId);
  }

  /**
   * 获取角色权限
   * @param roleName 角色名
   */
  async getRolePermissions(roleName: string): Promise<PermissionsResultDto> {
    return this.getPermissions("Role", roleName);
  }

  /**
   * 更新用户权限
   * @param userId 用户ID
   * @param permissions 权限数据
   */
  async updateUserPermissions(
    userId: string,
    permissions: { name: string; isGranted: boolean }[]
  ): Promise<void> {
    const permissionNames = permissions
      .filter(p => p.isGranted)
      .map(p => p.name);
    return this.updatePermissions("User", userId, permissionNames);
  }

  /**
   * 更新角色权限
   * @param roleName 角色名
   * @param permissions 权限数据
   */
  async updateRolePermissions(
    roleName: string,
    permissions: { name: string; isGranted: boolean }[]
  ): Promise<void> {
    const permissionNames = permissions
      .filter(p => p.isGranted)
      .map(p => p.name);
    return this.updatePermissions("Role", roleName, permissionNames);
  }
}

// 导出单例实例
export const permissionApi = new PermissionApi();

// 导出用于 Composition API 的 hook
export function usePermissionApi() {
  return permissionApi;
}
