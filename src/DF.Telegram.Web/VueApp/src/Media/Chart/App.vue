<template>
    <div>
        <el-row class="mb-4">
            <el-button>Default</el-button>
            <el-button type="primary">Primary</el-button>
            <el-button type="success">Success</el-button>
            <el-button type="info">Info</el-button>
            <el-button type="warning">Warning</el-button>
            <el-button type="danger">Danger</el-button>
        </el-row>
        <canvas id="chart"></canvas>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { Chart } from 'chart.js/auto'
import { ChartDataDto } from '../Dto/dto'
// const Chart = require('chart.js/auto');

const l:Function = abp.localization.getResource('Telegram') as Function;

onMounted(async () => {
    console.log(l('Media:ExternalLinkTitle:CopySuccessMesage'));
    let dto: ChartDataDto = await dF.telegram.media.mediaInfo.getChartData() as ChartDataDto;
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