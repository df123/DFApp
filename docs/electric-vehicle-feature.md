# 电动车成本跟踪功能实现总结

## 功能概述

实现了完整的电动车成本跟踪系统，包括车辆管理、成本记录、充电记录、油电对比等功能。

## 技术架构

### 后端
- **框架**: ASP.NET Core 10.0 + ABP Framework
- **数据库**: SQLite (DFApp.db)
- **认证**: OpenID Connect (OpenIddict)

### 前端
- **框架**: Vue 3 + TypeScript
- **UI组件**: Element Plus
- **路由**: Vue Router
- **HTTP客户端**: Axios

## 后端实现

### Domain 层 (`src/DFApp.Domain/ElectricVehicle/`)

#### 实体类

**ElectricVehicle.cs** - 电动车实体
```csharp
public class ElectricVehicle : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? LicensePlate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? BatteryCapacity { get; set; }  // kWh
    public decimal TotalMileage { get; set; }     // km
    public string? Remark { get; set; }
}
```

**ElectricVehicleCost.cs** - 成本记录实体
```csharp
public class ElectricVehicleCost : AuditedAggregateRoot<Guid>
{
    public Guid VehicleId { get; set; }
    public CostType CostType { get; set; }  // 1-6
    public DateTime CostDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsBelongToSelf { get; set; }  // 个人/家庭
    public string? Remark { get; set; }
}
```

**ElectricVehicleChargingRecord.cs** - 充电记录实体
```csharp
public class ElectricVehicleChargingRecord : AuditedAggregateRoot<Guid>
{
    public Guid VehicleId { get; set; }
    public DateTime ChargingDate { get; set; }
    public string? StationName { get; set; }
    public int? ChargingDuration { get; set; }      // 分钟
    public decimal? Energy { get; set; }            // kWh
    public decimal Amount { get; set; }
    public int? StartSOC { get; set; }             // %
    public int? EndSOC { get; set; }               // %
    public bool IsBelongToSelf { get; set; }
    public string? Remark { get; set; }
}
```

**GasolinePrice.cs** - 油价实体
```csharp
public class GasolinePrice : AuditedAggregateRoot<Guid>
{
    public string Province { get; set; }       // 省份
    public DateTime Date { get; set; }         // 日期
    public decimal? Price0H { get; set; }      // 0号柴油
    public decimal? Price89H { get; set; }     // 89号汽油
    public decimal? Price90H { get; set; }     // 90号汽油
    public decimal? Price92H { get; set; }     // 92号汽油
    public decimal? Price93H { get; set; }     // 93号汽油
    public decimal? Price95H { get; set; }     // 95号汽油
    public decimal? Price97H { get; set; }     // 97号汽油
    public decimal? Price98H { get; set; }     // 98号汽油
}
```

#### 枚举定义 (`src/DFApp.Domain.Shared/ElectricVehicle/Enums.cs`)

```csharp
public enum CostType
{
    Charging = 1,      // 充电
    Maintenance = 2,    // 保养
    Insurance = 3,      // 保险
    Parking = 4,        // 停车
    Repair = 5,         // 维修
    Other = 6           // 其他
}

public enum GasolineGrade
{
    H92 = 92,
    H95 = 95,
    H98 = 98
}
```

### Application 层 (`src/DFApp.Application/ElectricVehicle/`)

#### 应用服务

**ElectricVehicleService.cs**
- CRUD 操作
- 继承自 `CrudAppService`

**ElectricVehicleCostService.cs**
- CRUD 操作
- **GetOilCostComparisonAsync** - 油电对比计算

**ElectricVehicleChargingRecordService.cs**
- CRUD 操作
- 筛选支持

**GasolinePriceService.cs**
- CRUD 操作
- **GetLatestPriceAsync** - 获取最新油价
- **RefreshGasolinePricesAsync** - 刷新油价（调用 Tanshu API）

#### 油电对比算法

```csharp
// 油费计算公式
oilVehicleFuelCost = (fuelConsumption / 100) * gasolinePrice * totalMileage;

// 节省金额
savings = oilVehicleTotalCost - electricVehicleTotalCost;

// 节省比例
savingsPercentage = (savings / oilVehicleTotalCost) * 100;
```

### Background 层 (`src/DFApp.Application/Background/`)

**GasolinePriceRefreshWorker.cs**
- Quartz 后台任务
- 每天凌晨 2:00 执行
- 自动刷新全国油价数据

### 数据库

**迁移名称**: `AddElectricVehicles`
**数据库文件**: `/home/df/dfapp/DFApp/DFApp.db`

#### 数据库表

1. **AppElectricVehicle** - 电动车表
2. **AppElectricVehicleCost** - 成本记录表
3. **AppElectricVehicleChargingRecord** - 充电记录表
4. **AppGasolinePrice** - 油价表

