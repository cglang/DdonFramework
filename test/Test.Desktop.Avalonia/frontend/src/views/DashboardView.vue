<script setup lang="ts">
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { ElMessage, ElMessageBox } from 'element-plus';

  const router = useRouter();
  type DataDict = Record<string, unknown>;

  const data = ref<DataDict>({});
  const events = ref<{ time: string; msg: string; highlight: boolean }[]>([]);
  const statusText = ref('就绪');
  const autoRefresh = ref(false);
  let refreshTimer: ReturnType<typeof setInterval> | null = null;

  function log(msg: string, highlight = false) {
    const time = new Date().toLocaleTimeString();
    events.value.unshift({ time, msg, highlight });
    if (events.value.length > 100) events.value.pop();
  }

  async function readAll() {
    statusText.value = '读取中...';
    try {
      data.value = await window.ui.invoke<DataDict>('Plc.ReadAllPlc');
      statusText.value = '就绪';
    } catch (err: any) {
      statusText.value = `错误: ${err.message}`;
    }
  }

  async function writePlc(address: string) {
    try {
      const newValue = await ElMessageBox.prompt(`输入 ${address} 的新值:`, '写入', { inputPattern: /.+/, inputErrorMessage: '值不能为空' });
      await window.ui.invoke('Plc.WritePlc', { address, value: newValue.value });
      log(`已写入 ${address} = ${newValue.value}`, true);
      ElMessage.success(`写入 ${address} 成功`);
      await readAll();
    } catch {}
  }

  function toggleAutoRefresh() {
    autoRefresh.value = !autoRefresh.value;
    if (autoRefresh.value) {
      refreshTimer = setInterval(readAll, 5000);
      ElMessage.info('自动刷新已开启');
    } else if (refreshTimer) {
      clearInterval(refreshTimer);
      refreshTimer = null;
      ElMessage.info('自动刷新已关闭');
    }
  }

  async function doLogout() {
    try {
      await window.ui.invoke('Auth.Logout');
    } catch {}
    sessionStorage.removeItem('token');
    router.push('/login');
  }

  onMounted(() => {
    readAll();
    window.ui.on('PlcDataUpdatedEvent', (event: any) => {
      log(`数据变更: ${event.address} → ${JSON.stringify(event.value)}`);
    });
  });

  onUnmounted(() => {
    if (refreshTimer) clearInterval(refreshTimer);
    window.ui.off('PlcDataUpdatedEvent');
  });
</script>

<template>
  <div style="max-width: 1200px; margin: 0 auto; padding: 20px">
    <el-row justify="space-between" align="middle" style="margin-bottom: 20px">
      <el-col :span="12">
        <h2 style="color: var(--el-color-primary); margin: 0">PLC 监控面板</h2>
      </el-col>
      <el-col :span="12" style="text-align: right">
        <el-button-group>
          <el-button type="primary" @click="readAll" :icon="'Refresh'">读取全部</el-button>
          <el-button @click="toggleAutoRefresh" :type="autoRefresh ? 'warning' : 'default'">
            {{ autoRefresh ? '停止刷新' : '自动刷新' }}
          </el-button>
          <el-button @click="doLogout" plain>退出登录</el-button>
        </el-button-group>
        <el-tag :type="statusText === '就绪' ? 'success' : 'danger'" style="margin-left: 10px">{{ statusText }}</el-tag>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :span="14">
        <el-card>
          <template #header><span style="font-weight: 600">数据监控</span></template>
          <el-table :data="Object.entries(data).map(([addr, val]) => ({ addr, val }))" stripe style="width: 100%" max-height="500">
            <el-table-column prop="addr" label="地址" />
            <el-table-column prop="val" label="值">
              <template #default="{ row }">
                <el-tag type="info">{{ row.val }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="100">
              <template #default="{ row }">
                <el-button size="small" @click="writePlc(row.addr)">写入</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <el-col :span="10">
        <el-card>
          <template #header><span style="font-weight: 600">实时事件</span></template>
          <div style="max-height: 500px; overflow-y: auto">
            <div v-for="(evt, i) in events" :key="i" style="padding: 6px 0; border-bottom: 1px solid var(--el-border-color-light); font-size: 13px; font-family: monospace" :style="evt.highlight ? 'color:var(--el-color-danger)' : 'color:var(--el-text-color-secondary)'">[{{ evt.time }}] {{ evt.msg }}</div>
            <el-empty v-if="events.length === 0" description="暂无事件" />
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
