-- ============================================================
-- RBAC 权限管理种子数据
-- 包含：角色管理、权限授予管理、用户角色管理
-- 执行时间：2026-04-09
-- 说明：插入 AbpPermissionGroups + AbpPermissions + AbpPermissionGrants
-- admin 角色ID: 77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9
-- ============================================================

-- ============================================================
-- 第1步：插入 AbpPermissionGroups（3 个权限组）
-- 格式参考现有数据：
--   Id (GUID) | Name | DisplayName | ExtraProperties
-- ============================================================

-- 检查是否已存在，避免重复插入
INSERT OR IGNORE INTO AbpPermissionGroups (Id, Name, DisplayName, ExtraProperties) VALUES
('A1B2C301-0001-4000-8000-000000000001', 'RoleManagement', 'L:DFApp,Permission:RoleManagement', '{}');

INSERT OR IGNORE INTO AbpPermissionGroups (Id, Name, DisplayName, ExtraProperties) VALUES
('A1B2C301-0001-4000-8000-000000000002', 'PermissionGrantManagement', 'L:DFApp,Permission:PermissionGrantManagement', '{}');

INSERT OR IGNORE INTO AbpPermissionGroups (Id, Name, DisplayName, ExtraProperties) VALUES
('A1B2C301-0001-4000-8000-000000000003', 'UserRoleManagement', 'L:DFApp,Permission:UserRoleManagement', '{}');

-- ============================================================
-- 第2步：插入 AbpPermissions（10 个权限定义）
-- 格式参考现有数据（以 DFApp.Medias 为模板）：
--   Id | GroupName | Name | ParentName | DisplayName | IsEnabled | MultiTenancySide | Providers | StateCheckers | ExtraProperties
-- ============================================================

-- RoleManagement 组（4 个权限）
INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000001', 'RoleManagement', 'DFApp.RoleManagement', NULL, 'L:DFApp,Permission:RoleManagement', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000002', 'RoleManagement', 'DFApp.RoleManagement.Create', 'DFApp.RoleManagement', 'L:DFApp,Permission:RoleManagement.Create', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000003', 'RoleManagement', 'DFApp.RoleManagement.Update', 'DFApp.RoleManagement', 'L:DFApp,Permission:RoleManagement.Update', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000004', 'RoleManagement', 'DFApp.RoleManagement.Delete', 'DFApp.RoleManagement', 'L:DFApp,Permission:RoleManagement.Delete', 1, 3, NULL, NULL, '{}');

-- PermissionGrantManagement 组（3 个权限）
INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000005', 'PermissionGrantManagement', 'DFApp.PermissionGrantManagement', NULL, 'L:DFApp,Permission:PermissionGrantManagement', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000006', 'PermissionGrantManagement', 'DFApp.PermissionGrantManagement.Grant', 'DFApp.PermissionGrantManagement', 'L:DFApp,Permission:PermissionGrantManagement.Grant', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000007', 'PermissionGrantManagement', 'DFApp.PermissionGrantManagement.Revoke', 'DFApp.PermissionGrantManagement', 'L:DFApp,Permission:PermissionGrantManagement.Revoke', 1, 3, NULL, NULL, '{}');

-- UserRoleManagement 组（3 个权限）
INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000008', 'UserRoleManagement', 'DFApp.UserRoleManagement', NULL, 'L:DFApp,Permission:UserRoleManagement', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000009', 'UserRoleManagement', 'DFApp.UserRoleManagement.Assign', 'DFApp.UserRoleManagement', 'L:DFApp,Permission:UserRoleManagement.Assign', 1, 3, NULL, NULL, '{}');

INSERT OR IGNORE INTO AbpPermissions (Id, GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties) VALUES
('A1B2C302-0001-4000-8000-000000000010', 'UserRoleManagement', 'DFApp.UserRoleManagement.Remove', 'DFApp.UserRoleManagement', 'L:DFApp,Permission:UserRoleManagement.Remove', 1, 3, NULL, NULL, '{}');

-- ============================================================
-- 第3步：插入 AbpPermissionGrants（授予 admin 角色）
-- ProviderName = 'R', ProviderKey = admin 角色GUID（大写）
-- ============================================================

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000001', NULL, 'DFApp.RoleManagement', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000002', NULL, 'DFApp.RoleManagement.Create', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000003', NULL, 'DFApp.RoleManagement.Update', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000004', NULL, 'DFApp.RoleManagement.Delete', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000005', NULL, 'DFApp.PermissionGrantManagement', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000006', NULL, 'DFApp.PermissionGrantManagement.Grant', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000007', NULL, 'DFApp.PermissionGrantManagement.Revoke', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000008', NULL, 'DFApp.UserRoleManagement', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000009', NULL, 'DFApp.UserRoleManagement.Assign', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');

INSERT OR IGNORE INTO AbpPermissionGrants (Id, TenantId, Name, ProviderName, ProviderKey) VALUES
('A1B2C303-0001-4000-8000-000000000010', NULL, 'DFApp.UserRoleManagement.Remove', 'R', '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9');
