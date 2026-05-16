# OpenCode Orchestrator 模式配置说明

## 概述
本配置实现了类似 Kilo Code 的 Orchestrator 模式，可以将复杂任务分配给专门的子模式处理。

## 架构

```
Orchestrator (主模式)
    ├── Architect (架构师)
    ├── Code (代码实现)
    ├── Ask (问答解释)
    ├── Debug (调试诊断)
    └── Review (代码审查)
```

## 模式说明

### 1. Orchestrator (协调器)
- **类型**: 主模式 (primary)
- **职责**: 协调复杂任务，将任务分解并分配给专门的子模式
- **权限**: 可以调用所有子模式
- **使用场景**: 复杂的多步骤任务、需要多种专业技能的任务

### 2. Architect (架构师)
- **类型**: 子模式 (subagent)
- **职责**: 系统设计、架构规划、任务分解
- **权限**: 只读访问，可读取文件和搜索代码
- **使用场景**: 需求分析、系统设计、技术选型

### 3. Code (代码实现)
- **类型**: 子模式 (subagent)
- **职责**: 编写高质量、可维护的代码
- **权限**: 完整的文件读写权限，可执行构建命令
- **使用场景**: 功能实现、代码编写、重构优化

### 4. Ask (问答解释)
- **类型**: 子模式 (subagent)
- **职责**: 回答技术问题，提供解释和指导
- **权限**: 只读访问，可搜索代码
- **使用场景**: 技术咨询、代码解释、学习指导

### 5. Debug (调试诊断)
- **类型**: 子模式 (subagent)
- **职责**: 诊断问题、分析错误、提供解决方案
- **权限**: 可执行构建和测试命令，只读访问代码
- **使用场景**: 错误调试、性能分析、问题排查

### 6. Review (代码审查)
- **类型**: 子模式 (subagent)
- **职责**: 代码质量评估、最佳实践检查
- **权限**: 只读访问，不可修改代码
- **使用场景**: 代码审查、质量评估、改进建议

## 使用方式

### 1. 直接使用 Orchestrator
```bash
# 启动 opencode 并选择 orchestrator 模式
opencode --agent orchestrator
```

### 2. 通过 @ 提及使用子模式
```txt
@architect 帮我设计一个用户认证系统
@code 实现用户注册功能
@debug 这个API返回500错误，帮我排查
@review 审查这个控制器的代码质量
```

### 3. 在 orchestrator 模式中自动分配
当使用 orchestrator 模式时，它会根据任务类型自动分配给合适的子模式。

## 配置文件位置

```
.opencode/
├── agents/
│   ├── architect.md    # 架构师模式配置
│   ├── code.md         # 代码实现模式配置
│   ├── ask.md          # 问答模式配置
│   ├── debug.md        # 调试模式配置
│   └── review.md       # 代码审查模式配置
└── opencode.json       # 主配置文件
```

## 自定义配置

### 修改模型
在对应的 `.md` 文件中修改 `model` 字段：
```yaml
model: anthropic/claude-sonnet-4-20250514
```

### 修改温度参数
调整 `temperature` 字段：
- 0.0-0.2: 更专注、确定性
- 0.3-0.5: 平衡、适合开发
- 0.6-1.0: 更有创意

### 修改权限
在 `permission` 部分调整工具权限：
```yaml
permission:
  edit: allow/deny/ask
  bash: allow/deny/ask
  webfetch: allow/deny/ask
```

## 工作流程示例

### 示例 1: 新功能开发
1. **Orchestrator**: 接收需求"实现用户个人资料编辑功能"
2. **Architect**: 设计系统架构和API接口
3. **Code**: 实现后端API和前端组件
4. **Review**: 审查代码质量
5. **Debug**: 测试并修复问题

### 示例 2: 问题排查
1. **Orchestrator**: 接收问题"用户登录失败"
2. **Debug**: 分析日志和错误信息
3. **Ask**: 解释可能的原因
4. **Code**: 实施修复方案

## 注意事项

1. **模型选择**: 根据任务复杂度选择合适的模型
2. **权限控制**: 确保子模式有适当的权限
3. **上下文管理**: 每个子模式在独立上下文中运行
4. **结果汇总**: 子模式的结果会自动汇总到主任务

## 故障排除

### 子模式无法调用
- 检查 `opencode.json` 中的 `permission.task` 配置
- 确保子模式名称拼写正确
- 验证子模式配置文件语法

### 权限问题
- 检查子模式的 `permission` 配置
- 确保必要的工具权限已启用
- 查看 opencode 日志获取详细信息

## 参考资源

- [OpenCode 官方文档](https://opencode.ai/docs)
- [Kilo Code Orchestrator 模式](https://blog.kilo.ai/p/how-kilo-codes-orchestrator-mode)
- [OpenCode Agents 配置](https://opencode.ai/docs/agents)