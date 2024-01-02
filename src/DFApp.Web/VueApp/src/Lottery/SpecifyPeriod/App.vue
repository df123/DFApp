<template>
    <div>
        <el-row>
            <div class="botton-area">
                <el-input v-model="purchasedPeriod" placeholder="购买期号" />
                <el-input v-model="winningPeriod" placeholder="开奖期号" />
                <el-button type="primary" @click="statistics">计算</el-button>
            </div>
        </el-row>
        <el-row>
            <div class="botton-area" style="margin-top: 10px;">
                <el-input v-model="period" placeholder="期号" />
                <el-input v-model="reds" placeholder="红球组" />
                <el-input v-model="blues" placeholder="蓝球组" />
                <el-button type="primary" @click="groupImport">复式导入</el-button>
            </div>
        </el-row>
        <el-row style="margin-top: 10px;">
            <el-table :data="statisticsWinItemDtos" style="width: 100%">
                <el-table-column prop="code" label="购买期号" />
                <el-table-column prop="buyLotteryString" label="购买号码" />
                <el-table-column prop="winCode" label="开奖期号" />
                <el-table-column prop="winLotteryString" label="开奖号码" />
                <el-table-column prop="winAmount" label="中奖金额" />
            </el-table>
        </el-row>
        <el-row style="margin-top: 10px;">
            <canvas id="chart"></canvas>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Chart } from 'chart.js/auto'
import { StatisticsWinDto } from '../Dto/StatisticsWinDto'
import { StatisticsWinItemDto } from '../Dto/StatisticsWinItemDto'
import { LotteryCombinationDto } from '../Dto/LotteryCombinationDto'
import { LotteryDto } from '../Dto/LotteryDto'


const purchasedPeriod = ref('');
const winningPeriod = ref('');
const statisticsWinItemDtos = ref([]);

const period = ref('');
const reds = ref('');
const blues = ref('');

const combinationDto = ref({});

async function statistics() {

    let winDtos: StatisticsWinDto[] = await dFApp.lottery.lottery.getStatisticsWin(purchasedPeriod.value, winningPeriod.value) as StatisticsWinDto[];
    statisticsWinItemDtos.value = (await dFApp.lottery.lottery.getStatisticsWinItem(purchasedPeriod.value, winningPeriod.value)).items as StatisticsWinItemDto[];

    if (winDtos && winDtos.length > 0) {
        let chartDataLabels: string[] = [];
        let chartDataBuy: number[] = [];
        let chartDataWin: number[] = [];
        winDtos.forEach(item => {
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
}

async function groupImport() {

    let combinationDto: LotteryCombinationDto = { period: 0, reds: [], blues: [] };

    if (!(isValidString(period.value) && isValidString(reds.value) && isValidString(blues.value))) {
        alert("输入有误");
        return;
    }
    combinationDto.period = parseInt(period.value);
    combinationDto.reds = reds.value.split(',');
    combinationDto.blues = blues.value.split(',');

    let dtos: LotteryDto[] = await dFApp.lottery.lottery.calculateCombination(combinationDto) as LotteryDto[];
    if (dtos != undefined && dtos != null && dtos.length > 0 && dtos[0].id > 0) {
        alert("添加成功");
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
</style>