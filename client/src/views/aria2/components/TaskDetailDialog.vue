<template>
  <el-dialog
    v-model="visible"
    :title="taskDetail?.btName || '下载任务详情'"
    width="900px"
    @close="handleClose"
  >
    <div v-loading="loading" class="task-detail-container">
      <template v-if="taskDetail">
        <!-- 基本信息 -->
        <el-card class="info-card" shadow="never">
          <template #header>
            <span class="card-title">基本信息</span>
          </template>
          <el-descriptions :column="2" border>
            <el-descriptions-item label="GID">
              {{ taskDetail.gid }}
            </el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="getStatusType(taskDetail.status)">
                {{ getStatusText(taskDetail.status) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="文件大小">
              {{ formatBytes(taskDetail.totalLength) }}
            </el-descriptions-item>
            <el-descriptions-item label="已完成">
              {{ formatBytes(taskDetail.completedLength) }} ({{
                getProgressPercentage
              }}%)
            </el-descriptions-item>
            <el-descriptions-item label="下载速度">
              {{ formatSpeed(taskDetail.downloadSpeed) }}
            </el-descriptions-item>
            <el-descriptions-item label="上传速度">
              {{ formatSpeed(taskDetail.uploadSpeed) }}
            </el-descriptions-item>
            <el-descriptions-item label="已上传">
              {{ formatBytes(taskDetail.uploadedLength) }}
            </el-descriptions-item>
            <el-descriptions-item label="分享率">
              <el-tag
                :type="taskDetail.shareRatio >= 1 ? 'success' : 'warning'"
              >
                {{ taskDetail.shareRatio.toFixed(2) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item v-if="taskDetail.connections" label="连接数">
              {{ taskDetail.connections }}
            </el-descriptions-item>
            <el-descriptions-item label="保存路径" :span="2">
              {{ taskDetail.dir || "-" }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <!-- 文件列表 -->
        <el-card class="files-card" shadow="never">
          <template #header>
            <span class="card-title"
              >文件列表 ({{ taskDetail.files.length }})</span
            >
          </template>
          <el-table :data="taskDetail.files" stripe max-height="300">
            <el-table-column prop="index" label="#" width="60" />
            <el-table-column
              prop="path"
              label="文件名"
              min-width="200"
              show-overflow-tooltip
            />
            <el-table-column label="大小" width="120">
              <template #default="{ row }">
                {{ formatBytes(row.length) }}
              </template>
            </el-table-column>
            <el-table-column label="已完成" width="120">
              <template #default="{ row }">
                {{ formatBytes(row.completedLength) }}
              </template>
            </el-table-column>
            <el-table-column label="进度" width="150">
              <template #default="{ row }">
                <el-progress
                  :percentage="getFileProgress(row)"
                  :status="
                    row.completedLength >= row.length ? 'success' : undefined
                  "
                />
              </template>
            </el-table-column>
            <el-table-column label="选中" width="80">
              <template #default="{ row }">
                <el-tag :type="row.selected ? 'success' : 'info'" size="small">
                  {{ row.selected ? "是" : "否" }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- Peer信息 -->
        <el-card class="peers-card" shadow="never">
          <template #header>
            <span class="card-title"
              >连接的 Peers ({{ taskDetail.peers.length }})</span
            >
          </template>
          <el-table :data="taskDetail.peers" stripe max-height="400">
            <el-table-column prop="ip" label="IP地址" width="140" />
            <el-table-column prop="port" label="端口" width="80" />
            <el-table-column label="国家" width="100">
              <template #default="{ row }">
                {{ row.country || "查询中..." }}
              </template>
            </el-table-column>
            <el-table-column label="城市" width="120">
              <template #default="{ row }">
                {{ row.city || "-" }}
              </template>
            </el-table-column>
            <el-table-column label="进度" width="120">
              <template #default="{ row }">
                {{ (row.progress * 100).toFixed(1) }}%
              </template>
            </el-table-column>
            <el-table-column label="下载速度" width="120">
              <template #default="{ row }">
                {{ formatSpeed(row.downloadSpeed) }}
              </template>
            </el-table-column>
            <el-table-column label="上传速度" width="120">
              <template #default="{ row }">
                {{ formatSpeed(row.uploadSpeed) }}
              </template>
            </el-table-column>
            <el-table-column label="类型" width="80">
              <template #default="{ row }">
                <el-tag :type="row.seeder ? 'success' : 'primary'" size="small">
                  {{ row.seeder ? "做种者" : "下载者" }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag size="small">
                  {{ row.peerChoking ? "阻塞" : "未阻塞" }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </template>
    </div>

    <template #footer>
      <el-button @click="handleClose">关闭</el-button>
      <el-button type="primary" @click="refreshDetail">刷新</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { ElMessage } from "element-plus";
import { useAria2Api } from "@/api/aria2";
import type { Aria2TaskDetailDto } from "@/types/business";

interface Props {
  modelValue: boolean;
  gid: string;
}

interface Emits {
  (e: "update:modelValue", value: boolean): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const aria2Api = useAria2Api();
const visible = ref(false);
const loading = ref(false);
const taskDetail = ref<Aria2TaskDetailDto | null>(null);

// 同步 modelValue
watch(
  () => props.modelValue,
  newVal => {
    visible.value = newVal;
    if (newVal && props.gid) {
      loadTaskDetail();
    }
  },
  { immediate: true }
);

watch(visible, newVal => {
  emit("update:modelValue", newVal);
});

// 加载任务详情
const loadTaskDetail = async () => {
  if (!props.gid) return;

  loading.value = true;
  try {
    taskDetail.value = await aria2Api.getTaskDetail(props.gid);

    // 为每个peer查询国家信息
    if (taskDetail.value.peers && taskDetail.value.peers.length > 0) {
      await loadPeerCountries();
    }
  } catch (error) {
    console.error("获取任务详情失败:", error);
    ElMessage.error("获取任务详情失败");
  } finally {
    loading.value = false;
  }
};

// 查询peer的国家和城市信息
const loadPeerCountries = async () => {
  if (!taskDetail.value?.peers) return;

  try {
    // 构建批量查询请求体（最多100个IP）
    const ips = taskDetail.value.peers.map(peer => peer.ip);

    // 通过后端代理调用 ip-api.com（解决 HTTPS 页面调用 HTTP API 的混合内容问题）
    const data = await aria2Api.getIpGeolocation(ips);

    // 将批量响应映射回对应的peer
    data.forEach((item: any, index: number) => {
      if (item.status === "success") {
        taskDetail.value!.peers[index].country = item.country || "";
        taskDetail.value!.peers[index].city = item.city || "";
      } else {
        taskDetail.value!.peers[index].country = "未知";
        taskDetail.value!.peers[index].city = "";
      }
    });
  } catch (error) {
    console.error("批量查询IP地理位置信息失败:", error);
    // 如果批量请求失败，将所有国家设置为未知
    taskDetail.value.peers.forEach(peer => {
      peer.country = "未知";
      peer.city = "";
    });
  }
};

// 刷新详情
const refreshDetail = () => {
  loadTaskDetail();
};

// 关闭对话框
const handleClose = () => {
  visible.value = false;
};

// 工具函数
const formatBytes = (bytes: number): string => {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + " " + sizes[i];
};

const formatSpeed = (speed: number): string => {
  return formatBytes(speed) + "/s";
};

const getProgressPercentage = computed(() => {
  if (!taskDetail.value || taskDetail.value.totalLength === 0) return 0;
  return Math.round(
    (taskDetail.value.completedLength / taskDetail.value.totalLength) * 100
  );
});

const getFileProgress = (file: any): number => {
  if (file.length === 0) return 0;
  return Math.round((file.completedLength / file.length) * 100);
};

const getStatusText = (status: string): string => {
  const statusMap: Record<string, string> = {
    active: "下载中",
    waiting: "等待中",
    paused: "已暂停",
    error: "错误",
    complete: "已完成",
    removed: "已移除"
  };
  return statusMap[status] || status;
};

const getStatusType = (status: string) => {
  const typeMap: Record<string, any> = {
    active: "primary",
    waiting: "info",
    paused: "warning",
    error: "danger",
    complete: "success",
    removed: "info"
  };
  return typeMap[status] || "info";
};
</script>

<style scoped>
.task-detail-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
}

.info-card,
.files-card,
.peers-card {
  margin-bottom: 0;
}

:deep(.el-card__header) {
  padding: 12px 16px;
  background-color: #f5f7fa;
}

:deep(.el-card__body) {
  padding: 16px;
}
</style>
