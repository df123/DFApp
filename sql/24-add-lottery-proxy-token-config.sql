-- 新增彩票代理共享令牌配置项
-- 模块名：DFApp.Web.Lottery
-- 说明：Web 端的 X-Proxy-Token 唯一配置途径是数据库（appsettings/环境变量途径已移除），
--       值必须与 LotteryProxy 容器的 ProxySettings__ProxyToken 环境变量一致。
-- 注意：表的 ConcurrencyStamp/CreationTime 为 NOT NULL（应用写入时由 AOP 自动填充），
--       裸 SQL 必须显式给值；令牌填在 ConfigurationValue 列，不要填到 Remark。
--       留空表示未启用令牌校验（请求不携带该头），改库即时生效无需重启；
--       也可通过界面"配置管理"（ConfigurationInfo）直接新增/修改本条配置。

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark, ConcurrencyStamp, CreationTime)
VALUES ('DFApp.Web.Lottery', 'LotteryProxyToken', '6cd8722b04086497f457a6ef95aecc124f41b5e37b8822b621a69968bb2cb31d',
        '公网彩票代理 X-Proxy-Token 共享密钥，与 LotteryProxy 容器 ProxySettings__ProxyToken 一致，openssl rand -hex 32 生成',
        lower(hex(randomblob(16))), datetime('now'));
