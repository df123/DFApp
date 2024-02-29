<template>
    <div>
        <el-row>
            <div class="select-area">
                <el-select v-model="lotteryTypeValue" class="m-2" placeholder="彩票类型" @change="lotteryTypeChange">
                    <el-option v-for="item in lotteryTypeItems" :key="item.value" :label="item.label" :value="item.value" />
                </el-select>
                <el-input v-model="purchasedPeriod" placeholder="购买期号" @change="inputChange" />
                <el-input v-model="winningPeriod" placeholder="开奖期号" @change="inputChange" />
            </div>
        </el-row>
        <el-row class="chart-height">
            <canvas id="chart"></canvas>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { Chart } from 'chart.js/auto'
import { StatisticsWinDto } from '../Dto/StatisticsWinDto'
import { LotteryConstsDto } from '../Dto/LotteryConstsDto'

const l: Function = abp.localization.getResource('DFApp') as Function;

const lotteryTypeValue = ref('kl8');
const lotteryTypeItems = ref([]);
const purchasedPeriod = ref(undefined);
const winningPeriod = ref(undefined);


const chart = ref(undefined);

onMounted(async () => {

    let typeItem = await dFApp.lottery.lottery.getLotteryConst() as LotteryConstsDto[];
    typeItem.forEach((item) => {
        lotteryTypeItems.value.push({
            value: item.lotteryTypeEng,
            label: item.lotteryType
        });
    })

    await chartStatistics();

});

async function lotteryTypeChange() {
    await chartStatistics();
}

async function inputChange() {

    if (isValidString(purchasedPeriod.value) && (!isValidString(winningPeriod.value))) {
        winningPeriod.value = purchasedPeriod.value;
    }

    if (isValidString(winningPeriod.value) && (!isValidString(purchasedPeriod.value))) {
        purchasedPeriod.value = winningPeriod.value;
    }

    await chartStatistics();
}

async function chartStatistics() {
    let lotteryType = lotteryTypeItems.value.find(item => item.value === lotteryTypeValue.value);

    let dto: StatisticsWinDto[] = await dFApp.lottery.lottery.getStatisticsWin(purchasedPeriod.value, winningPeriod.value, lotteryType.label) as StatisticsWinDto;
    if (chart.value !== undefined && chart.value !== null) {
        chart.value.destroy();
    }
    if (dto && dto.length > 0) {
        let chartDataLabels: string[] = [];
        let chartDataBuy: number[] = [];
        let chartDataWin: number[] = [];
        dto.forEach(item => {
            chartDataLabels.push(item.code);
            chartDataBuy.push(item.buyAmount);
            chartDataWin.push(item.winAmount);
        })

        chart.value = new Chart('chart', {
            type: 'bar',
            data: {
                labels: chartDataLabels,
                datasets: [{
                    label: '# of Buy',
                    data: chartDataBuy,
                    borderWidth: 1
                },
                {
                    label: '# of Win',
                    data: chartDataWin,
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        })


    }
}


function isValidString(input: string | null | undefined): boolean {
    if (input !== null && input !== undefined && input.trim() !== '') {
        return true;
    } else {
        return false;
    }
}

</script>

<style scoped>
button {
    font-weight: bold;
}

.select-area {
    display: flex;
    align-items: center;
    width: 100%;
}

.chart-height {
    height: 70vh;
}
</style>