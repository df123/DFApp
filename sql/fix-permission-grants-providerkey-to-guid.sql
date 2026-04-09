-- 修复 PermissionGrants 表中 ProviderName='R' 的 ProviderKey
-- 将 ProviderKey 从角色名称改为角色 ID（GUID 大写），与 ABP 标准保持一致
-- 执行时间：2026-04-09

-- 角色名称 → 角色 ID 映射
-- admin           → 77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9
-- log             → ECBEA0A5-701A-B2AC-CDA5-3A0FE65B249B
-- bookkeeping     → C861A565-FDFC-78CF-67C4-3A100AF27165
-- file            → C587B3E3-3184-4AF5-A35C-3A100B087923
-- telegram        → 77D85089-23FF-50EF-55DF-3A10121F414D
-- cms             → 3C6F3B72-FC71-152A-9981-3A103ED82109
-- Lottery         → 7DF6DE33-2FC0-F64C-35AC-3A103ED95A9B
-- IP              → C21E3C2B-667B-03AC-564D-3A10D3F1B6F8
-- management_background → A09D711B-1921-E748-EE7D-3A10D3F1B6F8
-- down-ex         → CB410581-3D19-62D8-7635-3A1130B3C76C
-- aria2           → 12294449-FC9E-C64A-28E9-3A1265DED8EB
-- rss             → 03951B85-E4BA-93D7-AC0E-3A1ED466334F
-- ElectricVehicle → CA14ECA5-AE0D-CF0C-A750-3A1F82344C6B

UPDATE AbpPermissionGrants SET ProviderKey = '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9' WHERE ProviderName = 'R' AND ProviderKey = 'admin';
UPDATE AbpPermissionGrants SET ProviderKey = 'ECBEA0A5-701A-B2AC-CDA5-3A0FE65B249B' WHERE ProviderName = 'R' AND ProviderKey = 'log';
UPDATE AbpPermissionGrants SET ProviderKey = 'C861A565-FDFC-78CF-67C4-3A100AF27165' WHERE ProviderName = 'R' AND ProviderKey = 'bookkeeping';
UPDATE AbpPermissionGrants SET ProviderKey = 'C587B3E3-3184-4AF5-A35C-3A100B087923' WHERE ProviderName = 'R' AND ProviderKey = 'file';
UPDATE AbpPermissionGrants SET ProviderKey = '77D85089-23FF-50EF-55DF-3A10121F414D' WHERE ProviderName = 'R' AND ProviderKey = 'telegram';
UPDATE AbpPermissionGrants SET ProviderKey = '3C6F3B72-FC71-152A-9981-3A103ED82109' WHERE ProviderName = 'R' AND ProviderKey = 'cms';
UPDATE AbpPermissionGrants SET ProviderKey = '7DF6DE33-2FC0-F64C-35AC-3A103ED95A9B' WHERE ProviderName = 'R' AND ProviderKey = 'Lottery';
UPDATE AbpPermissionGrants SET ProviderKey = 'C21E3C2B-667B-03AC-564D-3A10D3F1B6F8' WHERE ProviderName = 'R' AND ProviderKey = 'IP';
UPDATE AbpPermissionGrants SET ProviderKey = 'A09D711B-1921-E748-EE7D-3A10D3F1B6F8' WHERE ProviderName = 'R' AND ProviderKey = 'management_background';
UPDATE AbpPermissionGrants SET ProviderKey = 'CB410581-3D19-62D8-7635-3A1130B3C76C' WHERE ProviderName = 'R' AND ProviderKey = 'down-ex';
UPDATE AbpPermissionGrants SET ProviderKey = '12294449-FC9E-C64A-28E9-3A1265DED8EB' WHERE ProviderName = 'R' AND ProviderKey = 'aria2';
UPDATE AbpPermissionGrants SET ProviderKey = '03951B85-E4BA-93D7-AC0E-3A1ED466334F' WHERE ProviderName = 'R' AND ProviderKey = 'rss';
UPDATE AbpPermissionGrants SET ProviderKey = 'CA14ECA5-AE0D-CF0C-A750-3A1F82344C6B' WHERE ProviderName = 'R' AND ProviderKey = 'ElectricVehicle';
