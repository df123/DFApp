<template>
  <div class="expenditure-analysis-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">支出分析</span>
        </div>
      </template>

      <!-- 筛选条件区域 -->
      <el-row class="hidden-md-and-down filter-row" :gutter="16">
        <el-col :span="4">
          <el-select
            v-model="isBelongToSelf"
            class="filter-item"
            @change="isBelongToSelfChange"
          >
            <el-option
              v-for="item in isBelongToSelfItem"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-col>
        <el-col :span="4">
          <el-select
            v-model="compareTypeValue"
            class="filter-item"
            placeholder="对比模式"
            @change="compareTypeChange"
          >
            <el-option
              v-for="item in compareTypeItem"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-col>
        <el-col :span="4">
          <el-select
            v-model="chartTypeItemValue"
            class="filter-item"
            placeholder="图类型"
            @change="chartChange"
          >
            <el-option
              v-for="item in chartTypeItem"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-col>
        <el-col :span="4">
          <el-select
            v-model="numberTypeValue"
            class="filter-item"
            placeholder="数字模式"
            @change="numberChange"
          >
            <el-option
              v-for="item in numberTypeItem"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-col>
        <el-col :span="8">
          <el-date-picker
            v-model="dateRagen"
            class="filter-item"
            type="daterange"
            range-separator="To"
            start-placeholder="开始时间"
            end-placeholder="结束时间"
            @change="dateChange"
          />
        </el-col>
      </el-row>

      <!-- 移动端筛选条件 -->
      <div class="hidden-md-and-up">
        <el-row :gutter="5" style="margin-bottom: 5px">
          <el-col :span="12">
            <el-select v-model="isBelongToSelf" @change="isBelongToSelfChange">
              <el-option
                v-for="item in isBelongToSelfItem"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-col>
          <el-col :span="12">
            <el-select
              v-model="compareTypeValue"
              placeholder="对比模式"
              @change="compareTypeChange"
            >
              <el-option
                v-for="item in compareTypeItem"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-col>
        </el-row>
        <el-row :gutter="5" style="margin-bottom: 5px">
          <el-col :span="12">
            <el-select
              v-model="chartTypeItemValue"
              placeholder="图类型"
              @change="chartChange"
            >
              <el-option
                v-for="item in chartTypeItem"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-col>
          <el-col :span="12">
            <el-select
              v-model="numberTypeValue"
              placeholder="数字模式"
              @change="numberChange"
            >
              <el-option
                v-for="item in numberTypeItem"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-col>
        </el-row>
        <el-row>
          <el-date-picker
            v-model="dateRagen"
            type="daterange"
            range-separator="To"
            start-placeholder="开始时间"
            end-placeholder="结束时间"
            @change="dateChange"
          />
        </el-row>
      </div>

      <!-- 图表区域 -->
      <div class="chart-container">
        <canvas id="chart" />
      </div>

      <!-- 统计卡片 -->
      <el-row>
        <div class="statistic-card">
          <el-statistic :value="sumText">
            <template #title>
              <div style="display: inline-flex; align-items: center">合计</div>
            </template>
          </el-statistic>
          <div class="statistic-footer">
            <div class="footer-item">
              <span>{{ thanText }}</span>
              <span v-if="parseFloat(differenceText) > 0" class="green">
                {{ differenceText }}
                <el-icon>
                  <CaretTop />
                </el-icon>
              </span>
              <span v-else-if="parseFloat(differenceText) < 0" class="red">
                {{ differenceText }}
                <el-icon>
                  <CaretBottom />
                </el-icon>
              </span>
            </div>
          </div>
        </div>
      </el-row>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { Chart } from "chart.js/auto";
import { CaretTop, CaretBottom } from "@element-plus/icons-vue";
import { useBookkeepingExpenditureApi } from "@/api/bookkeeping";
import type { ChartJSDto } from "@/types/api";

const bookkeepingExpenditureApi = useBookkeepingExpenditureApi();

// 修复日期选择器类型问题 - 使用 Element Plus 期望的类型
const dateRagen = ref<[Date, Date] | null>(null);

const isBelongToSelf = ref("null");
const isBelongToSelfItem = ref([
  {
    label: "自用",
    value: "true"
  },
  {
    label: "非自用",
    value: "false"
  },
  {
    label: "全部",
    value: "null"
  }
]);

const compareTypeValue = ref(0);

const compareTypeItem = [
  {
    label: "禁用",
    value: 0
  },
  {
    label: "天",
    value: 1
  },
  {
    label: "月",
    value: 2
  },
  {
    label: "年",
    value: 3
  }
];

const chartTypeItemValue = ref("bar");

