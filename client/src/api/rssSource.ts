import { http } from "@/utils/http";
import type { PagedResultDto } from "@/types/api";
import type { RssSourceDto, CreateUpdateRssSourceDto } from "@/types/business";

class RssSourceApi {
  private baseUrl = "/api/app/rss-source";

  async getList(params?: {
    pageIndex?: number;
    pageSize?: number;
    filter?: string;
    sorting?: string;
  }): Promise<PagedResultDto<RssSourceDto>> {
    const pageIndex = params?.pageIndex ?? 1;
    const pageSize = params?.pageSize ?? 10;
    return http.get(this.baseUrl, {
      params: {
        ...params,
        skipCount: (pageIndex - 1) * pageSize,
        maxResultCount: pageSize
      }
    });
  }

  /**
   * 获取RSS源详情
   */
  async get(id: number): Promise<RssSourceDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建RSS源
   */
  async create(input: CreateUpdateRssSourceDto): Promise<RssSourceDto> {
    return http.post(this.baseUrl, { data: input });
  }

  /**
   * 更新RSS源
   */
  async update(
    id: number,
    input: CreateUpdateRssSourceDto
  ): Promise<RssSourceDto> {
    return http.put(`${this.baseUrl}/${id}`, { data: input });
  }

  /**
   * 删除RSS源
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 手动触发RSS源抓取
   */
  async triggerFetch(id: number): Promise<void> {
    return http.post(`${this.baseUrl}/${id}/trigger-fetch`);
  }
}

// 导出单例实例
export const rssSourceApi = new RssSourceApi();

// 导出用于 Composition API 的 hook
export function useRssSourceApi() {
  return rssSourceApi;
}

export default rssSourceApi;
