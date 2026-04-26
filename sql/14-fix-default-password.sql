-- 修复默认密码哈希值
-- 使用与 PasswordHasher.cs (PBKDF2-HMAC-SHA256, 16字节盐, 10000次迭代, 32字节哈希) 相同的算法生成
-- 密码: qwe123#

UPDATE AbpUsers
SET PasswordHash = 'ad1UIl5Y6YqFeRC+5ixQnPy9cW3wjY0QVFT25NiRu/DQmle5JZ+mJSxScxfOOsWV'
WHERE PasswordHash IS NULL OR PasswordHash = '8rZB1hd/U7b290OS9NGoVwQ13WanO9EfDHjqNzTQGsyIriXgmxg3dfAoaMCpP9pz';

-- 输出更新的用户数量
SELECT COUNT(*) AS UpdatedCount
FROM AbpUsers
WHERE PasswordHash = 'ad1UIl5Y6YqFeRC+5ixQnPy9cW3wjY0QVFT25NiRu/DQmle5JZ+mJSxScxfOOsWV';
