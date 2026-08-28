<!--
  TablePagination 表格分页器
  包含：共 N 条（左侧）+ 每页条数 + 页码上下页（中部）+ 跳转指定页（右侧）
  通过 v-model 双向绑定 page / pageSize，内部维护 goToPage 用于跳转。
-->
<template>
  <div class="table-pagination">
    <span class="total-count">共 {{ total }} 条</span>

    <span class="page-size-control">
      <n-select
        :value="pageSize"
        :options="pageSizeOptions.map((s) => ({ label: `${s}条/页`, value: s }))"
        size="small"
        style="width: 96px"
        @update:value="onPageSizeChange"
      />
    </span>

    <n-pagination
      :value="page"
      :page-count="pageCount"
      :page-slot="5"
      size="small"
      @update:value="onPageChange"
    >
      <template #goto>
        <span class="goto-label">前往</span>
        <n-input-number
          :value="goToPage"
          :min="1"
          :max="pageCount"
          size="small"
          class="goto-input"
          @update:value="(v: number | null) => (goToPage = v ?? 1)"
        />
        <span class="goto-label">页</span>
      </template>
    </n-pagination>

    <n-button
      type="primary"
      size="small"
      ghost
      class="goto-btn"
      @click="onGotoPage"
    >
      跳转
    </n-button>
  </div>
</template>

<script setup lang="ts">
  import { computed, ref, watch } from 'vue';

  const props = withDefaults(
    defineProps<{
      page: number;
      pageSize: number;
      total: number;
      pageSizes?: number[];
    }>(),
    {
      pageSizes: () => [10, 20, 50, 100],
    },
  );

  const emit = defineEmits<{
    (e: 'update:page', v: number): void;
    (e: 'update:pageSize', v: number): void;
  }>();

  const pageSizeOptions = computed(() => props.pageSizes);
  const pageCount = computed(() => Math.max(1, Math.ceil(props.total / props.pageSize)));

  const goToPage = ref(props.page);

  watch(
    () => props.page,
    (v) => (goToPage.value = v),
  );

  const onPageChange = (v: number) => emit('update:page', v);

  const onPageSizeChange = (v: number) => {
    emit('update:pageSize', v);
    // 切换页大小时回到第 1 页
    emit('update:page', 1);
    goToPage.value = 1;
  };

  const onGotoPage = () => {
    const target = Math.min(Math.max(1, goToPage.value), pageCount.value);
    emit('update:page', target);
    goToPage.value = target;
  };
</script>

<style scoped>
  .table-pagination {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 16px;
    padding: 12px 0 4px 0;
    color: #606266;
    font-size: 13px;
  }

  .total-count {
    color: #909399;
  }

  .page-size-control {
    display: inline-flex;
    align-items: center;
  }

  .goto-label {
    color: #606266;
    font-size: 13px;
    margin: 0 4px;
  }

  .goto-input :deep(.n-input-number) {
    width: 56px;
  }

  .goto-btn {
    margin-left: 4px;
  }
</style>
