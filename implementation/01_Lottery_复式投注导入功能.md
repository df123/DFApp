# Implementation Plan

[Overview]
实现彩票购买系统中复式快乐8和双色球的导入功能。

本功能旨在扩展现有彩票系统的投注能力，支持用户通过Web界面手动输入复式投注号码，系统自动计算所有可能的组合并保存。该功能将集成到现有的彩票模块中，利用现有的数据结构和业务逻辑，提供用户友好的多行输入界面，支持双色球和快乐8两种彩票类型的复式投注。

[Types]  
扩展现有的彩票类型系统以支持复式投注。

详细类型定义：

1. **复式投注输入DTO**
```csharp
public class CompoundLotteryInputDto
{
    public int Period { get; set; }
    public string LotteryType { get; set; }
    public List<string> RedNumbers { get; set; } // 红球号码列表
    public List<string> BlueNumbers { get; set; } // 蓝球号码列表（仅双色球）
    public LotteryKL8PlayType? PlayType { get; set; } // 快乐8玩法类型
}
```

2. **复式投注响应DTO**
```csharp
public class CompoundLotteryResultDto
{
    public int TotalCombinations { get; set; }
    public decimal TotalAmount { get; set; }
    public List<LotteryDto> CreatedLotteries { get; set; }
}
```

3. **快乐8玩法类型枚举**（已存在）
```csharp
public enum LotteryKL8PlayType
{
    Select1 = 1,    // 选一
    Select2 = 2,    // 选二
    Select3 = 3,    // 选三
    Select4 = 4,    // 选四
    Select5 = 5,    // 选五
    Select6 = 6,    // 选六
    Select7 = 7,    // 选七
    Select8 = 8,    // 选八
    Select9 = 9,    // 选九
    Select10 = 10   // 选十
}
```

[Files]
创建新文件并修改现有文件以支持复式投注功能。

详细文件修改计划：

**新文件创建：**
1. `src/DFApp.Application.Contracts/Lottery/CompoundLotteryInputDto.cs` - 复式投注输入DTO
2. `src/DFApp.Application.Contracts/Lottery/CompoundLotteryResultDto.cs` - 复式投注响应DTO
3. `src/DFApp.Application.Contracts/Lottery/ICompoundLotteryService.cs` - 复式投注服务接口
4. `src/DFApp.Application/Lottery/CompoundLotteryService.cs` - 复式投注服务实现
5. `DFApp.Vue/src/views/lottery/components/CompoundLotteryInput.vue` - 前端复式投注输入组件

**现有文件修改：**
1. `src/DFApp.Application.Contracts/Lottery/ILotteryService.cs` - 添加复式投注服务引用
2. `src/DFApp.Application/Lottery/LotteryService.cs` - 集成复式投注服务
3. `DFApp.Vue/src/views/lottery/index.vue` - 添加复式投注界面
4. `DFApp.Vue/src/api/lottery.ts` - 添加复式投注API调用

**配置更新：**
1. `src/DFApp.Application/DFAppApplicationModule.cs` - 注册复式投注服务
2. `src/DFApp.HttpApi/Controllers/LotteryController.cs` - 添加复式投注API端点

[Functions]
扩展彩票服务以支持复式投注的组合计算和保存。

详细函数定义：

**新函数：**
1. `CalculateCompoundCombination(CompoundLotteryInputDto dto)` - 计算复式投注组合
   - 文件路径：`src/DFApp.Application/Lottery/CompoundLotteryService.cs`
   - 功能：根据输入号码计算所有可能的组合
   - 参数：CompoundLotteryInputDto
   - 返回：CompoundLotteryResultDto

2. `GenerateSSQCombinations(List<string> reds, List<string> blues)` - 生成双色球组合
   - 文件路径：`src/DFApp.Application/Lottery/CompoundLotteryService.cs`
   - 功能：计算双色球复式投注的所有组合
   - 参数：红球列表、蓝球列表
   - 返回：组合列表

3. `GenerateKL8Combinations(List<string> numbers, LotteryKL8PlayType playType)` - 生成快乐8组合
   - 文件路径：`src/DFApp.Application/Lottery/CompoundLotteryService.cs`
   - 功能：计算快乐8复式投注的所有组合
   - 参数：号码列表、玩法类型
   - 返回：组合列表

