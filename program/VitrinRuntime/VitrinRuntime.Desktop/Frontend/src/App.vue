<script setup lang="ts">
  import { ref, provide, onMounted, type Ref } from 'vue';
  import { useRouter, useRoute } from 'vue-router';
  import { init } from './bridge';

  export interface HeaderAction {
    label: string;
    icon?: string;
    type?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'default';
    onClick: () => void;
  }

  const router = useRouter();
  const route = useRoute();

  const headerActions = ref<HeaderAction[]>([]);
  provide<Ref<HeaderAction[]>>('headerActions', headerActions);

  onMounted(() => init());

  function handleActionClick(action: HeaderAction) {
    action.onClick();
  }

  const onTitleBarMouseDown = (e: MouseEvent) => {
    // 只处理鼠标左键
    if (e.button !== 0) {
      return;
    }

    // 点击窗口按钮时，不触发拖动
    if ((e.target as HTMLElement).closest('button')) {
      return;
    }

    window.ui.seedMessage('windowDrag');
  };
</script>

<template>
  <div style="height: 100vh; display: flex; flex-direction: column; overflow: hidden">
    <!-- 全局顶部导航栏 -->
    <div @mousedown="onTitleBarMouseDown">
      <el-row align="middle" style="padding: 0 20px; height: 60px; border-bottom: 1px solid var(--el-border-color-light); background: #fff; flex-shrink: 0">
        <el-button text @click="router.push('/main')" style="font-size: 20px; font-weight: 600; color: #303133">上位机平台</el-button>
        <span v-if="route.meta.location" style="font-size: 14px; color: #909399; margin-left: 12px">位置：{{ route.meta.location }}</span>
        <div style="margin-left: auto; display: flex; gap: 8px">
          <template v-for="action in headerActions" :key="action.label">
            <el-button :type="action.type || 'default'" size="small" @click="handleActionClick(action)">
              <el-icon v-if="action.icon">
                <component :is="action.icon" />
              </el-icon>
              {{ action.label }}
            </el-button>
          </template>
        </div>
      </el-row>
    </div>
    <!-- 页面内容 -->
    <div style="flex: 1; overflow: auto">
      <router-view />
    </div>
  </div>
</template>
