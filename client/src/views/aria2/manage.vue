<template>
  <div class="aria2-manage-container">
    <!-- 连接状态卡片 -->
    <el-card v-loading="statusLoading" class="status-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">Aria2 服务状态</span>
          <el-button
            size="small"
            :loading="statusLoading"
            @click="refreshConnectionStatus"
          >
            <el-icon><Refresh /></el-icon>
            刷新
          </el-button>
        </div>
      </template>
      <div class="status-content">
        <div class="status-item">
          <span class="status-label">连接状态:</span>
          <el-tag :type="connectionStatus.isConnected ? 'success' : 'danger'">
            {{ connectionStatus.isConnected ? "已连接" : "未连接" }}
          </el-tag>
        </div>
        <div v-if="connectionStatus.version" class="status-item">
          <span class="status-label">版本:</span>
          <span class="status-value">{{ connectionStatus.version }}</span>
        </div>
        <div v-if="globalStat" class="status-item">
          <span class="status-label">下载速度:</span>
          <span class="status-value">{{
            formatSpeed(globalStat.downloadSpeed)
          }}</span>
        </div>
        <div v-if="globalStat" class="status-item">
          <span class="status-label">上传速度:</span>
          <span class="status-value">{{
            formatSpeed(globalStat.uploadSpeed)
          }}</span>
        </div>
        <div class="status-item">
          <span class="status-label">活跃任务:</span>
          <span class="status-value">{{
            globalStat?.activeCount || activeTasks.length || "0"
          }}</span>
        </div>
        <div class="status-item">
          <span class="status-label">等待任务:</span>
          <span class="status-value">{{
            globalStat?.waitingCount || waitingTasks.length || "0"
          }}</span>
        </div>
        <div class="status-item">
          <span class="status-label">停止任务:</span>
          <span class="status-value">{{
            globalStat?.stoppedTotal || stoppedTasks.length || "0"
          }}</span>
        </div>
      </div>
    </el-card>

    <!-- 添加下载任务对话框 -->
    <el-dialog
      v-model="addTaskDialogVisible"
      title="添加下载任务"
      width="700px"
      :before-close="handleCloseAddDialog"
    >
      <el-tabs v-model="addTaskTab" class="add-task-tabs">
        <!-- 链接下载 -->
        <el-tab-pane label="链接下载" name="uri">
          <el-form :model="addTaskForm" label-width="120px">
            <el-form-item label="下载链接">
              <el-input
                v-model="addTaskForm.urlsText"
                type="textarea"
                :rows="6"
                placeholder="每行一个链接，支持直链和磁力链接"
              />
            </el-form-item>
            <el-form-item label="保存路径">
              <el-input
                v-model="addTaskForm.savePath"
                placeholder="可选，留空使用默认路径"
              />
            </el-form-item>
            <el-form-item label="过滤选项">
              <el-checkbox v-model="addTaskForm.videoOnly"
                >只下载视频</el-checkbox
              >
              <el-checkbox v-model="addTaskForm.enableKeywordFilter"
                >启用关键词过滤</el-checkbox
              >
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <!-- 种子文件下载 -->
        <el-tab-pane label="种子文件" name="torrent">
          <el-form :model="torrentForm" label-width="120px">
            <el-form-item label="种子文件">
              <el-upload
                ref="uploadRef"
                v-model:file-list="torrentFileList"
                :auto-upload="false"
                :on-change="handleTorrentChange"
                :on-remove="handleTorrentRemove"
                multiple
                accept=".torrent"
                drag
              >
                <el-icon class="el-icon--upload"><upload-filled /></el-icon>
                <div class="el-upload__text">
                  拖拽文件到此处或<em>点击上传</em>
                </div>
                <template #tip>
                  <div class="el-upload__tip">
                    支持 .torrent 格式，可批量选择多个文件
                  </div>
                </template>
              </el-upload>
            </el-form-item>
            <el-form-item label="保存路径">
              <el-input
                v-model="torrentForm.savePath"
                placeholder="可选，留空使用默认路径"
              />
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>

      <template #footer>
        <el-button
          type="primary"
          :loading="addLoading"
          @click="handleAddDownload"
        >
          <el-icon><Plus /></el-icon>
          添加下载
        </el-button>
        <el-button @click="handleCloseAddDialog">取消</el-button>
      </template>
    </el-dialog>

    <!-- 任务列表卡片 -->
    <el-card class="tasks-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">下载任务</span>
          <div class="card-actions">
            <el-button type="primary" @click="addTaskDialogVisible = true">
              <el-icon><Plus /></el-icon>
              添加下载任务
            </el-button>
            <el-button-group>
              <el-button
                :type="activeTab === 'active' ? 'primary' : ''"
                @click="activeTab = 'active'"
              >
                活跃 ({{ activeTasks.length }})
              </el-button>
              <el-button
                :type="activeTab === 'waiting' ? 'primary' : ''"
                @click="activeTab = 'waiting'"
              >
                等待 ({{ waitingTasks.length }})
              </el-button>
              <el-button
                :type="activeTab === 'stopped' ? 'primary' : ''"
                @click="activeTab = 'stopped'"
              >
                已停止 ({{ stoppedTasks.length }})
              </el-button>
            </el-button-group>
            <el-button
              size="small"
              :loading="tasksLoading"
              @click="refreshTasks"
            >
              <el-icon><Refresh /></el-icon>
              刷新
            </el-button>
          </div>
        </div>
      </template>

      <!-- 批量操作栏 -->
      <div v-if="selectedGids.length > 0" class="batch-actions">
        <span class="selected-count"
          >已选择 {{ selectedGids.length }} 个任务</span
        >
        <el-button-group>
          <el-button
            v-if="activeTab !== 'stopped'"
            size="small"
            type="warning"
            @click="handlePauseSelected"
          >
            <el-icon><VideoPause /></el-icon>
            暂停
          </el-button>
          <el-button
            v-if="activeTab !== 'stopped'"
            size="small"
            type="success"
            @click="handleUnpauseSelected"
          >
            <el-icon><VideoPlay /></el-icon>
            恢复
          </el-button>
          <el-button
            v-if="activeTab !== 'stopped'"
            size="small"
            type="danger"
            @click="handleStopSelected"
          >
            <el-icon><CircleClose /></el-icon>
            停止
          </el-button>
          <el-button
            v-if="activeTab === 'stopped'"
            size="small"
            type="danger"
            @click="handleRemoveSelected"
          >
            <el-icon><Delete /></el-icon>
            删除
          </el-button>
        </el-button-group>
      </div>

      <!-- 任务列表 -->
      <el-table
        v-loading="tasksLoading"
        :data="displayTasks"
        stripe
        style="width: 100%; margin-top: 16px"
        @selection-change="handleSelectionChange"
      >
        <el-table-column
          v-if="activeTab !== 'stopped'"
          type="selection"
          width="55"
        />
        <el-table-column
          prop="gid"
          label="GID"
          width="150"
          show-overflow-tooltip
        />
        <el-table-column label="文件名" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            {{ getFileName(row) }}
          </template>
        </el-table-column>
        <el-table-column label="大小" width="120">
          <template #default="{ row }">
            {{ formatBytes(row.totalLength) }}
          </template>
        </el-table-column>
        <el-table-column label="进度" width="150">
          <template #default="{ row }">
            <el-progress
              :percentage="getProgress(row)"
              :status="getProgressStatus(row.status)"
            />
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
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="primary"
              link
              @click="handleViewDetail(row.gid)"
            >
              详情
            </el-button>
            <el-button
              v-if="row.status === 'active'"
              size="small"
              type="warning"
              link
              @click="handlePauseTask(row.gid)"
            >
              暂停
            </el-button>
            <el-button
              v-if="row.status === 'paused'"
              size="small"
              type="success"
              link
              @click="handleUnpauseTask(row.gid)"
            >
              恢复
            </el-button>
            <el-button
              v-if="activeTab !== 'stopped'"
              size="small"
              type="danger"
              link
              @click="handleStopTask(row.gid)"
            >
              停止
            </el-button>
            <el-button
              v-if="activeTab === 'stopped'"
              size="small"
              type="danger"
              link
              @click="handleRemoveTask(row.gid)"
            >
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 任务详情对话框 -->
    <TaskDetailDialog v-model="detailDialogVisible" :gid="selectedGid" />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import {
  Refresh,
  Plus,
  VideoPause,
  VideoPlay,
  CircleClose,
  Delete,
  UploadFilled
} from "@element-plus/icons-vue";
import { useAria2Api } from "@/api/aria2";
import type {
  Aria2GlobalStatDto,
  Aria2TaskDto,
  Aria2ConnectionStatusDto,
  AddDownloadRequestDto,
  TorrentFileItemDto,
  BatchAddTorrentRequestDto,
  BatchAddUriRequestDto
} from "@/types/business";
import type { UploadFile, UploadUserFile } from "element-plus";
import TaskDetailDialog from "./components/TaskDetailDialog.vue";
import * as signalR from "@microsoft/signalr";

