-- 修复默认密码哈希值
-- 使用与 PasswordHasher.cs (PBKDF2-HMAC-SHA256, 16字节盐, 10000次迭代, 32字节哈希) 相同的算法生成
-- 密码: 123456

UPDATE AbpUsers
SET PasswordHash = 'EgF/qeET7z/4R73QxvdpsQEx2guIRo4N6gvKv/Funm0qfjs0+hNRY6BTHzUYsKkT'
WHERE PasswordHash IS NULL OR PasswordHash = '8rZB1hd/U7b290OS9NGoVwQ13WanO9EfDHjqNzTQGsyIriXgmxg3dfAoaMCpP9pz';

-- 输出更新的用户数量
SELECT COUNT(*) AS UpdatedCount
FROM AbpUsers
WHERE PasswordHash = 'EgF/qeET7z/4R73QxvdpsQEx2guIRo4N6gvKv/Funm0qfjs0+hNRY6BTHzUYsKkT';
