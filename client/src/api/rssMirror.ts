import { http } from "@/utils/http";
import type { PagedResultDto } from "@/types/api";
import type {
  RssMirrorItemDto,
  GetRssMirrorItemsRequestDto
} from "@/types/business";

class RssMirrorApi {
  private baseUrl = "/api/app/rss-mirror-item";

  /**
   * 获取RSS镜像条目列表
   */
  async getList(
    input: GetRssMirrorItemsRequestDto
  ): Promise<PagedResultDto<RssMirrorItemDto>> {
    const pageIndex = input?.pageIndex ?? 1;
    const pageSize = input?.pageSize ?? 10;
    // 构建参数对象，过滤掉空字符串和 undefined，避免后端模型绑定失败
    const params: Record<string, any> = {
      skipCount: (pageIndex - 1) * pageSize,
      maxResultCount: pageSize
    };
    if (input?.sorting) params.sorting = input.sorting;
    if (input?.rssSourceId != null) params.rssSourceId = input.rssSourceId;
    if (input?.filter) params.filter = input.filter;
    if (input?.startTime) params.startTime = input.startTime;
    if (input?.endTime) params.endTime = input.endTime;
    if (input?.isDownloaded != null) params.isDownloaded = input.isDownloaded;
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取RSS镜像条目详情
   */
  async get(id: number): Promise<RssMirrorItemDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 删除RSS镜像条目
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 批量删除RSS镜像条目
   */
  async deleteMany(ids: number[]): Promise<void> {
    return http.request("delete", `${this.baseUrl}/many`, { data: ids });
  }

  /**
   * 清空所有RSS镜像条目
   */
  async clearAll(): Promise<void> {
    return http.request("delete", `${this.baseUrl}/clear-all`);
  }

  /**
   * 下载到Aria2
   */
  async downloadToAria2(
    id: number,
    videoOnly: boolean = false,
    enableKeywordFilter: boolean = false
  ): Promise<string> {
    return http.post(`${this.baseUrl}/${id}/download-to-aria2`, {
      params: { videoOnly, enableKeywordFilter },
      timeout: 30000
    });
  }
}

// 导出单例实例
export const rssMirrorApi = new RssMirrorApi();

// 导出用于 Composition API 的 hook
export function useRssMirrorApi() {
  return rssMirrorApi;
}

export default rssMirrorApi;
