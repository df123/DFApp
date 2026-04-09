import { http } from "@/utils/http";
import type { PagedResultDto } from "@/types/api";
import type {
  RssWordSegmentWithItemDto,
  GetRssWordSegmentsRequestDto,
  WordSegmentStatisticsDto
} from "@/types/business";

class RssWordSegmentApi {
  private baseUrl = "/api/app/rss-word-segment";

  /**
   * 获取分词列表（分页）
   */
  async getList(
    input: GetRssWordSegmentsRequestDto
  ): Promise<PagedResultDto<RssWordSegmentWithItemDto>> {
    return http.get(this.baseUrl, { params: input });
  }

  /**
   * 获取分词统计（分页）
   */
  async getStatistics(
    input: GetRssWordSegmentsRequestDto
  ): Promise<PagedResultDto<WordSegmentStatisticsDto>> {
    return http.get(`${this.baseUrl}/statistics`, { params: input });
  }

  /**
   * 删除指定RSS镜像条目的所有分词
   */
  async deleteByItem(rssMirrorItemId: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/by-item/${rssMirrorItemId}`);
  }

  /**
   * 删除指定RSS源的所有分词
   */
  async deleteBySource(rssSourceId: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/by-source/${rssSourceId}`);
  }
}

// 导出单例实例
export const rssWordSegmentApi = new RssWordSegmentApi();

// 导出用于 Composition API 的 hook
export function useRssWordSegmentApi() {
  return rssWordSegmentApi;
}

export default rssWordSegmentApi;
