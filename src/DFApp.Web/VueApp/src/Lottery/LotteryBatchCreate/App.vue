<template>
    <div>
        <el-row>
            <div class="botton-area select-area">
                <el-select v-model="lotteryTypeValue" class="m-2" placeholder="彩票类型">
                    <el-option v-for="item in lotteryTypeItems" :key="item.value" :label="item.label" :value="item.value" />
                </el-select>
            </div>
        </el-row>
        <el-row>
            <div v-if="lotteryTypeValue == 'ssq'" class="margin-left-12" style="margin-top: 10px;">
                <el-input class="m-1" v-model="period" placeholder="期号" />
                <el-input class="m-1" v-model="reds" placeholder="红球" />
                <el-input class="m-1" v-model="blues" placeholder="蓝球" />
                <el-button class="m-1" type="primary" @click="groupImport">导入</el-button>
            </div>
            <div v-if="lotteryTypeValue != 'ssq'" class="margin-left-12">
                <el-input class="m-1" v-model="period" placeholder="期号" />
                <el-input class="m-1" v-model="reds" :rows="5" type="textarea" placeholder="红球" />
                <el-button class="m-1" type="primary" @click="groupImport">导入</el-button>
            </div>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { LotteryDto } from '../Dto/LotteryDto'
import { LotteryConstsDto } from '../Dto/LotteryConstsDto'
import { CreateUpdateLotteryDto } from '../Dto/CreateUpdateLotteryDto'
import { LotteryCombinationDto } from '../Dto/LotteryCombinationDto'

const period = ref('');
const reds = ref('');
const blues = ref('');

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

});

async function groupImport() {

    if (!(isValidString(period.value) && isValidString(reds.value) && (isValidString(blues.value) || lotteryTypeValue.value !== "ssq"))) {
        alert("输入有误");
        return;
    }

    let lotteryType = lotteryTypeItems.value.find(item => item.value === lotteryTypeValue.value);

    let createDtos: CreateUpdateLotteryDto[] = [];
    // 支持空格、逗号和换行符作为分隔符
    let tempStr = reds.value.split(/[\s,]+/).filter(item => item.trim() !== '');
    tempStr.forEach(element => {
        createDtos.push({
            indexNo: parseInt(period.value),
            number: element,
            colorType: '0',
            lotteryType: lotteryType.label,
            groupId: 0
        });
    });

    if (lotteryTypeValue.value === "ssq") {
        // 蓝球也支持空格、逗号和换行符作为分隔符
        let blueStr = blues.value.split(/[\s,]+/).filter(item => item.trim() !== '');
        blueStr.forEach(element => {
            createDtos.push({
                indexNo: parseInt(period.value),
                number: element,
                colorType: '1',
                lotteryType: lotteryType.label,
                groupId: 0
            });
        });

        if(tempStr.length > 6 || blueStr.length > 1){
            await groupImportF();
            return;
        }
    }

    let dtos: LotteryDto = await dFApp.lottery.lottery.createLotteryBatch(createDtos) as LotteryDto;
    if (dtos != undefined && dtos != null && dtos.id > 0) {
        ElMessage({
            message: '添加成功.',
            type: 'success',
            onClose: () => { location.reload(); },
            duration: 1000
        })
    }
}

async function groupImportF() {

    let combinationDto: LotteryCombinationDto = { period: 0, reds: [], blues: [] };

    if (!(isValidString(period.value) && isValidString(reds.value) && isValidString(blues.value))) {
        alert("输入有误");
        return;
    }
    combinationDto.period = parseInt(period.value);
    // 支持空格、逗号和换行符作为分隔符
    combinationDto.reds = reds.value.split(/[\s,]+/).filter(item => item.trim() !== '');
    combinationDto.blues = blues.value.split(/[\s,]+/).filter(item => item.trim() !== '');

    console.log(combinationDto);

    let dtos: LotteryDto[] = await dFApp.lottery.lottery.calculateCombination(combinationDto) as LotteryDto[];
    if (dtos != undefined && dtos != null && dtos.length > 0 && dtos[0].id > 0) {
        ElMessage({
            message: '添加成功.',
            type: 'success',
            onClose: () => { location.reload(); },
            duration: 1000
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
.botton-area {
    display: flex;
    font-weight: bold;
    margin-left: unset;
}

.select-area {
    width: 100%;
}

.margin-left-12 {
    margin-left: 0.375rem;
    margin-right: 1.1rem;
}
</style>
