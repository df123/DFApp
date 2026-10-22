import axios from 'axios'
import { ref } from 'vue'

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
  syncMissedOnStartup: boolean
}

/** 密码回显掩码：提交该值表示保留原密码 */
export const PASSWORD_MASK = '********'

/** 当前认证状态（缓存会话检查结果，登录/登出后更新） */
const authenticated = ref(false)
const authChecked = ref(false)

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
})

// 会话失效时跳转登录页（整页跳转以重置应用状态）
api.interceptors.response.use(
  response => response,
  async error => {
    const status = error?.response?.status
    const url: string = error?.config?.url ?? ''
    if (status === 401 && !url.includes('/auth/login')) {
      authenticated.value = false
      const redirect = window.location.pathname + window.location.search
      if (window.location.pathname !== '/login') {
        window.location.href = `/login?redirect=${encodeURIComponent(redirect)}`
      }
    }
    return Promise.reject(error)
  }
)

/** 检查会话（结果缓存，登出或 401 后重置） */
export async function checkAuth(): Promise<boolean> {
  if (!authChecked.value) {
    try {
      await api.get('/auth/session')
      authenticated.value = true
    } catch {
      authenticated.value = false
    }
    authChecked.value = true
  }
  return authenticated.value
}

/** 登录成功/登出后由页面更新认证状态 */
export function setAuthenticated(value: boolean) {
  authenticated.value = value
  authChecked.value = true
}

export function isAuthenticated(): boolean {
  return authenticated.value
}

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

export const authApi = {
  login: (username: string, password: string) =>
    api.post<{ username: string }>('/auth/login', { username, password }),

  logout: () =>
    api.post('/auth/logout'),

  changePassword: (oldPassword: string, newPassword: string) =>
    api.post<{ message: string }>('/auth/change-password', { oldPassword, newPassword }),
}

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
