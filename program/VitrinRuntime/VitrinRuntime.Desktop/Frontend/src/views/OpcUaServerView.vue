<script setup lang="ts">
  import { ref, reactive, onMounted, onUnmounted, inject, nextTick } from 'vue';
  import type { Ref } from 'vue';
  import { ElMessage, ElMessageBox } from 'element-plus';
  import { opcUaServer, type ServerStatus, type NodeInfo, type NodeDetail } from '../api/opcUaApi';
  import type { HeaderAction } from '../App.vue';

  // ── 状态 ────────────────────────────────────
  const headerActions = inject<Ref<HeaderAction[]>>('headerActions')!;
  const serverStatus = ref<ServerStatus | null>(null);
  const treeData = ref<NodeInfo[]>([]);
  const selectedNode = ref<NodeDetail | null>(null);
  const loading = ref(false);
  const writeValue = ref('');
  const showWrite = ref(false);

  // 时间日志
  interface LogEntry {
    time: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'danger';
  }
  const eventLog = ref<LogEntry[]>([]);
  const logContainer = ref<HTMLElement | null>(null);

  function addLog(message: string, type: LogEntry['type'] = 'info') {
    const now = new Date();
    const time = now.toLocaleTimeString('zh-CN', { hour12: false });
    eventLog.value.unshift({ time, message, type });
    if (eventLog.value.length > 200) {
      eventLog.value = eventLog.value.slice(0, 200);
    }
    nextTick(() => {
      if (logContainer.value) {
        logContainer.value.scrollTop = 0;
      }
    });
  }

  // ── Server 控制 ─────────────────────────────

  async function loadStatus() {
    try {
      const status = await opcUaServer.getServerStatus();
      serverStatus.value = status;
    } catch (e: any) {
      console.warn('获取 OPC UA Server 状态失败:', e.message || e);
    }
  }

  async function handleStart() {
    try {
      await opcUaServer.startServer();
      ElMessage.success('OPC UA Server 已启动');
      addLog('Server 已启动', 'success');
    } catch (e: any) {
      ElMessage.error('启动失败: ' + (e.message || e));
      addLog('启动失败: ' + (e.message || e), 'danger');
    }
  }

  async function handleStop() {
    try {
      await opcUaServer.stopServer();
      ElMessage.success('OPC UA Server 已停止');
      addLog('Server 已停止', 'warning');
    } catch (e: any) {
      ElMessage.error('停止失败: ' + (e.message || e));
      addLog('停止失败: ' + (e.message || e), 'danger');
    }
  }

  async function handleRestart() {
    try {
      addLog('Server 正在重启...', 'info');
      await opcUaServer.restartServer();
      ElMessage.success('OPC UA Server 已重启');
      addLog('Server 已重启', 'success');
      await loadTree();
    } catch (e: any) {
      ElMessage.error('重启失败: ' + (e.message || e));
      addLog('重启失败: ' + (e.message || e), 'danger');
    }
  }

  // ── 地址空间树 ──────────────────────────────

  interface TreeNode {
    nodePath: string;
    displayName: string;
    nodeClass: string;
    dataType: string;
    hasChildren: boolean;
    children?: TreeNode[];
    loading?: boolean;
  }

  const treeRef = ref<any>(null);
  const treeNodes = ref<TreeNode[]>([]);

  async function loadTree() {
    loading.value = true;
    try {
      const children = await opcUaServer.browseChildren(undefined);
      treeNodes.value = children.map(n => ({
        ...n,
        children: n.hasChildren ? [] as TreeNode[] : undefined,
        loading: false,
      }));
    } catch (e: any) {
      ElMessage.error('加载地址空间失败: ' + (e.message || e));
    } finally {
      loading.value = false;
    }
  }

  async function handleNodeClick(data: TreeNode) {
    if (data.nodeClass === 'Variable') {
      await loadNodeDetail(data.nodePath);
    }
  }

  async function handleNodeExpand(data: TreeNode) {
    if (data.hasChildren && (!data.children || data.children.length === 0)) {
      data.loading = true;
      try {
        const children = await opcUaServer.browseChildren(data.nodePath);
        data.children = children.map(n => ({
          ...n,
          children: n.hasChildren ? [] as TreeNode[] : undefined,
          loading: false,
        }));
      } catch (e: any) {
        ElMessage.error('加载子节点失败: ' + (e.message || e));
      } finally {
        data.loading = false;
      }
    }
  }

  function getNodeIcon(nodeClass: string): string {
    switch (nodeClass) {
      case 'Object': return 'folder-opened';
      case 'Variable': return 'document';
      case 'Method': return 'video-play';
      default: return 'question-filled';
    }
  }

  // ── 节点详情 ────────────────────────────────

  async function loadNodeDetail(nodePath: string) {
    try {
      selectedNode.value = await opcUaServer.getNodeDetail(nodePath);
      if (selectedNode.value && selectedNode.value.nodeClass === 'Variable') {
        writeValue.value = selectedNode.value.value ?? '';
        showWrite.value = true;
      } else {
        showWrite.value = false;
      }
    } catch (e: any) {
      ElMessage.error('获取节点详情失败: ' + (e.message || e));
    }
  }

  async function handleWriteValue() {
    if (!selectedNode.value) return;
    try {
      await opcUaServer.writeNodeValue(selectedNode.value.nodePath, writeValue.value);
      ElMessage.success('值已写入');
      addLog(`节点 ${selectedNode.value.displayName} 值写入: ${writeValue.value}`, 'success');
      await loadNodeDetail(selectedNode.value.nodePath);
    } catch (e: any) {
      ElMessage.error('写入失败: ' + (e.message || e));
    }
  }

  function clearSelectedNode() {
    selectedNode.value = null;
    showWrite.value = false;
  }

  // ── 生命周期 ────────────────────────────────

  onMounted(() => {
    headerActions.value = [
      { label: '刷新', icon: 'Refresh', type: 'default', onClick: () => { loadTree(); } },
    ];
    // 初始加载一次状态，后续通过事件推送更新
    loadStatus();
    loadTree();
    addLog('OPC UA 管理界面已加载', 'info');

    // 订阅后端推送的 Server 状态变化事件
    window.ui.on('ServerStatusChangedEvent', (status: ServerStatus) => {
      serverStatus.value = status;
    });
  });

  onUnmounted(() => {
    headerActions.value = [];
    window.ui.off('ServerStatusChangedEvent');
  });
