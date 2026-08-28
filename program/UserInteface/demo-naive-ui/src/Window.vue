<template>
  <div class="table-layout">
    <div class="row row-1">
      <WorkbenchHeader title="工作台" :info-items="headerInfoItems" @close="handleHeaderClose" @info-click="handleHeaderInfoClick" @mousedown="onTitleBarMouseDown" />
    </div>
    <div class="row row-2 window-body">
      <RouterView />
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed, h, reactive, ref } from 'vue';
  import { RouterLink, RouterView } from 'vue-router';
  import WorkbenchHeader, { type HeaderInfoItem } from './components/workbench/WorkbenchHeader.vue';
  import router from './router/index.ts';

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
    { key: 'window/wb', text: '工序绑定', icon: ProcessBindIcon, clickable: true },

    { key: 'window/naiveui', text: '测试页面', icon: ProcessBindIcon, clickable: true },

    { key: 'window', text: 'Home', icon: ProcessBindIcon, clickable: true },
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

  const onTitleBarMouseDown = (e: MouseEvent) => {
    // 只处理鼠标左键
    if (e.button !== 0) {
      return;
    }

    // 点击窗口按钮时，不触发拖动
    if ((e.target as HTMLElement).closest('button')) {
      return;
    }

    if ((e.target as HTMLElement).closest('span')) {
      return;
    }

    // WebView 模式下调用桥接服务 window.drag 实现窗体拖动
    if (window.platform === 'webview') {
      void window.ui.invoke('window.drag');
    }
  };
</script>

<style>
  .table-layout {
    width: 100%;
    height: 100%;

    display: grid;

    /* 每一行的高度 */
    grid-template-rows:
      50px
      1fr;
  }

  .row {
    /* border: 1px solid #3498db; */
    box-sizing: border-box;
  }

  .window-body {
    background: #f5f7fa;
    padding: 16px;
    overflow: auto;
  }
</style>
