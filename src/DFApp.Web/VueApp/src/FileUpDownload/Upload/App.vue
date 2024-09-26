<template>
    <el-upload class="upload-demo" :headers="uploadHeaders" :action="uploadUrl" :show-file-list="false"
        :on-error="handleError" :on-success="handleSuccess" :before-upload="handleBeforeUpload">
        <el-button type="primary">点击上传</el-button>
        <template #tip>
            <div class="el-upload__tip">
                最大上传1MB的文件
            </div>
        </template>
    </el-upload>
</template>
<script lang="ts" setup>
import { ref, onMounted } from 'vue'
import { ElMessageBox, UploadProps } from 'element-plus'
import CryptoJS from 'crypto-js';

const uploadHeaders: Record<string, any> = ref({ "Requestverificationtoken": "" })

const uploadUrl = ref('');

const handleError: UploadProps['onError'] = (error, uploadFile, uploadFiles) => {
    ElMessageBox.alert(error.message, '消息', {
        confirmButtonText: 'OK',
        type: 'error'
    })
}

const handleBeforeUpload: UploadProps['beforeUpload'] = async (rawFile) => {
    const reader = rawFile.stream().getReader();
    const chunks = [];
    let done = false;
    while (!done) {
        const { value, done: readerDone } = await reader.read();
        if (readerDone) {
            done = true;
        } else {
            chunks.push(value);
        }
    }
    const concatenated = Uint8Array.from(chunks.reduce((result, chunk) => result.concat(Array.from(chunk)), []));
    const wordArray = CryptoJS.lib.WordArray.create(concatenated);
    const hash = CryptoJS.SHA1(wordArray).toString(CryptoJS.enc.Hex);
    uploadHeaders.value["FileSHA1"] = hash;
}

const handleSuccess: UploadProps['onSuccess'] = (res, files, uploadFiles) => {

    ElMessageBox.alert(res, '消息', {
        confirmButtonText: 'OK',
        type: 'success'
    })

}

onMounted(() => {

    uploadUrl.value = `${window.location.origin}/api/FileUploadInfo/upload`;

    uploadHeaders.value["Requestverificationtoken"] = getCookie("XSRF-TOKEN");
})

function getCookie(name: string): string | undefined {
    const cookieName = name + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const cookieArray = decodedCookie.split(';');

    for (let i = 0; i < cookieArray.length; i++) {
        let cookie = cookieArray[i];
        while (cookie.charAt(0) === ' ') {
            cookie = cookie.substring(1);
        }
        if (cookie.indexOf(cookieName) === 0) {
            return cookie.substring(cookieName.length, cookie.length);
        }
    }
    return undefined;
}


</script>