4. `ValidateCompoundInput(CompoundLotteryInputDto dto)` - 验证复式输入
   - 文件路径：`src/DFApp.Application/Lottery/CompoundLotteryService.cs`
   - 功能：验证输入号码的有效性和合理性
   - 参数：CompoundLotteryInputDto
   - 返回：验证结果

**修改函数：**
1. `CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos)` - 扩展以支持复式投注
   - 文件路径：`src/DFApp.Application/Lottery/LotteryService.cs`
   - 修改：增加对复式投注数据的处理逻辑

[Classes]
创建新的服务类来处理复式投注业务逻辑。

详细类定义：

**新类：**
1. **CompoundLotteryService** - 复式投注服务
   - 文件路径：`src/DFApp.Application/Lottery/CompoundLotteryService.cs`
   - 继承：ApplicationService
   - 实现：ICompoundLotteryService
   - 关键方法：
     - CalculateCompoundCombination
     - GenerateSSQCombinations
     - GenerateKL8Combinations
     - ValidateCompoundInput

2. **CompoundLotteryInput** - 前端输入组件
   - 文件路径：`DFApp.Vue/src/views/lottery/components/CompoundLotteryInput.vue`
   - 类型：Vue组件
   - 功能：提供多行号码输入界面
   - 属性：彩票类型、期号、号码列表

**修改类：**
1. **LotteryService** - 现有彩票服务
   - 文件路径：`src/DFApp.Application/Lottery/LotteryService.cs`
   - 修改：注入CompoundLotteryService，提供复式投注入口

[Dependencies]
添加必要的数学组合计算库支持。

依赖修改详情：

**新依赖：**
1. 组合数学计算库（可选）：
   - 如果需要高性能组合计算，可引入 `MathNet.Numerics`
   - 或者使用现有的组合算法实现

**现有依赖利用：**
1. 利用现有的ABP框架基础设施
2. 利用现有的EntityFramework Core数据访问
3. 利用现有的Vue.js前端框架

[Testing]
创建单元测试验证复式投注的组合计算正确性。

测试方案：

**测试文件创建：**
1. `test/DFApp.Application.Tests/Lottery/CompoundLotteryService_Tests.cs` - 复式投注服务测试
2. `DFApp.Vue/src/views/lottery/components/__tests__/CompoundLotteryInput.spec.ts` - 前端组件测试

**测试用例：**
1. 双色球复式组合计算测试
   - 输入：7个红球，2个蓝球
   - 预期：7选6 × 2 = 14种组合
2. 快乐8复式组合计算测试
   - 输入：11个号码，选10玩法
   - 预期：11选10 = 11种组合
3. 输入验证测试
   - 无效号码格式
   - 超出范围号码
   - 重复号码
4. 边界条件测试
   - 最小/最大号码数量
   - 空输入处理

[Implementation Order]
按照依赖关系顺序实现复式投注功能。

实现步骤：

1. **第一步：后端DTO和服务接口**
   - 创建CompoundLotteryInputDto和CompoundLotteryResultDto
   - 创建ICompoundLotteryService接口
   - 注册服务到依赖注入容器

2. **第二步：组合计算算法**
   - 实现GenerateSSQCombinations方法
   - 实现GenerateKL8Combinations方法
   - 添加输入验证逻辑

3. **第三步：服务集成**
   - 实现CompoundLotteryService完整功能
   - 修改LotteryService集成复式投注
   - 添加API控制器端点

4. **第四步：前端组件**
   - 创建CompoundLotteryInput.vue组件
   - 实现多行号码输入界面
   - 添加输入格式验证

5. **第五步：界面集成**
   - 在彩票管理页面集成复式投注组件
   - 添加API调用逻辑
   - 实现结果展示

6. **第六步：测试验证**
   - 编写和运行单元测试
   - 进行集成测试
   - 用户验收测试

这个实现顺序确保了技术依赖的正确解决，从基础的数据结构开始，逐步构建业务逻辑，最后完成用户界面集成。
