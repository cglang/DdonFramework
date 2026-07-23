/** 桥接 API - 调用后端 [BridgeService] */

function invoke<T>(method: string, payload?: unknown): Promise<T> {
  return window.ui.invoke<T>(method, payload)
}

// ── 类型定义 ──────────────────────────────────

export interface PlcConfig {
  name: string
  ip: string
  port: number
  rack: number
  slot: number
  isConnected: boolean
  createdAt: string
  lastConnectedAt?: string
  errorMessage?: string
}

export interface PlcStatus {
  name: string
  ip: string
  port: number
  isConnected: boolean
  mirrorVersion?: number
  lastUpdateTime?: string
  tagCount?: number
  errorMessage?: string
}

export interface DbGroupInfo {
  id: string
  plcName: string
  name: string
  dbNumber: number
  tagCount: number
  createdAt: string
}

export interface TagInfo {
  id: string
  groupId: string
  name: string
  address: string
  dataType: string
  value: unknown
  createdAt: string
}

export interface AddTagResult {
  id: string
  name: string
  address: string
  dataType: string
}

export interface WriteTagResult {
  success: boolean
  error?: string
  needConfirmByScan: boolean
}

export interface CreateGroupResult {
  id: string
  name: string
  dbNumber: number
  dbSize: number
}

// ── PlcManager API ─────────────────────────────

export const plcManager = {
  listPlcs: () => invoke<PlcConfig[]>('PlcManager.ListPlcs'),

  addPlc: (name: string, ip: string, port = 102, rack = 0, slot = 1) =>
    invoke<PlcConfig>('PlcManager.AddPlc', { name, ip, port, rack, slot }),

  removePlc: (name: string) =>
    invoke<void>('PlcManager.RemovePlc', { name }),

  connectPlc: (name: string) =>
    invoke<void>('PlcManager.ConnectPlc', { name }),

  disconnectPlc: (name: string) =>
    invoke<void>('PlcManager.DisconnectPlc', { name }),

  getPlcStatus: (name: string) =>
    invoke<PlcStatus | null>('PlcManager.GetPlcStatus', { name }),
}

// ── PlcData API ────────────────────────────────

export const plcData = {
  listDbGroups: (plcName: string) =>
    invoke<DbGroupInfo[]>('PlcData.ListDbGroups', { plcName }),

  createDbGroup: (plcName: string, groupName: string, dbNumber: number, dbSize = 4096) =>
    invoke<CreateGroupResult>('PlcData.CreateDbGroup', { plcName, groupName, dbNumber, dbSize }),

  deleteDbGroup: (groupId: string) =>
    invoke<boolean>('PlcData.DeleteDbGroup', { groupId }),

  renameDbGroup: (groupId: string, newName: string) =>
    invoke<boolean>('PlcData.RenameDbGroup', { groupId, newName }),

  listTags: (groupId: string) =>
    invoke<TagInfo[]>('PlcData.ListTags', { groupId }),

  addTag: (groupId: string, tagName: string, address: string, dataType: string, stringLength = 0) =>
    invoke<AddTagResult>('PlcData.AddTag', { groupId, tagName, address, dataType, stringLength }),

  removeTag: (tagId: string) =>
    invoke<boolean>('PlcData.RemoveTag', { tagId }),

  updateTag: (tagId: string, tagName: string, address: string, dataType: string, stringLength = 0) =>
    invoke<AddTagResult>('PlcData.UpdateTag', { tagId, tagName, address, dataType, stringLength }),

  readTag: (tagId: string) =>
    invoke<unknown>('PlcData.ReadTag', { tagId }),

  writeTag: (tagId: string, value: unknown) =>
    invoke<WriteTagResult>('PlcData.WriteTag', { tagId, value }),
}