// API 服务
const aria2Api = useAria2Api();

// 响应式数据
const statusLoading = ref(false);
const tasksLoading = ref(false);
const addLoading = ref(false);
const addTaskDialogVisible = ref(false);
const addTaskTab = ref<"uri" | "torrent">("uri");
const activeTab = ref<"active" | "waiting" | "stopped">("active");

const connectionStatus = ref<Aria2ConnectionStatusDto>({
  isConnected: false
});
const globalStat = ref<Aria2GlobalStatDto | null>(null);
const activeTasks = ref<Aria2TaskDto[]>([]);
const waitingTasks = ref<Aria2TaskDto[]>([]);
const stoppedTasks = ref<Aria2TaskDto[]>([]);
const selectedGids = ref<string[]>([]);

// 是否允许通过 SignalR 更新任务列表（有选中内容时不更新）
const allowSignalRUpdate = ref(true);

// 添加任务表单（链接下载）
const addTaskForm = reactive({
  urlsText: "",
  savePath: "",
  videoOnly: true,
  enableKeywordFilter: true
});

// 种子文件表单
const torrentForm = reactive({
  savePath: ""
});

// 种子文件列表
const torrentFileList = ref<UploadUserFile[]>([]);
const uploadRef = ref();

// 详情对话框
const detailDialogVisible = ref(false);
const selectedGid = ref("");

