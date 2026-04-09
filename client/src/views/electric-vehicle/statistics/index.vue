<template>
  <div class="statistics-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">统计分析</span>
        </div>
      </template>

      <el-row :gutter="20" style="margin-bottom: 20px">
        <el-col :span="12">
          <el-radio-group v-model="timeRange" @change="handleTimeRangeChange">
            <el-radio-button label="all">全部时间</el-radio-button>
            <el-radio-button label="30">最近30天</el-radio-button>
            <el-radio-button label="90">最近90天</el-radio-button>
            <el-radio-button label="365">最近一年</el-radio-button>
            <el-radio-button label="custom">自定义</el-radio-button>
          </el-radio-group>
        </el-col>
        <el-col :span="12">
          <el-date-picker
            v-if="timeRange === 'custom'"
            v-model="customDateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
            @change="handleCustomDateChange"
          />
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="6">
          <el-card class="stat-card">
            <el-statistic
              title="电车总花费"
              :value="data?.electricVehicleTotalCost?.toFixed(2)"
              prefix="￥"
            />
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card class="stat-card">
            <el-statistic
              title="电车行驶里程"
              :value="data?.electricVehicleMileage?.toFixed(0)"
              suffix="km"
            />
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card class="stat-card">
            <el-statistic
              title="电车每公里成本"
              :value="data?.electricVehicleCostPerKm?.toFixed(2)"
              prefix="￥"
            />
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card class="stat-card">
            <el-statistic
              title="节省金额"
              :value="data?.savings?.toFixed(2)"
              prefix="￥"
              :value-style="{ color: '#67C23A' }"
            />
          </el-card>
        </el-col>
      </el-row>

      <el-divider>成本对比图表</el-divider>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span class="card-title">油电成本对比</span>
            </template>
            <div ref="comparisonChartRef" style="width: 100%; height: 400px" />
          </el-card>
        </el-col>
        <el-col :span="12">
          <el-card>
            <template #header>
              <span class="card-title">成本构成分析</span>
            </template>
            <div
              ref="costBreakdownChartRef"
              style="width: 100%; height: 400px"
            />
          </el-card>
        </el-col>
      </el-row>

      <el-divider>电费油费对比</el-divider>

      <el-row :gutter="20">
        <el-col :span="24">
          <el-card>
            <template #header>
              <span class="card-title">电费油费对比</span>
            </template>
            <div
              ref="fuelCostComparisonChartRef"
              style="width: 100%; height: 400px"
            />
          </el-card>
        </el-col>
      </el-row>

      <el-divider>油电对比分析</el-divider>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span class="card-title">电车成本明细</span>
            </template>
            <el-descriptions :column="2" border>
              <el-descriptions-item label="充电费用">
                ￥{{ data?.electricChargingCost?.toFixed(2) }}
              </el-descriptions-item>
              <el-descriptions-item label="其他费用">
                ￥{{ data?.electricOtherCost?.toFixed(2) }}
              </el-descriptions-item>
              <el-descriptions-item label="每公里成本">
                ￥{{ data?.electricVehicleCostPerKm?.toFixed(2) }}
              </el-descriptions-item>
            </el-descriptions>
          </el-card>
        </el-col>
        <el-col :span="12">
          <el-card>
            <template #header>
              <span class="card-title">油车成本（相同里程）</span>
            </template>
            <el-descriptions :column="2" border>
              <el-descriptions-item label="油费">
                ￥{{ data?.oilVehicleFuelCost?.toFixed(2) }}
              </el-descriptions-item>
              <el-descriptions-item label="每公里成本">
                ￥{{ data?.oilVehicleCostPerKm?.toFixed(3) }}
              </el-descriptions-item>
              <el-descriptions-item label="总花费">
                ￥{{ data?.oilVehicleTotalCost?.toFixed(2) }}
              </el-descriptions-item>
            </el-descriptions>
          </el-card>
        </el-col>
      </el-row>

      <el-divider>油车参数</el-divider>

      <el-descriptions :column="3" border>
        <el-descriptions-item label="省份">
          {{ data?.province }}
        </el-descriptions-item>
        <el-descriptions-item label="汽油标号">
          {{ data?.gasolineGrade }}号
        </el-descriptions-item>
        <el-descriptions-item label="百公里油耗">
          {{ data?.fuelConsumption }}升
        </el-descriptions-item>
        <el-descriptions-item label="最新油价（参考）">
          ￥{{ data?.currentGasolinePrice?.toFixed(2) }}/升
        </el-descriptions-item>
        <el-descriptions-item label="计算说明" :span="3">
          油车费用根据充电日期对应的实际油价计算，而非使用单一最新油价
        </el-descriptions-item>
      </el-descriptions>

      <el-result
        icon="success"
        :title="`节省 ￥${data?.savings?.toFixed(2)}`"
        :sub-title="`节省比例：${data?.savingsPercentage?.toFixed(1)}%`"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, nextTick } from "vue";
