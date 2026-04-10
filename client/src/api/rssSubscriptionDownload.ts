import { http } from "@/utils/http";
import type { PagedResultDto } from "@/types/api";
import type {
  RssSubscriptionDownloadDto,
  GetRssSubscriptionDownloadsRequestDto
} from "@/types/business";

class RssSubscriptionDownloadApi {
  private baseUrl = "/api/app/rss-subscription-download";

  /**
   * 获取下载记录列表
   */
  async getList(
    params?: GetRssSubscriptionDownloadsRequestDto
  ): Promise<PagedResultDto<RssSubscriptionDownloadDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取下载记录详情
   */
  async get(id: number): Promise<RssSubscriptionDownloadDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 删除下载记录
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 批量删除下载记录
   */
  async deleteMany(ids: number[]): Promise<void> {
    return http.request("delete", `${this.baseUrl}/many`, { data: ids });
  }

  /**
   * 清空所有下载记录
   */
  async clearAll(): Promise<void> {
    return http.request("delete", `${this.baseUrl}/clear-all`);
  }

  /**
   * 重试下载
   */
  async retry(id: number): Promise<void> {
    return http.post(`${this.baseUrl}/${id}/retry`);
  }
}

export const rssSubscriptionDownloadApi = new RssSubscriptionDownloadApi();
