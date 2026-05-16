<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { message } from "@/utils/message";
import { bg, avatar, illustration } from "./utils/static";
import { useDataThemeChange } from "@/layout/hooks/useDataThemeChange";
import { useNav } from "@/layout/hooks/useNav";
import { useLayout } from "@/layout/hooks/useLayout";
import Motion from "./utils/motion";
import {
  sendPasswordResetCode,
  verifyPasswordResetToken,
  resetPassword
} from "@/api/user";

import dayIcon from "@/assets/svg/day.svg?component";
import darkIcon from "@/assets/svg/dark.svg?component";

defineOptions({
  name: "ResetPassword"
});

const router = useRouter();
const currentStep = ref(1);
const email = ref("");
const token = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const loading = ref(false);

const { initStorage } = useLayout();
initStorage();

const { dataTheme, overallStyle, dataThemeChange } = useDataThemeChange();
dataThemeChange(overallStyle.value);
const { title } = useNav();

const handleSendCode = async () => {
  if (!email.value) {
    message("请输入邮箱", { type: "warning" });
    return;
  }

  loading.value = true;
  try {
    await sendPasswordResetCode({ email: email.value });
    message("验证码已发送到您的邮箱", { type: "success" });
    currentStep.value = 2;
  } catch (error: any) {
    let errorMsg = "发送验证码失败";
    if (error?.response) {
      errorMsg =
        error?.response?.data?.error?.message ||
        `服务器错误: ${error?.response?.status}`;
    } else if (error?.request) {
      errorMsg = "网络连接失败，请检查网络设置";
    } else {
      errorMsg = error?.message || "发送验证码失败";
    }
    message(errorMsg, { type: "error" });
  } finally {
    loading.value = false;
  }
};

const handleVerifyToken = async () => {
  if (!token.value) {
    message("请输入验证码", { type: "warning" });
    return;
  }

  loading.value = true;
  try {
    const result = await verifyPasswordResetToken({ token: token.value });
    if (result.isValid) {
      message("验证成功", { type: "success" });
      currentStep.value = 3;
    } else {
      message("验证码无效或已过期", { type: "error" });
    }
  } catch (error: any) {
    let errorMsg = "验证失败";
    if (error?.response) {
      errorMsg =
        error?.response?.data?.error?.message ||
        `服务器错误: ${error?.response?.status}`;
    } else if (error?.request) {
      errorMsg = "网络连接失败，请检查网络设置";
    } else {
      errorMsg = error?.message || "验证失败";
    }
    message(errorMsg, { type: "error" });
  } finally {
    loading.value = false;
  }
};

const handleResetPassword = async () => {
  if (!newPassword.value) {
    message("请输入新密码", { type: "warning" });
    return;
  }

  if (!confirmPassword.value) {
    message("请确认新密码", { type: "warning" });
    return;
  }

  if (newPassword.value !== confirmPassword.value) {
    message("两次输入的密码不一致", { type: "warning" });
    return;
  }

  loading.value = true;
  try {
    await resetPassword({
      token: token.value,
      newPassword: newPassword.value
    });
    message("密码重置成功，请使用新密码登录", { type: "success" });
    router.push("/login");
  } catch (error: any) {
    let errorMsg = "重置密码失败";
    if (error?.response) {
      errorMsg =
        error?.response?.data?.error?.message ||
        `服务器错误: ${error?.response?.status}`;
    } else if (error?.request) {
      errorMsg = "网络连接失败，请检查网络设置";
    } else {
      errorMsg = error?.message || "重置密码失败";
    }
    message(errorMsg, { type: "error" });
  } finally {
    loading.value = false;
  }
};

const handleBackToLogin = () => {
  router.push("/login");
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
              <label>步骤 {{ currentStep }}/3</label>
              <el-steps :active="currentStep - 1" simple>
                <el-step title="发送验证码" />
                <el-step title="验证令牌" />
                <el-step title="重置密码" />
              </el-steps>
            </div>
          </Motion>
          <Motion :delay="200">
            <div v-if="currentStep === 1" class="form-group">
              <label>邮箱</label>
              <el-input
                v-model="email"
                type="email"
                placeholder="请输入邮箱"
                :disabled="loading"
                size="large"
                clearable
              />
            </div>
          </Motion>
          <Motion :delay="200">
            <div v-if="currentStep === 2" class="form-group">
              <label>验证码</label>
              <el-input
                v-model="token"
                type="text"
                placeholder="请输入邮箱收到的验证码"
                :disabled="loading"
                size="large"
                clearable
              />
            </div>
          </Motion>
          <Motion :delay="200">
            <div v-if="currentStep === 3" class="form-group">
              <label>新密码</label>
              <el-input
                v-model="newPassword"
                type="password"
                placeholder="请输入新密码"
                :disabled="loading"
                size="large"
                show-password
                clearable
              />
            </div>
          </Motion>
          <Motion :delay="200">
            <div v-if="currentStep === 3" class="form-group">
              <label>确认新密码</label>
              <el-input
                v-model="confirmPassword"
                type="password"
                placeholder="请再次输入新密码"
                :disabled="loading"
                size="large"
                show-password
                clearable
                @keyup.enter="handleResetPassword"
              />
            </div>
          </Motion>
          <Motion :delay="300">
            <el-button
              v-if="currentStep === 1"
              type="primary"
              size="large"
              :loading="loading"
              class="login-button"
              @click="handleSendCode"
            >
              {{ loading ? "发送中..." : "发送验证码" }}
            </el-button>
            <el-button
              v-if="currentStep === 2"
              type="primary"
              size="large"
              :loading="loading"
              class="login-button"
              @click="handleVerifyToken"
            >
              {{ loading ? "验证中..." : "验证" }}
            </el-button>
            <el-button
              v-if="currentStep === 3"
              type="primary"
              size="large"
              :loading="loading"
              class="login-button"
              @click="handleResetPassword"
            >
              {{ loading ? "重置中..." : "重置密码" }}
            </el-button>
          </Motion>
          <Motion :delay="400">
            <div class="back-link">
              <el-link type="primary" @click="handleBackToLogin">
                返回登录
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

.back-link {
  margin-top: 1rem;
  text-align: center;
}

:deep(.el-input-group__append, .el-input-group__prepend) {
  padding: 0;
}
</style>
