<!--
  WorkbenchView 工作台页面
  整合顶部标题栏、搜索、操作栏、数据表格、分页器。
  当前为生产计划静态视图，搜索/分页均为本地状态。
-->
<template>
  <!-- 主体内容卡片 -->
  <div class="workbench-body">
    <!-- 搜索条件卡片 -->
    <!-- <n-card class="search-card" :bordered="false">
      <SearchFormBar v-model="searchForm" :fields="searchFields" :initial-value="initialSearch" :cols="5" />

      <div class="action-row">
        <TableActionBar :buttons="actionButtons" @click="handleAction" />
      </div>
    </n-card> -->

    <!-- 数据表格卡片 -->
    <!-- <n-card class="table-card" :bordered="false">
        <DataTable :data="pagedData" :columns="tableColumns" :checkbox="true" :show-index="true" :index-offset="(pagination.page - 1) * pagination.pageSize" :row-key="'id'" :max-rows="8" v-model:checked-row-keys="checkedKeys" />

        <TablePagination v-model:page="pagination.page" v-model:page-size="pagination.pageSize" :total="tableData.length" />
      </n-card> -->
  </div>
</template>

<script setup lang="ts">
  import { computed, h, reactive, ref } from 'vue';
  import { NCard, NConfigProvider, NMessageProvider, NSpace, useMessage } from 'naive-ui';

  import SearchFormBar, { type SearchField } from '../components/workbench/SearchFormBar.vue';
  import TableActionBar, { type ActionButton } from '../components/workbench/TableActionBar.vue';
  import DataTable, { type TableColumn } from '../components/workbench/DataTable.vue';
  import TablePagination from '../components/workbench/TablePagination.vue';

  // ----------- 图标组件（内联 SVG） -----------

  // 搜索图标（实心，放大镜）
  const SearchIcon = {
    render() {
      return h(
        'svg',
        {
          viewBox: '0 0 14 14',
          width: 14,
          height: 14,
          fill: 'none',
          stroke: 'currentColor',
          'stroke-width': 1.6,
          'stroke-linecap': 'round',
        },
        [h('circle', { cx: 6, cy: 6, r: 4 }), h('line', { x1: 9, y1: 9, x2: 12.5, y2: 12.5 })],
      );
    },
  };

  // ----------- 类型定义 -----------
  interface ProductionPlanItem {
    id: number;
    workOrderNo: string;
    planNo: string;
    orderNo: string;
    productCode: string;
    productName: string;
    model: string;
    version: string;
    batchNo: string;
    drawingNo: string;
    stock: number;
    wip: number;
    planQty: number;
  }

  // ----------- 搜索字段配置 -----------
  const searchFields: SearchField[] = [
    { key: 'workOrderNo', label: '工单编号', placeholder: '请输入' },
    { key: 'planNo', label: '计划编号', placeholder: '请输入' },
    { key: 'productName', label: '产品名称', placeholder: '请输入' },
    { key: 'batchNo', label: '生产批次', placeholder: '请输入' },
    { key: 'dateRange', label: '时间范围', type: 'daterange', span: 1 },
  ];

  const initialSearch = {
    workOrderNo: '',
    planNo: '',
    productName: '',
    batchNo: '',
    dateRange: null,
  };

  const searchForm = ref<Record<string, unknown>>({ ...initialSearch });

  // ----------- 操作按钮配置 -----------
  const actionButtons: ActionButton[] = [
    { key: 'search', label: '搜索', icon: SearchIcon },
    { key: 'reset', label: '重置' },
    { key: 'pause', label: '暂停' },
    { key: 'revoke', label: '撤销' },
    { key: 'close', label: '关闭' },
    { key: 'freeze', label: '冻结' },
    { key: 'print', label: '打印' },
  ];

  const handleAction = (btn: ActionButton) => {
    switch (btn.key) {
      case 'search':
        handleSearch();
        break;
      case 'reset':
        handleReset();
        break;
      default:
        console.log(`点击了 ${btn.label}`);
    }
  };

  const handleSearch = () => {
    console.log('搜索条件:', searchForm.value);
  };

  const handleReset = () => {
    searchForm.value = { ...initialSearch };
  };

  // ----------- 表格列配置 -----------
  // 数值列通过 render 渲染，保持右对齐的等宽数字样式
  const renderNum = (row: Record<string, unknown>, key: string) => h('span', { class: 'num' }, String(row[key] ?? ''));

  const tableColumns: TableColumn[] = [
    { key: 'workOrderNo', title: '工单编号', width: 100 },
    { key: 'planNo', title: '计划编号', width: 100 },
    { key: 'orderNo', title: '订单号', width: 100 },
    { key: 'productCode', title: '产品编号', width: 100 },
    { key: 'productName', title: '产品名称', width: 100 },
    { key: 'model', title: '产品型号', width: 100 },
    { key: 'version', title: '产品版本', width: 100, align: 'center' },
    { key: 'batchNo', title: '生产批次', width: 100 },
    { key: 'drawingNo', title: '图号', width: 100 },
    {
      key: 'stock',
      title: '库存',
      width: 100,
      align: 'right',
      render: (row) => renderNum(row, 'stock'),
    },
    {
      key: 'wip',
      title: '在制品数量',
      width: 110,
      align: 'right',
      render: (row) => renderNum(row, 'wip'),
    },
    {
      key: 'planQty',
      title: '计划生产数量',
      width: 130,
      align: 'right',
      render: (row) => renderNum(row, 'planQty'),
    },
  ];

  // ----------- 表格模拟数据 -----------
  // 设计图上 1-6 行按字母 A-F 递增，第 7 行起回到 C,D,E,F，
  // 故分两段直接枚举，避免索引计算错误。
  const productNames = ['A', 'B', 'C', 'D', 'E', 'F', 'C', 'D', 'E', 'F'];
  const stockValues = [1500, 2000, 2225, 1500, 2000, 2225, 2225, 1500, 2000, 2225];

  const tableData = ref<ProductionPlanItem[]>(
    Array.from({ length: 50 }).map<ProductionPlanItem>((_, i) => ({
      id: i + 1,
      workOrderNo: 'GD001',
      planNo: 'JH001',
      orderNo: 'Nozzle',
      productCode: `MN00${i + 1}`,
      productName: `齿轮${productNames[i % productNames.length]}`,
      model: 'V1.0',
      version: '',
      batchNo: '',
      drawingNo: '',
      stock: stockValues[i] ?? 0,
      wip: 3000,
      planQty: 3000,
    })),
  );

  const checkedKeys = ref<Array<string | number>>([]);

  // ----------- 分页状态 -----------
  const pagination = reactive({
    page: 1,
    pageSize: 50,
  });

  const pagedData = computed(() => {
    const start = (pagination.page - 1) * pagination.pageSize;
    return tableData.value.slice(start, start + pagination.pageSize);
  });

  // 抑制未使用变量警告
  void NSpace;
  void NConfigProvider;
  void NMessageProvider;
  void useMessage;
</script>

<style scoped>
  .workbench-body {
    padding: 20px 24px;
    height: 100%;
    background: #f5f7fa;
  }

  .search-card {
    margin-bottom: 16px;
    border-radius: 6px;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  }
  .search-card :deep(.n-card__content) {
    padding: 16px 20px 4px;
  }

  .action-row {
    border-top: 1px dashed #ebeef5;
    margin-top: 12px;
    padding-top: 14px;
  }

  .table-card {
    border-radius: 6px;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  }
  .table-card :deep(.n-card__content) {
    padding: 16px 20px;
  }

  .num {
    font-variant-numeric: tabular-nums;
    color: #4a5568;
  }
</style>
