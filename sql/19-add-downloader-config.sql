-- 新增 Downloader 相关配置项
-- 模块名：DFApp.Media
-- 注：DownloaderEnabled（是否推送下载通知）已移除——通知推送为下载器的必需功能，不再需要开关（2026-08-14）

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApacheBaseUrl', '', 'Apache 下载服务器基础 URL，如 http://192.168.1.100:8080');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApacheUsername', '', 'Apache Basic Auth 用户名');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApachePassword', '', 'Apache Basic Auth 密码');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'Aria2ApachePathPrefix', '', 'Aria2 下载目录的 Apache 虚拟路径，如 http://192.168.1.100:8080/aria2');
