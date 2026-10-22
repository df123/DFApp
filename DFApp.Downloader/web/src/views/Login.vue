<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authApi, setAuthenticated, checkAuth } from '../api/downloader'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()

const username = ref('admin')
const password = ref('')
const loading = ref(false)

const handleLogin = async () => {
  if (!username.value || !password.value) {
    ElMessage.warning('请输入用户名和密码')
    return
  }

  loading.value = true
  try {
    await authApi.login(username.value, password.value)
    setAuthenticated(true)
    const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/')
      ? route.query.redirect
      : '/dashboard'
    router.replace(redirect)
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message ?? '登录失败')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  // 已登录则直接进入
  checkAuth().then(ok => {
    if (ok) router.replace('/dashboard')
  })
})
</script>

<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <div class="login-title">DFApp Downloader</div>
      </template>
      <el-form @submit.prevent="handleLogin">
        <el-form-item>
          <el-input v-model="username" placeholder="用户名" size="large" autocomplete="username">
            <template #prefix><el-icon><User /></el-icon></template>
          </el-input>
        </el-form-item>
        <el-form-item>
          <el-input v-model="password" type="password" placeholder="密码" size="large" show-password
            autocomplete="current-password" @keyup.enter="handleLogin">
            <template #prefix><el-icon><Lock /></el-icon></template>
          </el-input>
        </el-form-item>
        <el-button type="primary" size="large" style="width: 100%" :loading="loading" @click="handleLogin">
          登 录
        </el-button>
        <div class="login-hint">首次使用默认账号 admin / admin，登录后请在设置中修改密码</div>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #304156 0%, #1f2d3d 100%);
}

.login-card {
  width: 380px;
}

.login-title {
  text-align: center;
  font-size: 18px;
  font-weight: bold;
}

.login-hint {
  margin-top: 12px;
  color: #909399;
  font-size: 12px;
  text-align: center;
}
</style>
