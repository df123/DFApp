<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { authApi, setAuthenticated } from './api/downloader'
import { ElMessage } from 'element-plus'

const router = useRouter()
const route = useRoute()
const activeMenu = ref('/dashboard')

// 登录页等公开页面不渲染管理布局
const isPublicRoute = computed(() => route.meta.public === true)

const handleLogout = async () => {
  try {
    await authApi.logout()
  } catch {
    /* 会话已失效时忽略 */
  }
  setAuthenticated(false)
  ElMessage.success('已退出登录')
  router.replace('/login')
}

router.afterEach((to) => {
  activeMenu.value = to.path
})
</script>

<template>
  <router-view v-if="isPublicRoute" />
  <el-container v-else style="height: 100vh">
    <el-aside width="200px" style="background-color: #304156; display: flex; flex-direction: column">
      <div style="padding: 20px; text-align: center; color: #fff; font-size: 16px; font-weight: bold">
        DFApp Downloader
      </div>
      <el-menu
        :default-active="activeMenu"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#409eff"
        router
      >
        <el-menu-item index="/dashboard">
          <el-icon><Monitor /></el-icon>
          <span>仪表盘</span>
        </el-menu-item>
        <el-menu-item index="/downloads">
          <el-icon><Download /></el-icon>
          <span>下载队列</span>
        </el-menu-item>
        <el-menu-item index="/gallery">
          <el-icon><Picture /></el-icon>
          <span>媒体库</span>
        </el-menu-item>
        <el-menu-item index="/logs">
          <el-icon><Document /></el-icon>
          <span>日志</span>
        </el-menu-item>
        <el-menu-item index="/settings">
          <el-icon><Setting /></el-icon>
          <span>设置</span>
        </el-menu-item>
      </el-menu>
      <div style="margin-top: auto; padding: 16px 20px">
        <el-button style="width: 100%" @click="handleLogout">
          <el-icon style="margin-right: 6px"><SwitchButton /></el-icon>
          退出登录
        </el-button>
      </div>
    </el-aside>
    <el-main id="main-content" style="padding: 20px; background-color: #f5f7fa">
      <router-view />
    </el-main>
  </el-container>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}
</style>
