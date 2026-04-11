import { http } from "@/utils/http/index";

export interface RoleDto {
  id: string;
  name: string;
  displayName?: string;
  isDefault?: boolean;
  isPublic?: boolean;
}

export interface CreateRoleDto {
  name: string;
  isDefault?: boolean;
  isPublic?: boolean;
}

export interface UpdateRoleDto {
  name: string;
  isDefault?: boolean;
  isPublic?: boolean;
  concurrencyStamp?: string;
}

export interface GetRolesInput {
  pageIndex?: number;
  pageSize?: number;
  filter?: string;
}

class RoleApi {
  private baseUrl = "/api/app/role-management";

  /**
   * 获取角色列表
   */
  async getRoles(input?: GetRolesInput): Promise<{
    items: RoleDto[];
    totalCount: number;
  }> {
    return http.get(this.baseUrl, {
      params: {
        pageIndex: input?.pageIndex ?? 1,
        pageSize: input?.pageSize ?? 100,
        filter: input?.filter
      }
    });
  }

  /**
   * 获取所有角色
   */
  async getAllRoles(): Promise<RoleDto[]> {
    const result = await this.getRoles({ pageSize: 1000 });
    return result.items;
  }

  /**
   * 根据ID获取角色
   */
  async getRole(id: string): Promise<RoleDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建角色
   */
  async createRole(input: CreateRoleDto): Promise<void> {
    return http.post(this.baseUrl, { data: input });
  }

  /**
   * 更新角色
   */
  async updateRole(id: string, input: UpdateRoleDto): Promise<void> {
    return http.put(`${this.baseUrl}/${id}`, { data: input });
  }

  /**
   * 删除角色
   */
  async deleteRole(id: string): Promise<void> {
    return http.delete(`${this.baseUrl}/${id}`);
  }
}

// 导出单例实例
export const roleApi = new RoleApi();

// 导出用于 Composition API 的 hook
export function useRoleApi() {
  return roleApi;
}
