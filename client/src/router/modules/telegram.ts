export default {
  path: "/telegram",
  name: "Telegram",
  redirect: "/telegram/tg/login",
  meta: {
    title: "Telegram管理",
    icon: "ep:message",
    rank: 3
  },
  children: [
    {
      path: "/telegram/tg/login",
      name: "TGLogin",
      component: () => import("@/views/telegram/login/index.vue"),
      meta: {
        title: "TG登录"
      }
    },
    {
      path: "/telegram/media",
      name: "Media",
      component: () => import("@/views/telegram/media/index.vue"),
      meta: {
        title: "媒体管理"
      }
    },
    {
      path: "/telegram/media/chart",
      name: "MediaChart",
      component: () => import("@/views/telegram/media/chart.vue"),
      meta: {
        title: "媒体图表"
      }
    },
    {
      path: "/telegram/media/externalLink",
      name: "ExternalLink",
      component: () => import("@/views/telegram/media/externalLink.vue"),
      meta: {
        title: "外部链接管理"
      }
    }
  ]
} satisfies RouteConfigsTable;
