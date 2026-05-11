-- 新增 Downloader 相关配置项
-- 模块名：DFApp.Media

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'DownloaderEnabled', 'false', '是否启用 Downloader 子程序推送通知');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApacheBaseUrl', '', 'Apache 下载服务器基础 URL，如 http://192.168.1.100:8080');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApacheUsername', '', 'Apache Basic Auth 用户名');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'ApachePassword', '', 'Apache Basic Auth 密码');

INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark)
VALUES ('DFApp.Media', 'Aria2ApachePathPrefix', '', 'Aria2 下载目录的 Apache 虚拟路径，如 http://192.168.1.100:8080/aria2');
