<template>
    <el-form v-if="isEnter" ref="loginForm" :model="form" :rules="rules">
        <el-form-item prop="username">
            <el-input v-model="form.username" placeholder="验证码" />
        </el-form-item>
        <el-form-item>
            <el-button type="primary" @click="submitForm">登录</el-button>
        </el-form-item>
    </el-form>
    <span v-if="!isEnter">{{loginStatus}}</span>
</template>
  
<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'

const loginForm = ref<any>(null)

const form = reactive({
    username: ''
})

const rules = reactive({
    username: [{ required: true, message: '请输入验证码' }]
})

const submitForm = () => {
    loginForm.value.validate(async valid => {
        if (valid) {
            await dFApp.tG.login.tGLogin.config(form.username)
        }
    })
}

const isEnter = ref(false);

const loginStatus = ref('');

onMounted(async () => {
    let str = await dFApp.tG.login.tGLogin.status() as string;
    if(str !== undefined && str !== null){
        isEnter.value = str.indexOf('Enter') >= 0; 
    }
    loginStatus.value = str;
});

</script>