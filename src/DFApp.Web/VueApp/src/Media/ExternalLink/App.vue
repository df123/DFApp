<template>
    <div>
        <el-row>
            <div class="botton-area">
                <el-button type="primary" @click="getLink">{{ l('Media:ExternalLinkTitle:GetLink') }}</el-button>
                <el-button type="primary" @click="copy">{{ l('Media:ExternalLinkTitle:Copy') }}</el-button>
                <el-button type="primary" @click="move">{{ l('Media:ExternalLinkTitle:Move') }}</el-button>
            </div>
        </el-row>
        <el-row style="margin-top: 10px;">
            <el-input v-model="textarea" :rows="2" type="textarea" :autosize="{ minRows: 2, maxRows: 30 }"
                placeholder="Please input" />
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'


const l: Function = abp.localization.getResource('DFApp') as Function;

const textarea = ref('')

async function getLink() {
    try {
        let links: string = await dFApp.media.mediaInfo.getExternalLinkDownload() as string;
        textarea.value = links;
    } catch (error) {
        alert(error);
    }
}

function copy() {
    navigator.clipboard.writeText(textarea.value);
    alert(l('Media:ExternalLinkTitle:CopySuccessMesage'));
}

async function move() {
    let result = await dFApp.media.mediaInfo.moveDownloaded() as string;
    textarea.value = result;
}

</script>

<style scoped>
botton-area {
    display: flex;
    margin-bottom: 10px;
}
</style>