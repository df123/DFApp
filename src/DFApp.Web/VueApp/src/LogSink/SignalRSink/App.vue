<template>
    <div>
        <el-row>
            <div class="tool-flex">
                <el-input v-model="searchInput" class="w-50 m-2" placeholder="搜索日志" :prefix-icon="Search"
                    @input="searchLog" />
                <el-button :icon="getPauseStartIcon()" circle @click="pause" />
            </div>
        </el-row>
        <el-row>
            <ul v-infinite-scroll="load" class="infinite-list" style="overflow: auto">
                <li v-for="i in count" :key="i" class="infinite-list-item">{{ getCountString(i) }}</li>
            </ul>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Search, VideoPause, VideoPlay } from '@element-plus/icons-vue'
import * as signalR from "@microsoft/signalr";

const l: Function = abp.localization.getResource('DFApp') as Function;

const logString = ref([]);

const countString = ref([]);

const count = ref(0);

const searchInput = ref('')

const isPause = ref(false);

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/signalr-hubs/sink")
    .build();

connection.on("SignalRSink", (message: string) => {

    if (logString.value.length > 10000) {
        logString.value = [];
    }
    logString.value.unshift(message);

    if (!isPause.value) {
        searchLog(searchInput.value);
    }
});

connection.start().catch((err) => console.log(err));


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
        if (isPause.value) {
            countString.value = JSON.parse(JSON.stringify(logString.value));
        }
        else {
            countString.value = logString.value;
        }
    }
    else {
        countString.value = logString.value.filter(str => str.toLowerCase().includes(strIn.toLowerCase()));
    }
}

function pause() {
    isPause.value = !isPause.value;
    if (isPause.value) {
        countString.value = JSON.parse(JSON.stringify(logString.value));
    }
}


function getPauseStartIcon() {
    return isPause.value ? VideoPlay : VideoPause;
}

</script>

<style scoped>
.infinite-list {
    height: 70vh;
    padding: 0;
    margin: 0;
    list-style: none;
    background: #000000;
}

.infinite-list .infinite-list-item {
    display: flex;
    align-items: center;
    justify-content: flex-start;
    margin: 10px;
    color: #FFFFFF;
}

.infinite-list .infinite-list-item+.list-item {
    margin-top: 10px;
}

.tool-flex {
    display: flex;
    align-items: center;
    justify-content: flex-start;
    width: 100%;
}
</style>

