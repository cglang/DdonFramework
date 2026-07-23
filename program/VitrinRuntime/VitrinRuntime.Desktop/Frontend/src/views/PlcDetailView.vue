<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { plcManager, plcData, type PlcConfig, type DbGroupInfo } from '../api/plcApi'
import TagTable from '../components/TagTable.vue'

const props = defineProps<{
  name: string
}>()

const router = useRouter()

const plcName = props.name
const plc = ref<PlcConfig | null>(null)
const groups = ref<DbGroupInfo[]>([])
const loading = ref(false)
const activeGroup = ref<string>('')
const groupDialogVisible = ref(false)
const renameDialogVisible = ref(false)
const renameTargetId = ref('')
const renameTargetName = ref('')
const groupForm = reactive({
  groupName: '',
  dbNumber: 1,
})

const tagTableRef = ref<InstanceType<typeof TagTable> | null>(null)

async function loadPlc() {
  try {
    const plcs = await plcManager.listPlcs()
    const target = plcName
    let found: PlcConfig | null = null
    for (let i = 0; i < plcs.length; i++) {
      const p = plcs[i]
      if (p && p.name === target) {
        found = p
        break
      }
    }
    plc.value = found
    if (!plc.value) {
      ElMessage.warning(`PLC "${target}" 未找到，共 ${plcs.length} 个设备`)
      router.push('/plc/list')
    }
  } catch (e: any) {
    ElMessage.error('获取 PLC 信息失败: ' + (e.message || e))
  }
}

async function loadGroups() {
  loading.value = true
  try {
    groups.value = await plcData.listDbGroups(plcName)
    if (groups.value.length > 0 && !activeGroup.value) {
      activeGroup.value = groups.value[0].id
    }
  } catch (e: any) {
    ElMessage.error('获取分组列表失败: ' + (e.message || e))
  } finally {
    loading.value = false
  }
}

async function handleCreateGroup() {
  if (!groupForm.groupName || !groupForm.dbNumber) {
    ElMessage.warning('请填写分组名称和 DB 块号')
    return
  }
  try {
    await plcData.createDbGroup(plcName, groupForm.groupName, groupForm.dbNumber)
    ElMessage.success('分组创建成功')
    groupDialogVisible.value = false
    Object.assign(groupForm, { groupName: '', dbNumber: 1 })
    activeGroup.value = ''
    await loadGroups()
  } catch (e: any) {
    ElMessage.error('创建分组失败: ' + (e.message || e))
  }
}