import { electricVehicleCostApi } from "@/api/electric-vehicle";
import type { OilCostComparisonDto } from "@/types/api";
import * as echarts from "echarts";

const data = ref<OilCostComparisonDto | null>(null);
const loading = ref(false);
const timeRange = ref("all");
const customDateRange = ref<[string, string] | null>(null);

const comparisonChartRef = ref<HTMLElement | null>(null);
const costBreakdownChartRef = ref<HTMLElement | null>(null);
const fuelCostComparisonChartRef = ref<HTMLElement | null>(null);
let comparisonChartInstance: echarts.ECharts | null = null;
let costBreakdownChartInstance: echarts.ECharts | null = null;
let fuelCostComparisonChartInstance: echarts.ECharts | null = null;

const today = new Date();
const thirtyDaysAgo = new Date(today);
thirtyDaysAgo.setDate(today.getDate() - 30);

const getDateRange = () => {
  const now = new Date();
  let startDate: Date;

  if (timeRange.value === "all") {
    startDate = new Date("2000-01-01");
  } else if (timeRange.value === "30") {
    startDate = new Date(now);
    startDate.setDate(now.getDate() - 30);
  } else if (timeRange.value === "90") {
    startDate = new Date(now);
    startDate.setDate(now.getDate() - 90);
  } else if (timeRange.value === "365") {
    startDate = new Date(now);
    startDate.setDate(now.getDate() - 365);
  } else if (timeRange.value === "custom" && customDateRange.value) {
    return {
      startDate: customDateRange.value[0],
      endDate: customDateRange.value[1]
    };
  } else {
    startDate = thirtyDaysAgo;
  }

  return {
    startDate: startDate.toISOString().split("T")[0],
    endDate: now.toISOString().split("T")[0]
  };
};

const handleTimeRangeChange = () => {
  loadData();
};

const handleCustomDateChange = () => {
  if (customDateRange.value) {
    loadData();
  }
};

const loadData = async () => {
  loading.value = true;
  try {
    const { startDate, endDate } = getDateRange();
    data.value = await electricVehicleCostApi.getOilCostComparison({
      startDate,
      endDate
    });
    await nextTick();
    updateCharts();
  } catch (error) {
    console.error("加载对比数据失败:", error);
  } finally {
    loading.value = false;
  }
};

