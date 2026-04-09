<template>
  <div class="tg-login-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">TG登录</span>
        </div>
      </template>

      <!-- TG登录区域 -->
      <div class="login-content">
        <el-form
          v-if="isEnter"
          ref="loginForm"
          :model="form"
          :rules="rules"
          label-width="80px"
        >
          <el-form-item label="验证码" prop="username">
            <el-input v-model="form.username" placeholder="请输入验证码" />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="submitForm">登录</el-button>
          </el-form-item>
        </el-form>

        <div v-else class="login-status">
          <el-alert
            :title="loginStatus"
            :type="getStatusType(loginStatus)"
            :closable="false"
            show-icon
          />
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { tgLoginApi } from "@/api/tgLogin";

const loginForm = ref<any>(null);

const form = reactive({
  username: ""
});

const rules = reactive({
  username: [{ required: true, message: "请输入验证码", trigger: "blur" }]
});

const submitForm = () => {
  loginForm.value.validate(async (valid: boolean) => {
    if (valid) {
      try {
        const result = await tgLoginApi.config(form.username);
        ElMessage.success("配置成功");
        // 重新获取状态
        await checkLoginStatus();
      } catch (error) {
        ElMessage.error("配置失败");
        console.error("TG配置错误:", error);
      }
    }
  });
};

const isEnter = ref(false);
const loginStatus = ref("");

const checkLoginStatus = async () => {
  try {
    const str = await tgLoginApi.getStatus();
    if (str !== undefined && str !== null) {
      isEnter.value = str.indexOf("Enter") >= 0;
    }
    loginStatus.value = str;
  } catch (error) {
    console.error("获取TG登录状态失败:", error);
    loginStatus.value = "获取登录状态失败";
    isEnter.value = false;
  }
};

const getStatusType = (status: string) => {
  if (!status) return "info";
  if (status.includes("Enter")) return "warning";
  if (status.includes("Success") || status.includes("已登录")) return "success";
  if (status.includes("Error") || status.includes("失败")) return "error";
  return "info";
};

onMounted(async () => {
  await checkLoginStatus();
});
</script>

<style scoped>
.tg-login-container {
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
}

.login-content {
  margin-top: 20px;
}

.login-status {
  padding: 20px;
}
</style>
