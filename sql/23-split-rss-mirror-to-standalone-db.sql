-- RSS 镜像数据拆分到独立 SQLite 库（RssMirror.db）
-- 背景：AppRssMirrorItem / AppRssWordSegment 为可再生的高速增长缓存数据，
--       导致主库 DFApp.db 膨胀、拖慢远程备份。2026-08-21 起镜像数据已改存独立的 RssMirror.db
--       （连接串 ConnectionStrings:RssMirror，新库由应用启动时自动建表，空库开始积累）。
-- 数据可抛弃，本脚本不迁移历史数据，直接删除主库两张旧表并 VACUUM 回收空间。
-- 执行时机：部署新版应用并确认启动正常（已生成 RssMirror.db）之后执行本脚本。

DROP TABLE IF EXISTS AppRssWordSegment;
DROP TABLE IF EXISTS AppRssMirrorItem;

-- 遗留表说明：AppRssSubscriptionDownloads 曾以外键引用 AppRssMirrorItem，
-- AppRssSubscriptions / AppRssSubscriptionDownloads 两张表在当前代码中无任何引用（历史遗留），
-- 如确认不再使用可一并删除（取消注释）：
-- DROP TABLE IF EXISTS AppRssSubscriptionDownloads;
-- DROP TABLE IF EXISTS AppRssSubscriptions;

-- 回收被删除数据占用的文件空间（主库文件变小的关键步骤）
VACUUM;
