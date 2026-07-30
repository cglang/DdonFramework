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
      name: 'PLC管理',
      description: '管理 PLC 连接、分组和点位数据',
      path: '/plc/list',
    },
    {
      icon: 'Coin',
      name: 'Lua脚本引擎',
      description: '加载、管理、执行 Lua 脚本，文件监控自动重载',
      path: '/lua/engine',
    },
    {
      icon: 'Connection',
      name: 'OPC UA Server',
      description: '管理 OPC UA 服务器、浏览地址空间',
      path: '/opcua/server',
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
