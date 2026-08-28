<template>
  <n-config-provider :theme-overrides="themeOverrides">
    <n-message-provider>
      <RouterView />
    </n-message-provider>
  </n-config-provider>
</template>

<script setup lang="ts">
  import { onMounted } from 'vue';
  import { init } from './bridge';

  const themeOverrides = {
    common: {
      primaryColor: '#2080F0', // 修改主色 这个变量会影响按钮、表单边框等
      primaryColorHover: '#2080F0', // 悬停时的颜色
      primaryColorPressed: '#2080F0', // 点击（按下）时的颜色

      borderRadius: '4px', // 设置全局基础圆角，这会影响大部分组件
      borderRadiusSmall: '2px', // 小尺寸组件圆角
      borderRadiusLarge: '6px', // 大尺寸组件圆角
    },
  };

  onMounted(() => init());
</script>
