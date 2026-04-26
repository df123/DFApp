export default {
  path: "/lottery",
  name: "Lottery",
  redirect: "/lottery/buy",
  meta: {
    title: "彩票管理",
    icon: "ep:pie-chart",
    rank: 2
  },
  children: [
    {
      path: "/lottery/buy",
      name: "LotteryBuy",
      component: () => import("@/views/lottery/index.vue"),
      meta: {
        title: "彩票购买"
      }
    },
    {
      path: "/lottery/result",
      name: "LotteryResult",
      component: () => import("@/views/lottery/result/index.vue"),
      meta: {
        title: "开奖结果"
      }
    },
    {
      path: "/lottery/statistics",
      name: "LotteryStatistics",
      component: () => import("@/views/lottery/statistics/index.vue"),
      meta: {
        title: "统计分析"
      }
    },
    {
      path: "/lottery/statistics-item",
      name: "LotteryStatisticsItem",
      component: () => import("@/views/lottery/statistics-item/index.vue"),
      meta: {
        title: "统计项管理"
      }
    },
    {
      path: "/lottery/simulation/ssq",
      name: "LotterySimulation",
      component: () => import("@/views/lottery/simulation/ssq/index.vue"),
      meta: {
        title: "双色球模拟"
      }
    },
    {
      path: "/lottery/simulation/kl8",
      name: "LotteryK8",
      component: () => import("@/views/lottery/simulation/kl8/index.vue"),
      meta: {
        title: "快乐8模拟"
      }
    },
    {
      path: "/lottery/data-fetch",
      name: "LotteryDataFetch",
      component: () => import("@/views/lottery/data-fetch.vue"),
      meta: {
        title: "数据获取测试"
      }
    }
  ]
} satisfies RouteConfigsTable;
