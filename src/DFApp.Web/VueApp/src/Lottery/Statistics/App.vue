<template>
    <div>
        <el-row>
            <div class="botton-area">
                <el-select v-model="lotteryTypeValue" class="m-2" placeholder="彩票类型">
                    <el-option v-for="item in lotteryTypeItems" :key="item.value" :label="item.label" :value="item.value" />
                </el-select>
            </div>
        </el-row>
        <el-row>
            <canvas id="chart"></canvas>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { onMounted,ref } from 'vue'
import { Chart } from 'chart.js/auto'
import { StatisticsWinDto } from '../Dto/StatisticsWinDto'
import { LotteryConstsDto } from '../Dto/LotteryConstsDto'

const l: Function = abp.localization.getResource('DFApp') as Function;

const lotteryTypeValue = ref('kl8');
const lotteryTypeItems = ref([]);

onMounted(async () => {

    let typeItem = await dFApp.lottery.lottery.getLotteryConst() as LotteryConstsDto[];
    typeItem.forEach((item) => {
        lotteryTypeItems.value.push({
            value: item.lotteryTypeEng,
            label: item.lotteryType
        });
    })

    let lotteryType = lotteryTypeItems.value.find(item => item.value === lotteryTypeValue.value);

    let dto: StatisticsWinDto[] = await dFApp.lottery.lottery.getStatisticsWin(undefined,undefined,lotteryType.label) as StatisticsWinDto;
    if (dto && dto.length > 0) {
        let chartDataLabels: string[] = [];
        let chartDataBuy: number[] = [];
        let chartDataWin: number[] = [];
        dto.forEach(item => {
            chartDataLabels.push(item.code);
            chartDataBuy.push(item.buyAmount);
            chartDataWin.push(item.winAmount);
        })
        console.log(chartDataBuy)
        new Chart('chart', {
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
});
</script>

<style scoped>
button {
    font-weight: bold;
}
</style>