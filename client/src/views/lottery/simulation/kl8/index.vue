<template>
  <div class="kl8-simulation-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">快乐8模拟</span>
          <div class="card-actions">
            <el-button
              type="danger"
              :icon="Delete"
              @click="handleDeleteByTermNumber"
            >
              按期号删除
            </el-button>
            <el-button
              type="info"
              :icon="TrendCharts"
              @click="handleStatistics"
            >
              统计分析
            </el-button>
            <el-button
              type="primary"
              :icon="Plus"
              @click="handleGenerateRandomNumbers"
            >
              生成随机号码
            </el-button>
          </div>
        </div>
      </template>

      <!-- 数据表格 -->
      <div class="table-container">
        <el-table
          v-loading="loading"
          :data="tableData"
          stripe
          style="width: 100%"
          @sort-change="handleSortChange"
        >
          <el-table-column
            prop="termNumber"
            label="期号"
            sortable="custom"
            width="120"
          />
          <el-table-column prop="redNumbers" label="号码" min-width="300">
            <template #default="scope">
              <div class="numbers-container">
                <el-tag
                  v-for="(num, index) in scope.row.redNumbers.split(',')"
                  :key="index"
                  :type="getNumberTagType(num, index)"
                  class="number-tag"
                >
                  {{ num }}
                </el-tag>
              </div>
            </template>
          </el-table-column>
          <el-table-column prop="gameType" label="游戏类型" width="100">
            <template #default="scope">
              {{ scope.row.gameType === 0 ? "双色球" : "快乐8" }}
            </template>
          </el-table-column>
          <el-table-column
            prop="groupId"
            label="组号"
            sortable="custom"
            width="100"
          />
        </el-table>

        <!-- 分页 -->
        <div class="pagination-container">
          <el-pagination
            v-model:current-page="pagination.currentPage"
            v-model:page-size="pagination.pageSize"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            :total="pagination.total"
            @size-change="handleSizeChange"
            @current-change="handleCurrentChange"
          />
        </div>
      </div>
    </el-card>

    <!-- 生成随机号码模态框 -->
    <el-dialog
      v-model="generateModalVisible"
      title="生成随机号码"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="generateFormRef"
        :model="generateForm"
        :rules="generateFormRules"
        label-width="100px"
      >
        <el-form-item label="生成组数" prop="count">
          <el-input-number
            v-model="generateForm.count"
            :min="1"
            :max="1000"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="游戏类型" prop="gameType">
          <el-select v-model="generateForm.gameType" style="width: 100%">
            <el-option label="双色球" :value="0" />
            <el-option label="快乐8" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item label="期号" prop="termNumber">
          <el-input
            v-model.number="generateForm.termNumber"
            placeholder="请输入期号，格式：2023001"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="generateModalVisible = false">取消</el-button>
          <el-button
            type="primary"
            :loading="generateLoading"
            @click="handleGenerateSubmit"
          >
            确定
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 按期号删除模态框 -->
    <el-dialog
      v-model="deleteModalVisible"
      title="按期号删除"
      width="400px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="deleteFormRef"
        :model="deleteForm"
        :rules="deleteFormRules"
        label-width="100px"
      >
        <el-form-item label="期号" prop="termNumber">
          <el-input
            v-model.number="deleteForm.termNumber"
            placeholder="请输入要删除的期号"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="deleteModalVisible = false">取消</el-button>
          <el-button
            type="danger"
            :loading="deleteLoading"
            @click="handleDeleteSubmit"
          >
            删除
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 统计图表模态框 -->
    <el-dialog
      v-model="statisticsModalVisible"
      title="统计分析"
      width="80%"
      :close-on-click-modal="false"
    >
      <div class="statistics-container">
        <div v-if="statisticsLoading" class="statistics-loading">
          <el-skeleton :rows="6" animated />
        </div>
        <div
          v-else-if="!statisticsData.terms || statisticsData.terms.length === 0"
          class="statistics-empty"
        >
          <el-empty description="暂无统计数据" />
        </div>
        <div v-else class="statistics-chart">
          <canvas
            ref="statisticsChart"
            style="width: 100%; height: 100%"
            :width="800"
            :height="400"
          />
        </div>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="statisticsModalVisible = false">关闭</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick, watch } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { Delete, Plus, TrendCharts } from "@element-plus/icons-vue";
import { lotteryKL8SimulationApi } from "@/api/lottery";
import type {
  LotterySimulationDto,
  GenerateRandomNumbersDto,
  DeleteByTermNumberDto,
  StatisticsDto,
  PagedRequestDto
} from "@/types/business";
import { LotteryKL8PlayType } from "@/types/business";
import Chart from "chart.js/auto";
import dayjs from "dayjs";

