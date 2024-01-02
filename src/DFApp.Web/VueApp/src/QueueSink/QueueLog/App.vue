<template>
    <div>
        <el-row>
            <el-input v-model="searchInput" class="w-50 m-2" placeholder="搜索日志" :prefix-icon="Search" @input="searchLog" />
            <el-button :icon="Refresh" circle @click="refreshClick" />
        </el-row>
        <el-row>
            <ul v-infinite-scroll="load" class="infinite-list" style="overflow: auto">
                <li v-for="i in count" :key="i" class="infinite-list-item">{{ getCountString(i) }}</li>
            </ul>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Search, Refresh } from '@element-plus/icons-vue'

const l: Function = abp.localization.getResource('DFApp') as Function;

const logString = ref([]);

const countString = ref([]);

const count = ref(0);

const searchInput = ref('')

onMounted(async () => {

    // logString.value = countString.value = await dF.telegram.serilogSink.queueSink.getLogs() as string[];
    await refreshClick();

});

const load = () => {
    count.value += 20;
}

function getCountString(i) {
    if (i > countString.value.length) {
        return countString.value[(countString.value.length - 1)]
    }
    else {
        return countString.value[i]
    }
}

function searchLog(strIn) {
    if (strIn === undefined || strIn === null || strIn === "") {
        countString.value = logString.value
    }
    else {
        countString.value = logString.value.filter(str => str.includes(strIn));
    }

}

async function refreshClick(){
    logString.value = countString.value = await dFApp.serilogSink.queueSink.getLogs() as string[];
}

</script>

<style scoped>
.infinite-list {
    height: 70vh;
    padding: 0;
    margin: 0;
    list-style: none;
}

.infinite-list .infinite-list-item {
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--el-color-primary-light-9);
    margin: 10px;
    color: var(--el-color-primary);
}

.infinite-list .infinite-list-item+.list-item {
    margin-top: 10px;
}
</style>