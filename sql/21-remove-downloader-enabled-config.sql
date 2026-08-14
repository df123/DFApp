-- 移除已废弃的 DownloaderEnabled 配置项（2026-08-14）
-- 通知推送为下载器的必需功能，不再需要开关，避免"未启用导致静默不推送"
DELETE FROM AppConfigurationInfo
WHERE ModuleName = 'DFApp.Media' AND ConfigurationName = 'DownloaderEnabled';
