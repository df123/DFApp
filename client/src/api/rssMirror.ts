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
    return http.get(this.baseUrl, {
      params: {
        skipCount: (pageIndex - 1) * pageSize,
        maxResultCount: pageSize,
        sorting: input?.sorting,
        rssSourceId: input?.rssSourceId,
        filter: input?.filter,
        startTime: input?.startTime,
        endTime: input?.endTime,
        isDownloaded: input?.isDownloaded
      }
    });
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
