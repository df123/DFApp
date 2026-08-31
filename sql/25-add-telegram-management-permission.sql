-- 新增 Telegram 登录管理权限并授予 admin 角色
-- 日期：2026-08-24

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey, CreationTime)
SELECT 'DFApp.TelegramManagement', 'Role', 'admin', datetime('now')
WHERE NOT EXISTS (
    SELECT 1
    FROM AppPermissionGrants
    WHERE PermissionName = 'DFApp.TelegramManagement'
      AND ProviderType = 'Role'
      AND ProviderKey = 'admin'
);
