import { http } from "@/utils/http";
import type {
  PagedRequestDto,
  PagedResultDto,
  ElectricVehicleDto,
  CreateUpdateElectricVehicleDto,
  ElectricVehicleCostDto,
  CreateUpdateElectricVehicleCostDto,
  ElectricVehicleChargingRecordDto,
  CreateUpdateElectricVehicleChargingRecordDto,
  GasolinePriceDto,
  OilCostComparisonDto,
  OilCostComparisonRequestDto
} from "@/types/api";

class ElectricVehicleApi {
  private baseUrl = "/api/app/electric-vehicle";

  async getVehicles(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<ElectricVehicleDto>> {
    return http.get(this.baseUrl, { params });
  }

  async getVehicle(id: string): Promise<ElectricVehicleDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  async createVehicle(
    request: CreateUpdateElectricVehicleDto
  ): Promise<ElectricVehicleDto> {
    return http.post(this.baseUrl, { data: request });
  }

  async updateVehicle(
    id: string,
    request: CreateUpdateElectricVehicleDto
  ): Promise<ElectricVehicleDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  async deleteVehicle(id: string): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }
}

class ElectricVehicleCostApi {
  private baseUrl = "/api/app/electric-vehicle-cost";

  async getCosts(
    params?: any
  ): Promise<PagedResultDto<ElectricVehicleCostDto>> {
    return http.get(this.baseUrl, { params });
  }

  async getCost(id: string): Promise<ElectricVehicleCostDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  async createCost(
    request: CreateUpdateElectricVehicleCostDto
  ): Promise<ElectricVehicleCostDto> {
    return http.post(this.baseUrl, { data: request });
  }

  async updateCost(
    id: string,
    request: CreateUpdateElectricVehicleCostDto
  ): Promise<ElectricVehicleCostDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  async deleteCost(id: string): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }

  async getOilCostComparison(
    params: OilCostComparisonRequestDto
  ): Promise<OilCostComparisonDto> {
    return http.get(`${this.baseUrl}/oil-cost-comparison`, { params });
  }
}

class ElectricVehicleChargingRecordApi {
  private baseUrl = "/api/app/electric-vehicle-charging-record";

  async getChargingRecords(
    params?: any
  ): Promise<PagedResultDto<ElectricVehicleChargingRecordDto>> {
    return http.get(this.baseUrl, { params });
  }

  async getChargingRecord(
    id: string
  ): Promise<ElectricVehicleChargingRecordDto> {
    return http.get(`${this.baseUrl}/${id}`);
  }

  async createChargingRecord(
    request: CreateUpdateElectricVehicleChargingRecordDto
  ): Promise<ElectricVehicleChargingRecordDto> {
    return http.post(this.baseUrl, { data: request });
  }

  async updateChargingRecord(
    id: string,
    request: CreateUpdateElectricVehicleChargingRecordDto
  ): Promise<ElectricVehicleChargingRecordDto> {
    return http.request("put", `${this.baseUrl}/${id}`, { data: request });
  }

  async deleteChargingRecord(id: string): Promise<void> {
    return http.request("delete", `${this.baseUrl}/${id}`);
  }
}

class GasolinePriceApi {
  private baseUrl = "/api/app/gasoline-price";

  async getPrices(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<GasolinePriceDto>> {
    return http.get(this.baseUrl, { params });
  }

  async getLatestPrice(province: string): Promise<GasolinePriceDto> {
    return http.get(`${this.baseUrl}/latest-price`, { params: { province } });
  }

  async refreshPrices(): Promise<void> {
    return http.post(`${this.baseUrl}/refresh-gasoline-prices`);
  }
}

export const electricVehicleApi = new ElectricVehicleApi();
export const electricVehicleCostApi = new ElectricVehicleCostApi();
export const electricVehicleChargingRecordApi =
  new ElectricVehicleChargingRecordApi();
export const gasolinePriceApi = new GasolinePriceApi();

export function useElectricVehicleApi() {
  return electricVehicleApi;
}

export function useElectricVehicleCostApi() {
  return electricVehicleCostApi;
}

export function useElectricVehicleChargingRecordApi() {
  return electricVehicleChargingRecordApi;
}

export function useGasolinePriceApi() {
  return gasolinePriceApi;
}
