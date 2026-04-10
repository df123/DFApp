import { http } from "@/utils/http";

// RSS获取请求DTO
export interface RssFetchRequestDto {
  url?: string;
  maxItems?: number;
  query?: string;
  proxyUrl?: string;
  proxyUsername?: string;
  proxyPassword?: string;
}

// RSS条目DTO
export interface RssItemDto {
  title: string;
  link: string;
  description: string;
  publishDate?: string;
  author: string;
  category: string;
  seeders?: number;
  leechers?: number;
  downloads?: number;
  extensions: Record<string, string>;
}

// RSS获取响应DTO
export interface RssFetchResponseDto {
  success: boolean;
  message: string;
  items: RssItemDto[];
  totalCount: number;
  requestUrl: string;
  statusCode: number;
  responseTime: number;
  rawContent: string;
}

// RSS获取 API
export class RssFetchApi {
  private baseUrl = "/api/app/rss-fetch";

  // POST /api/app/rss-fetch
  async fetchRssFeed(
    request: RssFetchRequestDto
  ): Promise<RssFetchResponseDto> {
    return http.post(this.baseUrl, { data: request });
  }
}

// 导出实例
export const rssFetchApi = new RssFetchApi();
