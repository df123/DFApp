<script setup lang="ts">
import { ref, onMounted } from "vue";
import { configurationApi } from "@/api/configuration";
import { getToken, hasPerms } from "@/utils/auth";

defineOptions({
  name: "Welcome"
});

const remainingDiskSpace = ref<string>("");
const isLoading = ref<boolean>(false);
const isAuthenticated = ref<boolean>(false);
const hasPermission = ref<boolean>(false);

// 检查用户是否已认证
const checkAuthentication = async () => {
  const token = await getToken();
  isAuthenticated.value = !!(token && token.accessToken);
};

// 检查用户是否有权限
const checkUserPermission = async () => {
  try {
    hasPermission.value = hasPerms("DFApp.ConfigurationInfo");
  } catch (error) {
    console.error("检查权限失败:", error);
    hasPermission.value = false;
  }
};

// 获取剩余磁盘空间
const fetchRemainingDiskSpace = async () => {
  if (!isAuthenticated.value || !hasPermission.value) return;

  try {
    isLoading.value = true;
    remainingDiskSpace.value = await configurationApi.getRemainingDiskSpace();
  } catch (error) {
    console.error("获取剩余磁盘空间失败:", error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  await checkAuthentication();
  await checkUserPermission();
  await fetchRemainingDiskSpace();
});
</script>

<template>
  <div class="container">
    <div class="p-5 text-center">
      <div
        class="d-inline-block bg-success text-white p-1 h5 rounded mb-4"
        role="alert"
      >
        <h5 class="m-1">
          <i class="fas fa-rocket" /> 恭喜，<strong>DFApp</strong> 已成功运行！
        </h5>
      </div>
      <h1>欢迎使用应用程序</h1>

      <p class="lead px-lg-5 mx-lg-5">
        欢迎使用 DFApp 应用程序，这是一个功能强大的管理系统。
      </p>

      <!-- 显示剩余磁盘空间 -->
      <div v-if="isAuthenticated && hasPermission" class="mt-4">
        <div
          v-if="isLoading"
          class="p-3 bg-primary text-white rounded shadow-lg"
        >
          <h5 class="mb-0">
            <i class="fas fa-spinner fa-spin" /> 正在获取磁盘空间信息...
          </h5>
        </div>
        <div
          v-else-if="remainingDiskSpace"
          class="p-3 bg-primary text-white rounded shadow-lg"
        >
          <h5 class="mb-0">
            <i class="fas fa-hdd" /> 剩余磁盘空间:
            <strong>{{ remainingDiskSpace }}</strong>
          </h5>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.container {
  max-width: 1200px;
  margin: 0 auto;
}

.p-5 {
  padding: 3rem !important;
}

.text-center {
  text-align: center !important;
}

.bg-success {
  background-color: #28a745 !important;
}

.text-white {
  color: #fff !important;
}

.p-1 {
  padding: 0.25rem !important;
}

.h5 {
  font-size: 1.25rem;
}

.rounded {
  border-radius: 0.25rem !important;
}

.mb-4 {
  margin-bottom: 1.5rem !important;
}

.m-1 {
  margin: 0.25rem !important;
}

.lead {
  font-size: 1.25rem;
  font-weight: 300;
}

.px-lg-5 {
  padding-right: 3rem !important;
  padding-left: 3rem !important;
}

.mx-lg-5 {
  margin-right: 3rem !important;
  margin-left: 3rem !important;
}

.mt-4 {
  margin-top: 1.5rem !important;
}

.p-3 {
  padding: 1rem !important;
}

.bg-primary {
  background-color: #007bff !important;
}

.shadow-lg {
  box-shadow: 0 1rem 3rem rgb(0 0 0 / 17.5%) !important;
}

.mb-0 {
  margin-bottom: 0 !important;
}
</style>