// SignalR 连接
let hubConnection: signalR.HubConnection | null = null;

// 计算属性
const displayTasks = computed(() => {
  switch (activeTab.value) {
    case "active":
      return activeTasks.value;
    case "waiting":
      return waitingTasks.value;
    case "stopped":
      return stoppedTasks.value;
    default:
      return [];
  }
});

// 方法
const refreshConnectionStatus = async () => {
  statusLoading.value = true;
  try {
    connectionStatus.value = await aria2Api.getConnectionStatus();
    if (connectionStatus.value.isConnected) {
      await refreshGlobalStat();
    }
  } catch (error) {
    console.error("获取连接状态失败:", error);
    ElMessage.error("获取连接状态失败");
  } finally {
    statusLoading.value = false;
  }
};

const refreshGlobalStat = async () => {
  try {
    globalStat.value = await aria2Api.getGlobalStat();
  } catch (error) {
    console.error("获取全局状态失败:", error);
  }
};

const refreshTasks = async () => {
  tasksLoading.value = true;
  try {
    const [active, waiting, stopped] = await Promise.all([
      aria2Api.getActiveTasks(),
      aria2Api.getWaitingTasks(),
      aria2Api.getStoppedTasks(0, 100)
    ]);

    activeTasks.value = active;
    waitingTasks.value = waiting;
    stoppedTasks.value = stopped;
  } catch (error) {
    console.error("获取任务列表失败:", error);
    ElMessage.error("获取任务列表失败");
  } finally {
    tasksLoading.value = false;
  }
};

