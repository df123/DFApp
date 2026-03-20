-- 删除 OpenIddict 相关表
-- 注意：执行前请备份数据库

-- 删除 OpenIddictTokens 表
DROP TABLE IF EXISTS OpenIddictTokens;

-- 删除 OpenIddictAuthorizations 表
DROP TABLE IF EXISTS OpenIddictAuthorizations;

-- 删除 OpenIddictScopes 表
DROP TABLE IF EXISTS OpenIddictScopes;

-- 删除 OpenIddictApplications 表
DROP TABLE IF EXISTS OpenIddictApplications;
