<template>
  <div class="window-title-bar" :class="{ draggable: draggable }" @mousedown="startDrag">
    <!-- 左侧：应用图标和名称 -->
    <div class="title-bar-left">
      <div v-if="icon" class="app-icon">
        <component :is="icon" />
      </div>
      <span class="app-title">{{ title }}</span>
    </div>

    <!-- 中间：预留空间，可用于显示其他信息 -->
    <div class="title-bar-center">
      <slot name="center"></slot>
    </div>

    <!-- 右侧：窗口控制按钮 -->
    <div class="title-bar-right">
      <!-- 自定义按钮插槽 -->
      <slot name="custom-buttons"></slot>

      <!-- 最小化按钮 -->
      <button v-if="showMinimize" class="window-btn minimize-btn" @click.stop="handleMinimize" @mouseenter="hoverBtn = 'minimize'" @mouseleave="hoverBtn = null" title="最小化">
        <svg viewBox="0 0 10 10" width="10" height="10">
          <rect x="0" y="4" width="10" height="2" rx="1" />
        </svg>
      </button>

      <!-- 最大化/还原按钮 -->
      <button v-if="showMaximize" class="window-btn maximize-btn" @click.stop="handleMaximize" @mouseenter="hoverBtn = 'maximize'" @mouseleave="hoverBtn = null" :title="isMaximized ? '还原' : '最大化'">
        <svg viewBox="0 0 10 10" width="10" height="10">
          <!-- 最大化图标 -->
          <rect x="1" y="1" width="8" height="8" rx="1" fill="none" stroke="currentColor" stroke-width="1.2" />
          <!-- 还原图标（两个重叠的方框） -->
          <!-- <g v-else>
            <rect x="1.5" y="0.5" width="6" height="6" rx="0.5" fill="none" stroke="currentColor" stroke-width="1" />
            <rect x="0" y="2.5" width="6" height="6" rx="0.5" fill="none" stroke="currentColor" stroke-width="1" />
          </g> -->
        </svg>
      </button>

      <!-- 关闭按钮 -->
      <button v-if="showClose" class="window-btn close-btn" @click.stop="handleClose" @mouseenter="hoverBtn = 'close'" @mouseleave="hoverBtn = null" title="关闭">
        <svg viewBox="0 0 10 10" width="10" height="10">
          <line x1="1.5" y1="1.5" x2="8.5" y2="8.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
          <line x1="8.5" y1="1.5" x2="1.5" y2="8.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  // 定义 props
  const props = defineProps({
    // 标题文字
    title: {
      type: String,
      default: '应用程序',
    },
    // 图标组件或 SVG
    icon: {
      type: [String, Object],
      default: null,
    },
    // 是否显示最小化按钮
    showMinimize: {
      type: Boolean,
      default: true,
    },
    // 是否显示最大化按钮
    showMaximize: {
      type: Boolean,
      default: true,
    },
    // 是否显示关闭按钮
    showClose: {
      type: Boolean,
      default: true,
    },
    // 是否可拖拽
    draggable: {
      type: Boolean,
      default: true,
    },
  });

  // 定义事件
  const emit = defineEmits(['minimize', 'maximize', 'restore', 'close', 'drag-start', 'drag-end']);

  // 状态
  const isMaximized = ref(false);
  const hoverBtn = ref(null);

  // 处理最小化
  const handleMinimize = () => {
    emit('minimize');
  };

  // 处理最大化/还原
  const handleMaximize = () => {
    isMaximized.value = !isMaximized.value;
    if (isMaximized.value) {
      emit('maximize');
    } else {
      emit('restore');
    }
  };

  // 处理关闭
  const handleClose = () => {
    emit('close');
  };

  // 拖拽功能
  const startDrag = (e) => {
    if (!props.draggable) return;

    // 如果点击的是按钮，不触发拖拽
    if (e.target.closest('button')) return;
  };
</script>

<style scoped>
  .window-title-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 32px;
    padding: 0 8px;
    background: #f0f0f0;
    border-bottom: 1px solid #d0d0d0;
    user-select: none;
    position: relative;
    min-width: 200px;
  }

  /* 拖拽样式 */
  .draggable {
    cursor: default;
  }

  /* 左侧区域 */
  .title-bar-left {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }

  .app-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 16px;
    height: 16px;
    font-size: 16px;
  }

  .app-icon svg {
    width: 100%;
    height: 100%;
  }

  .app-title {
    font-size: 12px;
    color: #333;
    font-weight: 500;
    white-space: nowrap;
  }

  /* 中间区域 */
  .title-bar-center {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 8px;
    overflow: hidden;
  }

  /* 右侧区域 */
  .title-bar-right {
    display: flex;
    align-items: center;
    gap: 2px;
    flex-shrink: 0;
  }

  /* 窗口控制按钮 */
  .window-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border: none;
    background: transparent;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.15s;
    color: #333;
    padding: 0;
  }

  .window-btn svg {
    display: block;
  }

  /* 悬停效果 */
  .window-btn:hover {
    background-color: rgba(0, 0, 0, 0.1);
  }

  .window-btn:active {
    background-color: rgba(0, 0, 0, 0.15);
  }

  /* 关闭按钮特殊样式 */
  .close-btn:hover {
    background-color: #e81123;
    color: white;
  }

  .close-btn:active {
    background-color: #c50f1f;
  }

  /* 最大化按钮在最大化状态下的样式 */
  .maximize-btn.is-maximized {
    /* 可以添加特殊样式 */
    font-size: 11px;
  }

  /* 响应式处理 */
  @media (max-width: 400px) {
    .window-title-bar {
      padding: 0 4px;
    }

    .window-btn {
      width: 24px;
      height: 24px;
    }

    .app-title {
      font-size: 11px;
    }
  }
</style>