// 表格数据
const loading = ref(false);
const tableData = ref<LotterySimulationDto[]>([]);
const pagination = reactive({
  currentPage: 1,
  pageSize: 10,
  total: 0
});
const sortField = ref<string>("");
const sortOrder = ref<string>("");

// 生成随机号码模态框
const generateModalVisible = ref(false);
const generateLoading = ref(false);
const generateFormRef = ref<FormInstance>();
const generateForm = reactive<GenerateRandomNumbersDto>({
  count: 200,
  gameType: 1, // 默认为快乐8
  termNumber: 0
});
const generateFormRules: FormRules = {
  count: [
    { required: true, message: "请输入生成组数", trigger: "blur" },
    {
      type: "number",
      min: 1,
      max: 1000,
      message: "生成组数必须在1-1000之间",
      trigger: "blur"
    }
  ],
  gameType: [{ required: true, message: "请选择游戏类型", trigger: "change" }],
  termNumber: [
    { required: true, message: "请输入期号", trigger: "blur" },
    { type: "number", min: 2023001, message: "期号格式不正确", trigger: "blur" }
  ]
};

// 按期号删除模态框
const deleteModalVisible = ref(false);
const deleteLoading = ref(false);
const deleteFormRef = ref<FormInstance>();
const deleteForm = reactive<DeleteByTermNumberDto>({
  termNumber: 0
});
const deleteFormRules: FormRules = {
  termNumber: [
    { required: true, message: "请输入期号", trigger: "blur" },
    { type: "number", min: 2023001, message: "期号格式不正确", trigger: "blur" }
  ]
};

// 统计图表模态框
const statisticsModalVisible = ref(false);
const statisticsLoading = ref(false);
const statisticsData = ref<StatisticsDto>({
  terms: [],
  purchaseAmounts: [],
  winningAmounts: []
});
const statisticsChart = ref<HTMLCanvasElement>();
let chartInstance: Chart | null = null;

// KL8玩法类型映射
const playTypeNames: Record<LotteryKL8PlayType, string> = {
  [LotteryKL8PlayType.Select1]: "选一",
  [LotteryKL8PlayType.Select2]: "选二",
  [LotteryKL8PlayType.Select3]: "选三",
  [LotteryKL8PlayType.Select4]: "选四",
  [LotteryKL8PlayType.Select5]: "选五",
  [LotteryKL8PlayType.Select6]: "选六",
  [LotteryKL8PlayType.Select7]: "选七",
  [LotteryKL8PlayType.Select8]: "选八",
  [LotteryKL8PlayType.Select9]: "选九",
  [LotteryKL8PlayType.Select10]: "选十"
};

// 获取号码标签类型
const getNumberTagType = (num: string, index: number) => {
  // 快乐8号码1-80，根据区间返回不同颜色
  const numValue = parseInt(num);
  if (numValue >= 1 && numValue <= 20) return "danger"; // 红色
  if (numValue >= 21 && numValue <= 40) return "primary"; // 蓝色
  if (numValue >= 41 && numValue <= 60) return "success"; // 绿色
  if (numValue >= 61 && numValue <= 80) return "warning"; // 黄色
  return "info";
};

// 加载数据
const loadData = async () => {
  loading.value = true;
  try {
    const params: PagedRequestDto = {
      skipCount: (pagination.currentPage - 1) * pagination.pageSize,
      maxResultCount: pagination.pageSize,
      sorting: sortField.value ? `${sortField.value} ${sortOrder.value}` : ""
    };

    const result = await lotteryKL8SimulationApi.getKL8Simulations(params);
    tableData.value = result.items || [];
    pagination.total = result.totalCount || 0;
  } catch (error) {
    console.error("加载数据失败:", error);
    ElMessage.error("加载数据失败");
  } finally {
    loading.value = false;
  }
};

// 格式化日期时间
const formatDateTime = (dateTime?: string) => {
  if (!dateTime) return "-";
  return dayjs(dateTime).format("YYYY-MM-DD HH:mm:ss");
};

// 处理排序变化
const handleSortChange = ({ prop, order }: { prop: string; order: string }) => {
  sortField.value = prop;
  sortOrder.value = order === "ascending" ? "asc" : "desc";
  loadData();
};

// 处理分页大小变化
const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadData();
};

// 处理当前页变化
const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadData();
};

// 处理生成随机号码
const handleGenerateRandomNumbers = () => {
  // 重置表单
  generateForm.count = 200;
  generateForm.gameType = 1; // 默认为快乐8
  generateForm.termNumber = 0;
  generateModalVisible.value = true;
  nextTick(() => {
    generateFormRef.value?.resetFields();
  });
};

