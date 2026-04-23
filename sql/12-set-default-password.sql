-- 为所有用户设置默认密码 "123456"
-- 密码哈希使用 PBKDF2-HMAC-SHA256 算法，16 字节盐，10000 次迭代，32 字节哈希

UPDATE AbpUsers
SET PasswordHash = '8rZB1hd/U7b290OS9NGoVwQ13WanO9EfDHjqNzTQGsyIriXgmxg3dfAoaMCpP9pz'
WHERE PasswordHash IS NULL;

-- 输出更新的用户数量
SELECT COUNT(*) AS UpdatedCount
FROM AbpUsers
WHERE PasswordHash = '8rZB1hd/U7b290OS9NGoVwQ13WanO9EfDHjqNzTQGsyIriXgmxg3dfAoaMCpP9pz';
