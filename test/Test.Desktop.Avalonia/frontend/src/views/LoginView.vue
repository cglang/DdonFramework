<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const router = useRouter()
const username = ref('admin')
const password = ref('123456')
const loading = ref(false)

async function doLogin() {
  loading.value = true
  try {
    const token = await window.ui.invoke<string | null>('Auth.Login', { username: username.value, password: password.value })
    if (token) {
      sessionStorage.setItem('token', token)
      await window.ui.invoke('Auth.StartPlcSimulator')
      router.push('/dashboard')
    } else {
      ElMessage.error('用户名或密码错误')
    }
  } catch (err: any) {
    ElMessage.error(err.message)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div style="display:flex;align-items:center;justify-content:center;min-height:100vh;background:var(--el-bg-color)">
    <el-card style="width:380px;padding:20px">
      <template #header>
        <div style="text-align:center">
          <h2 style="margin:0;color:var(--el-color-primary)">Ddon 上位机</h2>
          <p style="margin:4px 0 0;font-size:13px;color:var(--el-text-color-secondary)">PLC 监控系统</p>
        </div>
      </template>
      <el-form @submit.prevent="doLogin" label-position="right" label-width="60px">
        <el-form-item label="用户名">
          <el-input v-model="username" placeholder="admin" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="password" type="password" placeholder="123456" show-password />
        </el-form-item>
        <div style="margin-top:8px">
          <el-button type="primary" native-type="submit" :loading="loading" style="width:100%">
            {{ loading ? '登录中...' : '登 录' }}
          </el-button>
        </div>
      </el-form>
    </el-card>
  </div>
</template>