// 文件转 Base64
const fileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      const result = reader.result as string;
      // 移除 data URL 前缀 (例如: data:application/x-bittorrent;base64,)
      const base64 = result.split(",")[1];
      resolve(base64);
    };
    reader.onerror = error => reject(error);
  });
};

// 种子文件选择变化
const handleTorrentChange = (file: UploadFile, fileList: UploadUserFile[]) => {
  // 直接使用传入的 fileList 参数
  torrentFileList.value = fileList;

  if (!file.raw) {
    return;
  }

  // 验证文件类型
  if (!file.name.endsWith(".torrent")) {
    ElMessage.warning("只能上传 .torrent 格式的文件");
    return false;
  }

  return true;
};

// 种子文件移除
const handleTorrentRemove = (file: UploadFile) => {
  const index = torrentFileList.value.findIndex(f => f.uid === file.uid);
  if (index > -1) {
    torrentFileList.value.splice(index, 1);
  }
};

const handleAddDownload = async () => {
  // 链接下载模式
  if (addTaskTab.value === "uri") {
    if (!addTaskForm.urlsText.trim()) {
      ElMessage.warning("请输入下载链接");
      return;
    }

    const urls = addTaskForm.urlsText
      .split("\n")
      .map(url => url.trim())
      .filter(url => url.length > 0);

    if (urls.length === 0) {
      ElMessage.warning("请输入有效的下载链接");
      return;
    }

    addLoading.value = true;
    try {
      const request: BatchAddUriRequestDto = {
        urls,
        savePath: addTaskForm.savePath || undefined,
        videoOnly: addTaskForm.videoOnly,
        enableKeywordFilter: addTaskForm.enableKeywordFilter
      };

      const gids = await aria2Api.batchAddUri(request);
      ElMessage.success(`已成功添加 ${gids.length} 个下载任务`);
      addTaskDialogVisible.value = false;
      resetAddForm();
      await refreshTasks();
    } catch (error) {
      console.error("添加下载任务失败:", error);
      ElMessage.error("添加下载任务失败");
    } finally {
      addLoading.value = false;
    }
  }
  // 种子文件下载模式
  else if (addTaskTab.value === "torrent") {
    if (torrentFileList.value.length === 0) {
      ElMessage.warning("请选择种子文件");
      return;
    }

    addLoading.value = true;
    try {
      // 读取所有种子文件并转换为 Base64
      const torrentItems: TorrentFileItemDto[] = [];
      for (const file of torrentFileList.value) {
        if (file.raw) {
          const base64 = await fileToBase64(file.raw);
          torrentItems.push({
            torrentData: base64,
            fileName: file.name
          });
        }
      }

      const request: BatchAddTorrentRequestDto = {
        torrents: torrentItems,
        savePath: torrentForm.savePath || undefined
      };

      const gids = await aria2Api.batchAddTorrent(request);
      ElMessage.success(`已成功添加 ${gids.length} 个种子文件下载任务`);
      addTaskDialogVisible.value = false;
      resetAddForm();
      await refreshTasks();
    } catch (error) {
      console.error("添加种子文件下载任务失败:", error);
      ElMessage.error("添加种子文件下载任务失败");
    } finally {
      addLoading.value = false;
    }
  }
};

const resetAddForm = () => {
  addTaskForm.urlsText = "";
  addTaskForm.savePath = "";
  addTaskForm.videoOnly = true;
  addTaskForm.enableKeywordFilter = true;
  torrentForm.savePath = "";
  torrentFileList.value = [];
  addTaskTab.value = "uri";
};

