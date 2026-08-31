# SQL 迁移脚本

本目录包含数据库迁移和运维脚本，用于从 ABP Framework 迁移到轻量级 ASP.NET Core 架构。

## 执行顺序

### 阶段 A：ABP 迁移清理

| 顺序 | 文件 | 说明 |
|------|------|------|
| 01 | `cleanup-soft-deleted-users.sql` | 清理软删除用户（cms, down, cms2）及其关联数据 |
| 02 | `migrate-abpusers-table.sql` | 精简 AbpUsers 表（31列→10列） |
| 03 | `remove-abp-legacy-columns-from-app-tables.sql` | 移除 App 表的 ABP 遗留列（ExtraProperties, IsDeleted） |
| 04 | `add-missing-audit-columns.sql` | 补充缺失的审计列（CreatorId, LastModifierId 等） |
| 05 | `fix-guid-case-migration.sql` | 统一所有表中的 GUID 为小写 |
| 06 | `migrate-to-app-permission-grants.sql` | 创建 AppPermissionGrants 表并从旧表迁移数据 |
| 07 | `cleanup-all-abp-tables.sql` | 删除 31 张 ABP 遗留表 |
| 08 | `verify-identity-data.sql` | 验证迁移后数据的完整性 |

### 阶段 B：业务表创建

| 顺序 | 文件 | 说明 |
|------|------|------|
| 09 | `rss-subscription-tables.sql` | 创建 RSS 订阅和下载表 |
| 10 | `add-disk-space-check.sql` | RSS 下载表添加磁盘空间检查字段 |

### 阶段 C：权限种子数据

| 顺序 | 文件 | 说明 |
|------|------|------|
| 11 | `grant-admin-all-permissions.sql` | 为 admin 角色授予所有 DFApp.* 权限 |

### 阶段 D：运维工具（按需使用）

| 顺序 | 文件 | 说明 |
|------|------|------|
| 12 | `set-default-password.sql` | 为无密码用户设置默认密码 "123456" |
| 13 | `reset-passwords.sql` | 重置所有用户密码 |

### 阶段 E：后续业务与安全迁移

| 顺序 | 文件 | 说明 |
|------|------|------|
| 14 | `fix-default-password.sql` | 修正默认密码数据 |
| 15 | `grant-admin-new-permissions.sql` | 为 admin 角色补齐新增权限 |
| 16 | `add-missing-concurrency-stamp.sql` | 补充并发戳字段 |
| 17 | `drop-media-external-link-extra-properties.sql` | 清理媒体外链遗留字段 |
| 18 | `migrate-electric-vehicle-cost-type.sql` | 迁移电动车成本类型 |
| 19 | `add-downloader-config.sql` | 新增下载器配置 |
| 20 | `add-retrieval-protection-config.sql` | 新增取回保护配置 |
| 21 | `remove-downloader-enabled-config.sql` | 移除 DownloaderEnabled 配置 |
| 22 | `add-downloader-speed-samples.sql` | 新增下载速度采样表 |
| 23 | `split-rss-mirror-to-standalone-db.sql` | 拆分 RSS 镜像独立数据库 |
| 24 | `add-lottery-proxy-token-config.sql` | 新增彩票代理令牌配置项 |
| 25 | `add-telegram-management-permission.sql` | 新增 Telegram 管理权限并授予 admin |
| 26 | `invalidate-lottery-proxy-token.sql` | 使已泄露的彩票代理令牌失效 |

## 使用说明

### 新环境部署

全新环境按照编号顺序执行脚本；已部署环境只执行尚未执行的编号。12、13、26 为安全或运维脚本，执行前必须确认影响。

### 执行方式

```bash
sqlite3 /path/to/DFApp.db < sql/01-cleanup-soft-deleted-users.sql
```

或批量执行：

```bash
for f in sql/*.sql; do
  echo "执行: $f"
  sqlite3 /path/to/DFApp.db < "$f"
done
```

## 注意事项

- 所有脚本针对 SQLite 数据库编写
- 执行前请备份数据库
- 脚本已按依赖关系排序，请勿打乱执行顺序
