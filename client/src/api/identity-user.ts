import { http } from "@/utils/http/index";

export interface IdentityUserDto {
  id: string;
  userName: string;
  name?: string;
  surname?: string;
  email?: string;
  emailConfirmed?: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed?: boolean;
  twoFactorEnabled?: boolean;
  lockoutEnabled?: boolean;
  lockoutEnd?: string;
  concurrencyStamp?: string;
  creationTime?: string;
  isActive?: boolean;
  roleNames?: string[];
}

export interface CreateIdentityUserDto {
  userName: string;
  name?: string;
  surname?: string;
  email?: string;
  phoneNumber?: string;
  password?: string;
  roleNames?: string[];
  isActive?: boolean;
}

export interface UpdateIdentityUserDto {
  userName: string;
  name?: string;
  surname?: string;
  email?: string;
  phoneNumber?: string;
  concurrencyStamp?: string;
  roleNames?: string[];
  isActive?: boolean;
}

export interface GetIdentityUsersInput {
  pageIndex?: number;
  pageSize?: number;
  filter?: string;
}

class IdentityUserApi {
  private baseUrl = "/api/app/user-management";

  /**
   * 获取用户列表
   */
  async getUsers(input?: GetIdentityUsersInput): Promise<{
    items: IdentityUserDto[];
    totalCount: number;
  }> {
    const pageIndex = input?.pageIndex ?? 1;
    const pageSize = input?.pageSize ?? 100;
    return http.get(this.baseUrl, {
      params: {
        skipCount: (pageIndex - 1) * pageSize,
        maxResultCount: pageSize,
        filter: input?.filter
      }
    });
  }

  /**
   * 获取所有用户
   */
  async getAllUsers(): Promise<IdentityUserDto[]> {
    const result = await this.getUsers({ pageSize: 1000 });
    return result.items;
  }

  /**
   * 根据ID获取用户
   */
  async getUser(id: string): Promise<IdentityUserDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建用户
   */
  async createUser(input: CreateIdentityUserDto): Promise<void> {
    return http.post(this.baseUrl, { data: input });
  }

  /**
   * 更新用户
   */
  async updateUser(id: string, input: UpdateIdentityUserDto): Promise<void> {
    return http.put(`${this.baseUrl}/${id}`, { data: input });
  }

  /**
   * 删除用户
   */
  async deleteUser(id: string): Promise<void> {
    return http.delete(`${this.baseUrl}/${id}`);
  }
}

// 导出单例实例
export const identityUserApi = new IdentityUserApi();

// 导出用于 Composition API 的 hook
export function useIdentityUserApi() {
  return identityUserApi;
}
