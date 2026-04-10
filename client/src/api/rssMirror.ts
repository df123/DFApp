import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "@/types/api";
import type {
  RssMirrorItemDto,
  GetRssMirrorItemsRequestDto,
  WordSegmentStatisticsDto
} from "@/types/business";

class RssMirrorApi {
  private baseUrl = "/api/app/rss-mirror-item";

  /**
   * 获取RSS镜像条目列表
   */
  async getList(
    input: GetRssMirrorItemsRequestDto
  ): Promise<PagedResultDto<RssMirrorItemDto>> {
    return http.get(this.baseUrl, { params: input });
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
   * 获取分词统计
   */
  async getWordSegmentStatistics(
    rssSourceId?: number,
    languageType?: number,
    top: number = 100
  ): Promise<WordSegmentStatisticsDto[]> {
    return http.get(`${this.baseUrl}/word-segment-statistics`, {
      params: { rssSourceId, languageType, top }
    });
  }

  /**
   * 根据分词查询RSS镜像条目
   */
  async getByWordToken(
    wordToken: string,
    params?: PagedRequestDto
  ): Promise<PagedResultDto<RssMirrorItemDto>> {
    return http.get(`${this.baseUrl}/by-word-token`, {
      params: { wordToken, ...params }
    });
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
      params: { videoOnly, enableKeywordFilter }
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
