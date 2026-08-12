import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
})

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
  createdAt: string
  updatedAt: string
  completedAt?: string
}

export interface DownloaderSettings {
  dfAppUrl: string
  dfAppUsername: string
  dfAppPassword: string
  apacheBaseUrl: string
  apacheUsername: string
  apachePassword: string
  downloadPath: string
  maxConcurrentDownloads: number
  maxSegmentsPerFile: number
  segmentSize: number
  webServerPort: number
  autoStart: boolean
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
}

export interface ConnectionInfo {
  isConnected: boolean
  lastError?: string | null
}

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

  getSettings: () =>
    api.get<DownloaderSettings>('/settings'),

  updateSettings: (settings: DownloaderSettings) =>
    api.put<DownloaderSettings>('/settings', settings),

  getStatus: () =>
    api.get<GlobalStatus>('/status'),

  getConnection: () =>
    api.get<ConnectionInfo>('/connection'),

  reconnect: () =>
    api.post<ConnectionInfo>('/connection/reconnect'),

  syncMissed: () =>
    api.post<{ scanned: number; added: number }>('/downloads/sync-missed'),

  getLogList: () =>
    api.get<{ items: LogFileInfo[] }>('/logs'),

  getLogContent: (fileName: string, lines?: number, order?: LogOrder) =>
    api.get<LogContent>(`/logs/${encodeURIComponent(fileName)}`, { params: { lines, order } }),
}

export default api
