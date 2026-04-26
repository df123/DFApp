import { http } from "@/utils/http";
import type { PagedResultDto } from "@/types/api";
import type {
  RssSubscriptionDto,
  CreateUpdateRssSubscriptionDto,
  GetRssSubscriptionsRequestDto
} from "@/types/business";

class RssSubscriptionApi {
  private baseUrl = "/api/app/rss-subscription";

  /**
   * 获取订阅列表
   */
  async getList(
    params?: GetRssSubscriptionsRequestDto
  ): Promise<PagedResultDto<RssSubscriptionDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取订阅详情
   */
  async get(id: number): Promise<RssSubscriptionDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建订阅
   */
  async create(
    input: CreateUpdateRssSubscriptionDto
  ): Promise<RssSubscriptionDto> {
    return http.post(this.baseUrl, { data: input });
  }

  /**
   * 更新订阅
   */
  async update(
    id: number,
    input: CreateUpdateRssSubscriptionDto
  ): Promise<RssSubscriptionDto> {
    return http.put(`${this.baseUrl}/${id}`, { data: input });
  }

  /**
   * 删除订阅
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 启用/禁用订阅
   */
  async toggleEnable(id: number): Promise<void> {
    return http.post(`${this.baseUrl}/${id}/toggle-enable`);
  }
}

export const rssSubscriptionApi = new RssSubscriptionApi();
