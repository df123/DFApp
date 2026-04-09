import { http } from "@/utils/http";
import type { LogFileInfoDto } from "@/types/business";

class LogViewerApi {
  private baseUrl = "/api/LogViewer";

  /**
   * 获取日志文件列表
   */
  async getLogFiles(): Promise<LogFileInfoDto[]> {
    return http.get(`${this.baseUrl}/log-files`);
  }

  /**
   * 获取日志内容
   * @param fileName 文件名
   * @param isTail 是否从文件末尾开始读取
   */
  async getLogContent(fileName: string, isTail = true): Promise<string> {
    const params = { fileName, isTail };
    return http.get(`${this.baseUrl}/log-content`, { params });
  }

  /**
   * 下载日志文件
   * @param fileName 文件名
   */
  downloadLogFile(fileName: string): void {
    window.location.href = `${this.baseUrl}/download?fileName=${encodeURIComponent(fileName)}`;
  }
}

// 导出单例实例
export const logViewerApi = new LogViewerApi();

// 导出用于 Composition API 的 hook
export function useLogViewerApi() {
  return logViewerApi;
}
