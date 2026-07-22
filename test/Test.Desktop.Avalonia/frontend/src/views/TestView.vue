<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";

const router = useRouter();

async function test() {
  alert(123);
}
</script>

<template>
  <div>
    <div style="margin-top: 8px">
      <el-button type="primary" style="width: 100%" @click="test()"> 测试 </el-button>
    </div>
  </div>
</template>
