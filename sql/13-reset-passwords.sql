-- 重置所有用户密码
-- 将所有用户的 PasswordHash 设置为 NULL
-- 用户需要通过密码重置功能来设置新密码

UPDATE AbpUsers
SET PasswordHash = NULL
WHERE PasswordHash IS NOT NULL;

-- 输出重置的用户数量
SELECT COUNT(*) AS ResetCount
FROM AbpUsers
WHERE PasswordHash IS NULL;
