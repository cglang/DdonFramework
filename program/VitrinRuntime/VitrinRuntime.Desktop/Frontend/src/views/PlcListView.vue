<script setup lang="ts">
  import { ref, onMounted, reactive, inject, onUnmounted } from 'vue';
  import type { Ref } from 'vue';
  import { useRouter } from 'vue-router';
  import { ElMessage, ElMessageBox } from 'element-plus';
  import { plcManager, type PlcConfig } from '../api/plcApi';
  import type { HeaderAction } from '../App.vue';

  const plcTypeOptions = [
    { value: 'Siemens', label: '西门子' },
    { value: 'Mitsubishi', label: '三菱' },
  ];

  function getMcFrameLabel(value: string | number): string {
    const map: Record<string, string> = { '4': '1E', '11': '3E', '15': '4E' };
    return map[String(value)] || '3E';
  }

  const mcFrameOptions = [
    { value: 11, label: '3E 帧' },
    { value: 15, label: '4E 帧' },
    { value: 4, label: '1E 帧' },
  ];

  const cpuTypeOptions = [
    { value: 0, label: 'S7-200' },
    { value: 1, label: 'Logo 0BA8' },
    { value: 2, label: 'S7-200 Smart' },
    { value: 10, label: 'S7-300' },
    { value: 20, label: 'S7-400' },
    { value: 30, label: 'S7-1200' },
    { value: 40, label: 'S7-1500' },
  ];

  const router = useRouter();
  const headerActions = inject<Ref<HeaderAction[]>>('headerActions')!;
  const plcs = ref<PlcConfig[]>([]);
  const loading = ref(false);
  const dialogVisible = ref(false);

  const form = reactive({
    name: '',
    plcType: 'Siemens' as string,
    ip: '127.0.0.1',
    port: 102,
    scanInterval: 200,
    autoConnect: false,
    rack: 0,
    slot: 1,
    cpuType: 40,
    mcProtocolFrame: 11,
  });

  const editDialogVisible = ref(false);
  const editForm = reactive({
    oldName: '',
    name: '',
    plcType: 'Siemens' as string,
    ip: '',
    port: 102,
    scanInterval: 200,
    autoConnect: false,
    rack: 0,
    slot: 1,
    cpuType: 40,
    mcProtocolFrame: 11,
  });

  async function loadPlcs() {
    loading.value = true;
    try {
      plcs.value = await plcManager.listPlcs();
    } catch (e: any) {
      ElMessage.error('获取 PLC 列表失败: ' + (e.message || e));
    } finally {
      loading.value = false;
    }
  }

  function buildConnectionOptions(plcType: string, data: any): Record<string, string> {
    if (plcType === 'Mitsubishi') {
      return { mcProtocolFrame: String(data.mcProtocolFrame) };
    }
    return { rack: String(data.rack), slot: String(data.slot), cpuType: String(data.cpuType) };
  }

  async function handleAddPlc() {
    if (!form.name || !form.ip) {
      ElMessage.warning('请填写 PLC 名称和 IP 地址');
      return;
    }
    try {
      await plcManager.addPlc(form.name, form.plcType, form.ip, form.port, form.scanInterval, form.autoConnect, buildConnectionOptions(form.plcType, form));
      ElMessage.success('PLC 添加成功');
      dialogVisible.value = false;
      Object.assign(form, { name: '', plcType: 'Siemens', ip: '192.168.1.10', port: 102, scanInterval: 200, autoConnect: false, rack: 0, slot: 1, cpuType: 40, mcProtocolFrame: 11 });
      await loadPlcs();
    } catch (e: any) {
      ElMessage.error('添加失败: ' + (e.message || e));
    }
  }

  async function handleConnect(plc: PlcConfig) {
    try {
      await plcManager.connectPlc(plc.name);
      ElMessage.success(`PLC "${plc.name}" 已连接`);
      plc.isConnected = true;
    } catch (e: any) {
      ElMessage.error('连接失败: ' + (e.message || e));
    }
  }

  async function handleDisconnect(plc: PlcConfig) {
    try {
      await plcManager.disconnectPlc(plc.name);
      ElMessage.success(`PLC "${plc.name}" 已断开`);
      plc.isConnected = false;
    } catch (e: any) {
      ElMessage.error('断开失败: ' + (e.message || e));
    }
  }

  async function handleRemove(plc: PlcConfig) {
    try {
      await ElMessageBox.confirm(`确定移除 PLC "${plc.name}" 吗？`, '确认', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      });
      await plcManager.removePlc(plc.name);
      ElMessage.success(`PLC "${plc.name}" 已移除`);
      await loadPlcs();
    } catch (e: any) {
      if (e !== 'cancel') ElMessage.error('移除失败: ' + (e.message || e));
    }
  }

  async function handleEditPlc(plc: PlcConfig) {
    editForm.oldName = plc.name;
    editForm.name = plc.name;
    editForm.plcType = plc.plcType;
    editForm.ip = plc.ip;
    editForm.port = plc.port;
    editForm.scanInterval = plc.scanInterval;
    editForm.autoConnect = plc.autoConnect;
    if (plc.plcType === 'Mitsubishi') {
      editForm.mcProtocolFrame = Number(plc.connectionOptions.mcProtocolFrame) || 11;
    } else {
      editForm.rack = Number(plc.connectionOptions.rack) || 0;
      editForm.slot = Number(plc.connectionOptions.slot) || 1;
      editForm.cpuType = Number(plc.connectionOptions.cpuType) || 40;
    }
    editDialogVisible.value = true;
  }

  async function handleUpdatePlc() {
    if (!editForm.name || !editForm.ip) {
      ElMessage.warning('请填写 PLC 名称和 IP 地址');
      return;
    }
    try {
      await plcManager.updatePlc(editForm.oldName, editForm.name, editForm.ip, editForm.port, editForm.scanInterval, editForm.autoConnect, buildConnectionOptions(editForm.plcType, editForm));
      ElMessage.success('PLC 已更新');
      editDialogVisible.value = false;
      await loadPlcs();
    } catch (e: any) {
      ElMessage.error('更新失败: ' + (e.message || e));
    }
  }

  function goToDetail(name: string) {
    router.push({ name: 'PlcDetail', params: { name } });
  }

  function getStatusType(plc: PlcConfig) {
    return plc.isConnected ? 'success' : 'danger';
  }

  function getStatusText(plc: PlcConfig) {
    return plc.isConnected ? '已连接' : '未连接';
  }

  function getCpuTypeLabel(value: number): string {
    const opt = cpuTypeOptions.find((o) => o.value === value);
    return opt ? opt.label : `S7-${value}`;
  }

  onMounted(() => {
    loadPlcs();
    headerActions.value = [{ label: '添加 PLC', icon: 'Plus', type: 'primary', onClick: () => (dialogVisible.value = true) }];
  });

  onUnmounted(() => {
    headerActions.value = [];
  });
