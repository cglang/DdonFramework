<script setup lang="ts">
  import { useRouter } from 'vue-router';

  interface MenuItem {
    icon: string;
    name: string;
    description: string;
    path: string;
  }

  const menuItems: MenuItem[] = [
    {
      icon: 'Monitor',
      name: '西门子PLC管理',
      description: '管理西门子 PLC 连接、DB 块分组和点位数据',
      path: '/plc/list',
    },
    {
      icon: 'Monitor',
      name: '西门子PLC管理',
      description: '管理西门子 PLC 连接、DB 块分组和点位数据',
      path: '/plc/list',
    },
    {
      icon: 'Monitor',
      name: '西门子PLC管理',
      description: '管理西门子 PLC 连接、DB 块分组和点位数据',
      path: '/plc/list',
    },
    {
      icon: 'Monitor',
      name: '西门子PLC管理',
      description: '管理西门子 PLC 连接、DB 块分组和点位数据',
      path: '/plc/list',
    },
  ];

  const router = useRouter();

  function navigateTo(path: string) {
    router.push(path);
  }
</script>

<template>
  <div style="display: flex; align-items: center; justify-content: center; min-height: 80%; box-sizing: border-box; padding: 40px 20px">
    <div style="display: flex; flex-wrap: wrap; justify-content: center; gap: 50px; width: 100%">
      <el-card v-for="item in menuItems" :key="item.name" shadow="hover" style="cursor: pointer; text-align: center; width: 180px; flex-shrink: 0" @click="navigateTo(item.path)">
        <div style="padding: 24px 12px 16px">
          <el-icon :size="60" color="#409eff">
            <component :is="item.icon" />
          </el-icon>
          <div style="font-size: 16px; font-weight: 600; color: #303133; margin-top: 12px">{{ item.name }}</div>
          <div style="font-size: 12px; color: #909399; margin-top: 6px; line-height: 1.5">{{ item.description }}</div>
        </div>
      </el-card>
    </div>
  </div>
</template>
