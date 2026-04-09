<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { message } from "@/utils/message";
import { http } from "@/utils/http";
import { bg, avatar, illustration } from "./utils/static";
import { useDataThemeChange } from "@/layout/hooks/useDataThemeChange";
import { useNav } from "@/layout/hooks/useNav";
import { useLayout } from "@/layout/hooks/useLayout";
import Motion from "./utils/motion";

import dayIcon from "@/assets/svg/day.svg?component";
import darkIcon from "@/assets/svg/dark.svg?component";

defineOptions({
  name: "Login"
});

const router = useRouter();
const username = ref("");
const password = ref("");
const loading = ref(false);

const { initStorage } = useLayout();
initStorage();

const { dataTheme, overallStyle, dataThemeChange } = useDataThemeChange();
dataThemeChange(overallStyle.value);
const { title } = useNav();

const handleLogin = async () => {
  if (!username.value || !password.value) {
    message("请输入用户名和密码", { type: "warning" });
    return;
  }

  loading.value = true;
  try {
    const result = await http.post<any, any>("/api/app/account/login", {
      data: {
        username: username.value,
        password: password.value
      }
    });

    sessionStorage.setItem("access_token", result.accessToken);
    sessionStorage.setItem(
      "user-info",
      JSON.stringify({
        username: result.username,
        email: result.email,
        expires: result.expiresAt * 1000,
        roles: result.roles || [],
        permissions: result.permissions || []
      })
    );

    message("登录成功", { type: "success" });
    router.push("/");
  } catch (error: any) {
    let errorMsg = "登录失败";
    if (error?.response) {
      // 服务器响应了错误状态码
      errorMsg =
        error?.response?.data?.error?.message ||
        `服务器错误: ${error?.response?.status}`;
    } else if (error?.request) {
      // 请求已发出但没有收到响应
      errorMsg = "网络连接失败，请检查网络设置";
    } else {
      // 设置请求时发生错误
      errorMsg = error?.message || "登录失败";
    }
    message(errorMsg, { type: "error" });
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="select-none">
    <img :src="bg" class="wave" />
    <div class="flex-c absolute right-5 top-3">
      <el-switch
        v-model="dataTheme"
        inline-prompt
        :active-icon="dayIcon"
        :inactive-icon="darkIcon"
        @change="dataThemeChange"
      />
    </div>
    <div class="login-container">
      <div class="img">
        <component :is="illustration" />
      </div>
      <div class="login-box">
        <div class="login-form">
          <avatar class="avatar" />
          <Motion>
            <h2 class="outline-hidden">{{ title }}</h2>
          </Motion>
          <Motion :delay="100">
            <div class="form-group">
              <label>用户名</label>
              <el-input
                v-model="username"
                type="text"
                placeholder="请输入用户名"
                :disabled="loading"
                size="large"
                clearable
              />
            </div>
          </Motion>
          <Motion :delay="200">
            <div class="form-group">
              <label>密码</label>
              <el-input
                v-model="password"
                type="password"
                placeholder="请输入密码"
                :disabled="loading"
                size="large"
                show-password
                clearable
                @keyup.enter="handleLogin"
              />
            </div>
          </Motion>
          <Motion :delay="300">
            <el-button
              type="primary"
              size="large"
              :loading="loading"
              class="login-button"
              @click="handleLogin"
            >
              {{ loading ? "登录中..." : "登录" }}
            </el-button>
          </Motion>
          <Motion :delay="400">
            <div class="forgot-password-link">
              <el-link
                type="primary"
                @click="router.push('/login/reset-password')"
              >
                忘记密码
              </el-link>
            </div>
          </Motion>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@import url("@/style/login.css");

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
  color: #666;
}

.login-button {
  width: 100%;
  margin-top: 1rem;
}

.forgot-password-link {
  margin-top: 1rem;
  text-align: center;
}

:deep(.el-input-group__append, .el-input-group__prepend) {
  padding: 0;
}
</style>