async function handleDeleteGroup(groupId: string, groupName: string) {
  try {
    await ElMessageBox.confirm(`确定删除分组 "${groupName}" 及其所有点位吗？`, '确认', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await plcData.deleteDbGroup(groupId)
    ElMessage.success('分组已删除')
    if (activeGroup.value === groupId) {
      activeGroup.value = ''
    }
    await loadGroups()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败: ' + (e.message || e))
  }
}

function startRename(groupId: string, currentName: string) {
  renameTargetId.value = groupId
  renameTargetName.value = currentName
  renameDialogVisible.value = true
}

async function handleRename() {
  if (!renameTargetName.value.trim()) {
    ElMessage.warning('分组名称不能为空')
    return
  }
  try {
    await plcData.renameDbGroup(renameTargetId.value, renameTargetName.value.trim())
    ElMessage.success('分组已重命名')
    renameDialogVisible.value = false
    await loadGroups()
  } catch (e: any) {
    ElMessage.error('重命名失败: ' + (e.message || e))
  }
}

async function handleConnect() {
  if (!plc.value) return
  try {
    await plcManager.connectPlc(plc.value.name)
    ElMessage.success('PLC 已连接')
    await loadPlc()
  } catch (e: any) {
    ElMessage.error('连接失败: ' + (e.message || e))
  }
}

async function handleDisconnect() {
  if (!plc.value) return
  try {
    await plcManager.disconnectPlc(plc.value.name)
    ElMessage.success('PLC 已断开')
    await loadPlc()
  } catch (e: any) {
    ElMessage.error('断开失败: ' + (e.message || e))
  }
}

function handleGroupTabClick(groupId: string) {
  activeGroup.value = groupId
}

function refreshCurrentGroup() {
  if (tagTableRef.value) {
    tagTableRef.value.refresh()
  }
}

onMounted(() => {
  loadPlc()
  loadGroups()
})
</script>

<template>
  <div style="height: 100%; display: flex; flex-direction: column">
    <!-- 顶部信息栏 -->
    <el-row align="middle" style="padding: 12px 20px; border-bottom: 1px solid var(--el-border-color-light); background: #fff; flex-shrink: 0">
      <el-button text @click="router.push('/plc/list')">
        <el-icon><ArrowLeft /></el-icon> 返回列表
      </el-button>
      <template v-if="plc">
        <span style="font-size: 18px; font-weight: 600; color: #303133; margin: 0 12px">{{ plc.name }}</span>
        <el-tag :type="plc.isConnected ? 'success' : 'danger'" size="small" effect="dark">
          {{ plc.isConnected ? '已连接' : '未连接' }}
        </el-tag>
        <span style="font-size: 13px; color: #606266; margin-left: 12px">{{ plc.ip }}:{{ plc.port }}</span>
        <span style="font-size: 13px; color: #909399; margin-left: 8px">Rack {{ plc.rack }} / Slot {{ plc.slot }}</span>
        <div style="margin-left: auto">
          <el-button v-if="!plc.isConnected" type="success" size="small" @click="handleConnect">连接</el-button>
          <el-button v-if="plc.isConnected" type="warning" size="small" @click="handleDisconnect">断开</el-button>
        </div>
      </template>
    </el-row>

    <!-- 主体区域：左侧分组列表 + 右侧点位表格 -->
    <el-container style="flex: 1; overflow: hidden" v-loading="loading">
      <el-aside width="260px" style="border-right: 1px solid var(--el-border-color-light); background: var(--el-fill-color-light); display: flex; flex-direction: column">
        <el-row justify="space-between" align="middle" style="padding: 12px 16px; border-bottom: 1px solid var(--el-border-color-light); flex-shrink: 0">
          <span style="font-size: 14px; font-weight: 600">DB 块分组</span>
          <el-button type="primary" size="small" @click="groupDialogVisible = true">
            <el-icon><Plus /></el-icon> 新建
          </el-button>
        </el-row>
        <div style="flex: 1; overflow-y: auto; padding: 8px">
          <div
            v-for="group in groups"
            :key="group.id"
            class="group-item"
            :class="{ active: activeGroup === group.id }"
            @click="handleGroupTabClick(group.id)"
          >
            <div>
              <div style="font-size: 14px; font-weight: 500; color: #303133">{{ group.name }}</div>
              <div style="font-size: 12px; color: #909399">DB{{ group.dbNumber }}</div>
            </div>
            <div style="display: flex; align-items: center; gap: 4px">
              <el-tag size="small" type="info" effect="plain">{{ group.tagCount }}</el-tag>
              <div class="item-actions" @click.stop>
                <el-button text size="small" @click="startRename(group.id, group.name)">
                  <el-icon><Edit /></el-icon>
                </el-button>
                <el-button text size="small" type="danger" @click="handleDeleteGroup(group.id, group.name)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
          <el-empty v-if="groups.length === 0" description="暂无分组" :image-size="60" />
        </div>
      </el-aside>
      <el-main style="padding: 16px 20px; background: #fff">
        <TagTable
          v-if="activeGroup"
          :key="activeGroup"
          ref="tagTableRef"
          :group-id="activeGroup"
          @refresh="loadGroups"
        />
        <el-empty v-else description="请选择一个 DB 分组" :image-size="80" />
      </el-main>
    </el-container>

    <!-- 新建分组对话框 -->
    <el-dialog v-model="groupDialogVisible" title="新建 DB 分组" width="400px">
      <el-form :model="groupForm" label-width="100px">
        <el-form-item label="分组名称" required>
          <el-input v-model="groupForm.groupName" placeholder="如: 温度数据" />
        </el-form-item>
        <el-form-item label="DB 块号" required>
          <el-input-number v-model="groupForm.dbNumber" :min="1" :max="65535" style="width: 100%" />
          <el-text type="info" size="small" style="margin-top: 4px; display: block">西门子 DB 数据块编号</el-text>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="groupDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreateGroup">确定创建</el-button>
      </template>
    </el-dialog>

    <!-- 重命名对话框 -->
    <el-dialog v-model="renameDialogVisible" title="重命名分组" width="350px">
      <el-input v-model="renameTargetName" placeholder="请输入新名称" />
      <template #footer>
        <el-button @click="renameDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleRename">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.group-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
  border-radius: 6px;
  cursor: pointer;
  margin-bottom: 4px;
  border: 1px solid transparent;
  transition: background 0.2s;
}
.group-item:hover {
  background: #ecf5ff;
}
.group-item.active {
  background: #e6f1ff;
  border-color: #b3d8ff;
}
.item-actions {
  display: flex;
  gap: 2px;
  opacity: 0;
  transition: opacity 0.2s;
}
.group-item:hover .item-actions {
  opacity: 1;
}
</style>