</script>

<template>
  <div style="height: 100%; display: flex; flex-direction: column; box-sizing: border-box; overflow: hidden">
    <!-- 状态栏 -->
    <div v-if="serverStatus" style="padding: 12px 20px; border-bottom: 1px solid var(--el-border-color-light); display: flex; align-items: center; gap: 16px; flex-shrink: 0; background: #fff">
      <div style="display: flex; align-items: center; gap: 6px">
        <span :style="{ display: 'inline-block', width: 10, height: 10, borderRadius: '50%', backgroundColor: serverStatus.isRunning ? '#67c23a' : '#f56c6c' }"></span>
        <span :style="{ fontSize: 14, fontWeight: 600, color: serverStatus.isRunning ? '#67c23a' : '#f56c6c' }">
          {{ serverStatus.isRunning ? '运行中' : '已停止' }}
        </span>
      </div>
      <span style="font-size: 13px; color: #909399; font-family: monospace">{{ serverStatus.endpointUrl }}</span>
      <span style="font-size: 12px; color: #909399">会话数: {{ serverStatus.sessionCount }}</span>
      <div style="margin-left: auto; display: flex; gap: 8px">
        <el-button v-if="serverStatus.isRunning" type="warning" size="small" @click="handleRestart">重启</el-button>
        <el-button v-if="!serverStatus.isRunning" type="primary" size="small" @click="handleStart">启动</el-button>
        <el-button v-if="serverStatus.isRunning" type="danger" size="small" @click="handleStop">停止</el-button>
      </div>
    </div>

    <!-- 主体内容 -->
    <div style="flex: 1; display: flex; overflow: hidden">
      <!-- 地址空间树 -->
      <div style="width: 320px; min-width: 240px; border-right: 1px solid var(--el-border-color-light); display: flex; flex-direction: column; overflow: hidden">
        <div style="padding: 12px 16px; border-bottom: 1px solid var(--el-border-color-light); font-size: 14px; font-weight: 600; color: #303133; display: flex; align-items: center; justify-content: space-between">
          <span>地址空间</span>
          <el-button size="small" text @click="loadTree()">
            <el-icon><refresh /></el-icon>
          </el-button>
        </div>
        <div style="flex: 1; overflow: auto; padding: 8px" v-loading="loading">
          <el-tree
            ref="treeRef"
            :data="treeNodes"
            node-key="nodePath"
            :props="{ children: 'children', label: 'displayName' }"
            highlight-current
            @node-click="handleNodeClick"
            @node-expand="handleNodeExpand"
          >
            <template #default="{ data }">
              <span style="display: flex; align-items: center; gap: 4px; font-size: 13px">
                <el-icon><component :is="getNodeIcon(data.nodeClass)" /></el-icon>
                <span>{{ data.displayName }}</span>
                <span v-if="data.dataType" style="color: #909399; font-size: 11px; margin-left: 4px">({{ data.dataType }})</span>
              </span>
            </template>
          </el-tree>
          <el-empty v-if="!loading && treeNodes.length === 0" description="地址空间为空" :image-size="60" />
        </div>
      </div>

      <!-- 节点详情 + 日志 -->
      <div style="flex: 1; display: flex; flex-direction: column; overflow: hidden">
        <!-- 节点详情 -->
        <div style="flex: 1; overflow: auto; padding: 16px">
          <div v-if="!selectedNode" style="display: flex; align-items: center; justify-content: center; height: 100%; color: #909399; font-size: 14px">
            请从左侧地址空间选择一个节点
          </div>

          <template v-if="selectedNode">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px">
              <h3 style="margin: 0; font-size: 16px; color: #303133">节点详情</h3>
              <el-button size="small" text @click="clearSelectedNode">关闭</el-button>
            </div>

            <el-descriptions :column="1" size="small" border style="margin-bottom: 16px">
              <el-descriptions-item label="名称">{{ selectedNode.displayName }}</el-descriptions-item>
              <el-descriptions-item label="节点路径">{{ selectedNode.nodePath }}</el-descriptions-item>
              <el-descriptions-item label="类型">{{ selectedNode.nodeClass }}</el-descriptions-item>
              <el-descriptions-item label="数据类型">{{ selectedNode.dataType || '-' }}</el-descriptions-item>
              <el-descriptions-item v-if="selectedNode.nodeClass === 'Variable'" label="当前值">
                <span style="font-family: monospace; font-weight: 600; color: #409eff">{{ selectedNode.value ?? '(空)' }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="数据源类型">{{ selectedNode.sourceType || '-' }}</el-descriptions-item>
            </el-descriptions>

            <!-- 写入值 -->
            <el-card v-if="showWrite" shadow="never" style="margin-bottom: 16px">
              <template #header>
                <span style="font-size: 14px; font-weight: 600">写入值</span>
              </template>
              <div style="display: flex; gap: 12px; align-items: center">
                <el-input v-model="writeValue" placeholder="输入新值" style="flex: 1" />
                <el-button type="primary" @click="handleWriteValue">写入</el-button>
              </div>
            </el-card>
          </template>
        </div>

        <!-- 事件日志 -->
        <div style="height: 180px; border-top: 1px solid var(--el-border-color-light); display: flex; flex-direction: column; flex-shrink: 0">
          <div style="padding: 8px 16px; border-bottom: 1px solid var(--el-border-color-light); font-size: 13px; font-weight: 600; color: #303133">事件日志</div>
          <div ref="logContainer" style="flex: 1; overflow-y: auto; padding: 4px 12px; font-size: 12px; font-family: monospace; background: #fafafa">
            <div v-if="eventLog.length === 0" style="color: #c0c4cc; text-align: center; padding: 20px">暂无日志</div>
            <div v-for="(log, idx) in eventLog" :key="idx" style="padding: 2px 0; display: flex; gap: 8px">
              <span style="color: #909399; flex-shrink: 0">[{{ log.time }}]</span>
              <span :style="{ color: log.type === 'danger' ? '#f56c6c' : log.type === 'success' ? '#67c23a' : log.type === 'warning' ? '#e6a23c' : '#303133' }">
                {{ log.message }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
