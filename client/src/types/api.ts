// 登录结果 DTO
export interface LoginResultDto {
  accessToken: string;
  expiresAt: number;
  username: string;
  email: string;
  roles: string[];
  permissions: string[];
}

// ABP 标准分页参数
export interface PagedRequestDto {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
}

// ABP 标准分页响应
export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}

// ABP 应用配置响应
export interface ApplicationConfigurationDto {
  localization: Record<string, any>;
  auth: Record<string, any>;
  setting: Record<string, any>;
  currentUser: Record<string, any>;
  features: Record<string, any>;
  globalFeatures: Record<string, any>;
  multiTenancy: Record<string, any>;
  currentTenant: Record<string, any>;
  timing: Record<string, any>;
  clock: Record<string, any>;
  objectExtensions: Record<string, any>;
}

// API 响应包装（通用）
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    message: string;
    code?: string;
  };
}

// 配置管理类型
export interface ConfigurationInfoDto {
  id: number;
  moduleName: string;
  configurationName: string;
  configurationValue: string;
  remark: string;
  creationTime?: string;
  lastModificationTime?: string;
}

export interface CreateUpdateConfigurationInfoDto {
  moduleName: string;
  configurationName: string;
  configurationValue: string;
  remark?: string;
}

// 动态IP类型
export interface DynamicIPDto {
  id: string; // UUID 格式
  ip: string;
  port: string;
  creationTime?: string;
  lastModificationTime?: string;
}

export interface CreateUpdateDynamicIPDto {
  ip: string;
  port: string;
}

// 记账分类类型
export interface BookkeepingCategoryDto {
  id: number;
  category: string;
  creationTime?: string;
  lastModificationTime?: string;
}

export interface CreateUpdateBookkeepingCategoryDto {
  category: string;
}

// 记账支出类型
export interface BookkeepingExpenditureDto {
  id: number;
  expenditureDate: string; // ISO 8601 格式
  expenditure: number; // double 类型
  remark: string;
  isBelongToSelf: boolean;
  category: BookkeepingCategoryDto;
  categoryId: number;
  creationTime?: string;
  lastModificationTime?: string;
}

export interface CreateUpdateBookkeepingExpenditureDto {
  expenditureDate: string;
  expenditure: number;
  remark?: string;
  isBelongToSelf: boolean;
  categoryId: number;
}

// 支出查询参数
export interface GetExpendituresRequestDto extends PagedRequestDto {
  filter?: string;
  categoryId?: number;
  isBelongToSelf?: boolean;
}

// 图表数据 DTO
export interface ChartJSDto {
  labels: string[];
  datasets: ChartDataset[];
  total: number;
  differenceTotal: number;
}

export interface ChartDataset {
  label: string;
  data: number[];
  backgroundColor: string[];
  borderColor: string[];
  borderWidth: number;
}

// 图表查询参数
export interface GetChartDataRequestDto {
  start?: string;
  end?: string;
  compareType?: number;
  numberType?: string;
  isBelongToSelf?: boolean | null;
}

// 月度支出 DTO
export interface MonthlyExpenditureDto {
  labels: string[];
  totalData: number[];
  selfData: number[];
  nonSelfData: number[];
  totalAverage: number;
  selfAverage: number;
  nonSelfAverage: number;
}

// 电动车类型
export interface ElectricVehicleDto {
  id: string;
  name: string;
  brand?: string;
  model?: string;
  licensePlate?: string;
  purchaseDate?: string;
  batteryCapacity?: number;
  totalMileage: number;
  remark?: string;
  creationTime: string;
  lastModificationTime?: string;
}

export interface CreateUpdateElectricVehicleDto {
  name: string;
  brand?: string;
  model?: string;
  licensePlate?: string;
  purchaseDate?: string;
  batteryCapacity?: number;
  totalMileage: number;
  remark?: string;
}

export interface ElectricVehicleCostDto {
  id: string;
  vehicleId: string;
  costType: CostType;
  costDate: string;
  amount: number;
  isBelongToSelf: boolean;
  remark?: string;
  vehicle?: ElectricVehicleDto;
  creationTime: string;
  lastModificationTime?: string;
}

export interface CreateUpdateElectricVehicleCostDto {
  vehicleId: string;
  costType: CostType;
  costDate: string;
  amount: number;
  isBelongToSelf: boolean;
  remark?: string;
}

export interface ElectricVehicleChargingRecordDto {
  id: string;
  vehicleId: string;
  chargingDate: string;
  energy?: number;
  amount: number;
  vehicle?: ElectricVehicleDto;
  creationTime: string;
  lastModificationTime?: string;
  currentMileage?: number;
  mileageDiff?: number;
}

export interface CreateUpdateElectricVehicleChargingRecordDto {
  vehicleId: string;
  chargingDate: string;
  energy?: number;
  amount: number;
}

export interface GasolinePriceDto {
  id: string;
  province: string;
  date: string;
  price0H?: number;
  price89H?: number;
  price90H?: number;
  price92H?: number;
  price93H?: number;
  price95H?: number;
  price97H?: number;
  price98H?: number;
  creationTime: string;
}

export interface OilCostComparisonDto {
  electricVehicleTotalCost: number;
  electricVehicleMileage: number;
  electricVehicleCostPerKm: number;
  electricChargingCost: number;
  electricOtherCost: number;
  oilVehicleCostPerKm: number;
  oilVehicleTotalCost: number;
  oilVehicleFuelCost: number;
  savings: number;
  savingsPercentage: number;
  province: string;
  currentGasolinePrice: number;
  gasolineGrade: GasolineGrade;
  fuelConsumption: number;
  startDate: string;
  endDate: string;
}

export interface OilCostComparisonRequestDto {
  vehicleId: string;
  oilVehicleMileage: number;
  oilVehicleFuelConsumption: number;
  oilVehicleGasolineGrade: number;
}

export enum CostType {
  Charging = 1,
  Maintenance = 2,
  Insurance = 3,
  Parking = 4,
  Repair = 5,
  Other = 6
}

export enum GasolineGrade {
  H92 = 92,
  H95 = 95,
  H98 = 98
}

// 发送重置密码验证码请求
export interface SendPasswordResetCodeDto {
  email: string;
}

// 验证重置密码令牌请求
export interface VerifyPasswordResetTokenDto {
  token: string;
}

// 验证重置密码令牌响应
export interface VerifyPasswordResetTokenResultDto {
  isValid: boolean;
}

// 重置密码请求
export interface ResetPasswordDto {
  token: string;
  newPassword: string;
}
