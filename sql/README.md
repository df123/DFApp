# SQL 脚本说明

本目录包含数据库迁移、维护和业务相关的 SQL 脚本。

## 执行前准备

1. **备份数据库**：执行任何脚本前，请务必备份数据库文件
   ```bash
   cp DFApp.db DFApp.db.backup
   ```

2. **停止应用**：确保应用程序已停止运行

## 执行方法

### 方法一：SQLite 命令行
```bash
# 在 /home/df/dfapp/DFApp 目录下
sqlite3 DFApp.db < sql/<脚本文件名>.sql
```

### 方法二：SQLite 交互式
```bash
sqlite3 DFApp.db
.read sql/<脚本文件名>.sql
.quit
```

### 方法三：DB Browser for SQLite
1. 打开 `DFApp.db`
2. 点击"执行 SQL"标签
3. 加载脚本文件并执行

---

## 脚本分类

### 一、ABP 迁移清理脚本（Phase 8）

> 移除 ABP Framework 过程中产生的迁移脚本，按推荐顺序执行。

| 文件名 | 状态 | 说明 |
|--------|------|------|
| `migrate-abpusers-table.sql` | ✅已执行 | AbpUsers 表字段精简迁移（31列→10列） |
| `migrate-identity-entities-to-custom-base-classes.sql` | ✅已执行 | Identity 实体基类迁移记录 |
| `migrate-account-user-entity-to-custom-base-class.sql` | ✅已执行 | User 实体基类迁移记录 |
| `migrate-rss-entities-to-custom-base-classes.sql` | ✅已执行 | RSS 实体基类迁移记录 |
| `fix-permission-grants-data.sql` | ✅已执行 | 修复权限授予数据格式问题 |
| `verify-identity-data.sql` | ✅已执行 | 身份数据完整性验证 |
| `cleanup-all-abp-tables.sql` | ✅已执行 | **统一清理脚本**，删除 ABP 残留的 31 张表（整合了下面的两个脚本） |
| `cleanup-soft-deleted-users.sql` | ✅已执行 | 清理已软删除的用户（3 个） |

#### 废弃脚本（已被 `cleanup-all-abp-tables.sql` 替代）

| 文件名 | 状态 | 说明 |
|--------|------|------|
| `cleanup-abp-obsolete-tables.sql` | ⛔废弃 | ABP 废弃表清理（25 张），功能已整合到 `cleanup-all-abp-tables.sql` |
| `remove-openiddict-tables.sql` | ⛔废弃 | OpenIddict 表清理（4 张），功能已整合到 `cleanup-all-abp-tables.sql` |

### 二、业务脚本

| 文件名 | 状态 | 说明 |
|--------|------|------|
| `rss-subscription-tables.sql` | ✅已执行 | RSS 订阅表创建 |
| `add-disk-space-check.sql` | ✅已执行 | RSS 订阅下载表添加字段 |

### 三、数据修复脚本

| 文件名 | 状态 | 说明 |
|--------|------|------|
| `fix-guid-case-migration.sql` | ✅已执行 | 将所有表中的大写 Guid 统一转为小写（ABP 遗留数据兼容 SqlSugar） |

### 四、运维脚本

| 文件名 | 状态 | 说明 |
|--------|------|------|
| `set-default-password.sql` | 📋待执行 | 设置默认密码（按需使用） |
| `reset-passwords.sql` | 📋待执行 | 重置密码（按需使用） |

---

## 推荐执行顺序

**Phase 8 ABP 迁移（已全部完成）：**

1. `migrate-abpusers-table.sql` — 先精简用户表
2. `migrate-identity-entities-to-custom-base-classes.sql` — Identity 实体基类迁移
3. `migrate-account-user-entity-to-custom-base-class.sql` — User 实体基类迁移
4. `migrate-rss-entities-to-custom-base-classes.sql` — RSS 实体基类迁移
5. `fix-permission-grants-data.sql` — 修复权限数据
6. `verify-identity-data.sql` — 验证身份数据完整性
7. `cleanup-all-abp-tables.sql` — 清理所有 ABP 残留表
8. `cleanup-soft-deleted-users.sql` — 清理软删除用户

**业务脚本（按需）：**

9. `rss-subscription-tables.sql` — 创建 RSS 表
10. `add-disk-space-check.sql` — RSS 表添加字段

**运维脚本（按需使用）：**

11. `set-default-password.sql` — 设置默认密码
12. `reset-passwords.sql` — 重置密码

---

## 注意事项

- 所有删除操作不可逆，执行前务必确认已备份数据库
- 已标记为 ⛔废弃 的脚本不应再执行，其功能已被更完整的脚本替代
- 运维脚本为按需使用，非必要不执行
