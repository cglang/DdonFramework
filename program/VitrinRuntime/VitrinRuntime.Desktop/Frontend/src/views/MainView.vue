<script setup lang="ts">
  import { ref } from 'vue';
  import { useRouter } from 'vue-router';
  import { ElMessage } from 'element-plus';

  const router = useRouter();

  function navigateTo(path: string) {
    router.push(path);
  }
</script>

<template>
  <div style="display: flex; flex-direction: column; align-items: center; padding: 60px 20px; min-height: 100%; box-sizing: border-box">
    <div style="text-align: center; margin-bottom: 40px">
      <h1 style="font-size: 28px; color: #303133; margin: 0 0 8px">Ddon 上位机案例</h1>
      <p style="font-size: 14px; color: #909399; margin: 0">VitrinRuntime 桌面应用程序</p>
    </div>

    <el-row :gutter="20" justify="center" style="max-width: 800px">
      <el-col :span="12">
        <el-card shadow="hover" @click="navigateTo('/plc/list')">
          <template #header>
            <el-row align="middle" style="font-size: 18px; font-weight: 600; color: #303133">
              <el-icon size="28" color="#409eff" style="margin-right: 10px"><Monitor /></el-icon>
              <span>PLC 管理</span>
            </el-row>
          </template>
          <p style="margin: 0; color: #606266; font-size: 13px; line-height: 1.6">管理西门子 PLC 连接、DB 块分组和点位数据</p>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
