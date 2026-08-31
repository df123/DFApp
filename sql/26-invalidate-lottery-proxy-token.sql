-- 使已提交到仓库的彩票代理共享令牌立即失效
-- 日期：2026-08-24
-- 说明：该令牌曾出现在 Git 历史中，必须先置空停用，再生成新令牌并手动同步到：
--       1. Web 端 AppConfigurationInfo 中 DFApp.Web.Lottery/LotteryProxyToken；
--       2. LotteryProxy 的 ProxySettings__ProxyToken 环境变量或部署机 .env 文件。

UPDATE AppConfigurationInfo
SET ConfigurationValue = '',
    LastModificationTime = datetime('now')
WHERE ModuleName = 'DFApp.Web.Lottery'
  AND ConfigurationName = 'LotteryProxyToken';
