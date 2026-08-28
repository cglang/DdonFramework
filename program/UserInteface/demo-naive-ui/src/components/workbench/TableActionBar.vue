<!--
  TableActionBar 表格上方操作按钮组
  第一项默认作为"主操作"（带图标、实心样式），其余为次要按钮（无图标）。
  通过 buttons prop 灵活配置，可整体替换。
-->
<template>
  <n-space :size="8" :wrap="true" class="table-action-bar">
    <n-button
      v-for="(btn, index) in buttons"
      :key="btn.key"
      :type="getType(btn, index)"
      :ghost="index !== 0 && btn.type !== 'primary'"
      :focusable="false"
      size="small"
      class="action-btn"
      @click="handleClick(btn)"
    >
      <template #icon>
        <component :is="btn.icon" v-if="btn.icon" class="action-icon" />
      </template>
      {{ btn.label }}
    </n-button>
  </n-space>
</template>

<script setup lang="ts">
  import type { Component } from 'vue';

  export interface ActionButton {
    key: string | number;
    label: string;
    /** 可选图标 */
    icon?: Component | string;
    /** 强制按钮类型，默认第一项 primary，其余 tertiary */
    type?: 'primary' | 'tertiary' | 'default' | 'error' | 'warning' | 'success' | 'info';
    /** 是否禁用 */
    disabled?: boolean;
  }

  const props = withDefaults(
    defineProps<{
      buttons: ActionButton[];
    }>(),
    { buttons: () => [] },
  );

  const emit = defineEmits<{
    (e: 'click', button: ActionButton): void;
  }>();

  // 第一项主样式(primary)，其余默认 ghost 浅边框
  const getType = (btn: ActionButton, index: number) => {
    if (btn.type) return btn.type;
    return index === 0 ? 'primary' : 'tertiary';
  };

  const handleClick = (btn: ActionButton) => {
    if (btn.disabled) return;
    emit('click', btn);
  };

  void props;
</script>

<style scoped>
  .table-action-bar {
    display: flex;
    align-items: center;
  }

  .action-btn {
    min-width: 64px;
  }

  .action-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 14px;
    height: 14px;
    margin-right: 4px;
  }

  .action-icon :deep(svg) {
    width: 100%;
    height: 100%;
  }
</style>
