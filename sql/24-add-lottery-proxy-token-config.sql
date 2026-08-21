-- 新增彩票代理共享令牌配置项
-- 模块名：DFApp.Web.Lottery
-- 说明：Web 端的 X-Proxy-Token 改为数据库配置（优先于 appsettings 的 LotteryProxy:Token），
--       值必须与 LotteryProxy 容器的 ProxySettings__ProxyToken 环境变量一致。
-- 生成令牌：openssl rand -hex 32
-- 注意：留空表示未启用令牌（回退 appsettings，默认也为空即不携带）；
--       也可通过界面"配置管理"（ConfigurationInfo）直接新增/修改本条配置。

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Web.Lottery', 'LotteryProxyToken', '', '公网彩票代理 X-Proxy-Token 共享密钥，与 LotteryProxy 容器 ProxySettings__ProxyToken 一致，openssl rand -hex 32 生成');
