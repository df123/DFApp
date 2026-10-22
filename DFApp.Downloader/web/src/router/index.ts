import { createRouter, createWebHistory } from 'vue-router'
import { checkAuth } from '../api/downloader'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/Login.vue'),
      meta: { public: true },
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('../views/Dashboard.vue'),
    },
    {
      path: '/downloads',
      name: 'DownloadQueue',
      component: () => import('../views/DownloadQueue.vue'),
    },
    {
      path: '/gallery',
      name: 'Gallery',
      component: () => import('../views/Gallery.vue'),
    },
    {
      path: '/logs',
      name: 'Logs',
      component: () => import('../views/Logs.vue'),
    },
    {
      path: '/settings',
      name: 'Settings',
      component: () => import('../views/Settings.vue'),
    },
  ],
})

// 未登录一律跳转登录页（会话检查结果缓存，避免每次导航都发请求）
router.beforeEach(async (to) => {
  if (to.meta.public) {
    return true
  }

  const ok = await checkAuth()
  if (!ok) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }
  return true
})

export default router
