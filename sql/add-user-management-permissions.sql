-- 为 admin 角色添加缺失的 UserManagement 权限授予记录
-- 原因：AppPermissionGrants 表中缺少 DFApp.UserManagement 系列权限，
--       导致所有需要用户管理权限的 API 返回 403
-- 日期：2026-04-11
--
-- 注意：运行时数据库路径为 /home/df/dfapp/DFApp/DFApp.db
--       （由 appsettings.Development.json 配置）
--       项目目录下的 src/DFApp.Web/DFApp.db 不是运行时使用的数据库

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement', 'Role', 'admin');

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Create', 'Role', 'admin');

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Update', 'Role', 'admin');

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.Delete', 'Role', 'admin');

INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.UserManagement.ChangePassword', 'Role', 'admin');
