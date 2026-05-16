<template>
  <div class="expenditure-chart-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">支出图表 - 月度统计</span>
          <div class="year-selector">
            <el-select
              v-model="selectedYear"
              placeholder="选择年份"
              class="year-select"
              @change="handleYearChange"
            >
              <el-option
                v-for="year in yearOptions"
                :key="year"
                :label="`${year}年`"
                :value="year"
              />
            </el-select>
          </div>
        </div>
      </template>

      <!-- 图表区域 -->
      <div class="chart-content">
        <div class="chart-wrapper">
          <canvas id="monthly-chart" ref="chartCanvas" />
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from "vue";
import Chart from "chart.js/auto";
import { bookkeepingExpenditureApi } from "@/api/bookkeeping";
import type { MonthlyExpenditureDto } from "@/types/api";

// 响应式数据
const selectedYear = ref(new Date().getFullYear());
const chartCanvas = ref<HTMLCanvasElement | null>(null);
let chartInstance: Chart | null = null;

// 年份选项（从当前年份到2024年）
const yearOptions = ref<number[]>([]);
for (let year = new Date().getFullYear(); year >= 2024; year--) {
  yearOptions.value.push(year);
}

// 初始化图表
const initChart = () => {
  if (!chartCanvas.value) return;

  const ctx = chartCanvas.value.getContext("2d");
  if (!ctx) return;

  chartInstance = new Chart(ctx, {
    type: "bar",
    data: {
      labels: [],
      datasets: []
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: "金额 (元)"
          }
        },
        x: {
          title: {
            display: true,
            text: "月份"
          }
        }
      },
      plugins: {
        legend: {
          position: "top",
          labels: {
            usePointStyle: true
          }
        },
        tooltip: {
          mode: "point",
          intersect: true,
          callbacks: {
            title: function (context) {
              return "月份: " + context[0].label;
            },
            label: function (context) {
              return (
                context.dataset.label +
                ": " +
                context.parsed.y.toFixed(2) +
                " 元"
              );
            }
          }
        }
      }
    }
  });

  // 加载初始数据
  loadChartData();
};

// 加载图表数据
const loadChartData = async () => {
  try {
    const result = await bookkeepingExpenditureApi.getMonthlyExpenditure(
      selectedYear.value
    );
    updateChart(result);
  } catch (error) {
    console.error("加载图表数据失败:", error);
  }
};

// 更新图表数据
const updateChart = (data: MonthlyExpenditureDto) => {
  if (!chartInstance) return;

  chartInstance.data = {
    labels: data.labels || [],
    datasets: [
      {
        label: "总支出",
        data: data.totalData || [],
        backgroundColor: "rgba(54, 162, 235, 0.5)",
        borderColor: "rgba(54, 162, 235, 1)",
        borderWidth: 1
      },
      {
        label: "个人支出",
        data: data.selfData || [],
        backgroundColor: "rgba(75, 192, 192, 0.5)",
        borderColor: "rgba(75, 192, 192, 1)",
        borderWidth: 1
      },
      {
        label: "非个人支出",
        data: data.nonSelfData || [],
        backgroundColor: "rgba(255, 99, 132, 0.5)",
        borderColor: "rgba(255, 99, 132, 1)",
        borderWidth: 1
      },
      {
        label: "总支出平均值",
        data: Array(12).fill(data.totalAverage || 0),
        type: "line",
        borderColor: "rgba(54, 162, 235, 1)",
        borderWidth: 2,
        borderDash: [5, 5],
        fill: false
      },
      {
        label: "个人支出平均值",
        data: Array(12).fill(data.selfAverage || 0),
        type: "line",
        borderColor: "rgba(75, 192, 192, 1)",
        borderWidth: 2,
        borderDash: [5, 5],
        fill: false
      },
      {
        label: "非个人支出平均值",
        data: Array(12).fill(data.nonSelfAverage || 0),
        type: "line",
        borderColor: "rgba(255, 99, 132, 1)",
        borderWidth: 2,
        borderDash: [5, 5],
        fill: false
      }
    ]
  };

  chartInstance.update();
};

// 年份变化处理
const handleYearChange = () => {
  loadChartData();
};

// 生命周期
onMounted(() => {
  nextTick(() => {
    initChart();
  });
});

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.destroy();
    chartInstance = null;
  }
});
</script>

<style scoped>
.expenditure-chart-container {
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

.year-selector {
  display: flex;
  align-items: center;
}

.year-select {
  width: 150px;
}

.chart-content {
  margin-top: 20px;
}

.chart-wrapper {
  position: relative;
  width: 100%;
  height: 400px;
}

#monthly-chart {
  width: 100% !important;
  height: 100% !important;
}
</style>
