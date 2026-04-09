import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  KeywordFilterRuleDto,
  CreateUpdateKeywordFilterRuleDto,
  KeywordFilterTestResultDto
} from "../types/business";

class KeywordFilterApi {
  private baseUrl = "/api/app/keyword-filter-rule";

  /**
   * 获取关键词过滤规则列表
   */
  async getList(
    params?: PagedRequestDto & { filter?: string }
  ): Promise<PagedResultDto<KeywordFilterRuleDto>> {
    return http.get(this.baseUrl, { params });
  }

  /**
   * 获取单个规则
   */
  async get(id: number): Promise<KeywordFilterRuleDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 创建规则
   */
  async create(
    data: CreateUpdateKeywordFilterRuleDto
  ): Promise<KeywordFilterRuleDto> {
    return http.post(this.baseUrl, { data });
  }

  /**
   * 更新规则
   */
  async update(
    id: number,
    data: CreateUpdateKeywordFilterRuleDto
  ): Promise<KeywordFilterRuleDto> {
    return http.put(`${this.baseUrl}/${id}`, { data });
  }

  /**
   * 删除规则
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 测试文件名过滤
   */
  async testFilter(fileName: string): Promise<KeywordFilterTestResultDto> {
    return http.post(`${this.baseUrl}/test-filter`, { data: { fileName } });
  }

  /**
   * 批量测试文件名过滤
   */
  async testFilterBatch(
    fileNames: string[]
  ): Promise<KeywordFilterTestResultDto[]> {
    return http.post(`${this.baseUrl}/test-filter-batch`, {
      data: { fileNames }
    });
  }

  /**
   * 获取匹配的规则列表（调试用）
   */
  async getMatchingRules(fileName: string): Promise<any[]> {
    return http.get(`${this.baseUrl}/matching-rules`, { params: { fileName } });
  }

  /**
   * 启用/禁用规则
   */
  async toggleRule(id: number, isEnabled: boolean): Promise<void> {
    return http.post(`${this.baseUrl}/${id}/toggle-rule`, {
      data: { isEnabled }
    });
  }
}

// 导出单例实例
export const keywordFilterApi = new KeywordFilterApi();

// 导出用于 Composition API 的 hook
export function useKeywordFilterApi() {
  return keywordFilterApi;
}

export default keywordFilterApi;
