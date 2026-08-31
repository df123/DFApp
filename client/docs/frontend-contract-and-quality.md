# 前端契约修正与质量检查

## API 契约修正

- 密码重置类型与后端对齐：`userNameOrEmail`、`token`、`newPassword`、`confirmNewPassword`；令牌验证接口返回 `boolean`。
- 电动车充电记录创建/更新类型补充 `currentMileage`，页面移除后端不存在的车站、时长、SOC、归属和备注字段。
- 油车配置改用 `/api/app/configuration-info/module/{moduleName}`，不再给分页接口传递不支持的 `moduleName`。
- 油电对比请求类型改为 `startDate`、`endDate`、可选 `vehicleId`、可选 `isBelongToSelf`。
- RSS 源 API 在前端把 `pageIndex/pageSize` 转换为后端要求的 `skipCount/maxResultCount`，并传递 `filter` 与 `sorting`。
- 彩票数据页按组创建红球与蓝球记录，删除时调用期号加组号接口。
- 彩票统计使用包含 `buyAmount` 的 `StatisticsWinDto`。
- RSS 镜像日期范围的空值改为二元组，保持类型稳定。

## 密码重置入口

后端尚未接入邮件或短信发送服务。登录页已隐藏“忘记密码”入口，直达重置路由时显示“请联系管理员”的安全占位页，不再提交无法送达的验证码请求。

## 质量检查

`package.json` 新增非修复式检查脚本：

```bash
pnpm typecheck
pnpm lint:check
pnpm build
```

现有代码已按 ESLint、Prettier、Stylelint 规则完成格式化。