const updateCharts = () => {
  if (!data.value) return;

  if (comparisonChartRef.value && !comparisonChartInstance) {
    comparisonChartInstance = echarts.init(comparisonChartRef.value);
  }
  if (costBreakdownChartRef.value && !costBreakdownChartInstance) {
    costBreakdownChartInstance = echarts.init(costBreakdownChartRef.value);
  }
  if (fuelCostComparisonChartRef.value && !fuelCostComparisonChartInstance) {
    fuelCostComparisonChartInstance = echarts.init(
      fuelCostComparisonChartRef.value
    );
  }

  const comparisonOption = {
    tooltip: {
      trigger: "axis",
      axisPointer: {
        type: "shadow"
      }
    },
    legend: {
      data: ["电车总成本", "油车总成本", "电车每公里成本", "油车每公里成本"]
    },
    grid: {
      left: "3%",
      right: "4%",
      bottom: "3%",
      containLabel: true
    },
    xAxis: {
      type: "category",
      data: ["总成本", "每公里成本"]
    },
    yAxis: [
      {
        type: "value",
        name: "总成本（元）",
        position: "left",
        axisLabel: {
          formatter: "{value} 元"
        }
      },
      {
        type: "value",
        name: "每公里成本（元）",
        position: "right",
        axisLabel: {
          formatter: "{value} 元"
        }
      }
    ],
    series: [
      {
        name: "电车总成本",
        type: "bar",
        data: [data.value.electricVehicleTotalCost || 0, null],
        itemStyle: {
          color: "#67C23A"
        },
        yAxisIndex: 0
      },
      {
        name: "油车总成本",
        type: "bar",
        data: [data.value.oilVehicleTotalCost || 0, null],
        itemStyle: {
          color: "#F56C6C"
        },
        yAxisIndex: 0
      },
      {
        name: "电车每公里成本",
        type: "bar",
        data: [null, (data.value.electricVehicleCostPerKm || 0).toFixed(2)],
        itemStyle: {
          color: "#95D475"
        },
        yAxisIndex: 1
      },
      {
        name: "油车每公里成本",
        type: "bar",
        data: [null, (data.value.oilVehicleCostPerKm || 0).toFixed(2)],
        itemStyle: {
          color: "#F89898"
        },
        yAxisIndex: 1
      }
    ]
  };

  const breakdownOption = {
    tooltip: {
      trigger: "item",
      formatter: "{a} <br/>{b}: {c} 元"
    },
    legend: {
      orient: "vertical",
      left: "left"
    },
    series: [
      {
        name: "电车成本构成",
        type: "pie",
        radius: ["40%", "70%"],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: "#fff",
          borderWidth: 2
        },
        label: {
          show: true,
          formatter: "{b}: {c}元 ({d}%)"
        },
        labelLine: {
          show: true
        },
        data: [
          {
            value: data.value.electricChargingCost || 0,
            name: "充电费用",
            itemStyle: { color: "#91CC75" }
          },
          {
            value: data.value.electricOtherCost || 0,
            name: "其他费用",
            itemStyle: { color: "#FAC858" }
          }
        ]
      }
    ]
  };

  const fuelCostComparisonOption = {
    tooltip: {
      trigger: "axis",
      axisPointer: {
        type: "shadow"
      }
    },
    legend: {
      data: ["电费总费用", "油费总费用", "电费每公里成本", "油费每公里成本"]
    },
    grid: {
      left: "3%",
      right: "4%",
      bottom: "3%",
      containLabel: true
    },
    xAxis: {
      type: "category",
      data: ["总费用", "每公里成本"]
    },
    yAxis: [
      {
        type: "value",
        name: "总费用（元）",
        position: "left",
        axisLabel: {
          formatter: "{value} 元"
        }
      },
      {
        type: "value",
        name: "每公里成本（元）",
        position: "right",
        axisLabel: {
          formatter: "{value} 元"
        }
      }
    ],
    series: [
      {
        name: "电费总费用",
        type: "bar",
        data: [data.value.electricChargingCost || 0, null],
        itemStyle: {
          color: "#91CC75"
        },
        label: {
          show: true,
          position: "top",
          formatter: (params: any) => {
            return params.value ? `${params.value.toFixed(2)} 元` : "";
          }
        },
        yAxisIndex: 0
      },
      {
        name: "油费总费用",
        type: "bar",
        data: [data.value.oilVehicleFuelCost || 0, null],
        itemStyle: {
          color: "#F56C6C"
        },
        label: {
          show: true,
          position: "top",
          formatter: (params: any) => {
            return params.value ? `${params.value.toFixed(2)} 元` : "";
          }
        },
        yAxisIndex: 0
      },
      {
        name: "电费每公里成本",
        type: "bar",
        data: [
          null,
          (
            (data.value.electricChargingCost || 0) /
            (data.value.electricVehicleMileage || 1)
          ).toFixed(2)
        ],
        itemStyle: {
          color: "#B3E19D"
        },
        label: {
          show: true,
          position: "top",
          formatter: (params: any) => {
            return params.value ? `${params.value} 元` : "";
          }
        },
        yAxisIndex: 1
      },
      {
        name: "油费每公里成本",
        type: "bar",
        data: [
          null,
          (
            (data.value.oilVehicleFuelCost || 0) /
            (data.value.electricVehicleMileage || 1)
          ).toFixed(2)
        ],
        itemStyle: {
          color: "#F89898"
        },
        label: {
          show: true,
          position: "top",
          formatter: (params: any) => {
            return params.value ? `${params.value} 元` : "";
          }
        },
        yAxisIndex: 1
      }
    ]
  };

  if (comparisonChartInstance) {
    comparisonChartInstance.setOption(comparisonOption, true);
  }
  if (costBreakdownChartInstance) {
    costBreakdownChartInstance.setOption(breakdownOption, true);
  }
  if (fuelCostComparisonChartInstance) {
    fuelCostComparisonChartInstance.setOption(fuelCostComparisonOption, true);
  }
};

watch(
  () => data.value,
  () => {
    if (data.value) {
      updateCharts();
    }
  }
);

const handleResize = () => {
  comparisonChartInstance?.resize();
  costBreakdownChartInstance?.resize();
  fuelCostComparisonChartInstance?.resize();
};

onMounted(() => {
  window.addEventListener("resize", handleResize);
  loadData();
});

onUnmounted(() => {
  window.removeEventListener("resize", handleResize);
  comparisonChartInstance?.dispose();
  costBreakdownChartInstance?.dispose();
  fuelCostComparisonChartInstance?.dispose();
});
</script>

<style scoped>
.statistics-container {
  padding: 20px;
}

.stat-card {
  text-align: center;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
}
</style>
