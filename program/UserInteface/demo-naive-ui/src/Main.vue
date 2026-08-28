<script setup lang="ts">
  import { computed, h, reactive, ref } from 'vue';
  import { RouterLink, RouterView } from 'vue-router';
  import { NConfigProvider } from 'naive-ui';
  import WorkbenchHeader, { type HeaderInfoItem } from './components/workbench/WorkbenchHeader.vue';
  import router from './router/index.ts';

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

  // 工序绑定图标（扳手+结点）
  const ProcessBindIcon = {
    render() {
      return h(
        'svg',
        {
          viewBox: '0 0 18 18',
          width: 18,
          height: 18,
          fill: 'none',
          stroke: 'currentColor',
          'stroke-width': 1.4,
          'stroke-linecap': 'round',
          'stroke-linejoin': 'round',
        },
        [
          // 头部
          h('path', { d: 'M11.5 2.5a3 3 0 0 1 4 4l-2-2-2 2z' }),
          // 颈部
          h('path', { d: 'M11.4 4.6 5.6 10.4a2 2 0 0 0 0 2.8l1.2 1.2a2 2 0 0 0 2.8 0L15.4 7' }),
          // 节点
          h('circle', { cx: 4.5, cy: 13.5, r: 1.6 }),
        ],
      );
    },
  };

  // 用户图标
  const UserIcon = {
    render() {
      return h(
        'svg',
        {
          viewBox: '0 0 18 18',
          width: 18,
          height: 18,
          fill: 'none',
          stroke: 'currentColor',
          'stroke-width': 1.4,
          'stroke-linecap': 'round',
        },
        [h('circle', { cx: 9, cy: 6.5, r: 3 }), h('path', { d: 'M2.5 16c0-3 3-5 6.5-5s6.5 2 6.5 5' })],
      );
    },
  };

  // ----------- 顶部标题栏信息项 -----------
  const headerInfoItems: HeaderInfoItem[] = [
    { key: 'wb', text: '工序绑定', icon: ProcessBindIcon, clickable: true },
    { key: 'home', text: 'Home', icon: ProcessBindIcon, clickable: true },
    { key: 'user', text: 'admin', icon: UserIcon },
  ];

  // ----------- 标题栏交互 -----------
  const handleHeaderClose = () => {
    console.log('关闭工作台');
  };

  const handleHeaderInfoClick = (item: HeaderInfoItem) => {
    console.log('点击了', item.text);
    router.push(`/${item.key}`);
  };
</script>

<template>
  <n-config-provider :theme-overrides="themeOverrides">
    <n-message-provider>
      <!-- 上下两段式布局：header 固定，content 占满剩余高度并在内部滚动 -->
      <div class="app-shell">
        <WorkbenchHeader title="工作台" :info-items="headerInfoItems" @close="handleHeaderClose" @info-click="handleHeaderInfoClick" />
        <main class="app-content">
          <RouterView />
        </main>
      </div>
    </n-message-provider>
  </n-config-provider>
</template>

<style scoped>
  .app-shell {
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;
  }

  .app-content {
    flex: 1;
    /* 关键：允许 flex 子项收缩，否则内容会把 shell 撑高 */
    min-height: 0;
    /* 内容超高时在容器内滚动，不撑破屏幕 */
    overflow-y: auto;
    overflow-x: hidden;
  }
</style>
