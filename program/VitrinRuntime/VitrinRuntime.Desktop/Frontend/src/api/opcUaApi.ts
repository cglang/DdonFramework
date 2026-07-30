/** 桥接 API - 调用后端 OpcUaServer BridgeService */

function invoke<T>(method: string, payload?: unknown): Promise<T> {
  return window.ui.invoke<T>(method, payload)
}

// ── 类型定义 ──────────────────────────────────

export interface ServerStatus {
  isRunning: boolean
  endpointUrl: string
  serverName: string
  startedAt: string
  sessionCount: number
}

export interface NodeInfo {
  nodePath: string
  displayName: string
  nodeClass: string
  dataType: string
  hasChildren: boolean
}

export interface NodeDetail {
  nodePath: string
  displayName: string
  nodeClass: string
  dataType: string
  value?: string
  sourceType?: string
  plcName?: string
  tagName?: string
}

export interface EventLogEntry {
  time: string
  message: string
}

// ── OpcUaServer API ───────────────────────────

export const opcUaServer = {
  // Server 控制
  getServerStatus: () => invoke<ServerStatus>('OpcUaServer.GetServerStatus'),

  startServer: () => invoke<void>('OpcUaServer.StartServer'),

  stopServer: () => invoke<void>('OpcUaServer.StopServer'),

  restartServer: () => invoke<void>('OpcUaServer.RestartServer'),

  // 地址空间浏览
  browseChildren: (nodePath?: string) =>
    invoke<NodeInfo[]>('OpcUaServer.BrowseChildren', nodePath ?? ''),

  getNodeDetail: (nodePath: string) =>
    invoke<NodeDetail | null>('OpcUaServer.GetNodeDetail', nodePath),

  // 节点值操作
  readNodeValue: (nodePath: string) =>
    invoke<unknown>('OpcUaServer.ReadNodeValue', nodePath),

  writeNodeValue: (nodePath: string, value: unknown) =>
    invoke<void>('OpcUaServer.WriteNodeValue', { nodePath, value }),
}
