# 用户管理页面

## 修改密码对话框（2026-09-10 安全更新）

配合后端 `/api/app/user-management/change-password` 的安全加固，对话框行为更新：

- **目标为当前登录用户本人**：显示"原密码"字段且必填，提交时携带 `currentPassword`，
  由服务端校验原密码正确后才允许修改。
- **目标为其他用户**：无需原密码；但服务端要求调用者持有静态 `admin` 角色
  （仅持有可委托的 `UserManagement.ChangePassword` 权限不再足够，管理员账户一律仅 admin 角色可改）。

判定逻辑：`isSelfPasswordChange` 比较目标用户名与登录用户名（`useUserStoreHook().username`）。
