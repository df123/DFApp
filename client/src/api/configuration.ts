import { http } from "@/utils/http";
import type {
  PagedRequestDto,
  PagedResultDto,
  ConfigurationInfoDto,
  CreateUpdateConfigurationInfoDto
} from "@/types/api";

class ConfigurationApi {
  private baseUrl = "/api/app/configuration-info";

  /**
   * 获取配置列表
   */
  async getConfigurations(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<ConfigurationInfoDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 创建配置
   */
  async createConfiguration(
    request: CreateUpdateConfigurationInfoDto
  ): Promise<ConfigurationInfoDto> {
    return http.post(this.baseUrl, { data: request });
  }

  /**
   * 更新配置
   */
  async updateConfiguration(
    id: number,
    request: CreateUpdateConfigurationInfoDto
  ): Promise<ConfigurationInfoDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  /**
   * 删除配置
   */
  async deleteConfiguration(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 获取剩余磁盘空间
   */
  async getRemainingDiskSpace(): Promise<string> {
    return http.get(`${this.baseUrl}/remaining-disk-space`);
  }
}

// 导出单例实例
export const configurationApi = new ConfigurationApi();

// 导出用于 Composition API 的 hook
export function useConfigurationApi() {
  return configurationApi;
}
