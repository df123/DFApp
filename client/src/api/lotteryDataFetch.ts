import { http } from "@/utils/http";

// 彩票数据获取请求DTO
export interface LotteryDataFetchRequestDto {
  dayStart?: string;
  dayEnd?: string;
  pageNo?: number;
  lotteryType?: string;
  saveToDatabase?: boolean;
}

// 彩票数据获取响应DTO
export interface LotteryDataFetchResponseDto {
  success: boolean;
  message: string;
  data?: any;
  savedCount: number;
  requestUrl: string;
  statusCode: number;
  responseTime: number;
}

// 彩票数据获取 API
export class LotteryDataFetchApi {
  private baseUrl = "/api/app/lottery-data-fetch";

  // POST /api/app/lottery-data-fetch/fetch
  async fetchLotteryData(
    request: LotteryDataFetchRequestDto
  ): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch`, { data: request });
  }

  // POST /api/app/lottery-data-fetch/fetch-ssq
  async fetchSSQLatestData(): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch-ssq`);
  }

  // POST /api/app/lottery-data-fetch/fetch-kl8
  async fetchKL8LatestData(): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch-kl8`);
  }

  // POST /api/app/lottery-data-fetch/test-connection
  async testLotteryApiConnection(
    lotteryType?: string
  ): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/test-connection`, {
      params: { lotteryType }
    });
  }
}

// 导出实例
export const lotteryDataFetchApi = new LotteryDataFetchApi();
