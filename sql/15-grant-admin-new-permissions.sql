-- 为 admin 角色补齐脚本 11 之后新增的权限
-- 背景：脚本 11 (11-grant-admin-all-permissions.sql) 之后，DFAppPermissions.cs 中
--       新增了 UserManagement、RoleManagement、PermissionGrantManagement、
--       UserRoleManagement、FileFilter、RssSubscription 共 6 个权限组 24 个权限，
--       需要为 admin 角色补齐授予
-- 日期：2026-04-26
--
-- 注意：使用 INSERT OR IGNORE 确保幂等性，重复执行不会出错
--       不指定 Id 字段（自增）

-- ========================================
-- UserManagement 用户管理（5个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Update', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.ChangePassword', 'Role', 'admin');

-- ========================================
-- RoleManagement 角色管理（4个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RoleManagement', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RoleManagement.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RoleManagement.Update', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RoleManagement.Delete', 'Role', 'admin');

-- ========================================
-- PermissionGrantManagement 权限授予管理（3个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.PermissionGrantManagement', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.PermissionGrantManagement.Grant', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.PermissionGrantManagement.Revoke', 'Role', 'admin');

-- ========================================
-- UserRoleManagement 用户角色管理（3个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserRoleManagement', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserRoleManagement.Assign', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserRoleManagement.Remove', 'Role', 'admin');

-- ========================================
-- FileFilter 文件过滤（4个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileFilter', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileFilter.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileFilter.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileFilter.Delete', 'Role', 'admin');

-- ========================================
-- RssSubscription RSS订阅（5个权限）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RssSubscription', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RssSubscription.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RssSubscription.Update', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RssSubscription.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.RssSubscription.Download', 'Role', 'admin');
