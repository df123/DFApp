export default {
  path: "/bookkeeping",
  name: "Bookkeeping",
  redirect: "/bookkeeping/expenditure",
  meta: {
    title: "记账管理",
    icon: "ep:notebook",
    rank: 1
  },
  children: [
    {
      path: "/bookkeeping/expenditure",
      name: "Expenditure",
      component: () => import("@/views/bookkeeping/expenditure/index.vue"),
      meta: {
        title: "支出管理"
      }
    },
    {
      path: "/bookkeeping/expenditure/analysis",
      name: "ExpenditureAnalysis",
      component: () => import("@/views/bookkeeping/expenditure/analysis.vue"),
      meta: {
        title: "支出分析"
      }
    },
    {
      path: "/bookkeeping/expenditure/chart",
      name: "ExpenditureChart",
      component: () => import("@/views/bookkeeping/expenditure/chart.vue"),
      meta: {
        title: "支出图表"
      }
    },
    {
      path: "/bookkeeping/category",
      name: "Category",
      component: () => import("@/views/bookkeeping/category/index.vue"),
      meta: {
        title: "分类管理"
      }
    }
  ]
} satisfies RouteConfigsTable;
