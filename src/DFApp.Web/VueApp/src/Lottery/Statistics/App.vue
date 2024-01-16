<template>
    <div>
        <el-row>
            <div class="botton-area">
                <el-select v-model="lotteryTypeValue" class="m-2" placeholder="彩票类型" @change="lotteryTypeChange">
                    <el-option v-for="item in lotteryTypeItems" :key="item.value" :label="item.label" :value="item.value" />
                </el-select>
            </div>
        </el-row>
        <el-row class="chart-height" >
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

async function lotteryTypeChange(e) {
    console.log(e);
    await chartStatistics();
}

async function chartStatistics() {
    let lotteryType = lotteryTypeItems.value.find(item => item.value === lotteryTypeValue.value);

    let dto: StatisticsWinDto[] = await dFApp.lottery.lottery.getStatisticsWin(undefined, undefined, lotteryType.label) as StatisticsWinDto;
    if (dto && dto.length > 0) {
        let chartDataLabels: string[] = [];
        let chartDataBuy: number[] = [];
        let chartDataWin: number[] = [];
        dto.forEach(item => {
            chartDataLabels.push(item.code);
            chartDataBuy.push(item.buyAmount);
            chartDataWin.push(item.winAmount);
        })

        if(chart.value !== undefined && chart.value !== null){
            chart.value.destroy();
        }

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

</script>

<style scoped>
button {
    font-weight: bold;
}

.chart-height {
    height: 70vh;
}

</style>