export default {
  path: "/electric-vehicle",
  name: "ElectricVehicle",
  redirect: "/electric-vehicle/vehicles",
  meta: {
    title: "电车管理",
    icon: "ri:car-line",
    rank: 2
  },
  children: [
    {
      path: "/electric-vehicle/vehicles",
      name: "ElectricVehicles",
      component: () => import("@/views/electric-vehicle/vehicles/index.vue"),
      meta: { title: "车辆管理", icon: "ep:van" }
    },
    {
      path: "/electric-vehicle/costs",
      name: "ElectricVehicleCosts",
      component: () => import("@/views/electric-vehicle/costs/index.vue"),
      meta: { title: "成本记录", icon: "ep:money" }
    },
    {
      path: "/electric-vehicle/charging",
      name: "ElectricVehicleCharging",
      component: () => import("@/views/electric-vehicle/charging/index.vue"),
      meta: { title: "充电记录", icon: "ep:lightning" }
    },
    {
      path: "/electric-vehicle/statistics",
      name: "ElectricVehicleStatistics",
      component: () => import("@/views/electric-vehicle/statistics/index.vue"),
      meta: { title: "统计分析", icon: "ep:data-analysis" }
    },
    {
      path: "/electric-vehicle/oil-config",
      name: "ElectricVehicleOilConfig",
      component: () => import("@/views/electric-vehicle/oil-config/index.vue"),
      meta: { title: "油车参数配置", icon: "ep:setting" }
    },
    {
      path: "/electric-vehicle/gasoline-prices",
      name: "GasolinePrices",
      component: () =>
        import("@/views/electric-vehicle/gasoline-prices/index.vue"),
      meta: { title: "油价列表", icon: "ep:price-tag" }
    }
  ]
} satisfies RouteConfigsTable;
