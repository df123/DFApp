# SQL 迁移脚本说明

本目录包含用于数据库迁移的 SQL 脚本。

## remove-openiddict-tables.sql

### 说明
此脚本用于删除 OpenIddict 相关的数据库表。在移除 OpenIddict 并添加 JWT 认证后，这些表已经不再需要。

### 将删除的表
- `OpenIddictApplications`
- `OpenIddictAuthorizations`
- `OpenIddictScopes`
- `OpenIddictTokens`

### 执行前准备
1. **备份数据库**：在执行脚本前，请务必备份数据库文件 `DFApp.db`
   ```bash
   cp DFApp/DFApp.db DFApp/DFApp.db.backup
   ```

2. **停止应用**：确保应用程序已停止运行

### 执行方法

#### 方法一：使用 SQLite 命令行工具
```bash
cd DFApp
sqlite3 DFApp.db < sql/remove-openiddict-tables.sql
```

#### 方法二：使用 SQLite 命令行工具交互式执行
```bash
cd DFApp
sqlite3 DFApp.db
```
然后在 SQLite 提示符下执行：
```sql
.read sql/remove-openiddict-tables.sql
.quit
```

#### 方法三：使用 DB Browser for SQLite
1. 打开 DB Browser for SQLite
2. 打开 `DFApp.db` 文件
3. 点击"执行 SQL"标签
4. 打开 `sql/remove-openiddict-tables.sql` 文件
5. 点击执行按钮

### 验证执行结果
执行完成后，可以使用以下命令验证表是否已被删除：
```bash
cd DFApp
sqlite3 DFApp.db ".tables" | grep -i openiddict
```

如果没有输出，说明 OpenIddict 相关的表已成功删除。

### 注意事项
- 此操作不可逆，请确保已备份数据库
- 删除后，应用程序将不再使用 OpenIddict，而是使用 JWT 认证
- 确保在执行此脚本前，应用程序代码已经完成了从 OpenIddict 到 JWT 的迁移
