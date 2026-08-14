import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  MediaInfoDto,
  CreateUpdateMediaInfoDto,
  ChartDataDto
} from "../types/business";

class MediaInfoApi {
  private baseUrl = "/api/app/media-info";

  /**
   * 获取图表数据
   */
  async getChartData(): Promise<ChartDataDto> {
    return http.get(`${this.baseUrl}/chart-data`);
  }

  /**
   * 删除无效项目
   */
  async deleteInvalidItems(): Promise<void> {
    return http.request("delete", `${this.baseUrl}/invalid`);
  }

  /**
   * 一键清理已标记取回但服务器仍保留的媒体文件
   * @returns 删除数、跳过数（被有效外链引用）、无需处理数
   */
  async cleanupRetrievedFiles(): Promise<{
    deleted: number;
    skipped: number;
    noFile: number;
  }> {
    return http.post(`${this.baseUrl}/cleanup-retrieved-files`);
  }

  /**
   * 创建媒体信息
   */
  async create(request: CreateUpdateMediaInfoDto): Promise<MediaInfoDto> {
    return http.post(this.baseUrl, { data: request });
  }

  /**
   * 获取媒体信息列表
   */
  async getList(
    params?: PagedRequestDto & { filter?: string }
  ): Promise<PagedResultDto<MediaInfoDto>> {
    return http.get(`${this.baseUrl}/paged`, { params });
  }

  /**
   * 获取单个媒体信息
   */
  async get(id: number): Promise<MediaInfoDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  /**
   * 更新媒体信息
   */
  async update(
    id: number,
    request: CreateUpdateMediaInfoDto
  ): Promise<MediaInfoDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  /**
   * 删除媒体信息
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }
}

// 导出单例实例
export const mediaInfoApi = new MediaInfoApi();

// 导出用于 Composition API 的 hook
export function useMediaInfoApi() {
  return mediaInfoApi;
}

export default mediaInfoApi;