// 处理生成提交
const handleGenerateSubmit = async () => {
  if (!generateFormRef.value) return;

  const valid = await generateFormRef.value.validate().catch(() => false);
  if (!valid) return;

  generateLoading.value = true;
  try {
    await lotteryKL8SimulationApi.generateKL8Simulation(generateForm);
    ElMessage.success("生成成功");
    generateModalVisible.value = false;
    loadData();
  } catch (error) {
    console.error("生成失败:", error);
    ElMessage.error("生成失败");
  } finally {
    generateLoading.value = false;
  }
};

// 处理按期号删除
const handleDeleteByTermNumber = () => {
  // 重置表单
  deleteForm.termNumber = 0;
  deleteModalVisible.value = true;
  nextTick(() => {
    deleteFormRef.value?.resetFields();
  });
};

// 处理删除提交
const handleDeleteSubmit = async () => {
  if (!deleteFormRef.value) return;

  const valid = await deleteFormRef.value.validate().catch(() => false);
  if (!valid) return;

  try {
    await ElMessageBox.confirm(
      `确定要删除期号为 ${deleteForm.termNumber} 的数据吗？`,
      "确认删除",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );
  } catch {
    return;
  }

  deleteLoading.value = true;
  try {
    await lotteryKL8SimulationApi.deleteKL8ByTermNumber(deleteForm);
    ElMessage.success("删除成功");
    deleteModalVisible.value = false;
    loadData();
  } catch (error) {
    console.error("删除失败:", error);
    ElMessage.error("删除失败");
  } finally {
    deleteLoading.value = false;
  }
};

// 处理统计分析
const handleStatistics = async () => {
  statisticsModalVisible.value = true;
};

// 加载统计数据
const loadStatisticsData = async () => {
  statisticsLoading.value = true;
  try {
    const data = await lotteryKL8SimulationApi.getKL8Statistics();
    statisticsData.value = data;

    // 等待DOM更新后初始化图表
    await nextTick();
    setTimeout(() => {
      initChart();
    }, 100);
  } catch (error) {
    console.error("加载统计数据失败:", error);
    ElMessage.error("加载统计数据失败");
  } finally {
    statisticsLoading.value = false;
  }
};

