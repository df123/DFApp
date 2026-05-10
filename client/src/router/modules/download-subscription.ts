export default {
  path: "/download-subscription",
  name: "DownloadSubscription",
  redirect: "/download-subscription/aria2",
  meta: {
    title: "下载与订阅",
    icon: "ep:download",
    rank: 4
  },
  children: [
    {
      path: "/download-subscription/aria2",
      name: "Aria2Subscription",
      component: () => import("@/views/aria2/index.vue"),
      meta: {
        title: "Aria2管理"
      }
    },
    {
      path: "/download-subscription/download-manage",
      name: "DownloadManage",
      component: () => import("@/views/aria2/manage.vue"),
      meta: {
        title: "下载管理"
      }
    },
    {
      path: "/download-subscription/rss",
      name: "Rss",
      component: () => import("@/views/rss/index.vue"),
      meta: {
        title: "RSS阅读器"
      }
    },
    {
      path: "/download-subscription/rss-sources",
      name: "RssSources",
      component: () => import("@/views/rss-mirror/sources/index.vue"),
      meta: {
        title: "RSS源管理"
      }
    },
    {
      path: "/download-subscription/rss-mirror-items",
      name: "RssMirrorItems",
      component: () => import("@/views/rss-mirror/items/index.vue"),
      meta: {
        title: "RSS镜像条目"
      }
    },
    {
      path: "/download-subscription/rss-word-segments",
      name: "RssWordSegments",
      component: () => import("@/views/rss/word-segments.vue"),
      meta: {
        title: "分词统计"
      }
    },
    {
      path: "/download-subscription/filterKeyword",
      name: "FilterKeyword",
      component: () => import("@/views/filterKeyword/index.vue"),
      meta: {
        title: "关键词过滤管理"
      }
    }
  ]
} satisfies RouteConfigsTable;
