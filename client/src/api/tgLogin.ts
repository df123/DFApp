import { http } from "@/utils/http";
import type { TGChatsDto } from "../types/business";

class TGLoginApi {
  private baseUrl = "/api/app/t-gLogin";

  /**
   * 获取 TG 登录状态
   */
  async getStatus(): Promise<string> {
    return http.post(`${this.baseUrl}/status`);
  }

  /**
   * 配置 TG
   */
  async config(value: string): Promise<string> {
    return http.post(`${this.baseUrl}/config`, {
      params: { value }
    });
  }

  /**
   * 获取 TG 聊天 (403 Forbidden)
   */
  async getChats(): Promise<TGChatsDto> {
    return http.post(`${this.baseUrl}/chats`);
  }
}

// 导出单例实例
export const tgLoginApi = new TGLoginApi();

// 导出用于 Composition API 的 hook
export function useTGLoginApi() {
  return tgLoginApi;
}

export default tgLoginApi;
