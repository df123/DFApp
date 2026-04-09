import { http } from "@/utils/http";
import type {
  PagedRequestDto,
  PagedResultDto,
  DynamicIPDto,
  CreateUpdateDynamicIPDto
} from "@/types/api";

class DynamicIpApi {
  private baseUrl = "/api/app/dynamic-ip";

  /**
   * 获取动态IP列表
   */
  async getDynamicIPs(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<DynamicIPDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 创建动态IP
   */
  async createDynamicIP(
    request: CreateUpdateDynamicIPDto
  ): Promise<DynamicIPDto> {
    return http.post(this.baseUrl, { data: request });
  }

  /**
   * 更新动态IP
   */
  async updateDynamicIP(
    id: string,
    request: CreateUpdateDynamicIPDto
  ): Promise<DynamicIPDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  /**
   * 删除动态IP
   */
  async deleteDynamicIP(id: string): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }
}

// 导出单例实例
export const dynamicIpApi = new DynamicIpApi();

// 导出用于 Composition API 的 hook
export function useDynamicIpApi() {
  return dynamicIpApi;
}