const chartTypeItem = [
  {
    value: "bar",
    label: "柱状图"
  },
  {
    value: "pie",
    label: "饼状图"
  }
];

const numberTypeValue = ref("0");

const numberTypeItem = [
  {
    label: "数值",
    value: "0"
  },
  {
    label: "百分数",
    value: "1"
  }
];

const chart = ref<Chart | undefined>(undefined);

const sumText = ref(0);
const thanText = ref("");
const differenceText = ref("");

async function runChartDraw() {
  await chartDraw(
    chartTypeItemValue.value,
    dateRagen.value,
    compareTypeValue.value,
    numberTypeValue.value,
    isBelongToSelf.value
  );
}

onMounted(async () => {
  dateRagen.value = getDefaultValue();
  await runChartDraw();
});

async function dateChange() {
  await runChartDraw();
}

async function chartChange() {
  await runChartDraw();
}

async function isBelongToSelfChange() {
  await runChartDraw();
}

async function compareTypeChange() {
  await runChartDraw();
  compareText();
}

async function numberChange() {
  await runChartDraw();
  compareText();
}

async function chartDraw(
  chartTy: string,
  dateRa: [Date, Date] | null,
  compareTyVa: number,
  numTyVa: string,
  isBelongToSe: string
) {
  if (!dateRa) return;

  let isBelongToSelfParam: boolean | null = null;

  if (isBelongToSe === "null") {
    isBelongToSelfParam = null;
  } else {
    isBelongToSelfParam = isBelongToSe === "true";
  }

  try {
    const dto: ChartJSDto = await bookkeepingExpenditureApi.getChartData({
      start: formatDate(dateRa[0]),
      end: formatDate(dateRa[1]),
      compareType: compareTyVa,
      numberType: numTyVa,
      isBelongToSelf: isBelongToSelfParam
    });

    if (chart.value !== undefined && chart.value !== null) {
      chart.value.destroy();
    }

    chart.value = new Chart("chart", {
      type: chartTy as any,
      data: dto
    });

    sumText.value = dto.total;
    compareText();
    differenceText.value = dto.differenceTotal.toString();
  } catch (error) {
    console.error("获取图表数据失败:", error);
  }
}

function compareText() {
  const tempCompareLable = compareTypeItem.find(
    x => x.value === compareTypeValue.value
  );
  if (compareTypeValue.value === 3) {
    thanText.value = "对比去" + tempCompareLable?.label;
  } else if (compareTypeValue.value === 1) {
    thanText.value = "对比昨" + tempCompareLable?.label;
  } else if (compareTypeValue.value === 2) {
    thanText.value = "对比上" + tempCompareLable?.label;
  } else {
    thanText.value = "";
  }
}

function getDefaultValue(): [Date, Date] {
  // 获取当前日期
  const currentDate = new Date();

  // 获取当月的第一天
  const firstDay = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth(),
    1
  );

  // 获取当月的最后一天
  const lastDay = new Date(
    currentDate.getFullYear(),
    currentDate.getMonth() + 1,
    0
  );

  return [firstDay, lastDay];
}

function formatDate(date: Date): string {
  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, "0");
  const day = date.getDate().toString().padStart(2, "0");

  return `${year}-${month}-${day}`;
}
</script>

<style scoped>
@media screen and (width <= 768px) {
  .hidden-md-and-down {
    display: none !important;
  }

  .hidden-md-and-up {
    display: block !important;
  }
}

/* 桌面端样式 */
@media screen and (width >= 769px) {
  .hidden-md-and-down {
    display: flex !important;
  }

  .hidden-md-and-up {
    display: none !important;
  }
}

.expenditure-analysis-container {
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

.chart-container {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 400px;
  margin: 20px 0;
}

button {
  font-weight: bold;
}

:global(h2#card-usage ~ .example .example-showcase) {
  background-color: var(--el-fill-color) !important;
}

.el-statistic {
  --el-statistic-content-font-size: 28px;
}

.statistic-card {
  height: 100%;
  padding: 20px;
  background-color: var(--el-bg-color-overlay);
  border-radius: 4px;
}

.statistic-footer {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  margin-top: 16px;
  font-size: 12px;
  color: var(--el-text-color-regular);
}

.statistic-footer .footer-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.statistic-footer .footer-item span:last-child {
  display: inline-flex;
  align-items: center;
  margin-left: 4px;
}

.green {
  color: var(--el-color-success);
}

.red {
  color: var(--el-color-error);
}

/* 筛选条件样式 */
.filter-row {
  margin-bottom: 20px;
}

.filter-item {
  width: 100%;
}

/* 响应式隐藏类 */
.hidden-md-and-down {
  display: block;
}

.hidden-md-and-up {
  display: none;
}
</style>
