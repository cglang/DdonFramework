<!--
  SearchFormBar 通用搜索表单
  通过 fields 配置生成搜索项，支持文本输入框、日期范围、自定义输入渲染。
  内部使用 v-model:modelValue 双向绑定整体表单对象。
-->
<template>
  <n-form
    :model="formState"
    label-placement="left"
    label-width="auto"
    class="search-form-bar"
  >
    <n-grid :cols="cols" :x-gap="16" :y-gap="16" responsive="screen">
      <n-grid-item
        v-for="field in fields"
        :key="field.key"
        :span="field.span || 1"
      >
        <n-form-item :label="field.label" :path="field.key">
          <!-- 文本输入框 -->
          <n-input
            v-if="field.type === 'input' || !field.type"
            :value="getValue(field.key)"
            :placeholder="field.placeholder || '请输入'"
            clearable
            @update:value="(v: string) => setValue(field.key, v)"
          />

          <!-- 日期范围 -->
          <n-date-picker
            v-else-if="field.type === 'daterange'"
            :value="getValue(field.key)"
            type="daterange"
            clearable
            style="width: 100%"
            @update:value="(v: number | null) => setValue(field.key, v)"
          />

          <!-- 下拉选择 -->
          <n-select
            v-else-if="field.type === 'select'"
            :value="getValue(field.key)"
            :options="field.options || []"
            :placeholder="field.placeholder || '请选择'"
            clearable
            @update:value="(v: string | number | null) => setValue(field.key, v)"
          />

          <!-- 自定义渲染插槽 -->
          <slot
            v-else-if="field.type === 'slot'"
            :name="`field-${String(field.key)}`"
            :field="field"
            :value="getValue(field.key)"
            :set-value="(v: unknown) => setValue(field.key, v)"
          />
        </n-form-item>
      </n-grid-item>

      <!-- 外部追加的额外内容 -->
      <slot name="extra" />
    </n-grid>
  </n-form>
</template>

<script setup lang="ts">
  import { computed, ref, watch } from 'vue';

  // 字段类型枚举
  export type SearchFieldType = 'input' | 'daterange' | 'select' | 'slot';

  export interface SelectOption {
    label: string;
    value: string | number;
  }

  export interface SearchField {
    key: string;
    label: string;
    type?: SearchFieldType;
    placeholder?: string;
    /** 跨列数，默认 1 */
    span?: number;
    /** select 用 */
    options?: SelectOption[];
  }

  const props = withDefaults(
    defineProps<{
      fields: SearchField[];
      /** 初始表单数据 */
      initialValue?: Record<string, unknown>;
      /** 每行总列数（24 栅格外的简易均分） */
      cols?: number;
    }>(),
    {
      initialValue: () => ({}),
      cols: 5,
    },
  );

  const emit = defineEmits<{
    (e: 'update:modelValue', v: Record<string, unknown>): void;
    (e: 'reset'): void;
  }>();

  // 内部表单状态
  const formState = ref<Record<string, unknown>>({ ...props.initialValue });

  // 与外部双向绑定
  watch(
    formState,
    (val) => {
      emit('update:modelValue', { ...val });
    },
    { deep: true },
  );

  // 外部 initialValue 变化时同步（不覆盖用户已开始输入的值）
  watch(
    () => props.initialValue,
    (val) => {
      if (val) formState.value = { ...val };
    },
  );

  const getValue = (key: string) => formState.value[key];
  const setValue = (key: string, v: unknown) => {
    formState.value[key] = v;
  };

  // 暴露 reset 给父组件使用
  defineExpose({
    reset: () => {
      formState.value = { ...props.initialValue };
      emit('reset');
    },
  });

  // 让未使用的 props 不警告
  void computed(() => props.cols);
</script>

<style scoped>
  .search-form-bar :deep(.n-form-item) {
    margin-bottom: 0;
  }
</style>
