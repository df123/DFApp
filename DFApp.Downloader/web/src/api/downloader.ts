import axios from 'axios'

export interface DownloadItem {
  id: number
  sourceType: string
  sourceId: number
  fileName: string
  fileSize: number
  downloadUrl: string
  localPath: string
  status: string
  downloadedBytes: number
  speedBytesPerSecond?: number
  mimeType: string
  chatTitle?: string
  errorMessage?: string
  retryCount?: number
  createdAt: string
  updatedAt: string
  completedAt?: string
}

export interface DownloaderSettings {
  dfAppUrl: string
  dfAppUsername: string
  dfAppPassword: string
  apacheUsername: string
  apachePassword: string
  downloadPath: string
  maxConcurrentDownloads: number
  maxSegmentsPerFile: number
  segmentSize: number
  webServerPort: number
  webServerBind: string
  managementToken: string
  autoStart: boolean
}

/** 管理令牌的本地存储键（非回环访问时需携带 X-Management-Token） */
const MANAGEMENT_TOKEN_KEY = 'downloaderManagementToken'

/** 密码回显掩码：提交该值表示保留原密码 */
export const PASSWORD_MASK = '********'

export function getManagementToken(): string {
  return localStorage.getItem(MANAGEMENT_TOKEN_KEY) ?? ''
}

export function setManagementToken(token: string) {
  localStorage.setItem(MANAGEMENT_TOKEN_KEY, token)
}

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
})

// 所有请求携带管理令牌（本机访问可留空）
api.interceptors.request.use(config => {
  const token = getManagementToken()
  if (token) {
    config.headers['X-Management-Token'] = token
  }
  return config
})

// 401 时提示输入令牌，保存后引导手动重试（避免自动重试造成循环）
api.interceptors.response.use(
  response => response,
  async error => {
    if (error?.response?.status === 401 && !error.config?.__tokenPrompted) {
      error.config.__tokenPrompted = true
      const { ElMessageBox } = await import('element-plus')
      try {
        const { value } = await ElMessageBox.prompt(
          '访问被拒绝：本界面来自非本机地址，需要管理令牌（settings.json 中的 ManagementToken）',
          '管理令牌',
          { confirmButtonText: '保存并刷新', cancelButtonText: '取消', inputType: 'password' }
        )
        if (value) {
          setManagementToken(value.trim())
          window.location.reload()
        }
      } catch {
        /* 用户取消 */
      }
    }
    return Promise.reject(error)
  }
)

export interface GlobalStatus {
  isConnected: boolean
  activeDownloads: number
  pending: number
  downloading: number
  completed: number
  failed: number
  totalSpeedBytesPerSecond: number
  lastError?: string | null
  totalDownloadedBytes: number
  videoCount: number
}

export interface ConnectionInfo {
  isConnected: boolean
  lastError?: string | null
}

export interface SpeedHistoryPoint {
  time: string
  avgSpeed: number
  maxSpeed: number
}

export interface SpeedHistory {
  range: string
  bucketSeconds: number
  items: SpeedHistoryPoint[]
}

export type SpeedHistoryRange = '1h' | '6h' | '24h' | '7d' | '30d'

export interface LogFileInfo {
  fileName: string
  sizeBytes: number
  lastWriteTime: string
}

export interface LogContent {
  fileName: string
  content: string
  returnedLines: number
  totalLines: number
  order: 'tail' | 'head'
}

export type LogOrder = 'tail' | 'head'

export const downloadApi = {
  getList: (page = 1, pageSize = 20, status?: string) =>
    api.get<{ items: DownloadItem[]; total: number }>('/downloads', { params: { page, pageSize, status } }),

  getDetail: (id: number) =>
    api.get<DownloadItem>(`/downloads/${id}`),

  getActive: () =>
    api.get<DownloadItem[]>('/downloads/active'),

  getQueue: () =>
    api.get<DownloadItem[]>('/downloads/queue'),

  pause: (id: number) =>
    api.post(`/downloads/${id}/pause`),

  resume: (id: number) =>
    api.post(`/downloads/${id}/resume`),

  cancel: (id: number) =>
    api.delete(`/downloads/${id}`),

  deleteFailed: () =>
    api.delete<{ deletedCount: number }>('/downloads/failed'),

  getSettings: () =>
    api.get<DownloaderSettings>('/settings'),

  updateSettings: (settings: DownloaderSettings) =>
    api.put<DownloaderSettings>('/settings', settings),

  getStatus: () =>
    api.get<GlobalStatus>('/status'),

  getSpeedHistory: (range: SpeedHistoryRange = '24h') =>
    api.get<SpeedHistory>('/speed-history', { params: { range } }),

  getConnection: () =>
    api.get<ConnectionInfo>('/connection'),

  reconnect: () =>
    api.post<ConnectionInfo>('/connection/reconnect'),

  syncMissed: () =>
    api.post<{ scanned: number; added: number; reconciled: number }>('/downloads/sync-missed'),

  getGallery: (page = 1, pageSize = 60) =>
    api.get<{ items: any[]; total: number }>('/gallery', { params: { page, pageSize } }),

  playWithVlc: (id: number) =>
    api.post<{ message: string; path: string }>(`/gallery/${id}/play`),

  backfillMessages: () =>
    api.post<{ updated: number }>('/gallery/backfill-messages'),

  getLogList: () =>
    api.get<{ items: LogFileInfo[] }>('/logs'),

  getLogContent: (fileName: string, lines?: number, order?: LogOrder) =>
    api.get<LogContent>(`/logs/${encodeURIComponent(fileName)}`, { params: { lines, order } }),
}

export default api
