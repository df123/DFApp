import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  ExternalLinkDto,
  CreateUpdateExternalLinkDto
} from "../types/business";

class ExternalLinkApi {
  private baseUrl = "/api/app/external-link";

  /**
   * 创建外部链接
   */
  async create(request: CreateUpdateExternalLinkDto): Promise<ExternalLinkDto> {
    return http.post(this.baseUrl, { data: request });
  }

  /**
   * 获取外部链接列表
   */
  async getList(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<ExternalLinkDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取单个外部链接
   */
  async get(id: number): Promise<ExternalLinkDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 更新外部链接
   */
  async update(
    id: number,
    request: CreateUpdateExternalLinkDto
  ): Promise<ExternalLinkDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  /**
   * 删除外部链接
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 获取外部链接状态
   */
  async getExternalLinkStatus(): Promise<boolean> {
    return http.get(`${this.baseUrl}/external-link`);
  }

  /**
   * 删除外部链接文件
   */
  async deleteFile(id: number): Promise<boolean> {
    return http.request("delete", `${this.baseUrl}/${id}/file`);
  }
}

// 导出单例实例
export const externalLinkApi = new ExternalLinkApi();

// 导出用于 Composition API 的 hook
export function useExternalLinkApi() {
  return externalLinkApi;
}

export default externalLinkApi;
