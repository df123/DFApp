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
- **RefreshGasolinePricesAsync** - 刷新油价（调用 GasolinePriceRefresher）

**GasolinePriceRefresher.cs**
- 内部服务（不暴露为 HTTP API）
- **RefreshGasolinePricesAsync** - 调用 Tanshu API 刷新油价，供后台任务使用

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
- **运行地址**: http://localhost:9949
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
- 执行时间：每天晚上 21:00
- 功能：刷新全国各省市油价数据
- 数据源：Tanshu API (https://api.tanshuapi.com/api/youjia/v1/index)
- 权限：`RefreshGasolinePricesAsync` 方法添加 `[AllowAnonymous]` 属性，允许后台任务调用

## 构建状态

- 后端构建：✅ 成功（0 错误）
- 前端构建：✅ 成功（0 错误）
- 数据库迁移：✅ 成功

## 测试访问

- 前端地址: http://localhost:9949
- 后端 Swagger: https://localhost:44369/swagger
- API 基础路径: https://localhost:44369/api/app/

## 注意事项

1. **Tanshu API Key**: 需要在油车参数配置页面设置 API Key 才能刷新油价
2. **油电对比**: 需要先添加车辆和成本记录才能显示对比数据
3. **归属区分**: "个人"表示自己支付的，"家庭"表示家庭公共的
4. **SOC 计算**: 起始和结束电量可以留空，非必填

## 2026-08-25 前端契约修正

- 充电记录创建/更新类型补充 `currentMileage`，页面仅保留后端实际支持的字段。
- 油车配置通过模块配置接口读取，不再向分页接口传递不支持的 `moduleName`。
- 油电对比请求使用 `startDate`、`endDate` 与可选 `vehicleId`、`isBelongToSelf` 字段。

## 2026-09-04 当前总里程独立更新

### 背景
总里程原先只能随充电记录新增/编辑联动更新（且必须记得填写），家用充电按月结算、
外部充电穿插，里程与充电记录天然不同步，导致车辆当前里程长期停留在旧值。

### 改动
- 数据库：`AppElectricVehicle` 新增 `MileageLastUpdatedTime` 列（`sql/27-add-ev-mileage-updated-time.sql`）。
- 后端：`PUT /api/app/electric-vehicle/{id}/mileage`（权限 `ElectricVehicle.Edit`）——
  只更新总里程与更新时间，负数/车辆不存在返回业务错误；
  充电记录联动更新里程时同样刷新 `MileageLastUpdatedTime`。
- 前端：车辆管理页总里程列下方显示"更新于 X 日期"（小字灰色），
  操作列新增"里程"按钮弹出输入框单独记录当前表显里程（预填当前值）。

### 验证（本地）
- 独立更新 6234.5 → 成功且返回更新时间；负数/不存在车辆/未授权分别返回 400/400/401。
- 充电记录带 currentMileage=6300 → 车辆里程与更新时间同步刷新。
- 前端 typecheck 通过。

### 2026-09-04 充电记录页移除"当前里程/里程差值"
使用模式为按月统一登记（家充月度结算），不逐次充电登记，充电记录的当前里程
长期缺失导致"里程差值"列常为空。处理：
- 删除充电记录页"里程差值"列与前端差值计算、"当前里程"列与"当前总里程"表单项。
- 后端字段与充电→车辆里程联动保留（API 兼容；历史已填数据继续参与时间段统计的
  里程差计算，无数据时统计回退用车辆总里程）。
- 里程统一由车辆管理页"里程"按钮独立维护（按月更新即可）。

### 2026-09-04 里程快照记录：油电对比的里程数据源替代
充电记录页移除"当前里程"后，时间段统计的区间行驶里程改由**里程快照**提供：
- 新表 `AppElectricVehicleMileageRecord`（`sql/28-create-ev-mileage-record.sql`，含历史充电里程回填）。
- 快照来源：车辆管理页"里程"按钮每次更新、充电记录联动更新（API 兼容路径）。
- 统计锚点算法：起点=开始日期前最近一条快照（无则取范围内最早），终点=结束日期前最近一条，
  区间里程=终点−起点；无快照时回退车辆总里程。
- 验证：2-4月区间对比返回 417 km（04-12:5000 − 02-23:4583），与预期一致。
使用节奏：每月结算时点一次"里程"更新表显读数即可，统计自动取相邻快照差值。

### 2026-09-04 充电记录 CurrentMileage 列彻底移除
迁移完成后的收尾清理（部署顺序：27 → 28 → 29 → 新后端）：
- `sql/29-drop-charging-record-current-mileage.sql` 删除 `AppElectricVehicleChargingRecord.CurrentMileage` 列。
- 实体/DTO/前端类型同步移除该字段；充电记录创建/更新不再联动车辆里程，
  `UpdateVehicleTotalMileageAsync` 及其调用一并删除。
- 油电对比的油费分段算法从"按充电记录里程分段"改为"按里程快照分段"：
  相邻快照段里程 × 段结束时点油价（快照按月更新即"按月油价"对比），
  段和与区间锚点里程一致；无快照时回退车辆总里程 + 最新油价兜底。
- 验证：删列后 2-4 月对比区间里程仍为 417 km（快照数据源），油费分段正常。

### 2026-09-04 双屏幕尺寸适配（4K@200% 桌面 + iPhone 17）
Playwright 视口测试（桌面 1920×1080=4K@200% 有效视口；移动 402×874=iPhone 17 CSS 视口）+ 识图子代理验收。修复：
- 表格列 `width` 改 `min-width`（车辆/充电/成本三页）——窄屏横向滚动替代挤压裁切，操作列保持固定。
- 统计页响应式栅格：统计卡 `:xs=24 :sm=12 :md=6`、图表卡 `:xs=24 :md=12`——窄屏单列堆叠。
- 分页器全局允许换行（element-plus.scss），窄屏不再右侧裁切。
- 成本/充电列表接口前端从 `/paged` 切换到 `/filtered`（后者回填车辆名），修复"车辆"列空白。
- 油电对比柱状图：图例固定顶部 + grid 上边距 60/右边距 110，修复图例压图与右轴名裁切。
验收：桌面 5 项全过；移动端表格可滚动、统计卡单列堆叠、里程弹窗适配优秀；控制台 0 错误。


## 里程记录查看与删除（2026-09-04）

新的里程快照表 `AppElectricVehicleMileageRecord` 提供查看入口：车辆管理页操作列新增「记录」按钮，打开该车辆的里程记录对话框。

### 后端

- `ElectricVehicleMileageRecordDto`（DTOs/ElectricVehicle/ElectricVehicleDto.cs）：Id/VehicleId/Mileage/RecordedTime/Remark/CreationTime
- `ElectricVehicleService.GetMileageRecordsAsync(vehicleId)`：按 RecordedTime 倒序返回全部快照
- `ElectricVehicleService.DeleteMileageRecordAsync(recordId)`：删除单条快照（用于清理误录），不存在时抛 `BusinessException("里程记录不存在")`；不影响车辆当前总里程字段
- `ElectricVehicleController`：
  - `GET /api/app/electric-vehicle/{id}/mileage-records`（ElectricVehicle.Default 权限）
  - `DELETE /api/app/electric-vehicle/mileage-records/{recordId}`（ElectricVehicle.Delete 权限）

### 前端（client/src/views/electric-vehicle/vehicles/index.vue）

- 操作列「里程 / 记录 / 编辑 / 删除」，记录对话框列：记录时间、里程(km)、较上次（与更早一条快照的差值，正数绿色/负数红色/最早一条显示 -）、备注、删除
- 删除前确认框提示"删除后油电对比将不再使用该快照"
- 对话框 `max-width: 94%` 适配窄屏；表格列 min-width，移动端横向滚动 + 操作列固定右侧

### 注意

删除快照会改变油电对比的区间里程锚点计算（统计页），属预期行为——快照即统计依据。

### 已知坑：Id 存储格式不一致导致删除 400（2026-09-04 修复）

sql/28 初版用 `lower(hex(randomblob(16)))` 生成的主键是 32 位**无连字符**字符串，而 SqlSugar `GetByIdAsync(Guid)` 的查询参数是**带连字符**格式，SQLite TEXT 列精确匹配失败——迁移来的快照删除时报 400「里程记录不存在」（API 新建的记录带连字符，不受影响）。

- sql/28 已修正为生成带连字符 UUID（新环境不再复现）
- sql/30 幂等修复存量数据：把 32 位无连字符 Id 规范化为带连字符小写