### 权限配置 (`src/DFApp.Application/Permissions/`)

```csharp
public static class DFAppPermissions
{
    public const string ElectricVehicleDefault = "DFApp.ElectricVehicle";
    public const string ElectricVehicleCreate = "DFApp.ElectricVehicle.Create";
    public const string ElectricVehicleEdit = "DFApp.ElectricVehicle.Edit";
    public const string ElectricVehicleDelete = "DFApp.ElectricVehicle.Delete";

    public const string ElectricVehicleCostDefault = "DFApp.ElectricVehicleCost";
    public const string ElectricVehicleCostCreate = "DFApp.ElectricVehicleCost.Create";
    // ... 其他权限
}
```

## 前端实现

### API 层 (`src/api/electric-vehicle.ts`)

#### API 类

**ElectricVehicleApi**
```typescript
class ElectricVehicleApi {
  async getVehicles(params?: PagedRequestDto): Promise<PagedResultDto<ElectricVehicleDto>>
  async getVehicle(id: string): Promise<ElectricVehicleDto>
  async createVehicle(request: CreateUpdateElectricVehicleDto): Promise<ElectricVehicleDto>
  async updateVehicle(id: string, request: CreateUpdateElectricVehicleDto): Promise<ElectricVehicleDto>
  async deleteVehicle(id: string): Promise<void>
}
```

**ElectricVehicleCostApi**
```typescript
class ElectricVehicleCostApi {
  async getCosts(params?: any): Promise<PagedResultDto<ElectricVehicleCostDto>>
  async createCost(request: CreateUpdateElectricVehicleCostDto): Promise<ElectricVehicleCostDto>
  async updateCost(id: string, request: CreateUpdateElectricVehicleCostDto): Promise<ElectricVehicleCostDto>
  async deleteCost(id: string): Promise<void>
  async getOilCostComparison(params: OilCostComparisonRequestDto): Promise<OilCostComparisonDto>
}
```

**ElectricVehicleChargingRecordApi**
```typescript
class ElectricVehicleChargingRecordApi {
  async getChargingRecords(params?: any): Promise<PagedResultDto<ElectricVehicleChargingRecordDto>>
  async createChargingRecord(request: CreateUpdateElectricVehicleChargingRecordDto): Promise<ElectricVehicleChargingRecordDto>
  async updateChargingRecord(id: string, request: CreateUpdateElectricVehicleChargingRecordDto): Promise<ElectricVehicleChargingRecordDto>
  async deleteChargingRecord(id: string): Promise<void>
}
```

**GasolinePriceApi**
```typescript
class GasolinePriceApi {
  async getPrices(params?: PagedRequestDto): Promise<PagedResultDto<GasolinePriceDto>>
  async getLatestPrice(province: string): Promise<GasolinePriceDto>
  async refreshPrices(province: string): Promise<void>
}
```

### 视图层 (`src/views/electric-vehicle/`)

#### 页面组件

1. **vehicles/index.vue** - 车辆管理
   - 车辆列表
   - 新增/编辑/删除车辆
   - 分页

2. **costs/index.vue** - 成本记录
   - 成本列表
   - 6种成本类型
   - 个人/家庭归属
   - 关联车辆

3. **charging/index.vue** - 充电记录
   - 充电列表
   - 充电站名称
   - 电量、金额、SOC等

4. **statistics/index.vue** - 统计分析
   - 电车总花费
   - 电车行驶里程
   - 油车成本（相同里程）
   - 节省金额和比例

5. **oil-config/index.vue** - 油车参数配置
   - 省份选择
   - 汽油标号
   - 百公里油耗
   - API Key 配置

### 类型定义 (`src/types/api.ts`)

```typescript
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

export enum CostType {
  Charging = 1,
  Maintenance = 2,
  Insurance = 3,
  Parking = 4,
  Repair = 5,
  Other = 6
}
```

### 路由配置 (`src/router/modules/electric-vehicle.ts`)

```typescript
export default {
  path: "/electric-vehicle",
  name: "ElectricVehicle",
  redirect: "/electric-vehicle/vehicles",
  meta: { title: "电车管理", icon: "ep:car" },
  children: [
    { path: "/electric-vehicle/vehicles", meta: { title: "车辆管理" } },
    { path: "/electric-vehicle/costs", meta: { title: "成本记录" } },
    { path: "/electric-vehicle/charging", meta: { title: "充电记录" } },
    { path: "/electric-vehicle/statistics", meta: { title: "统计分析" } },
    { path: "/electric-vehicle/oil-config", meta: { title: "油车参数配置" } }
  ]
}
```

## API 端点列表

