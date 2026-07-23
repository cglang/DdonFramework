<script setup lang="ts">
import { ref, watch, reactive, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { plcData, type TagInfo } from '../api/plcApi'

const props = defineProps<{
  groupId: string
}>()

const emit = defineEmits<{
  refresh: []
}>()

const tags = ref<TagInfo[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const editForm = reactive({
  tagName: '',
  address: '',
  dataType: 'Int32',
  stringLength: 0,
})
const editingValue = ref<{ [key: string]: unknown }>({})
const writingTags = ref<Set<string>>(new Set())

// WebView 模式下接收的点位变化事件数据类型
interface TagValueChangedData {
  tagName: string
  address: string
  dataType: string
  oldValue: unknown
  newValue: unknown
}

// 编辑点位状态
const editDialogVisible = ref(false)
const editingTagId = ref('')
const editFormState = reactive({
  tagName: '',
  address: '',
  dataType: 'Int32',
  stringLength: 0,
})

// 定时刷新 loading 状态（用于刷新按钮）
const refreshing = ref(false)

const dataTypeOptions = [
  { value: 'Bool', label: 'Bool (位)' },
  { value: 'Byte', label: 'Byte (字节)' },
  { value: 'Int16', label: 'Int16 (短整型)' },
  { value: 'UInt16', label: 'UInt16 (无符号短整型)' },
  { value: 'Int32', label: 'Int32 (整型)' },
  { value: 'UInt32', label: 'UInt32 (无符号整型)' },
  { value: 'Float', label: 'Float (浮点)' },
  { value: 'Double', label: 'Double (双精度)' },
  { value: 'String', label: 'String (字符串)' },
]

async function loadTags() {
  if (!props.groupId) return
  loading.value = true
  try {
    tags.value = await plcData.listTags(props.groupId)
    // 仅初始化新点位的修改值，不覆盖用户正在编辑的值
    const currentIds = new Set(tags.value.map(t => t.id))
    for (const tag of tags.value) {
      if (!(tag.id in editingValue.value)) {
        editingValue.value[tag.id] = tag.value
      }
    }
    // 清理已删除点位的编辑值
    for (const id of Object.keys(editingValue.value)) {
      if (!currentIds.has(id)) {
        delete editingValue.value[id]
      }
    }
  } catch (e: any) {
    ElMessage.error('获取点位列表失败: ' + (e.message || e))
  } finally {
    loading.value = false
  }
}

async function handleRefresh() {
  refreshing.value = true
  await loadTags()
  refreshing.value = false
}

async function handleAddTag() {
  if (!editForm.tagName || !editForm.address) {
    ElMessage.warning('请填写点位名称和地址')
    return
  }
  try {
    await plcData.addTag(props.groupId, editForm.tagName, editForm.address, editForm.dataType, editForm.stringLength)
    ElMessage.success('点位添加成功')
    dialogVisible.value = false
    Object.assign(editForm, { tagName: '', address: '', dataType: 'Int32', stringLength: 0 })
    await loadTags()
    emit('refresh')
  } catch (e: any) {
    ElMessage.error('添加点位失败: ' + (e.message || e))
  }
}

async function handleRemoveTag(tagId: string, tagName: string) {
  try {
    await ElMessageBox.confirm(`确定移除点位 "${tagName}" 吗？`, '确认', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await plcData.removeTag(tagId)
    ElMessage.success('点位已移除')
    await loadTags()
    emit('refresh')
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('移除失败: ' + (e.message || e))
  }
}

async function handleEditTag(tag: TagInfo) {
  editingTagId.value = tag.id
  editFormState.tagName = tag.name
  editFormState.address = tag.address
  editFormState.dataType = tag.dataType
  editFormState.stringLength = 0
  editDialogVisible.value = true
}

async function handleSaveEdit() {
  if (!editFormState.tagName || !editFormState.address) {
    ElMessage.warning('请填写点位名称和地址')
    return
  }
  try {
    await plcData.updateTag(editingTagId.value, editFormState.tagName, editFormState.address, editFormState.dataType, editFormState.stringLength)
    ElMessage.success('点位更新成功')
    editDialogVisible.value = false
    await loadTags()
    emit('refresh')
  } catch (e: any) {
    ElMessage.error('更新点位失败: ' + (e.message || e))
  }
}

async function handleWriteTag(tag: TagInfo) {
  writingTags.value.add(tag.id)
  try {
    const value = editingValue.value[tag.id]
    const result = await plcData.writeTag(tag.id, value)
    if (result.success) {
      ElMessage.success(`写入成功${result.needConfirmByScan ? ' (等待扫描确认)' : ''}`)
    } else {
      ElMessage.error('写入失败: ' + (result.error || '未知错误'))
    }
  } catch (e: any) {
    ElMessage.error('写入失败: ' + (e.message || e))
  } finally {
    writingTags.value.delete(tag.id)
  }
}

function getValueDisplay(tag: TagInfo): string {
  if (tag.value === null || tag.value === undefined) return '-'
  return String(tag.value)
}

// 监听 groupId 变化
watch(() => props.groupId, () => {
  loadTags()
}, { immediate: true })

onMounted(() => {
  // WebView 模式下订阅后端点位变化事件，精确更新对应点位值
  if (window.ui && typeof window.ui.on === 'function') {
    window.ui.on<TagValueChangedData>('TagValueChanged', (data) => {
      // 使用 for 循环避免 WebView 下 Array.find 兼容性问题
      for (let i = 0; i < tags.value.length; i++) {
        if (tags.value[i].name === data.tagName) {
          tags.value[i].value = data.newValue
          break
        }
      }
    })
  }
})

onUnmounted(() => {
  if (window.ui && typeof window.ui.off === 'function') {
    window.ui.off('TagValueChanged')
  }
})

defineExpose({ refresh: loadTags })
</script>

<template>
  <div style="height: 100%">
    <el-row justify="space-between" align="middle" style="margin-bottom: 12px">
      <span style="font-size: 15px; font-weight: 600; color: #303133">点位列表 ({{ tags.length }})</span>
      <el-space>
        <el-button :loading="refreshing" size="small" @click="handleRefresh">
          <el-icon><Refresh /></el-icon> 刷新
        </el-button>
        <el-button type="primary" size="small" @click="dialogVisible = true">
          <el-icon><Plus /></el-icon> 添加点位
        </el-button>
      </el-space>
    </el-row>

    <el-table v-loading="loading" :data="tags" border stripe size="small" max-height="500" style="width: 100%">
      <el-table-column prop="name" label="名称" min-width="120" show-overflow-tooltip />
      <el-table-column prop="address" label="地址" min-width="120" />
      <el-table-column prop="dataType" label="类型" width="110" />
      <el-table-column label="当前值" width="180">
        <template #default="{ row }: { row: TagInfo }">
          <el-tag v-if="row.dataType === 'Bool'" :type="row.value ? 'success' : 'info'" size="small">
            {{ row.value ? 'TRUE' : 'FALSE' }}
          </el-tag>
          <span v-else>{{ getValueDisplay(row) }}</span>
        </template>
      </el-table-column>
      <el-table-column label="修改值" width="200">
        <template #default="{ row }: { row: TagInfo }">
          <el-switch
            v-if="row.dataType === 'Bool'"
            v-model="editingValue[row.id]"
            size="small"
            active-text="ON"
            inactive-text="OFF"
          />
          <el-input-number
            v-else-if="['Int16', 'UInt16', 'Int32', 'UInt32'].includes(row.dataType)"
            v-model="editingValue[row.id]"
            size="small"
            controls-position="right"
          />
          <el-input-number
            v-else-if="['Float', 'Double'].includes(row.dataType)"
            v-model="editingValue[row.id]"
            size="small"
            :precision="row.dataType === 'Float' ? 4 : 8"
            controls-position="right"
          />
          <el-input
            v-else-if="row.dataType === 'String'"
            v-model="editingValue[row.id]"
            size="small"
          />
          <el-input-number
            v-else
            v-model="editingValue[row.id]"
            size="small"
            controls-position="right"
          />
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }: { row: TagInfo }">
          <el-button
            type="primary"
            size="small"
            :loading="writingTags.has(row.id)"
            @click="handleWriteTag(row)"
          >写入</el-button>
          <el-button size="small" @click="handleEditTag(row)">编辑</el-button>
          <el-button type="danger" size="small" @click="handleRemoveTag(row.id, row.name)">移除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!loading && tags.length === 0" description="暂无点位，请点击上方按钮添加" />

    <!-- 添加点位对话框 -->
    <el-dialog v-model="dialogVisible" title="添加点位" width="450px">
      <el-form :model="editForm" label-width="100px">
        <el-form-item label="名称" required>
          <el-input v-model="editForm.tagName" placeholder="如: Temperature" />
        </el-form-item>
        <el-form-item label="地址" required>
          <el-input v-model="editForm.address" placeholder="如: DB1.DBD0" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">西门子格式: DB1.DBD0 / DB1.DBX10.5 / DB1.DBW4</el-text>
        </el-form-item>
        <el-form-item label="数据类型">
          <el-select v-model="editForm.dataType" style="width: 100%">
            <el-option
              v-for="opt in dataTypeOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item v-if="editForm.dataType === 'String'" label="字符串长度">
          <el-input-number v-model="editForm.stringLength" :min="1" :max="1024" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleAddTag">确定添加</el-button>
      </template>
    </el-dialog>

    <!-- 编辑点位对话框 -->
    <el-dialog v-model="editDialogVisible" title="编辑点位" width="450px">
      <el-form :model="editFormState" label-width="100px">
        <el-form-item label="名称" required>
          <el-input v-model="editFormState.tagName" placeholder="如: Temperature" />
        </el-form-item>
        <el-form-item label="地址" required>
          <el-input v-model="editFormState.address" placeholder="如: DB1.DBD0" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">西门子格式: DB1.DBD0 / DB1.DBX10.5 / DB1.DBW4</el-text>
        </el-form-item>
        <el-form-item label="数据类型">
          <el-select v-model="editFormState.dataType" style="width: 100%">
            <el-option
              v-for="opt in dataTypeOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>
        </el-form-item>
        <el-form-item v-if="editFormState.dataType === 'String'" label="字符串长度">
          <el-input-number v-model="editFormState.stringLength" :min="1" :max="1024" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveEdit">保存修改</el-button>
      </template>
    </el-dialog>
  </div>
</template>