const handleCloseAddDialog = () => {
  addTaskDialogVisible.value = false;
  resetAddForm();
};

const handleSelectionChange = (selection: Aria2TaskDto[]) => {
  selectedGids.value = selection.map(task => task.gid);
  // 当有选中内容时，暂停 SignalR 自动更新，避免清空选择
  allowSignalRUpdate.value = selectedGids.value.length === 0;
};

const handlePauseSelected = async () => {
  if (selectedGids.value.length === 0) return;

  try {
    await aria2Api.pauseTasks({ gids: selectedGids.value });
    ElMessage.success(`已暂停 ${selectedGids.value.length} 个任务`);
    selectedGids.value = [];
    // 清空选择后重新启用 SignalR 更新
    allowSignalRUpdate.value = true;
    await refreshTasks();
  } catch (error) {
    console.error("暂停任务失败:", error);
    ElMessage.error("暂停任务失败");
  }
};

const handleUnpauseSelected = async () => {
  if (selectedGids.value.length === 0) return;

  try {
    await aria2Api.unpauseTasks({ gids: selectedGids.value });
    ElMessage.success(`已恢复 ${selectedGids.value.length} 个任务`);
    selectedGids.value = [];
    // 清空选择后重新启用 SignalR 更新
    allowSignalRUpdate.value = true;
    await refreshTasks();
  } catch (error) {
    console.error("恢复任务失败:", error);
    ElMessage.error("恢复任务失败");
  }
};