</script>

<template>
  <div style="padding: 20px; height: 100%; box-sizing: border-box; overflow-y: auto">
    <div v-loading="loading" style="min-height: 300px">
      <el-row :gutter="20">
        <el-col v-for="plc in plcs" :key="plc.name" :xs="24" :sm="12" :md="8" :lg="6">
          <el-card shadow="hover" style="margin-bottom: 20px; cursor: pointer" @click="goToDetail(plc.name)">
            <template #header>
              <el-row justify="space-between" align="middle">
                <span style="font-size: 16px; font-weight: 600; color: #303133">{{ plc.name }}</span>
                <el-tag :type="getStatusType(plc)" size="small" effect="dark">
                  {{ getStatusText(plc) }}
                </el-tag>
              </el-row>
            </template>

            <el-descriptions :column="1" size="small" border>
              <el-descriptions-item label="类型">{{ plc.plcType === 'Mitsubishi' ? '三菱' : '西门子' }}</el-descriptions-item>
              <el-descriptions-item label="IP:端口">{{ plc.ip }}:{{ plc.port }}</el-descriptions-item>
              <template v-if="plc.plcType === 'Mitsubishi'">
                <el-descriptions-item label="协议帧">{{ getMcFrameLabel(plc.connectionOptions.mcProtocolFrame) }} 帧</el-descriptions-item>
              </template>
              <template v-else>
                <el-descriptions-item label="CPU 类型/机架/槽位">{{ getCpuTypeLabel(Number(plc.connectionOptions.cpuType) || 40) }}/{{ plc.connectionOptions.rack || 0 }}/{{ plc.connectionOptions.slot || 1 }}</el-descriptions-item>
                <!-- <el-descriptions-item label="">{{ plc.connectionOptions.rack || 0 }}/{{ plc.connectionOptions.slot || 1 }}</el-descriptions-item> -->
              </template>
              <el-descriptions-item label="扫描频率">{{ plc.scanInterval }}ms</el-descriptions-item>
            </el-descriptions>

            <el-alert v-if="plc.errorMessage" :title="plc.errorMessage" type="error" show-icon :closable="false" size="small" style="margin-top: 8px" />

            <el-row justify="end" style="margin-top: 12px; padding-top: 12px; border-top: 1px solid var(--el-border-color-light)" @click.stop>
              <el-button v-if="!plc.isConnected" type="success" size="small" @click="handleConnect(plc)">连接</el-button>
              <el-button v-if="plc.isConnected" type="warning" size="small" @click="handleDisconnect(plc)">断开</el-button>
              <el-button type="danger" size="small" @click="handleRemove(plc)">移除</el-button>
              <el-button size="small" @click="handleEditPlc(plc)">编辑</el-button>
            </el-row>
          </el-card>
        </el-col>
      </el-row>

      <el-empty v-if="!loading && plcs.length === 0" description="暂无 PLC，请点击上方按钮添加" />
    </div>

    <el-dialog v-model="dialogVisible" title="添加 PLC" width="450px">
      <el-form :model="form" label-width="100px">
        <el-form-item label="名称" required>
          <el-input v-model="form.name" placeholder="如: MainPLC" />
        </el-form-item>
        <el-form-item label="PLC 类型" required>
          <el-select v-model="form.plcType" style="width: 100%">
            <el-option v-for="opt in plcTypeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="IP 地址" required>
          <el-input v-model="form.ip" placeholder="192.168.1.10" />
        </el-form-item>
        <el-form-item label="端口">
          <el-input-number v-model="form.port" :min="1" :max="65535" />
        </el-form-item>
        <template v-if="form.plcType === 'Siemens'">
          <el-form-item label="机架 (Rack)">
            <el-input-number v-model="form.rack" :min="0" :max="7" />
          </el-form-item>
          <el-form-item label="槽位 (Slot)">
            <el-input-number v-model="form.slot" :min="0" :max="7" />
          </el-form-item>
          <el-form-item label="CPU 类型">
            <el-select v-model="form.cpuType" style="width: 100%">
              <el-option v-for="opt in cpuTypeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
        </template>
        <template v-else>
          <el-form-item label="协议帧">
            <el-select v-model="form.mcProtocolFrame" style="width: 100%">
              <el-option v-for="opt in mcFrameOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
        </template>
        <el-form-item label="扫描频率 (ms)">
          <el-input-number v-model="form.scanInterval" :min="50" :max="10000" :step="50" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">PLC 数据轮询间隔，越小刷新越快</el-text>
        </el-form-item>
        <el-form-item label="自动连接">
          <el-switch v-model="form.autoConnect" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleAddPlc">确定添加</el-button>
      </template>
    </el-dialog>

    <!-- 编辑 PLC 对话框 -->
    <el-dialog v-model="editDialogVisible" title="编辑 PLC" width="450px">
      <el-form :model="editForm" label-width="100px">
        <el-form-item label="名称" required>
          <el-input v-model="editForm.name" placeholder="如: MainPLC" />
        </el-form-item>
        <el-form-item label="IP 地址" required>
          <el-input v-model="editForm.ip" placeholder="192.168.1.10" />
        </el-form-item>
        <el-form-item label="端口">
          <el-input-number v-model="editForm.port" :min="1" :max="65535" />
        </el-form-item>
        <template v-if="editForm.plcType === 'Siemens'">
          <el-form-item label="机架 (Rack)">
            <el-input-number v-model="editForm.rack" :min="0" :max="7" />
          </el-form-item>
          <el-form-item label="槽位 (Slot)">
            <el-input-number v-model="editForm.slot" :min="0" :max="7" />
          </el-form-item>
          <el-form-item label="CPU 类型">
            <el-select v-model="editForm.cpuType" style="width: 100%">
              <el-option v-for="opt in cpuTypeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
        </template>
        <template v-else>
          <el-form-item label="协议帧">
            <el-select v-model="editForm.mcProtocolFrame" style="width: 100%">
              <el-option v-for="opt in mcFrameOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
        </template>
        <el-form-item label="扫描频率 (ms)">
          <el-input-number v-model="editForm.scanInterval" :min="50" :max="10000" :step="50" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">PLC 数据轮询间隔，越小刷新越快</el-text>
        </el-form-item>
        <el-form-item label="自动连接">
          <el-switch v-model="editForm.autoConnect" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleUpdatePlc">保存修改</el-button>
      </template>
    </el-dialog>
  </div>
</template>
