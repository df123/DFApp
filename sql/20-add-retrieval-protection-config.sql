-- 新增下载器取回保护时长配置项
-- 模块名：DFApp.Web.Background.ListenTelegramService

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Web.Background.ListenTelegramService', 'RetrievalProtectionHours', '2', '下载器取回保护时长（小时）：已通知下载器但未确认取回的媒体在此时间内不被空间清理删除，缺省 2 小时');