| 端点 | 方法 | 描述 |
|------|------|------|
| `/api/app/electric-vehicle` | GET | 获取车辆列表 |
| `/api/app/electric-vehicle` | POST | 创建车辆 |
| `/api/app/electric-vehicle/{id}` | GET | 获取车辆详情 |
| `/api/app/electric-vehicle/{id}` | PUT | 更新车辆 |
| `/api/app/electric-vehicle/{id}` | DELETE | 删除车辆 |
| `/api/app/electric-vehicle-cost` | GET | 获取成本记录列表 |
| `/api/app/electric-vehicle-cost` | POST | 创建成本记录 |
| `/api/app/electric-vehicle-cost/{id}` | PUT | 更新成本记录 |
| `/api/app/electric-vehicle-cost/{id}` | DELETE | 删除成本记录 |
| `/api/app/electric-vehicle-cost/oil-cost-comparison` | GET | 油电对比 |
| `/api/app/electric-vehicle-charging-record` | GET | 获取充电商城列表 |
| `/api/app/electric-vehicle-charging-record` | POST | 创建充电商城 |
| `/api/app/electric-vehicle-charging-record/{id}` | PUT | 更新充电商城 |
| `/api/app/electric-vehicle-charging-record/{id}` | DELETE | 删除充电商城 |
| `/api/app/gasoline-price` | GET | 获取油价列表 |
| `/api/app/gasoline-price/latest-price` | GET | 获取最新油价 |
| `/api/app/gasoline-price/price-by-date` | GET | 获取指定日期油价 |
| `/api/app/gasoline-price/refresh-gasoline-prices` | POST | 刷新油价 |

## 使用流程

### 1. 车辆管理
1. 进入"电车管理" → "车辆管理"
2. 点击"新增车辆"按钮
3. 填写车辆信息（名称、品牌、型号、车牌号等）
4. 保存

### 2. 成本记录
1. 进入"电车管理" → "成本记录"
2. 点击"新增"按钮
3. 选择日期、金额、归属（个人/家庭）、类型（充电/保养/保险/停车/维修/其他）
4. 关联车辆
5. 保存

### 3. 充电记录
1. 进入"电车管理" → "充电记录"
2. 点击"新增"按钮
3. 填写充电商城信息（日期、充电站、电量、金额、SOC等）
4. 关联车辆
5. 保存

### 4. 统计分析
1. 进入"电车管理" → "统计分析"
2. 查看油电对比数据
3. 查看节省金额和比例

### 5. 油价配置
1. 进入"电车管理" → "油车参数配置"
2. 配置所在省份
3. 选择汽油标号（92/95/98）
4. 设置百公里油耗
5. 配置 Tanshu API Key（https://www.tanshuapi.com/）
6. 点击"保存配置"和"刷新油价"

## 服务状态

### 后端服务
- **进程 PID**: 73019
- **运行地址**: https://localhost:44369
- **状态**: ✅ 运行中

### 前端服务
- **进程 PID**: 75547
- **运行地址**: http://localhost:8848
- **状态**: ✅ 运行中

## 数据库状态

- **数据库文件**: /home/df/dfapp/DFApp/DFApp.db
- **迁移状态**: ✅ 已完成
- **表数量**: 4 张（AppElectricVehicle, AppElectricVehicleCost, AppElectricVehicleChargingRecord, AppGasolinePrice）

## 关键特性

1. ✅ **Guid 主键** - 所有实体使用 Guid 类型
2. ✅ **无软删除** - 移除了 IsDeleted 字段
3. ✅ **6种成本类型** - 包含"其他"类型
4. ✅ **油电对比** - 自动计算油车成本和节省金额
5. ✅ **油价自动刷新** - 每天凌晨 2 点自动刷新
6. ✅ **个人/家庭归属** - 支持区分成本归属
7. ✅ **完整 CRUD** - 所有实体支持增删改查
8. ✅ **分页支持** - 列表页面支持分页

## 后台任务

**GasolinePriceRefreshWorker**
- 执行时间：每天凌晨 2:00
- 功能：刷新全国各省市油价数据
- 数据源：Tanshu API (https://api.tanshuapi.com/api/youjia/v1/index)

## 构建状态

- 后端构建：✅ 成功（0 错误）
- 前端构建：✅ 成功（0 错误）
- 数据库迁移：✅ 成功

## 测试访问

- 前端地址: http://localhost:8848
- 后端 Swagger: https://localhost:44369/swagger
- API 基础路径: https://localhost:44369/api/app/

## 注意事项

1. **Tanshu API Key**: 需要在油车参数配置页面设置 API Key 才能刷新油价
2. **油电对比**: 需要先添加车辆和成本记录才能显示对比数据
3. **归属区分**: "个人"表示自己支付的，"家庭"表示家庭公共的
4. **SOC 计算**: 起始和结束电量可以留空，非必填
