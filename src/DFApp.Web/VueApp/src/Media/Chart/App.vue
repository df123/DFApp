<template>
    <div>
        <canvas id="chart"></canvas>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { Chart } from 'chart.js/auto'
import { ChartDataDto } from '../Dto/dto'

const l:Function = abp.localization.getResource('DFApp') as Function;

onMounted(async () => {
    let dto: ChartDataDto = await dFApp.media.mediaInfo.getChartData() as ChartDataDto;
    if (dto && dto.datas && dto.labels && dto.labels.length > 0) {
        new Chart('chart', {
            type: 'bar',
            data: {
                labels: dto.labels,
                datasets: [{
                    label: '# of Votes',
                    data: dto.datas,
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