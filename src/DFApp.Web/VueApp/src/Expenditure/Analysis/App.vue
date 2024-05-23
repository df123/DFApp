<template>
    <el-row>
        <el-col :span="2">
            <el-checkbox v-model="compareEnable" label="开启对比" @change="compareEnableChange" />
        </el-col>
        <el-col :span="2">
            <el-select v-model="isBelongToSelf" class="m-2" placeholder="自用" @change="isBelongToSelfChange">
                <el-option v-for="item in isBelongToSelfItem" :label="item.label" :value="item.value" />
            </el-select>
        </el-col>
        <el-col :span="3" v-if="compareEnable">
            <el-select v-model="compareTypeValue" class="m-2" placeholder="对比模式" @change="compareTypeChange">
                <el-option v-for="item in compareTypeItem" :key="item.value" :label="item.label" :value="item.value" />
            </el-select>
        </el-col>
        <el-col :span="3">
            <el-select v-model="chartTypeItemValue" class="m-2" placeholder="图类型" @change="chartChange">
                <el-option v-for="item in chartTypeItem" :key="item.value" :label="item.label" :value="item.value" />
            </el-select>
        </el-col>
        <el-col :span="3">
            <el-select v-model="numberTypeValue" class="m-2" placeholder="数字模式" @change="numberChange">
                <el-option v-for="item in numberTypeItem" :key="item.value" :label="item.label" :value="item.value" />
            </el-select>
        </el-col>
        <el-col :span="6">
            <el-date-picker class="m-2" v-model="dateRagen" type="daterange" range-separator="To" start-placeholder="开始时间"
                end-placeholder="结束时间" @change="dateChange" />
        </el-col>
    </el-row>
    <div>
        <canvas id="chart"></canvas>
    </div>
    <el-row>
        <div class="statistic-card">
            <el-statistic :value="sumText">
                <template #title>
                    <div style="display: inline-flex; align-items: center">
                        合计
                    </div>
                </template>
            </el-statistic>
            <div class="statistic-footer">
                <div class="footer-item">
                    <span>{{ thanText }}</span>
                    <span class="green">
                        {{ differenceText }}
                        <el-icon>
                            <CaretTop />
                        </el-icon>
                    </span>
                </div>
            </div>
        </div>
    </el-row>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Chart } from 'chart.js/auto'
import { ChartJSDto } from '../Dto'
import { CaretTop } from '@element-plus/icons-vue'

const l: Function = abp.localization.getResource('DFApp') as Function;

const dateRagen = ref([]);

const compareEnable = ref(false);

const isBelongToSelf = ref(true);
const isBelongToSelfItem = ref([
    {
        label:'自用',
        value:true
    },
    {
        label:'非自用',
        value:false
    },
    {
        label:'全部',
        value:null
    }
]);

const compareTypeValue = ref(1)

const compareTypeItem = [
    {
        label: '天',
        value: 0,
    },
    {
        label: '月',
        value: 1,
    },
    {
        label: '年',
        value: 2,
    }
]

const chartTypeItemValue = ref('bar')

const chartTypeItem = [
    {
        value: 'bar',
        label: '柱状图',
    },
    {
        value: 'pie',
        label: '饼状图',
    }
]

const numberTypeValue = ref('0')

const numberTypeItem = [
    {
        label: '数值',
        value: '0',
    },
    {
        label: '百分数',
        value: '1',
    }
]

const chart = ref(undefined);

const sumText = ref(0);
const thanText = ref('');
const differenceText = ref('');

async function runChartDraw(){
    await chartDraw(chartTypeItemValue.value, dateRagen.value, compareEnable.value, compareTypeValue.value, numberTypeValue.value,isBelongToSelf.value);
}

onMounted(async () => {
    dateRagen.value = getDefaultValue();
    await runChartDraw();
});

async function dateChange(e) {
    await runChartDraw();
}

async function chartChange(e: any) {
    await runChartDraw();
}

async function isBelongToSelfChange(e){
    await runChartDraw();
}

async function compareEnableChange(e) {
    await runChartDraw();
}

async function compareTypeChange(e) {
    await runChartDraw();
    compareText();
}

async function numberChange(e) {
    await runChartDraw();
    compareText();
}

async function chartDraw(chartTy, dateRa, compareEn, compareTyVa, numTyVa,isBelongToSe) {
    let dto: ChartJSDto = await dFApp.bookkeeping.expenditure.bookkeepingExpenditure
    .getChartJSDto(formatDate(dateRa[0]), formatDate(dateRa[1]), compareEn, compareTyVa, numTyVa,isBelongToSe) as ChartJSDto;

    if (chart.value !== undefined && chart.value !== null) {
        chart.value.destroy();
    }

    chart.value = new Chart('chart', {
        type: chartTy,
        data: dto,
    })


    sumText.value = dto.total;
    compareText();
    differenceText.value = dto.differenceTotal.toString();
}

function compareText() {
    let tempCompareLable = compareTypeItem.find(x => x.value == compareTypeValue.value);
    if (compareTypeValue.value === 0) {
        thanText.value = "对比昨" + tempCompareLable.label;
    }
    else if (compareTypeValue.value === 1) {
        thanText.value = "对比上" + tempCompareLable.label;
    }
    else if (compareTypeValue.value === 2) {
        thanText.value = "对比去" + tempCompareLable.label;
    }
}

function getDefaultValue() {
    // 获取当前日期
    let currentDate = new Date();

    // 获取当月的第一天
    let firstDay = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);

    // 获取当月的最后一天
    let lastDay = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);

    return [firstDay, lastDay];
}

function formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');

    return `${year}-${month}-${day}`;
}

</script>

<style scoped>
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
    border-radius: 4px;
    background-color: var(--el-bg-color-overlay);
}

.statistic-footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    font-size: 12px;
    color: var(--el-text-color-regular);
    margin-top: 16px;
}

.statistic-footer .footer-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
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
</style>