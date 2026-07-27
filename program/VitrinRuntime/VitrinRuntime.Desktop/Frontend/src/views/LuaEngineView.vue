<script setup lang="ts">
  import { ref, onMounted, reactive, inject, onUnmounted } from 'vue';
  import type { Ref } from 'vue';
  import { ElMessage, ElMessageBox } from 'element-plus';
  import { luaEngine, type LuaGroupInfo, type LuaGroupDetail, type LuaScriptInfo } from '../api/luaApi';
  import type { HeaderAction } from '../App.vue';

  const headerActions = inject<Ref<HeaderAction[]>>('headerActions')!;
  const groups = ref<LuaGroupInfo[]>([]);
  const loading = ref(false);
  const loadDialogVisible = ref(false);
  const loadForm = reactive({ directoryPath: '', groupName: '' });
  const watcherEnabled = ref(true);

  // 组详情
  const detailVisible = ref(false);
  const detailData = ref<LuaGroupDetail | null>(null);
  const detailLoading = ref(false);

  // Lua 执行
  const executeDialogVisible = ref(false);
  const executeForm = reactive({ groupName: '', code: '' });
  const executeResult = ref<string>('');
  const executing = ref(false);

  // VM 列表
  const vmListVisible = ref(false);
  const vmList = ref<{ groupName: string; hasVm: boolean }[]>([]);

  async function loadGroups() {
    loading.value = true;
    try {
      groups.value = await luaEngine.listGroups();
    } catch (e: any) {
      ElMessage.error('获取 Lua 组列表失败: ' + (e.message || e));
    } finally {
      loading.value = false;
    }
  }

  async function loadWatcherStatus() {
    try {
      const status = await luaEngine.getWatcherStatus();
      watcherEnabled.value = status.enabled;
    } catch (e: any) {
      // ignore
    }
  }

  async function handleLoadGroup() {
    if (!loadForm.directoryPath) {
      ElMessage.warning('请输入脚本目录路径');
      return;
    }
    try {
      const groupName = loadForm.groupName || undefined;
      await luaEngine.loadGroup(loadForm.directoryPath, groupName);
      ElMessage.success('脚本组加载成功');
      loadDialogVisible.value = false;
      loadForm.directoryPath = '';
      loadForm.groupName = '';
      await loadGroups();
    } catch (e: any) {
      ElMessage.error('加载失败: ' + (e.message || e));
    }
  }

  async function handleReloadGroup(groupName: string) {
    try {
      await luaEngine.reloadGroup(groupName);
      ElMessage.success(`组 "${groupName}" 已重新加载`);
      await loadGroups();
    } catch (e: any) {
      ElMessage.error('重载失败: ' + (e.message || e));
    }
  }

  async function handleUnloadGroup(groupName: string) {
    try {
      await ElMessageBox.confirm(`确定卸载组 "${groupName}" 吗？`, '确认', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      });
      await luaEngine.unloadGroup(groupName);
      ElMessage.success(`组 "${groupName}" 已卸载`);
      await loadGroups();
    } catch (e: any) {
      if (e !== 'cancel') ElMessage.error('卸载失败: ' + (e.message || e));
    }
  }

  async function showDetail(groupName: string) {
    detailLoading.value = true;
    detailVisible.value = true;
    try {
      detailData.value = await luaEngine.getGroupDetail(groupName);
    } catch (e: any) {
      ElMessage.error('获取详情失败: ' + (e.message || e));
    } finally {
      detailLoading.value = false;
    }
  }

  async function handleReloadScript(script: LuaScriptInfo) {
    if (!detailData.value) return;
    try {
      await luaEngine.reloadScript(detailData.value.name, script.fileName);
      ElMessage.success(`脚本 "${script.fileName}" 已重载`);
      await showDetail(detailData.value.name);
    } catch (e: any) {
      ElMessage.error('重载失败: ' + (e.message || e));
    }
  }

  async function handleUnloadScript(script: LuaScriptInfo) {
    if (!detailData.value) return;
    try {
      await luaEngine.unloadScript(detailData.value.name, script.fileName);
      ElMessage.success(`脚本 "${script.fileName}" 已卸载`);
      await showDetail(detailData.value.name);
    } catch (e: any) {
      ElMessage.error('卸载失败: ' + (e.message || e));
    }
  }

  async function handleToggleWatcher() {
    try {
      const status = await luaEngine.setWatcher(!watcherEnabled.value);
      watcherEnabled.value = status.enabled;
      ElMessage.success(status.enabled ? '文件监控已开启' : '文件监控已关闭');
    } catch (e: any) {
      ElMessage.error('切换失败: ' + (e.message || e));
    }
  }

  function openExecuteDialog(groupName: string) {
    executeForm.groupName = groupName;
    executeForm.code = '';
    executeResult.value = '';
    executeDialogVisible.value = true;
  }

  async function handleExecuteLua() {
    if (!executeForm.code) {
      ElMessage.warning('请输入 Lua 代码');
      return;
    }
    executing.value = true;
    executeResult.value = '';
    try {
      const result = await luaEngine.executeLua(executeForm.groupName, executeForm.code);
      if (result.success) {
        executeResult.value = result.result || '(无返回值)';
      } else {
        executeResult.value = '错误: ' + (result.error || '未知错误');
      }
    } catch (e: any) {
      executeResult.value = '执行异常: ' + (e.message || e);
    } finally {
      executing.value = false;
    }
  }

  async function showVmList() {
    try {
      vmList.value = await luaEngine.listVms();
      vmListVisible.value = true;
    } catch (e: any) {
      ElMessage.error('获取 VM 列表失败: ' + (e.message || e));
    }
  }

  function getLoadStatus(group: LuaGroupInfo): { type: 'success' | 'danger' | 'warning'; text: string } {
    if (group.vmLoaded && group.hasScripts) return { type: 'success', text: '已加载' };
    if (group.vmLoaded) return { type: 'warning', text: 'VM 就绪' };
    return { type: 'danger', text: '未加载' };
  }

  onMounted(() => {
    loadGroups();
    loadWatcherStatus();
    headerActions.value = [
      { label: '加载脚本组', icon: 'FolderOpened', type: 'primary', onClick: () => (loadDialogVisible.value = true) },
      { label: '文件监控', icon: 'VideoCameraFilled', type: watcherEnabled.value ? 'success' : 'danger', onClick: handleToggleWatcher },
      { label: 'VM 列表', icon: 'Cpu', type: 'info', onClick: showVmList },
    ];
  });

  onUnmounted(() => {
    headerActions.value = [];
  });
