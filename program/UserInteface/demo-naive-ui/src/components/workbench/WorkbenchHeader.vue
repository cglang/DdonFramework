<!--
  WorkbenchHeader 工作台标题栏
  用于工作台类页面顶部的深色标题栏，左侧展示页面标题，
  右侧支持信息项（如用户、绑定入口）和关闭按钮。
-->
<template>
  <div class="workbench-header">
    <!-- 左侧：标题 -->
    <div class="header-left">
      <span class="header-title">{{ title }}</span>
    </div>

    <!-- 右侧：信息项 + 关闭按钮 -->
    <div class="header-right">
      <div
        v-for="(item, index) in infoItems"
        :key="index"
        class="header-info"
        :class="{ 'is-link': item.clickable }"
        @click="item.clickable && handleInfoClick(item)"
      >
        <component :is="item.icon" v-if="item.icon" class="header-info-icon" />
        <span class="header-info-text">{{ item.text }}</span>
      </div>

      <button v-if="closable" class="header-close" @click="handleClose" title="关闭">
        <svg viewBox="0 0 14 14" width="14" height="14" fill="none">
          <path
            d="M2 2 L12 12 M12 2 L2 12"
            stroke="currentColor"
            stroke-width="1.6"
            stroke-linecap="round"
          />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
  import type { Component } from 'vue';

  // 信息项类型定义，方便复用
  export interface HeaderInfoItem {
    /** 唯一 key */
    key?: string | number;
    /** 显示文字 */
    text: string;
    /** 可选图标组件 */
    icon?: Component | string;
    /** 是否可点击 */
    clickable?: boolean;
  }

  defineProps({
    /** 标题文字 */
    title: { type: String, default: '工作台' },
    /** 右侧信息项（如工序绑定、admin） */
    infoItems: {
      type: Array as () => HeaderInfoItem[],
      default: () => [],
    },
    /** 是否显示关闭按钮 */
    closable: { type: Boolean, default: true },
  });

  const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'info-click', item: HeaderInfoItem): void;
  }>();

  const handleClose = () => emit('close');
  const handleInfoClick = (item: HeaderInfoItem) => emit('info-click', item);
</script>

<style scoped>
  .workbench-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 100%;
    padding: 0 24px;
    background: #2080f0;
    color: #fff;
    user-select: none;
  }

  .header-left {
    display: flex;
    align-items: center;
  }

  .header-title {
    font-size: 16px;
    font-weight: 500;
    letter-spacing: 0.5px;
    color: #fff;
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 24px;
  }

  .header-info {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 14px;
    color: #fff;
    cursor: default;
  }

  .header-info.is-link {
    cursor: pointer;
    transition: opacity 0.15s;
  }

  .header-info.is-link:hover {
    opacity: 0.85;
  }

  .header-info-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
  }

  .header-info-icon :deep(svg) {
    width: 100%;
    height: 100%;
  }

  .header-info-text {
    white-space: nowrap;
  }

  .header-close {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border: none;
    background: transparent;
    border-radius: 4px;
    color: #fff;
    cursor: pointer;
    transition: background 0.15s;
    padding: 0;
  }

  .header-close:hover {
    background: rgba(255, 255, 255, 0.18);
  }

  .header-close:active {
    background: rgba(255, 255, 255, 0.28);
  }
</style>
