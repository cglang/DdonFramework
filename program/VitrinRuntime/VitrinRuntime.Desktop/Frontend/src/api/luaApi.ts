/** 桥接 API - 调用后端 [BridgeService] (LuaEngine) */

function invoke<T>(method: string, payload?: unknown): Promise<T> {
  return window.ui.invoke<T>(method, payload)
}

// ── 类型定义 ──────────────────────────────────

export interface LuaGroupInfo {
  name: string
  path: string
  scriptCount: number
  watcherEnabled: boolean
  vmLoaded: boolean
  hasScripts: boolean
}

export interface LuaScriptInfo {
  fileName: string
  filePath: string
  isLoaded: boolean
  lastWriteTime: string
}

export interface LuaGroupDetail {
  name: string
  path: string
  scripts: LuaScriptInfo[]
}

export interface ExecuteLuaResult {
  success: boolean
  result?: string
  error?: string
}

export interface WatcherStatus {
  enabled: boolean
}

// ── LuaEngine API ─────────────────────────────

export const luaEngine = {
  listGroups: () => invoke<LuaGroupInfo[]>('LuaEngine.ListGroups'),

  loadGroup: (directoryPath: string, groupName?: string) =>
    invoke<LuaGroupInfo>('LuaEngine.LoadGroup', { directoryPath, groupName }),

  reloadGroup: (groupName: string) =>
    invoke<{ success: boolean }>('LuaEngine.ReloadGroup', { groupName }),

  unloadGroup: (groupName: string) =>
    invoke<{ success: boolean }>('LuaEngine.UnloadGroup', { groupName }),

  getGroupDetail: (groupName: string) =>
    invoke<LuaGroupDetail | null>('LuaEngine.GetGroupDetail', { groupName }),

  reloadScript: (groupName: string, fileName: string) =>
    invoke<{ success: boolean }>('LuaEngine.ReloadScript', { groupName, fileName }),

  unloadScript: (groupName: string, fileName: string) =>
    invoke<{ success: boolean }>('LuaEngine.UnloadScript', { groupName, fileName }),

  getWatcherStatus: () =>
    invoke<WatcherStatus>('LuaEngine.GetWatcherStatus'),

  setWatcher: (enabled: boolean) =>
    invoke<WatcherStatus>('LuaEngine.SetWatcher', { enabled }),

  listVms: () =>
    invoke<{ groupName: string; hasVm: boolean }[]>('LuaEngine.ListVms'),

  executeLua: (groupName: string, code: string) =>
    invoke<ExecuteLuaResult>('LuaEngine.ExecuteLua', { groupName, code }),
}
