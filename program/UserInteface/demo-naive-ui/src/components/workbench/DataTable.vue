<!--
  DataTable 通用数据表格（基于 n-data-table）
  通过 columns 配置列、通过 checkbox 启用行复选框、
  通过 rowKey / checkedRowKeys 支持受控选中，序号列支持跨页偏移。

  用法：
    <DataTable
      :data="rows"
      :columns="cols"
      checkbox
      row-key="id"
      v-model:checked-row-keys="keys"
    >
      <template #stock="{ row }">自定义单元格</template>
    </DataTable>
-->
<script setup lang="ts" generic="T">
  import { computed, useSlots, type VNodeChild } from 'vue';
  import type { DataTableColumns, DataTableRowKey } from 'naive-ui';

  export interface TableColumn {
    key: string;
    title: string;
    /** 固定宽度 */
    width?: number | string;
    /** 最小宽度 */
    minWidth?: number | string;
    /** 对齐方式 */
    align?: 'left' | 'center' | 'right';
    /** 内容超出省略 */
    ellipsis?: boolean;
    /** 自定义渲染插槽名（默认等于 key） */
    slotName?: string;
    /** 单元格类名 */
    className?: string;
    /** 自定义渲染函数，优先级最高（TS 项目中推荐，类型安全） */
    render?: (row: Record<string, unknown>, column: TableColumn) => VNodeChild;
  }

  export type TableRowKey = DataTableRowKey;

  const props = withDefaults(
    defineProps<{
      data: T[];
      columns: TableColumn[];
      /** 启用复选框 */
      checkbox?: boolean;
      /** 复选框列宽 */
      checkboxWidth?: number;
      /** 是否显示序号列 */
      showIndex?: boolean;
      /** 序号列宽 */
      indexWidth?: number;
      /** 序号列标题，默认 "序号" */
      indexTitle?: string;
      /** 数据主键字段，用于选择态追踪 */
      rowKey?: string;
      /** 受控选中行的 key 集合 */
      checkedRowKeys?: Array<TableRowKey>;
      /** 序号起始偏移（分页跨页时使用） */
      indexOffset?: number;
      /** 最多显示行数，超出后在表格内部滚动；0 或不传表示不限制 */
      maxRows?: number;
      /** 估算单行高度（px），用于把 maxRows 换算成最大高度，默认 40 */
      rowHeight?: number;
      /** 估算表头高度（px），用于把 maxRows 换算成最大高度，默认 40 */
      headerHeight?: number;
      /** 直接指定表格最大高度（px），优先级高于 maxRows */
      maxHeight?: number;
    }>(),
    {
      checkbox: false,
      checkboxWidth: 48,
      showIndex: true,
      indexWidth: 56,
      indexTitle: '序号',
      rowKey: 'id',
      checkedRowKeys: () => [],
      indexOffset: 0,
      maxRows: 0,
      rowHeight: 40,
      headerHeight: 40,
      maxHeight: 0,
    },
  );

  const emit = defineEmits<{
    (e: 'update:checked-row-keys', keys: Array<TableRowKey>): void;
  }>();

  const slots = useSlots();

  // 解析单元格内容：render 函数 > 命名插槽 > 字段值
  const resolveCell = (row: T, col: TableColumn): VNodeChild => {
    if (col.render) {
      return col.render(row as Record<string, unknown>, col);
    }
    const slot = slots[col.slotName || col.key];
    if (slot) return slot({ row, column: col });
    const record = row as Record<string, unknown>;
    const v = record[col.key];
    return v === null || v === undefined ? '' : String(v);
  };

  const rowKeyGetter = (row: T): TableRowKey => {
    return (row as Record<string, unknown>)[props.rowKey] as TableRowKey;
  };

  // 转换为 n-data-table 的 columns 结构
  const nativeColumns = computed<DataTableColumns<T>>(() => {
    const cols: DataTableColumns<T> = [];

    if (props.checkbox) {
      cols.push({ type: 'selection', width: props.checkboxWidth });
    }

    if (props.showIndex) {
      cols.push({
        key: '__index__',
        title: props.indexTitle,
        width: props.indexWidth,
        align: 'center',
        render: (_row: T, rowIndex: number) => rowIndex + 1 + (props.indexOffset || 0),
      });
    }

    for (const col of props.columns) {
      cols.push({
        key: col.key,
        title: col.title,
        width: col.width,
        minWidth: col.minWidth,
        align: col.align,
        ellipsis: col.ellipsis,
        className: col.className,
        render: (row: T) => resolveCell(row, col),
      });
    }

    return cols;
  });

  const onCheckedChange = (keys: Array<TableRowKey>) => {
    emit('update:checked-row-keys', keys);
  };

  // 滚动容器样式：maxRows / maxHeight 触发内部滚动
  const scrollStyle = computed<Record<string, string | undefined>>(() => {
    if (props.maxHeight && props.maxHeight > 0) {
      return { maxHeight: `${props.maxHeight}px`, overflowY: 'auto' };
    }
    if (props.maxRows && props.maxRows > 0) {
      const h = props.headerHeight + props.maxRows * props.rowHeight;
      return { maxHeight: `${h}px`, overflowY: 'auto' };
    }
    return {};
  });
</script>

<template>
  <div class="data-table-scroll" :style="scrollStyle">
    <n-data-table
      :columns="nativeColumns"
      :data="data"
      :row-key="rowKeyGetter"
      :checked-row-keys="checkedRowKeys"
      :bordered="false"
      :single-line="true"
      size="small"
      class="workbench-data-table"
      @update:checked-row-keys="onCheckedChange"
    />
  </div>
</template>

<style scoped>
  .data-table-scroll {
    /* 行数超限时由该容器滚动；不限制时自然撑开 */
  }

  .workbench-data-table {
    --n-th-color: #ecf2f8;
    --n-th-text-color: #303133;
    --n-td-text-color: #4a5568;
    --n-border-color: #e9eef3;
    --n-td-color-hover: #f5f9fd;
    font-size: 13px;
  }

  .workbench-data-table :deep(.n-data-table-td) {
    padding: 10px 12px;
  }

  .workbench-data-table :deep(.n-data-table-th) {
    font-weight: 500;
  }

  /*
   * 滚动时表头固定在顶部。
   * naive-ui 内部 .n-scrollbar-container(overflow: scroll) 和
   * .n-data-table-base-table-body(overflow: hidden) 都会成为 sticky 的
   * "最近滚动祖先"，导致表头跟随滚动而失效；
   * 这里统一改回 visible，让 sticky 相对外层 .data-table-scroll 生效。
   */
  .data-table-scroll :deep(.n-scrollbar-container),
  .data-table-scroll :deep(.n-data-table-base-table-body) {
    overflow: visible !important;
  }

  .data-table-scroll :deep(.n-data-table-thead) {
    position: sticky;
    top: 0;
    z-index: 10;
  }
</style>
