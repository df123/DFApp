import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  LotteryDto,
  CreateUpdateLotteryDto,
  LotteryGroupDto,
  LotteryResultDto,
  CreateUpdateLotteryResultDto,
  ConstsDto,
  GenerateRandomNumbersDto,
  LotterySimulationDto,
  StatisticsDto,
  StatisticsWinRequestDto,
  StatisticsWinItemDto,
  StatisticsWinItemRequestDto,
  LotteryCombinationDto,
  DeleteByTermNumberDto,
  WinningStatisticsDto,
  CompoundLotteryInputDto,
  CompoundLotteryResultDto
} from "../types/business";

// 彩票基础 API
export class LotteryApi {
  private baseUrl = "/api/app/lottery";

  // GET /api/app/lottery
  async getLotteries(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<LotteryDto>> {
    return http.get(this.baseUrl, { params });
  }

  // POST /api/app/lottery
  async createLottery(request: CreateUpdateLotteryDto): Promise<LotteryDto> {
    return http.post(this.baseUrl, { data: request });
  }

  // PUT /api/app/lottery/{id}
  async updateLottery(
    id: number,
    request: CreateUpdateLotteryDto
  ): Promise<LotteryDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  // DELETE /api/app/lottery/{id}
  async deleteLottery(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  // GET /api/app/lottery-grouped
  async getLotteryGroups(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<LotteryGroupDto>> {
    return http.get(`${this.baseUrl}/grouped`, { params });
  }

  // POST /api/app/lottery-grouped
  async createLotteryGroup(
    request: CreateUpdateLotteryDto
  ): Promise<LotteryGroupDto> {
    return http.post("/api/app/lottery-grouped", { data: request });
  }

  // DELETE /api/app/lottery-grouped/{groupId}
  async deleteLotteryGroup(groupId: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/grouped/${groupId}`);
  }

  // DELETE /api/app/lottery-grouped/by-index-no-and-group-id
  async deleteLotteryGroupByIndexNoAndGroupId(
    indexNo: number,
    groupId: number
  ): Promise<void> {
    return http.request(
      "delete",
      `${this.baseUrl}/lottery-group-by-index-no-and-group-id/${groupId}?indexNo=${indexNo}`
    );
  }

  // GET /api/app/lottery/const
  async getLotteryConsts(): Promise<ConstsDto[]> {
    return http.get(`${this.baseUrl}/lottery-const`);
  }

  // GET /api/app/lottery/latest-index-no
  async getLatestIndexNoByType(lotteryType: string): Promise<number> {
    return http.get(`${this.baseUrl}/latest-index-no-by-type`, {
      params: { lotteryType }
    });
  }

  // GET /api/app/lottery/statistics-win
  async getStatisticsWin(
    purchasedPeriod?: string,
    winningPeriod?: string,
    lotteryType?: string
  ): Promise<StatisticsWinItemDto[]> {
    return http.get(`${this.baseUrl}/statistics-win`, {
      params: { purchasedPeriod, winningPeriod, lotteryType }
    });
  }

  // GET /api/app/lottery/statistics-win-item
  async getStatisticsWinItem(
    params?: StatisticsWinItemRequestDto
  ): Promise<PagedResultDto<StatisticsWinItemDto>> {
    return http.get(`${this.baseUrl}/statistics-win-item`, { params });
  }

  // POST /api/app/compound-lottery
  async calculateCompoundCombination(
    request: CompoundLotteryInputDto
  ): Promise<CompoundLotteryResultDto> {
    return http.post(
      "/api/app/compound-lottery/calculate-compound-combination",
      { data: request }
    );
  }
}

// 彩票结果 API
export class LotteryResultApi {
  private baseUrl = "/api/app/lottery-result";

  // GET /api/app/lottery-result
  async getLotteryResults(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<LotteryResultDto>> {
    return http.get(this.baseUrl, { params });
  }

  // POST /api/app/lottery-result
  async createLotteryResult(
    request: CreateUpdateLotteryResultDto
  ): Promise<LotteryResultDto> {
    return http.post(this.baseUrl, { data: request });
  }

  // PUT /api/app/lottery-result/{id}
  async updateLotteryResult(
    id: number,
    request: CreateUpdateLotteryResultDto
  ): Promise<LotteryResultDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  // DELETE /api/app/lottery-result/{id}
  async deleteLotteryResult(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }
}

// 彩票模拟 API
export class LotterySimulationApi {
  private baseUrl = "/api/app/lottery-sSQSimulation";

  async getSSQSimulations(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<LotterySimulationDto>> {
    return http.get(this.baseUrl, { params });
  }

  async generateSSQSimulation(
    request: GenerateRandomNumbersDto
  ): Promise<LotterySimulationDto> {
    return http.post(`${this.baseUrl}/generate-random-numbers`, {
      data: request
    });
  }

  async deleteSSQByTermNumber(request: DeleteByTermNumberDto): Promise<void> {
    return http.request(
      "delete",
      `${this.baseUrl}/by-term-number?termNumber=${request.termNumber}`
    );
  }

  async getSSQStatistics(termNumber?: number): Promise<StatisticsDto> {
    return http.get(`${this.baseUrl}/statistics`, {
      params: { termNumber }
    });
  }
}

// KL8 彩票模拟 API
export class LotteryKL8SimulationApi {
  private baseUrl = "/api/app/lottery-kL8Simulation";

  // GET /api/app/lottery-kL8Simulation
  async getKL8Simulations(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<LotterySimulationDto>> {
    return http.get(this.baseUrl, { params });
  }

  // POST /api/app/lottery-kL8Simulation
  async createKL8Simulation(
    request: CreateUpdateLotteryDto
  ): Promise<LotterySimulationDto> {
    return http.post(this.baseUrl, { data: request });
  }

  // PUT /api/app/lottery-kL8Simulation/{id}
  async updateKL8Simulation(
    id: string,
    request: CreateUpdateLotteryDto
  ): Promise<LotterySimulationDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  // DELETE /api/app/lottery-kL8Simulation/{id}
  async deleteKL8Simulation(id: string): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  // DELETE /api/app/lottery-kL8Simulation/by-term-number
  async deleteKL8ByTermNumber(request: DeleteByTermNumberDto): Promise<void> {
    return http.request(
      "delete",
      `${this.baseUrl}/by-term-number?termNumber=${request.termNumber}`
    );
  }

  // GET /api/app/lottery-kL8Simulation/statistics
  async getKL8Statistics(): Promise<StatisticsDto> {
    return http.get(`${this.baseUrl}/statistics`);
  }

  // POST /api/app/lottery-kL8Simulation/generate-random-numbers
  async generateKL8Simulation(
    request: GenerateRandomNumbersDto
  ): Promise<boolean> {
    return http.post(`${this.baseUrl}/generate-random-numbers`, {
      data: request
    });
  }

  // POST /api/app/lottery-kL8Simulation/calculate-winning-amount
  async calculateKL8WinningAmount(
    termNumber: number
  ): Promise<WinningStatisticsDto> {
    return http.post(
      `${this.baseUrl}/calculate-winning-amount?termNumber=${termNumber}`
    );
  }

  // GET /api/app/lottery-kL8Simulation/{id}
  async getKL8SimulationById(id: string): Promise<LotterySimulationDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }
}

// 彩票统计 API
export class LotteryStatisticsApi {
  private baseUrl = "/api/app/lottery/statistics-win";

  // GET /api/app/lottery/statistics-win
  async getWinStatistics(
    params?: StatisticsWinRequestDto
  ): Promise<PagedResultDto<StatisticsWinItemDto>> {
    return http.get(this.baseUrl, { params });
  }
}

// 彩票组合 API
export class LotteryCombinationApi {
  private baseUrl = "/api/app/lottery-combination";

  // POST /api/app/lottery-combination
  async generateCombination(
    request: LotteryCombinationDto
  ): Promise<LotteryCombinationDto> {
    return http.post(this.baseUrl, { data: request });
  }
}

// 导出实例
export const lotteryApi = new LotteryApi();
export const lotteryResultApi = new LotteryResultApi();
export const lotterySimulationApi = new LotterySimulationApi();
export const lotteryKL8SimulationApi = new LotteryKL8SimulationApi();
export const lotteryStatisticsApi = new LotteryStatisticsApi();
export const lotteryCombinationApi = new LotteryCombinationApi();

export default {
  lottery: lotteryApi,
  result: lotteryResultApi,
  simulation: lotterySimulationApi,
  kl8Simulation: lotteryKL8SimulationApi,
  statistics: lotteryStatisticsApi,
  combination: lotteryCombinationApi
};
