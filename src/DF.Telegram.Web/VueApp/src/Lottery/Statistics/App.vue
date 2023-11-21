<template>
    <div>
        <canvas id="chart"></canvas>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { Chart } from 'chart.js/auto'
import { StatisticsWinDto } from '../Dto/StatisticsWinDto'

const l: Function = abp.localization.getResource('Telegram') as Function;

onMounted(async () => {
    let dto: StatisticsWinDto[] = await dF.telegram.lottery.lottery.getStatisticsWin() as StatisticsWinDto;
    if (dto && dto.length > 0) {
        let chartDataLabels: string[] = [];
        let chartDataBuy: number[] = [];
        let chartDataWin: number[] = [];
        dto.forEach(item => {
            chartDataLabels.push(item.code);
            chartDataBuy.push(item.buyAmount * 50000);
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