const handleStopSelected = async () => {
  if (selectedGids.value.length === 0) return;

  try {
    await ElMessageBox.confirm(
      `确定要停止选中的 ${selectedGids.value.length} 个任务吗？`,
      "停止确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await aria2Api.stopTasks({ gids: selectedGids.value });
    ElMessage.success(`已停止 ${selectedGids.value.length} 个任务`);
    selectedGids.value = [];
    // 清空选择后重新启用 SignalR 更新
    allowSignalRUpdate.value = true;
    await refreshTasks();
  } catch (error) {
    if (error !== "cancel") {
      console.error("停止任务失败:", error);
      ElMessage.error("停止任务失败");
    }
  }
};

const handleRemoveSelected = async () => {
  if (selectedGids.value.length === 0) return;

  try {
    await ElMessageBox.confirm(
      `确定要删除选中的 ${selectedGids.value.length} 个任务吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await aria2Api.removeTasks({ gids: selectedGids.value });
    ElMessage.success(`已删除 ${selectedGids.value.length} 个任务`);
    selectedGids.value = [];
    // 清空选择后重新启用 SignalR 更新
    allowSignalRUpdate.value = true;
    await refreshTasks();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除任务失败:", error);
      ElMessage.error("删除任务失败");
    }
  }
};

const handlePauseTask = async (gid: string) => {
  try {
    await aria2Api.pauseTasks({ gids: [gid] });
    ElMessage.success("任务已暂停");
    await refreshTasks();
  } catch (error) {
    console.error("暂停任务失败:", error);
    ElMessage.error("暂停任务失败");
  }
};

const handleUnpauseTask = async (gid: string) => {
  try {
    await aria2Api.unpauseTasks({ gids: [gid] });
    ElMessage.success("任务已恢复");
    await refreshTasks();
  } catch (error) {
    console.error("恢复任务失败:", error);
    ElMessage.error("恢复任务失败");
  }
};

const handleStopTask = async (gid: string) => {
  try {
    await ElMessageBox.confirm("确定要停止此任务吗？", "停止确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await aria2Api.stopTasks({ gids: [gid] });
    ElMessage.success("任务已停止");
    await refreshTasks();
  } catch (error) {
    if (error !== "cancel") {
      console.error("停止任务失败:", error);
      ElMessage.error("停止任务失败");
    }
  }
};

const handleRemoveTask = async (gid: string) => {
  try {
    await ElMessageBox.confirm("确定要删除此任务吗？", "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await aria2Api.removeTasks({ gids: [gid] });
    ElMessage.success("任务已删除");
    await refreshTasks();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除任务失败:", error);
      ElMessage.error("删除任务失败");
    }
  }
};

const handleViewDetail = (gid: string) => {
  selectedGid.value = gid;
  detailDialogVisible.value = true;
};

// 工具函数
const formatBytes = (bytes: number): string => {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB", "TB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + " " + sizes[i];
};

const formatSpeed = (speed: string | number): string => {
  let speedNum: number;
  if (typeof speed === "string") {
    speedNum = parseInt(speed) || 0;
  } else {
    speedNum = speed || 0;
  }
  return formatBytes(speedNum) + "/s";
};

const getFileName = (task: Aria2TaskDto): string => {
  if (task.files && task.files.length > 0) {
    return task.files[0].path.split("/").pop() || task.files[0].path;
  }
  return task.gid;
};

const getProgress = (task: Aria2TaskDto): number => {
  if (task.totalLength === 0) return 0;
  return Math.round((task.completedLength / task.totalLength) * 100);
};

const getProgressStatus = (status: string) => {
  if (status === "complete") return "success";
  if (status === "error") return "exception";
  return undefined;
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

// SignalR 连接
const startSignalRConnection = async () => {
  try {
    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/aria2")
      .withAutomaticReconnect()
      .build();

    hubConnection.on("ReceiveGlobalStat", (stat: Aria2GlobalStatDto) => {
      globalStat.value = stat;
    });

    hubConnection.on("ReceiveActiveTasks", (tasks: Aria2TaskDto[]) => {
      // 只有在允许更新时才更新任务列表（避免清空用户选择）
      if (allowSignalRUpdate.value) {
        activeTasks.value = tasks;
      }
    });

    hubConnection.on("ReceiveWaitingTasks", (tasks: Aria2TaskDto[]) => {
      // 只有在允许更新时才更新任务列表（避免清空用户选择）
      if (allowSignalRUpdate.value) {
        waitingTasks.value = tasks;
      }
    });

    hubConnection.on("ReceiveStoppedTasks", (tasks: Aria2TaskDto[]) => {
      // 只有在允许更新时才更新任务列表（避免清空用户选择）
      if (allowSignalRUpdate.value) {
        stoppedTasks.value = tasks;
      }
    });

    await hubConnection.start();
    console.log("SignalR 连接已建立");

    // 连接成功后加入监控组
    await hubConnection.invoke("JoinMonitorGroup");
    console.log("已加入 Aria2 监控组");
  } catch (error) {
    console.error("SignalR 连接失败:", error);
  }
};

const stopSignalRConnection = async () => {
  if (hubConnection) {
    try {
      // 离开监控组
      await hubConnection.invoke("LeaveMonitorGroup");
      console.log("已离开 Aria2 监控组");

      await hubConnection.stop();
      console.log("SignalR 连接已关闭");
    } catch (error) {
      console.error("关闭 SignalR 连接失败:", error);
    }
  }
};

// 生命周期
onMounted(async () => {
  await refreshConnectionStatus();
  await refreshTasks();
  // 单独加载全局统计信息
  await refreshGlobalStat();
  await startSignalRConnection();
});

onUnmounted(async () => {
  await stopSignalRConnection();
});
</script>

<style scoped>
.aria2-manage-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
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

.card-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.status-card .status-content {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.status-item {
  display: flex;
  gap: 8px;
  align-items: center;
}

.status-label {
  font-weight: 500;
  color: #606266;
}

.status-value {
  color: #303133;
}

.batch-actions {
  display: flex;
  gap: 12px;
  align-items: center;
  padding: 12px;
  margin-bottom: 16px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.selected-count {
  font-size: 14px;
  font-weight: 500;
  color: #606266;
}
</style>
