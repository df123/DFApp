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

  // POST /api/app/lottery-data-fetch/fetch-lottery-data
  async fetchLotteryData(
    request: LotteryDataFetchRequestDto
  ): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch-lottery-data`, { data: request });
  }

  // GET /api/app/lottery-data-fetch/fetch-ssq-latest-data
  async fetchSSQLatestData(): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch-sSQLatest-data`);
  }

  // GET /api/app/lottery-data-fetch/fetch-kl8-latest-data
  async fetchKL8LatestData(): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/fetch-kL8Latest-data`);
  }

  // GET /api/app/lottery-data-fetch/test-lottery-api-connection
  async testLotteryApiConnection(
    lotteryType?: string
  ): Promise<LotteryDataFetchResponseDto> {
    return http.post(`${this.baseUrl}/test-lottery-api-connection`, {
      params: { lotteryType }
    });
  }
}

// 导出实例
export const lotteryDataFetchApi = new LotteryDataFetchApi();
