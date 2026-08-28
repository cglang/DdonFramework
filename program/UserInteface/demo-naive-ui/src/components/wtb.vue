<!-- components/WindowTitleBar.vue -->
<template>
  <div class="window-title-bar" :class="[themeClass, { draggable: draggable }]" @mousedown="startDrag">
    <!-- 左侧 -->
    <div class="title-bar-left">
      <div v-if="icon" class="app-icon">
        <component :is="icon" />
      </div>
      <span class="app-title">{{ title }}</span>
    </div>

    <!-- 中间插槽 -->
    <div class="title-bar-center">
      <slot name="center"></slot>
    </div>

    <!-- 右侧 -->
    <div class="title-bar-right">
      <slot name="custom-buttons"></slot>

      <button v-if="showMinimize" class="window-btn minimize-btn" @click.stop="handleMinimize" title="最小化">
        <svg viewBox="0 0 10 10"><rect x="0" y="4" width="10" height="2" rx="1" /></svg>
      </button>

      <button v-if="showMaximize" class="window-btn maximize-btn" @click.stop="handleMaximize" :title="isMaximized ? '还原' : '最大化'">
        <svg viewBox="0 0 10 10">
          <rect v-if="!isMaximized" x="1" y="1" width="8" height="8" rx="1" fill="none" stroke="currentColor" stroke-width="1.2" />
          <g v-else>
            <rect x="1.5" y="0.5" width="6" height="6" rx="0.5" fill="none" stroke="currentColor" stroke-width="1" />
            <rect x="0" y="2.5" width="6" height="6" rx="0.5" fill="none" stroke="currentColor" stroke-width="1" />
          </g>
        </svg>
      </button>

      <button v-if="showClose" class="window-btn close-btn" @click.stop="handleClose" title="关闭">
        <svg viewBox="0 0 10 10">
          <line x1="1.5" y1="1.5" x2="8.5" y2="8.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
          <line x1="8.5" y1="1.5" x2="1.5" y2="8.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  const props = defineProps({
    title: { type: String, default: '应用程序' },
    icon: { type: [String, Object], default: null },
    showMinimize: { type: Boolean, default: true },
    showMaximize: { type: Boolean, default: true },
    showClose: { type: Boolean, default: true },
    draggable: { type: Boolean, default: true },
    theme: { type: String, default: 'dark' }, // 'dark' | 'light'
  });

  const emit = defineEmits(['minimize', 'maximize', 'restore', 'close', 'drag-start', 'drag-end']);

  const isMaximized = ref(false);
  const themeClass = props.theme === 'dark' ? 'theme-dark' : 'theme-light';

  const handleMinimize = () => emit('minimize');
  const handleMaximize = () => {
    isMaximized.value = !isMaximized.value;
    emit(isMaximized.value ? 'maximize' : 'restore');
  };
  const handleClose = () => emit('close');

  // 拖拽逻辑（略，与之前相同，可复制）
  const startDrag = (e) => {
    if (!props.draggable || e.target.closest('button')) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const offsetX = e.clientX - rect.left;
    const offsetY = e.clientY - rect.top;
    emit('drag-start', { offsetX, offsetY });
    const onMouseMove = (e) => {
      e.currentTarget.style.transform = `translate(${e.clientX - offsetX}px, ${e.clientY - offsetY}px)`;
    };
    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
      emit('drag-end');
    };
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  };
</script>

<style scoped>
  .window-title-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 36px;
    padding: 0 12px;
    user-select: none;
    position: relative;
    min-width: 200px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  /* 深色主题（默认） */
  .theme-dark {
    background: #2d3a4b;
    color: #ffffff;
  }
  .theme-dark .app-title {
    color: #ffffff;
  }
  .theme-dark .window-btn {
    color: #ffffff;
  }
  .theme-dark .window-btn:hover {
    background: rgba(255, 255, 255, 0.15);
  }
  .theme-dark .close-btn:hover {
    background: #e81123;
    color: #ffffff;
  }

  /* 浅色主题 */
  .theme-light {
    background: #f0f0f0;
    color: #333;
    border-bottom: 1px solid #d0d0d0;
  }
  .theme-light .app-title {
    color: #333;
  }
  .theme-light .window-btn {
    color: #333;
  }
  .theme-light .window-btn:hover {
    background: rgba(0, 0, 0, 0.1);
  }
  .theme-light .close-btn:hover {
    background: #e81123;
    color: #ffffff;
  }

  .title-bar-left {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }
  .app-icon {
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  .app-icon svg {
    width: 100%;
    height: 100%;
    fill: currentColor;
  }
  .app-title {
    font-size: 14px;
    font-weight: 500;
    white-space: nowrap;
  }
  .title-bar-center {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 8px;
    overflow: hidden;
  }
  .title-bar-right {
    display: flex;
    align-items: center;
    gap: 4px;
    flex-shrink: 0;
  }

  .window-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 30px;
    height: 30px;
    border: none;
    background: transparent;
    border-radius: 4px;
    cursor: pointer;
    transition: background 0.15s;
    padding: 0;
  }
  .window-btn svg {
    width: 12px;
    height: 12px;
    display: block;
  }
  .window-btn:hover {
    background: rgba(0, 0, 0, 0.1);
  }
  .window-btn:active {
    background: rgba(0, 0, 0, 0.2);
  }
  .draggable {
    cursor: default;
  }
</style>