// 初始化图表
const initChart = () => {
  if (!statisticsChart.value) {
    console.error("Canvas元素未找到");
    return;
  }

  const ctx = statisticsChart.value.getContext("2d");
  if (!ctx) {
    console.error("无法获取Canvas上下文");
    return;
  }

  // 销毁现有图表
  if (chartInstance) {
    chartInstance.destroy();
    chartInstance = null;
  }

  // 检查数据是否有效
  if (!statisticsData.value.terms || statisticsData.value.terms.length === 0) {
    console.error("统计数据为空");
    return;
  }

  // 准备数据集
  const datasets = [];
  const colors = [
    {
      backgroundColor: "rgba(255, 99, 132, 0.5)",
      borderColor: "rgb(255, 99, 132)"
    }, // 红色
    {
      backgroundColor: "rgba(54, 162, 235, 0.5)",
      borderColor: "rgb(54, 162, 235)"
    }, // 蓝色
    {
      backgroundColor: "rgba(255, 206, 86, 0.5)",
      borderColor: "rgb(255, 206, 86)"
    }, // 黄色
    {
      backgroundColor: "rgba(75, 192, 192, 0.5)",
      borderColor: "rgb(75, 192, 192)"
    }, // 青色
    {
      backgroundColor: "rgba(153, 102, 255, 0.5)",
      borderColor: "rgb(153, 102, 255)"
    }, // 紫色
    {
      backgroundColor: "rgba(255, 159, 64, 0.5)",
      borderColor: "rgb(255, 159, 64)"
    }, // 橙色
    {
      backgroundColor: "rgba(199, 199, 199, 0.5)",
      borderColor: "rgb(199, 199, 199)"
    }, // 灰色
    {
      backgroundColor: "rgba(83, 102, 255, 0.5)",
      borderColor: "rgb(83, 102, 255)"
    }, // 靛青
    {
      backgroundColor: "rgba(255, 99, 255, 0.5)",
      borderColor: "rgb(255, 99, 255)"
    }, // 粉红
    {
      backgroundColor: "rgba(159, 255, 64, 0.5)",
      borderColor: "rgb(159, 255, 64)"
    }, // 浅绿
    {
      backgroundColor: "rgba(255, 0, 0, 0.5)",
      borderColor: "rgb(255, 0, 0)"
    }, // 深红
    {
      backgroundColor: "rgba(0, 255, 0, 0.5)",
      borderColor: "rgb(0, 255, 0)"
    }, // 亮绿
    {
      backgroundColor: "rgba(0, 0, 255, 0.5)",
      borderColor: "rgb(0, 0, 255)"
    }, // 深蓝
    {
      backgroundColor: "rgba(128, 0, 128, 0.5)",
      borderColor: "rgb(128, 0, 128)"
    }, // 深紫
    {
      backgroundColor: "rgba(128, 128, 0, 0.5)",
      borderColor: "rgb(128, 128, 0)"
    }, // 橄榄
    {
      backgroundColor: "rgba(0, 128, 128, 0.5)",
      borderColor: "rgb(0, 128, 128)"
    }, // 蓝绿
    {
      backgroundColor: "rgba(255, 128, 0, 0.5)",
      borderColor: "rgb(255, 128, 0)"
    }, // 深橙
    {
      backgroundColor: "rgba(255, 0, 128, 0.5)",
      borderColor: "rgb(255, 0, 128)"
    }, // 玫红
    {
      backgroundColor: "rgba(128, 0, 255, 0.5)",
      borderColor: "rgb(128, 0, 255)"
    }, // 紫罗兰
    {
      backgroundColor: "rgba(0, 255, 128, 0.5)",
      borderColor: "rgb(0, 255, 128)"
    } // 春绿
  ];

  // 为每种选号类型创建数据集
  if (
    statisticsData.value.purchaseAmountsByType &&
    statisticsData.value.winningAmountsByType
  ) {
    const purchaseDataByType = statisticsData.value
      .purchaseAmountsByType as Record<string, number[]>;
    const winningDataByType = statisticsData.value
      .winningAmountsByType as Record<string, number[]>;

    Object.keys(purchaseDataByType).forEach((type, index) => {
      // 将字符串键转换为数字，然后再转换为枚举
      const playTypeNumber = parseInt(type) as LotteryKL8PlayType;
      const playTypeName = playTypeNames[playTypeNumber] || `玩法${type}`;

      const purchaseData = purchaseDataByType[type];
      const winningData = winningDataByType[type];

      if (index < colors.length && purchaseData && winningData) {
        datasets.push({
          label: `购买金额-${playTypeName}`,
          data: purchaseData,
          backgroundColor: colors[index].backgroundColor,
          borderColor: colors[index].borderColor,
          borderWidth: 1
        });

        // 为下一个数据集使用不同的颜色
        const nextColorIndex = (index + 1) % colors.length;
        datasets.push({
          label: `中奖金额-${playTypeName}`,
          data: winningData,
          backgroundColor: colors[nextColorIndex].backgroundColor,
          borderColor: colors[nextColorIndex].borderColor,
          borderWidth: 1
        });
      }
    });
  } else {
    // 如果没有按类型分组的数据，使用简单数据
    datasets.push({
      label: "购买金额",
      data: statisticsData.value.purchaseAmounts,
      backgroundColor: "rgba(75, 192, 192, 0.5)",
      borderColor: "rgb(75, 192, 192)",
      borderWidth: 1
    });

    datasets.push({
      label: "中奖金额",
      data: statisticsData.value.winningAmounts,
      backgroundColor: "rgba(255, 99, 132, 0.5)",
      borderColor: "rgb(255, 99, 132)",
      borderWidth: 1
    });
  }

  chartInstance = new Chart(ctx, {
    type: "bar",
    data: {
      labels: statisticsData.value.terms.map(term => `${term}`),
      datasets: datasets
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        title: {
          display: true,
          text: "快乐8统计分析"
        },
        legend: {
          position: "top"
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: "金额"
          }
        },
        x: {
          title: {
            display: true,
            text: "期号"
          }
        }
      }
    }
  });
};

// 监听模态框打开状态
watch(statisticsModalVisible, async newVal => {
  if (newVal) {
    // 模态框打开时，等待DOM更新后加载数据
    await nextTick();
    setTimeout(async () => {
      await loadStatisticsData();
    }, 300);
  }
});

// 组件挂载时加载数据
onMounted(() => {
  loadData();
});
</script>

<style scoped>
.kl8-simulation-container {
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
  gap: 8px;
}

.table-container {
  margin-top: 20px;
}

.numbers-container {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.number-tag {
  margin: 0;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.dialog-footer {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.statistics-container {
  min-height: 400px;
}

.statistics-loading,
.statistics-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 400px;
}

.statistics-chart {
  position: relative;
  width: 100%;
  height: 400px;
}
</style>
