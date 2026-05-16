export default {
  path: "/system",
  name: "System",
  redirect: "/system/configuration",
  component: () => import("@/views/system/index.vue"),
  meta: {
    title: "系统管理",
    icon: "ep:setting",
    rank: 5
  },
  children: [
    {
      path: "/system/configuration",
      name: "Configuration",
      component: () => import("@/views/configuration/index.vue"),
      meta: {
        title: "配置管理"
      }
    },
    {
      path: "/system/dynamicIp",
      name: "DynamicIp",
      component: () => import("@/views/dynamicIp/index.vue"),
      meta: {
        title: "动态IP管理"
      }
    },
    {
      path: "/system/fileUpload",
      name: "FileUpload",
      component: () => import("@/views/fileUpload/index.vue"),
      meta: {
        title: "文件上传管理"
      }
    },
    {
      path: "/system/logViewer",
      name: "LogViewer",
      component: () => import("@/views/logViewer/index.vue"),
      meta: {
        title: "日志查看器"
      }
    },
    {
      path: "/system/permission-management",
      name: "PermissionManagement",
      component: () => import("@/views/permission/management/index.vue"),
      meta: {
        title: "权限管理"
      }
    },
    {
      path: "/system/user-management",
      name: "UserManagement",
      component: () => import("@/views/user-management/index.vue"),
      meta: {
        title: "用户管理"
      }
    }
  ]
} satisfies RouteConfigsTable;
