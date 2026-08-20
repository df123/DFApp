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
