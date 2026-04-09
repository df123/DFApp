<script setup lang="ts">
import { computed } from "vue";
import { useUserStoreHook } from "@/store/modules/user";
import { useRouter } from "vue-router";

defineOptions({
  name: "PermissionPage"
});

const router = useRouter();
const username = computed(() => useUserStoreHook()?.username || "未登录");
const roles = computed(() => useUserStoreHook()?.roles || []);

function handleLogout() {
  router.push("/login");
}
</script>

<template>
  <div>
    <p class="mb-2!">当前用户权限信息</p>
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>当前用户：{{ username }}</span>
        </div>
      </template>
      <div class="mb-4">
        <strong>角色：</strong>
        <el-tag v-for="role in roles" :key="role" class="mr-2" type="success">
          {{ role }}
        </el-tag>
        <el-tag v-if="roles.length === 0" type="info">无角色</el-tag>
      </div>
      <el-alert title="提示" type="info" :closable="false" show-icon>
        如需切换角色或测试不同权限，请退出登录后重新登录
      </el-alert>
      <div class="mt-4">
        <el-button type="primary" @click="handleLogout"> 退出登录 </el-button>
      </div>
    </el-card>
  </div>
</template>
