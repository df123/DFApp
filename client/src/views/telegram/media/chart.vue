<template>
  <div class="media-chart-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">媒体图表</span>
        </div>
      </template>

      <!-- 图表区域 -->
      <div class="chart-content">
        <div class="chart-wrapper">
          <canvas id="media-chart" ref="chartCanvas" />
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from "vue";
import { mediaInfoApi } from "@/api/mediaInfo";
import Chart from "chart.js/auto";
import type { ChartDataDto } from "@/types/business";

// 图表相关
const chartCanvas = ref<HTMLCanvasElement | null>(null);
let chartInstance: Chart | null = null;

// 初始化图表
const initChart = () => {
  if (!chartCanvas.value) return;

  const ctx = chartCanvas.value.getContext("2d");
  if (!ctx) return;

  chartInstance = new Chart(ctx, {
    type: "bar",
    data: {
      labels: [],
      datasets: [
        {
          label: "媒体数量",
          data: [],
          backgroundColor: "rgba(54, 162, 235, 0.5)",
          borderColor: "rgba(54, 162, 235, 1)",
          borderWidth: 1
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: "数量"
          }
        },
        x: {
          title: {
            display: true,
            text: "类别"
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
          mode: "index",
          intersect: false
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
    const data = await mediaInfoApi.getChartData();
    updateChart(data);
  } catch (error) {
    console.error("加载图表数据失败:", error);
  }
};

// 更新图表数据
const updateChart = (data: ChartDataDto) => {
  if (!chartInstance) return;

  chartInstance.data = {
    labels: data.labels || [],
    datasets: [
      {
        label: "媒体数量",
        data: data.datas || [],
        backgroundColor: "rgba(54, 162, 235, 0.5)",
        borderColor: "rgba(54, 162, 235, 1)",
        borderWidth: 1
      }
    ]
  };

  chartInstance.update();
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
.media-chart-container {
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

.chart-content {
  margin-top: 20px;
}

.chart-wrapper {
  position: relative;
  width: 100%;
  height: 400px;
}

#media-chart {
  width: 100% !important;
  height: 100% !important;
}
</style>
