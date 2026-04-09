<template>
  <div class="external-link-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">外部链接管理</span>
          <el-button
            v-permission="['DFApp.Medias.Create']"
            type="primary"
            @click="handleCreateExternalLink"
          >
            <el-icon><Plus /></el-icon>
            新增外部链接
          </el-button>
        </div>
      </template>

      <!-- 数据表格 -->
      <pure-table
        ref="tableRef"
        :loading="loading"
        :data="tableData"
        :columns="columns"
        :pagination="pagination"
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
      >
        <template #operation="{ row }">
          <el-button
            size="small"
            type="primary"
            link
            @click="handleViewLink(row)"
          >
            查看链接
          </el-button>
          <el-button
            v-permission="['DFApp.Medias.Delete']"
            size="small"
            type="warning"
            link
            @click="handleRemoveFile(row)"
          >
            移除文件
          </el-button>
          <el-button
            v-permission="['DFApp.Medias.Delete']"
            size="small"
            type="danger"
            link
            @click="handleDelete(row)"
          >
            删除
          </el-button>
        </template>
      </pure-table>
    </el-card>

    <!-- 链接内容对话框 -->
    <el-dialog
      v-model="linkDialogVisible"
      title="链接内容"
      width="600px"
      :before-close="handleCloseLinkDialog"
    >
      <el-form label-width="100px">
        <el-form-item label="链接地址">
          <el-input v-model="linkContent" type="textarea" :rows="4" readonly />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button type="primary" @click="handleCopyLink">
          <el-icon><CopyDocument /></el-icon>
          复制链接
        </el-button>
        <el-button @click="handleCloseLinkDialog">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { Plus, CopyDocument } from "@element-plus/icons-vue";
import { useExternalLinkApi } from "@/api/externalLink";
import type { ExternalLinkDto } from "@/types/business";

// API 服务
const externalLinkApi = useExternalLinkApi();

// 响应式数据
const loading = ref(false);
const tableRef = ref();
const linkDialogVisible = ref(false);
const linkContent = ref("");

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const tableData = ref<ExternalLinkDto[]>([]);

// 表格列配置
const columns = ref([
  {
    label: "ID",
    prop: "id",
    width: 80
  },
  {
    label: "名称",
    prop: "name",
    minWidth: 120,
    showOverflowTooltip: true
  },
  {
    label: "大小",
    prop: "size",
    width: 100,
    formatter: (row: ExternalLinkDto) => {
      return row.size ? convertBytes(row.size) : "-";
    }
  },
  {
    label: "耗时",
    prop: "timeConsumed",
    width: 100,
    showOverflowTooltip: true
  },
  {
    label: "是否移除",
    prop: "isRemove",
    width: 100,
    formatter: (row: ExternalLinkDto) => {
      return row.isRemove ? "是" : "否";
    }
  },
  {
    label: "创建时间",
    prop: "creationTime",
    width: 180,
    formatter: (row: ExternalLinkDto) => {
      return row.creationTime
        ? new Date(row.creationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "修改时间",
    prop: "lastModificationTime",
    width: 180,
    formatter: (row: ExternalLinkDto) => {
      return row.lastModificationTime
        ? new Date(row.lastModificationTime).toLocaleString()
        : "-";
    }
  },
  {
    label: "操作",
    slot: "operation",
    fixed: "right",
    width: 200
  }
]);

// 方法
const loadTableData = async () => {
  loading.value = true;
  try {
    const params = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize
    };

    const result = await externalLinkApi.getList(params);
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载外部链接数据失败:", error);
    ElMessage.error("加载外部链接数据失败");
  } finally {
    loading.value = false;
  }
};

const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadTableData();
};

const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadTableData();
};

const handleCreateExternalLink = async () => {
  try {
    await externalLinkApi.getExternalLinkStatus();
    ElMessage.success("外部链接添加成功");
    loadTableData();
  } catch (error) {
    console.error("添加外部链接失败:", error);
    ElMessage.error("添加外部链接失败");
  }
};

const handleViewLink = async (row: ExternalLinkDto) => {
  try {
    const result = await externalLinkApi.get(row.id);
    linkContent.value = result.linkContent || "";
    linkDialogVisible.value = true;
  } catch (error) {
    console.error("获取链接内容失败:", error);
    ElMessage.error("获取链接内容失败");
  }
};

const handleCopyLink = async () => {
  try {
    await navigator.clipboard.writeText(linkContent.value);
    ElMessage.success("链接复制成功");
  } catch (error) {
    console.error("复制链接失败:", error);
    ElMessage.error("复制链接失败");
  }
};

const handleCloseLinkDialog = () => {
  linkDialogVisible.value = false;
  linkContent.value = "";
};

const handleRemoveFile = async (row: ExternalLinkDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要移除文件 "${row.name}" 吗？`,
      "移除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    loading.value = true;

    await externalLinkApi.deleteFile(row.id);
    ElMessage.success("文件移除成功");

    // 等待2秒后刷新表格
    setTimeout(() => {
      loadTableData();
    }, 2000);
  } catch (error) {
    loading.value = false;
    if (error !== "cancel") {
      console.error("移除文件失败:", error);
      ElMessage.error("移除文件失败");
    }
  }
};

const handleDelete = async (row: ExternalLinkDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除记录 "${row.name}" 吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await externalLinkApi.delete(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除记录失败:", error);
      ElMessage.error("删除记录失败");
    }
  }
};

// 工具函数：转换字节大小为可读格式
const convertBytes = (bytes: number | string): string => {
  const bytesNum = typeof bytes === "string" ? parseInt(bytes) : bytes;
  if (isNaN(bytesNum)) return "-";

  const sizes = ["Bytes", "KB", "MB", "GB", "TB"];
  if (bytesNum === 0) return "0 Bytes";

  const i = Math.floor(Math.log(bytesNum) / Math.log(1024));
  return (
    Math.round((bytesNum / Math.pow(1024, i)) * 100) / 100 + " " + sizes[i]
  );
};

// 生命周期
onMounted(() => {
  loadTableData();
});
</script>

<style scoped>
.external-link-container {
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
</style>
