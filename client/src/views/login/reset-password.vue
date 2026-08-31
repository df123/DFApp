<script setup lang="ts">
import { useRouter } from "vue-router";
import { bg, avatar, illustration } from "./utils/static";
import { useDataThemeChange } from "@/layout/hooks/useDataThemeChange";
import { useNav } from "@/layout/hooks/useNav";
import { useLayout } from "@/layout/hooks/useLayout";
import Motion from "./utils/motion";

import dayIcon from "@/assets/svg/day.svg?component";
import darkIcon from "@/assets/svg/dark.svg?component";

defineOptions({
  name: "ResetPassword"
});

const router = useRouter();

const { initStorage } = useLayout();
initStorage();

const { dataTheme, overallStyle, dataThemeChange } = useDataThemeChange();
dataThemeChange(overallStyle.value);
const { title } = useNav();

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
            <el-alert
              title="密码重置功能暂未开放"
              description="系统尚未接入邮件或短信发送服务。如需重置密码，请联系管理员处理。"
              type="warning"
              show-icon
              :closable="false"
            />
          </Motion>
          <Motion :delay="200">
            <el-button
              type="primary"
              size="large"
              class="login-button"
              @click="handleBackToLogin"
            >
              返回登录
            </el-button>
          </Motion>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@import url("@/style/login.css");

.login-button {
  width: 100%;
  margin-top: 1rem;
}
</style>
