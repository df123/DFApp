<template>
  <div class="lottery-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">开奖结果</span>
        </div>
      </template>

      <!-- 数据表格 -->
      <div class="table-container">
        <el-table
          v-loading="loading"
          :data="tableData"
          style="width: 100%"
          stripe
          @sort-change="handleSortChange"
        >
          <el-table-column prop="id" label="ID" sortable="custom" width="80" />
          <el-table-column
            prop="code"
            label="期号"
            sortable="custom"
            width="120"
          />
          <el-table-column
            prop="name"
            label="彩票名称"
            sortable="custom"
            width="150"
          />
          <el-table-column prop="blue" label="蓝球号码" min-width="100">
            <template #default="scope">
              <span class="blue-number">{{ scope.row.blue }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="red" label="红球号码" min-width="300">
            <template #default="scope">
              <span class="red-number">{{ scope.row.red }}</span>
            </template>
          </el-table-column>
          <el-table-column
            prop="date"
            label="开奖日期"
            sortable="custom"
            min-width="120"
          />
          <el-table-column
            prop="creationTime"
            label="创建时间"
            sortable="custom"
            min-width="180"
          >
            <template #default="scope">
              {{ formatDateTime(scope.row.creationTime) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" fixed="right" width="120">
            <template #default="scope">
              <el-button
                v-if="hasPermission('DFApp.Lottery.Delete')"
                size="small"
                type="danger"
                link
                @click="handleDelete(scope.row)"
              >
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalCount"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import type { Sort } from "element-plus";
import { lotteryResultApi } from "@/api/lottery";
import type { LotteryResultDto } from "@/types/business";
import type { PagedRequestDto, PagedResultDto } from "@/types/api";

// 简单的权限检查函数
const hasPermission = (permission: string): boolean => {
  // 这里应该根据实际的权限系统实现
  // 暂时返回 true 以便测试
  return true;
};

// 表格数据
const tableData = ref<LotteryResultDto[]>([]);
const loading = ref(false);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);

// 排序参数
const sortParams = reactive({
  sortBy: "creationTime",
  sortOrder: "desc"
});

// 获取彩票结果列表
const fetchLotteryResults = async () => {
  loading.value = true;
  try {
    const params: PagedRequestDto = {
      skipCount: (currentPage.value - 1) * pageSize.value,
      maxResultCount: pageSize.value,
      sorting: `${sortParams.sortBy} ${sortParams.sortOrder}`
    };

    const result: PagedResultDto<LotteryResultDto> =
      await lotteryResultApi.getLotteryResults(params);
    tableData.value = result.items || [];
    totalCount.value = result.totalCount || 0;
  } catch (error) {
    console.error("获取彩票结果失败:", error);
    ElMessage.error("获取彩票结果失败");
  } finally {
    loading.value = false;
  }
};

// 处理排序变化
const handleSortChange = (sort: Sort) => {
  if (sort.prop) {
    sortParams.sortBy = sort.prop as string;
    sortParams.sortOrder = sort.order === "ascending" ? "asc" : "desc";
    fetchLotteryResults();
  }
};

// 处理删除
const handleDelete = async (row: LotteryResultDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除期号为 ${row.code} 的彩票结果吗？`,
      "确认删除",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await lotteryResultApi.deleteLotteryResult(row.id);
    ElMessage.success("删除成功");
    fetchLotteryResults();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除彩票结果失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

// 处理分页大小变化
const handleSizeChange = (newSize: number) => {
  pageSize.value = newSize;
  currentPage.value = 1;
  fetchLotteryResults();
};

// 处理当前页变化
const handleCurrentChange = (newPage: number) => {
  currentPage.value = newPage;
  fetchLotteryResults();
};

// 格式化日期时间
const formatDateTime = (dateTime: string) => {
  if (!dateTime) return "";
  const date = new Date(dateTime);
  return date.toLocaleString("zh-CN");
};

// 初始化加载数据
onMounted(() => {
  fetchLotteryResults();
});
</script>

<style scoped>
.lottery-container {
  height: 100%;
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
  color: #303133;
}

.table-container {
  width: 100%;
  overflow-x: auto;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.red-number {
  font-weight: bold;
  color: #f56c6c;
}

.blue-number {
  font-weight: bold;
  color: #409eff;
}
</style>