</script>

<template>
  <div style="padding: 20px; height: 100%; box-sizing: border-box; overflow-y: auto">
    <div v-loading="loading" style="min-height: 300px">
      <el-row :gutter="20">
        <el-col v-for="group in groups" :key="group.name" :xs="24" :sm="12" :md="8" :lg="6">
          <el-card shadow="hover" style="margin-bottom: 20px">
            <template #header>
              <el-row justify="space-between" align="middle">
                <span style="font-size: 16px; font-weight: 600; color: #303133; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                  {{ group.name }}
                </span>
                <el-tag :type="getLoadStatus(group).type" size="small" effect="dark">
                  {{ getLoadStatus(group).text }}
                </el-tag>
              </el-row>
            </template>

            <el-descriptions :column="1" size="small" border>
              <el-descriptions-item label="目录路径">{{ group.path }}</el-descriptions-item>
              <el-descriptions-item label="脚本数量">{{ group.scriptCount }} 个</el-descriptions-item>
              <el-descriptions-item label="VM 状态">
                <el-tag :type="group.vmLoaded ? 'success' : 'info'" size="small">
                  {{ group.vmLoaded ? '已创建' : '未创建' }}
                </el-tag>
              </el-descriptions-item>
            </el-descriptions>

            <el-row justify="end" style="margin-top: 12px; padding-top: 12px; border-top: 1px solid var(--el-border-color-light)">
              <el-button type="primary" size="small" @click="showDetail(group.name)">详情</el-button>
              <el-button type="warning" size="small" @click="handleReloadGroup(group.name)">重载</el-button>
              <el-button size="small" @click="openExecuteDialog(group.name)">执行</el-button>
              <el-button type="danger" size="small" @click="handleUnloadGroup(group.name)">卸载</el-button>
            </el-row>
          </el-card>
        </el-col>
      </el-row>

      <el-empty v-if="!loading && groups.length === 0" description="暂无 Lua 脚本组，请点击上方按钮加载" />
    </div>

    <!-- 加载脚本组对话框 -->
    <el-dialog v-model="loadDialogVisible" title="加载脚本组" width="450px">
      <el-form :model="loadForm" label-width="100px">
        <el-form-item label="目录路径" required>
          <el-input v-model="loadForm.directoryPath" placeholder="如: C:\Scripts\MyLua" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">脚本目录的绝对路径，会递归加载所有 .lua 文件</el-text>
        </el-form-item>
        <el-form-item label="组名称">
          <el-input v-model="loadForm.groupName" placeholder="留空则使用目录名" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">可选，不填则自动使用目录名</el-text>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="loadDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleLoadGroup">确定加载</el-button>
      </template>
    </el-dialog>

    <!-- 组详情对话框 (脚本列表) -->
    <el-dialog v-model="detailVisible" title="脚本详情" width="650px" :loading="detailLoading">
      <template v-if="detailData">
        <el-descriptions :column="1" size="small" border style="margin-bottom: 16px">
          <el-descriptions-item label="组名称">{{ detailData.name }}</el-descriptions-item>
          <el-descriptions-item label="目录路径">{{ detailData.path }}</el-descriptions-item>
          <el-descriptions-item label="脚本数量">{{ detailData.scripts.length }} 个</el-descriptions-item>
        </el-descriptions>

        <el-table :data="detailData.scripts" stripe size="small" max-height="400">
          <el-table-column prop="fileName" label="文件名" min-width="180" />
          <el-table-column prop="lastWriteTime" label="修改时间" width="160" />
          <el-table-column label="状态" width="80">
            <template #default="{ row }">
              <el-tag :type="row.isLoaded ? 'success' : 'info'" size="small">
                {{ row.isLoaded ? '已加载' : '未加载' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="120" fixed="right">
            <template #default="{ row }">
              <el-button link type="primary" size="small" @click="handleReloadScript(row)">重载</el-button>
              <el-button link type="danger" size="small" @click="handleUnloadScript(row)">卸载</el-button>
            </template>
          </el-table-column>
        </el-table>
      </template>
    </el-dialog>

    <!-- Lua 代码执行对话框 -->
    <el-dialog v-model="executeDialogVisible" title="执行 Lua 代码" width="600px">
      <el-form :model="executeForm" label-width="80px">
        <el-form-item label="目标组">
          <el-tag type="primary">{{ executeForm.groupName }}</el-tag>
        </el-form-item>
        <el-form-item label="Lua 代码" required>
          <el-input
            v-model="executeForm.code"
            type="textarea"
            :rows="10"
            placeholder="-- 在此输入 Lua 代码&#10;print('Hello from Lua!')"
            font-family="'Consolas', 'Courier New', monospace"
          />
        </el-form-item>
        <el-form-item v-if="executeResult" label="执行结果">
          <pre style="background: #f5f7fa; padding: 12px; border-radius: 4px; width: 100%; margin: 0; white-space: pre-wrap; word-break: break-all; font-size: 13px;">{{ executeResult }}</pre>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="executeDialogVisible = false">关闭</el-button>
        <el-button type="primary" :loading="executing" @click="handleExecuteLua">执行</el-button>
      </template>
    </el-dialog>

    <!-- VM 列表对话框 -->
    <el-dialog v-model="vmListVisible" title="Lua VM 列表" width="450px">
      <el-table :data="vmList" stripe size="small">
        <el-table-column prop="groupName" label="组名称" min-width="180" />
        <el-table-column label="VM 状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.hasVm ? 'success' : 'info'" size="small">
              {{ row.hasVm ? '已创建' : '未创建' }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-if="vmList.length === 0" description="暂无 VM" />
    </el-dialog>
  </div>
</template>
