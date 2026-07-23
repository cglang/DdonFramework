# AI 约束规则 — Ddon.Desktop 前端

## 技术栈

- 框架：Vue 3 + TypeScript
- 构建：Vite
- UI 库：Element Plus
- 路由：Vue Router
- 桥接：src/bridge.ts（createBridge）

## 编码约束

### UI 样式

- 优先使用 Element Plus 组件（el-button, el-table, el-form 等），不要手写 HTML 标签 + CSS
- 布局使用 el-row / el-col，不要手写 flex / grid
- 对话框使用 el-dialog，不要手写模态框
- 表单使用 el-form + el-form-item + el-input / el-select 等，不要手写 form + input + CSS
- 通知使用 ElMessage / ElNotification，不要自定义提示
- 表格使用 el-table，不要手写 table 标签 + CSS
- 标签页使用 el-tabs，按钮组使用 el-button-group
- 分页使用 el-pagination

### CSS 限制

- **禁止**在 `<style>` 块中写自定义 CSS（scoped 也不行）
- Element Plus 组件自带样式已足够
- 需要微调时，使用 Element Plus 的 `:style` 属性绑定或行内 style
- 颜色、字体、间距等使用 Element Plus 的 CSS 变量（--el-color-primary 等）

### Element Plus 导入规范

- 全局注册，在 src/main.ts 中统一 `import ElementPlus from 'element-plus'`
- 样式全局导入：`import 'element-plus/dist/index.css'`
- 图标使用 `@element-plus/icons-vue`

### 组件开发规范

- 每个 `.vue` 文件只导出一个组件
- `<script setup lang="ts">` 语法
- props 使用 `defineProps` + 类型标注
- emit 使用 `defineEmits`  + 类型标注
- 不要在组件内写 `<style>` 标签
- 模板中不要写内联事件处理逻辑，全部抽到 `<script setup>` 中

### 通信规范

- 所有后端调用通过 `window.ui.invoke<T>(method, payload)` 完成
- 事件订阅通过 `window.ui.on<T>(eventName, handler)` 完成
- 通信类型在 bridge.ts 中定义

### 文件组织

```
src/
├── main.ts                     # 入口：注册 Element Plus、Router
├── App.vue                     # 根组件
├── bridge.ts                   # 桥接封装（不变动）
├── router/index.ts             # 路由定义
├── views/                      # 页面组件
│   ├── LoginView.vue
│   └── DashboardView.vue
└── components/                 # 可复用组件
```

## 禁止事项

- ❌ 不要手写 `<table>`、`<form>`、`<dialog>` 等基础标签
- ❌ 不要在 `.vue` 文件中写 `<style>` 块
- ❌ 不要使用 Tailwind CSS、Bootstrap 等其他 UI 库
- ❌ 不要修改 bridge.ts 的通信机制
- ❌ 不要直接操作 DOM（ref + 组件 API 除外）
- ❌ 不要使用 any 类型（使用 unknown 替代）
