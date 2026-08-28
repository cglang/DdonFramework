<script setup lang="ts">
  import type { FormInst } from 'naive-ui';
  import { ref } from 'vue';
  import type { DataTableColumns } from 'naive-ui';
  import { NButton, useMessage } from 'naive-ui';
  import { h } from 'vue';

  const formRef = ref<FormInst | null>(null);
  const message = useMessage();
  const size = ref<'small' | 'medium' | 'large'>('medium');
  const formValue = ref({
    user: {
      name: '',
      age: '',
    },
    phone: '',
  });

  const rules = {
    user: {
      name: {
        required: true,
        message: '请输入姓名',
        trigger: 'blur',
      },
      age: {
        required: true,
        message: '请输入年龄',
        trigger: ['input', 'blur'],
      },
    },
    phone: {
      required: true,
      message: '请输入电话号码',
      trigger: ['input'],
    },
  };

  function handleValidateClick(e: MouseEvent) {
    e.preventDefault();
    formRef.value?.validate((errors) => {
      if (!errors) {
        message.success('Valid');
      } else {
        console.log(errors);
        message.error('Invalid');
      }
    });
  }

  interface Song {
    no: number;
    title: string;
    length: string;
  }

  function createColumns({ play }: { play: (row: Song) => void }): DataTableColumns<Song> {
    return [
      {
        title: 'No',
        key: 'no',
      },
      {
        title: 'Title',
        key: 'title',
      },
      {
        title: 'Length',
        key: 'length',
      },
      {
        title: 'Action',
        key: 'actions',
        render(row) {
          return h(
            NButton,
            {
              strong: true,
              tertiary: true,
              size: 'small',
              onClick: () => play(row),
            },
            { default: () => 'Play' },
          );
        },
      },
    ];
  }

  const data: Song[] = [
    { no: 3, title: 'Wonderwall', length: '4:18' },
    { no: 4, title: "Don't Look Back in Anger", length: '4:48' },
    { no: 12, title: 'Champagne Supernova', length: '7:27' },
  ];

  const columns = createColumns({
    play(row: Song) {
      message.info(`Play ${row.title}`);
    },
  });
  const pagination = false as const;
</script>

<template>
  <n-card class="page-card">
    <n-space>
      <n-form ref="formRef" inline :label-width="80" :model="formValue" :rules="rules" :size="size" label-placement="left">
        <n-form-item label="姓名" path="user.name">
          <n-input v-model:value="formValue.user.name" placeholder="输入姓名" />
        </n-form-item>
        <n-form-item label="年龄" path="user.age">
          <n-input v-model:value="formValue.user.age" placeholder="输入年龄" />
        </n-form-item>
        <n-form-item label="电话号码" path="phone">
          <n-input v-model:value="formValue.phone" placeholder="电话号码" />
        </n-form-item>
        <n-form-item>
          <n-button attr-type="button" @click="handleValidateClick">验证</n-button>
        </n-form-item>
      </n-form>
    </n-space>
    <n-space>
      <n-button>Default</n-button>
      <n-button type="tertiary">Tertiary</n-button>
      <n-button type="primary">Primary</n-button>
      <n-button type="info">Info</n-button>
      <n-button type="success">Success</n-button>
      <n-button type="warning">Warning</n-button>
      <n-button type="error">Error</n-button>
    </n-space>
  </n-card>
  <n-card class="page-card">
    <n-data-table :columns="columns" :data="data" :pagination="pagination" :bordered="false" />
  </n-card>
  <n-card class="page-card">
    <n-data-table :columns="columns" :data="data" :pagination="pagination" :bordered="false" />
  </n-card>
  <n-card class="page-card">
    <n-data-table :columns="columns" :data="data" :pagination="pagination" :bordered="false" />
  </n-card>
  <n-card class="page-card">
    <n-data-table :columns="columns" :data="data" :pagination="pagination" :bordered="false" />
  </n-card>
  <n-card class="page-card">
    <n-data-table :columns="columns" :data="data" :pagination="pagination" :bordered="false" />
  </n-card>
</template>
