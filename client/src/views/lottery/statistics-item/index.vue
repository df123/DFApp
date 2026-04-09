<template>
  <div class="lottery-statistics-item-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">彩票统计项</span>
        </div>
      </template>

      <!-- 搜索区域 -->
      <el-form :model="filter" inline>
        <el-form-item label="彩票类型">
          <el-select
            v-model="filter.lotteryType"
            placeholder="请选择彩票类型"
            clearable
            style="width: 120px"
          >
            <el-option value="双色球" label="双色球" />
            <el-option value="快乐8" label="快乐8" />
          </el-select>
        </el-form-item>
        <el-form-item label="购买期号">
          <el-input
            v-model="filter.purchasedPeriod"
            placeholder="请输入购买期号"
            clearable
            @keyup.enter="handleFilterChange"
          />
        </el-form-item>
        <el-form-item label="开奖期号">
          <el-input
            v-model="filter.winningPeriod"
            placeholder="请输入开奖期号"
            clearable
            @keyup.enter="handleFilterChange"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleFilterChange">
            <el-icon><Search /></el-icon>
            搜索
          </el-button>
          <el-button @click="handleReset">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
        </el-form-item>
      </el-form>

      <!-- 数据表格 -->
      <pure-table
        ref="tableRef"
        :loading="loading"
        :data="dataSource"
        :columns="columns"
        :pagination="pagination"
        @page-size-change="handleSizeChange"
        @page-current-change="handleCurrentChange"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { Search, Refresh } from "@element-plus/icons-vue";
import { lotteryApi } from "@/api/lottery";
import type {
  StatisticsWinItemDto,
  StatisticsWinItemRequestDto
} from "@/types/business";

// 响应式数据
const loading = ref(false);
const tableRef = ref();

// 搜索表单
const filter = reactive({
  purchasedPeriod: "",
  winningPeriod: "",
  lotteryType: "双色球"
});

// 分页数据
const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

// 表格数据
const dataSource = ref<StatisticsWinItemDto[]>([]);

// 表格列配置
const columns = ref([
  {
    label: "期号",
    prop: "code",
    width: 120
  },
  {
    label: "中奖期号",
    prop: "winCode",
    width: 120
  },
  {
    label: "购买号码",
    prop: "buyLotteryString",
    minWidth: 200
  },
  {
    label: "中奖号码",
    prop: "winLotteryString",
    minWidth: 200
  },
  {
    label: "中奖金额",
    prop: "winAmount",
    width: 120,
    align: "right",
    formatter: (row: StatisticsWinItemDto) => {
      return row.winAmount > 0 ? `¥${row.winAmount.toLocaleString()}` : "-";
    },
    cellClassName: (row: StatisticsWinItemDto) => {
      return row.winAmount > 0 ? "win-amount" : "";
    }
  }
]);

// 获取数据
const loadTableData = async () => {
  loading.value = true;
  try {
    const params: StatisticsWinItemRequestDto = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      ...filter
    };

    const response = await lotteryApi.getStatisticsWinItem(params);
    dataSource.value = response.items;
    pagination.total = response.totalCount;
  } catch (error) {
    console.error("获取彩票统计项数据失败:", error);
  } finally {
    loading.value = false;
  }
};

// 处理筛选条件变化
const handleFilterChange = () => {
  pagination.currentPage = 1;
  loadTableData();
};

// 处理重置
const handleReset = () => {
  filter.purchasedPeriod = "";
  filter.winningPeriod = "";
  filter.lotteryType = "双色球";
  pagination.currentPage = 1;
  loadTableData();
};

// 处理分页大小变化
const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadTableData();
};

// 处理当前页变化
const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadTableData();
};

// 初始化加载数据
onMounted(() => {
  loadTableData();
});
</script>

<style scoped>
.lottery-statistics-item-container {
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

:deep(.win-amount) {
  font-weight: bold;
  color: #f5222d;
}
</style>
