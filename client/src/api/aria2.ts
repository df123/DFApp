import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  TellStatusResultDto,
  AddDownloadRequestDto,
  AddDownloadResponseDto,
  AddTorrentRequestDto,
  BatchAddTorrentRequestDto,
  BatchAddUriRequestDto,
  Aria2GlobalStatDto,
  Aria2TaskDto,
  Aria2TaskDetailDto,
  PauseTasksRequestDto,
  StopTasksRequestDto,
  RemoveTasksRequestDto,
  Aria2ConnectionStatusDto,
  IpGeolocationDto
} from "../types/business";

class Aria2Api {
  private baseUrl = "/api/app/aria2";
  private manageUrl = "/api/app/aria2-manage";

  /**
   * 获取下载状态列表
   */
  async getAria2Status(
    params?: PagedRequestDto & { filter?: string }
  ): Promise<PagedResultDto<TellStatusResultDto>> {
    return http.get(`${this.baseUrl}/filtered-list`, { params });
  }

  /**
   * 获取单个外部链接
   */
  async getExternalLink(id: number): Promise<string> {
    return http.get(`${this.baseUrl}/${id}/external-link`);
  }

  /**
   * 获取所有外部链接
   * @param videoOnly 是否只获取视频文件链接
   */
  async getAllExternalLinks(videoOnly: boolean = true): Promise<string[]> {
    return http.get(`${this.baseUrl}/all-external-links`, {
      params: { videoOnly }
    });
  }

  /**
   * 删除单个记录
   */
  async delete(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  /**
   * 删除所有记录
   */
  async deleteAll(): Promise<void> {
    return http.request("delete", `${this.baseUrl}/all`);
  }

  /**
   * 清空下载目录
   */
  async clearDownloadDirectory(): Promise<void> {
    return http.request("delete", `${this.baseUrl}/clear-directory`);
  }

  /**
   * 添加下载任务
   */
  async addDownload(
    request: AddDownloadRequestDto
  ): Promise<AddDownloadResponseDto> {
    return http.post(`${this.baseUrl}/add-download`, { data: request });
  }

  // ============ Aria2 管理相关 API (直接连接 aria2 RPC) ============

  /**
   * 获取 Aria2 全局状态
   */
  async getGlobalStat(): Promise<Aria2GlobalStatDto> {
    return http.get(`${this.manageUrl}/global-stat`);
  }

  /**
   * 获取活跃任务列表
   */
  async getActiveTasks(): Promise<Aria2TaskDto[]> {
    return http.get(`${this.manageUrl}/active-tasks`);
  }

  /**
   * 获取等待任务列表
   */
  async getWaitingTasks(): Promise<Aria2TaskDto[]> {
    return http.get(`${this.manageUrl}/waiting-tasks`);
  }

  /**
   * 获取停止任务列表
   */
  async getStoppedTasks(
    offset: number = 0,
    num: number = 100
  ): Promise<Aria2TaskDto[]> {
    return http.get(`${this.manageUrl}/stopped-tasks`, {
      params: { offset, num }
    });
  }

  /**
   * 获取任务状态
   */
  async getTaskStatus(gid: string): Promise<Aria2TaskDto> {
    return http.get(`${this.manageUrl}/task-status`, {
      params: { gid }
    });
  }

  /**
   * 获取任务详情（包含peers和文件列表）
   */
  async getTaskDetail(gid: string): Promise<Aria2TaskDetailDto> {
    return http.get(`${this.manageUrl}/task-detail`, {
      params: { gid }
    });
  }

  /**
   * 添加 URI 下载任务
   */
  async addUri(request: AddDownloadRequestDto): Promise<string> {
    return http.post(`${this.manageUrl}/add-uri`, { data: request });
  }

  /**
   * 批量添加 URI 下载任务（每条链接创建独立任务）
   */
  async batchAddUri(request: BatchAddUriRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/batch-add-uri`, { data: request });
  }

  /**
   * 添加种子文件下载任务
   */
  async addTorrent(request: AddTorrentRequestDto): Promise<string> {
    return http.post(`${this.manageUrl}/add-torrent`, { data: request });
  }

  /**
   * 批量添加种子文件下载任务
   */
  async batchAddTorrent(request: BatchAddTorrentRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/batch-add-torrent`, { data: request });
  }

  /**
   * 暂停任务
   */
  async pauseTasks(request: PauseTasksRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/pause`, { data: request });
  }

  /**
   * 暂停所有任务
   */
  async pauseAllTasks(): Promise<string> {
    return http.post(`${this.manageUrl}/pause-all`);
  }

  /**
   * 恢复任务
   */
  async unpauseTasks(request: PauseTasksRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/unpause`, { data: request });
  }

  /**
   * 恢复所有任务
   */
  async unpauseAllTasks(): Promise<string> {
    return http.post(`${this.manageUrl}/unpause-all`);
  }

  /**
   * 停止任务
   */
  async stopTasks(request: StopTasksRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/stop`, { data: request });
  }

  /**
   * 删除停止的任务
   */
  async removeTasks(request: RemoveTasksRequestDto): Promise<string[]> {
    return http.post(`${this.manageUrl}/remove`, { data: request });
  }

  /**
   * 清空停止的任务
   */
  async purgeDownloadResult(): Promise<string> {
    return http.post(`${this.manageUrl}/purge`);
  }

  /**
   * 获取 Aria2 连接状态
   */
  async getConnectionStatus(): Promise<Aria2ConnectionStatusDto> {
    return http.get(`${this.manageUrl}/connection-status`);
  }

  /**
   * 批量查询 IP 地理位置（通过后端代理）
   */
  async getIpGeolocation(ips: string[]): Promise<IpGeolocationDto[]> {
    return http.post(`${this.manageUrl}/ip-geolocation`, {
      data: { Ips: ips }
    });
  }
}

// 导出单例实例
export const aria2Api = new Aria2Api();

// 导出用于 Composition API 的 hook
export function useAria2Api() {
  return aria2Api;
}

export default aria2Api;
