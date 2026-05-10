import type { PagedRequestDto } from "./api";

// Aria2 模块类型

// Aria2 状态响应 (基于实际 Schema)
export interface TellStatusResultDto {
  id: number;
  gid: string;
  status: string;
  totalLength: string;
  completedLength: string;
  uploadLength: string;
  downloadSpeed: string;
  uploadSpeed: string;
  errorCode: string;
  errorMessage: string;
  creationTime?: string;
  files: Array<{
    path: string;
    length: string;
    completedLength: string;
  }>;
}

// 添加下载请求
export interface AddDownloadRequestDto {
  urls: string[];
  savePath?: string;
  options?: Record<string, any>;
  videoOnly?: boolean;
  enableKeywordFilter?: boolean;
}

// 添加下载响应
export interface AddDownloadResponseDto {
  id: string;
}

// 批量添加 URI 下载请求（每条链接创建独立任务）
export interface BatchAddUriRequestDto {
  urls: string[];
  savePath?: string;
  options?: Record<string, any>;
  videoOnly?: boolean;
  enableKeywordFilter?: boolean;
}

// 种子文件项
export interface TorrentFileItemDto {
  torrentData: string;
  fileName: string;
}

// 添加种子文件下载请求
export interface AddTorrentRequestDto {
  torrentData: string;
  savePath?: string;
  options?: Record<string, any>;
}

// 批量添加种子文件下载请求
export interface BatchAddTorrentRequestDto {
  torrents: TorrentFileItemDto[];
  savePath?: string;
}

// Aria2 管理相关类型

// Aria2 全局状态
export interface Aria2GlobalStatDto {
  downloadSpeed: string;
  uploadSpeed: string;
  activeCount: string;
  waitingCount: string;
  stoppedCount: string;
  stoppedTotal: string;
}

// Aria2 任务信息
export interface Aria2TaskDto {
  gid: string;
  status: string;
  totalLength: number;
  completedLength: number;
  downloadSpeed: number;
  uploadSpeed: number;
  errorCode?: string;
  errorMessage?: string;
  files?: Aria2FileDto[];
  dir?: string;
  connections?: number;
  shareRatio?: number;
  uploadedLength?: number;
  btName?: string;
  peers?: Aria2PeerDto[];
}

// Aria2 Peer信息（BitTorrent对等连接）
export interface Aria2PeerDto {
  peerId: string;
  ip: string;
  port: number;
  client?: string;
  amChoking: boolean;
  peerChoking: boolean;
  downloadSpeed: number;
  uploadSpeed: number;
  progress: number;
  seeder: boolean;
  country?: string; // 前端添加的国家字段
  city?: string; // 前端添加的城市字段
}

// Aria2 任务详情（包含完整信息）
export interface Aria2TaskDetailDto {
  gid: string;
  status: string;
  btName?: string;
  totalLength: number;
  completedLength: number;
  uploadedLength: number;
  shareRatio: number;
  downloadSpeed: number;
  uploadSpeed: number;
  dir?: string;
  files: Aria2FileDto[];
  peers: Aria2PeerDto[];
  connections?: number;
}

// Aria2 文件信息
export interface Aria2FileDto {
  index: string;
  path: string;
  length: number;
  completedLength: number;
  selected: boolean;
  uris?: Aria2UriDto[];
}

// Aria2 URI 信息
export interface Aria2UriDto {
  uri: string;
  status: string;
}

// 暂停任务请求
export interface PauseTasksRequestDto {
  gids: string[];
}

// 停止任务请求
export interface StopTasksRequestDto {
  gids: string[];
}

// 删除任务请求
export interface RemoveTasksRequestDto {
  gids: string[];
}

// Aria2 连接状态
export interface Aria2ConnectionStatusDto {
  isConnected: boolean;
  version?: string;
  sessionInfo?: string;
  errorMessage?: string;
}

// IP 地理位置 DTO
export interface IpGeolocationDto {
  status: string;
  query: string;
  country?: string;
  countryCode?: string;
  city?: string;
  message?: string;
}

// 外部链接响应
export interface ExternalLinkResponseDto {
  link: string;
}

// 记账模块类型

// 记账分类 DTO
export interface BookkeepingCategoryDto {
  id: number;
  category: string;
}

export interface CreateUpdateBookkeepingCategoryDto {
  category: string;
}

// 记账支出 DTO
export interface BookkeepingExpenditureDto {
  id: number;
  expenditureDate: string; // ISO 8601 格式
  expenditure: number; // double 类型
  remark: string;
  isBelongToSelf: boolean;
  category: BookkeepingCategoryDto;
  categoryId: number;
}

export interface CreateUpdateBookkeepingExpenditureDto {
  expenditureDate: string;
  expenditure: number;
  remark?: string;
  isBelongToSelf: boolean;
  categoryId: number;
}

// 支出查询参数
export interface GetExpendituresRequestDto extends PagedRequestDto {
  filter?: string;
  categoryId?: number;
  isBelongToSelf?: boolean;
}

// 图表数据类型 (基于实际 Schema)
export interface ChartJSDto {
  labels: string[];
  datasets: Array<{
    label: string;
    data: number[];
  }>;
  total: number;
  compareTotal: number;
  differenceTotal: number;
}

// 图表查询参数
export interface ChartDataRequestDto {
  start?: string; // ISO 8601 日期时间
  end?: string;
  compareType?: CompareType; // 枚举: 0,1,2,3
  numberType?: NumberType; // 枚举: 0,1
}

// 月度支出 DTO
export interface MonthlyExpenditureDto {
  labels: string[];
  totalData: number[];
  selfData: number[];
  nonSelfData: number[];
  totalAverage: number;
  selfAverage: number;
  nonSelfAverage: number;
}

// 枚举类型
export enum CompareType {
  None = 0,
  PreviousPeriod = 1,
  SamePeriodLastYear = 2,
  Custom = 3
}

export enum NumberType {
  Amount = 0,
  Count = 1
}

// 配置管理类型
export interface ConfigurationInfoDto {
  id: number;
  moduleName: string;
  configurationName: string;
  configurationValue: string;
  remark: string;
}

export interface CreateUpdateConfigurationInfoDto {
  moduleName: string;
  configurationName: string;
  configurationValue: string;
  remark?: string;
}

// 动态IP类型
export interface DynamicIPDto {
  id: string; // UUID 格式
  ip: string;
  port: string;
}

export interface CreateUpdateDynamicIPDto {
  ip: string;
  port: string;
}

// 文件上传类型
export interface FileUploadInfoDto {
  id: number;
  fileName: string;
  path: string;
  sha1: string;
  fileSize: number;
  creationTime?: string;
}

export interface CreateUpdateFileUploadInfoDto {
  fileName: string;
  path: string;
  sha1: string;
  fileSize: number;
}

export interface CustomFileTypeDto {
  configurationName: string;
  configurationValue: string;
}

// 彩票模块类型

// 基础彩票 DTO
export interface LotteryDto {
  id: number;
  indexNo: number;
  number: string;
  colorType: string;
  lotteryType: string;
  groupId: number;
}

export interface CreateUpdateLotteryDto {
  indexNo: number;
  number: string;
  colorType: string;
  lotteryType: string;
  groupId: number;
}

// 彩票组 DTO
export interface LotteryGroupDto {
  id: number;
  indexNo: number;
  lotteryType: string;
  groupId: number;
  redNumbers: string;
  blueNumber: string;
}

// 彩票结果 DTO
export interface LotteryResultDto {
  id: number;
  name: string;
  code: string;
  detailsLink: string;
  videoLink: string;
  date: string;
  week: string;
  red: string;
  blue: string;
  blue2: string;
  sales: string;
  poolMoney: string;
  content: string;
  addMoney: string;
  addMoney2: string;
  msg: string;
  z2Add: string;
  m2Add: string;
  creationTime?: string;
  prizegrades: Array<{
    type: number;
    typeNum: string;
    typeMoney: string;
  }>;
}

export interface CreateUpdateLotteryResultDto {
  name: string;
  code: string;
  detailsLink?: string;
  videoLink?: string;
  date: string;
  week: string;
  red: string;
  blue: string;
  blue2?: string;
  sales?: string;
  poolMoney?: string;
  content?: string;
  addMoney?: string;
  addMoney2?: string;
  msg?: string;
  z2Add?: string;
  m2Add?: string;
  prizegrades?: Array<{
    type: number;
    typeNum: string;
    typeMoney: string;
  }>;
}

// 彩票组合 DTO
export interface LotteryCombinationDto {
  [key: string]: any;
}

// 彩票常量 DTO
export interface ConstsDto {
  lotteryTypeEng: string;
  lotteryType: string;
}

// 彩票模拟相关类型
export interface GenerateRandomNumbersDto {
  count: number;
  gameType: LotteryGameType;
  termNumber: number;
}

export interface LotterySimulationDto {
  id: string;
  termNumber: number;
  redNumbers: string;
  blueNumber?: string;
  ballType: LotteryBallType;
  gameType: LotteryGameType;
  groupId: number;
  creationTime?: string;
}

export interface StatisticsDto {
  terms: number[];
  purchaseAmounts: number[];
  winningAmounts: number[];
  purchaseAmountsByType?: Record<LotteryKL8PlayType, number[]>;
  winningAmountsByType?: Record<LotteryKL8PlayType, number[]>;
}

// KL8 中奖统计 DTO
export interface WinningStatisticsDto {
  totalAmount: number;
  winningDetails?: WinningDetailDto[];
}

// KL8 中奖详情 DTO
export interface WinningDetailDto {
  groupId: number;
  redMatches: number;
  blueMatches: number;
  winningAmount: number;
}

export interface DeleteByTermNumberDto {
  termNumber: number;
}

// 彩票球类型枚举
export enum LotteryBallType {
  Red = 0,
  Blue = 1
}

// 彩票游戏类型枚举
export enum LotteryGameType {
  双色球 = 0,
  快乐8 = 1
}

// KL8 玩法类型枚举
export enum LotteryKL8PlayType {
  Select1 = 1, // 选一
  Select2 = 2, // 选二
  Select3 = 3, // 选三
  Select4 = 4, // 选四
  Select5 = 5, // 选五
  Select6 = 6, // 选六
  Select7 = 7, // 选七
  Select8 = 8, // 选八
  Select9 = 9, // 选九
  Select10 = 10 // 选十
}

// 中奖统计查询参数
export interface StatisticsWinRequestDto extends PagedRequestDto {
  purchasedPeriod?: string;
  winningPeriod?: string;
  lotteryType?: string;
}

export interface StatisticsWinItemDto {
  code: string;
  winCode: string;
  buyLotteryString: string;
  winLotteryString: string;
  winAmount: number;
}

// 彩票统计项查询参数
export interface StatisticsWinItemRequestDto extends PagedRequestDto {
  purchasedPeriod?: string;
  winningPeriod?: string;
  lotteryType?: string;
}

// ExternalLink 模块类型

// 外部链接 DTO
export interface ExternalLinkDto {
  id: number;
  creationTime: string; // ISO 8601 格式
  creatorId?: string; // UUID 格式
  lastModificationTime?: string; // ISO 8601 格式
  lastModifierId?: string; // UUID 格式
  name?: string;
  size?: string;
  timeConsumed?: string;
  isRemove: boolean;
  linkContent?: string;
}

// 创建/更新外部链接 DTO
export interface CreateUpdateExternalLinkDto {
  name?: string;
  size?: number;
  timeConsumed?: string;
  isRemove: boolean;
  linkContent?: string;
  mediaIds?: string;
}

// MediaInfo 模块类型

// 媒体信息 DTO
export interface MediaInfoDto {
  id: number;
  creationTime: string; // ISO 8601 格式
  creatorId?: string; // UUID 格式
  lastModificationTime?: string; // ISO 8601 格式
  lastModifierId?: string; // UUID 格式
  mediaId?: string;
  chatId?: number;
  chatTitle?: string;
  message?: string;
  size?: number;
  savePath?: string;
  mD5?: string;
  mimeType?: string;
  isExternalLinkGenerated: boolean;
  isDownloadCompleted: boolean;
  downloadTimeMs?: number;
  downloadSpeedBps?: number;
}

// 创建/更新媒体信息 DTO
export interface CreateUpdateMediaInfoDto {
  mediaId?: number;
  chatId?: number;
  chatTitle: string;
  message?: string;
  size?: number;
  savePath: string;
  mD5: string;
  mimeType: string;
  isExternalLinkGenerated?: boolean;
}

// 图表数据 DTO
export interface ChartDataDto {
  labels: string[];
  datas: number[];
}

// TGLogin 模块类型

// TG 登录状态响应
export interface TGLoginStatusDto {
  status: string;
}

// TG 配置响应
export interface TGConfigDto {
  config: string;
}

// TG 聊天响应 (403 Forbidden)
export interface TGChatsDto {
  [key: string]: any;
}

// 日志模块类型
export interface LogEntryDto {
  message: string;
  timestamp: string; // ISO 8601 格式
}

export interface GetLogsRequestDto {
  level?: string;
  startTime?: string;
  endTime?: string;
  keyword?: string;
}

// 日志文件信息 DTO
export interface LogFileInfoDto {
  name: string;
  size: number;
  lastModified: string; // ISO 8601 格式
}

// 复式投注相关类型

// 复式投注输入 DTO
export interface CompoundLotteryInputDto {
  Period: number;
  LotteryType: string;
  RedNumbers: string[];
  BlueNumbers: string[];
  KL8Numbers: string[];
  PlayType?: LotteryKL8PlayType;
}

// 复式投注结果 DTO
export interface CompoundLotteryResultDto {
  TotalCombinations: number;
  TotalAmount: number;
  CreatedLotteries: LotteryDto[];
  CombinationDetails: string[];
}

// 文件关键词过滤模块类型

// 匹配模式枚举
export enum MatchMode {
  Contains = 0, // 包含
  StartsWith = 1, // 开头
  EndsWith = 2, // 结尾
  Exact = 3, // 完全匹配
  Regex = 4 // 正则表达式
}

// 过滤类型枚举
export enum FilterType {
  Blacklist = 0, // 黑名单
  Whitelist = 1 // 白名单
}

// 关键词过滤规则DTO
export interface KeywordFilterRuleDto {
  id: number;
  creationTime?: string;
  keyword: string;
  matchMode: MatchMode;
  filterType: FilterType;
  isEnabled: boolean;
  priority: number;
  remark?: string;
  isCaseSensitive: boolean;
}

// 创建/更新关键词过滤规则DTO
export interface CreateUpdateKeywordFilterRuleDto {
  keyword: string;
  matchMode: MatchMode;
  filterType: FilterType;
  isEnabled: boolean;
  priority: number;
  remark?: string;
  isCaseSensitive: boolean;
}

// 关键词过滤匹配结果DTO
export interface KeywordFilterMatchResultDto {
  ruleId: number;
  keyword: string;
  matchMode: MatchMode;
  filterType: FilterType;
  priority: number;
  isCaseSensitive: boolean;
  matchedText?: string;
}

// 关键词过滤测试结果DTO
export interface KeywordFilterTestResultDto {
  fileName: string;
  shouldFilter: boolean;
  matchingRules: KeywordFilterMatchResultDto[];
  reason?: string;
}

// RSS镜像模块类型

// RSS源DTO
export interface RssSourceDto {
  id: number;
  name: string;
  url: string;
  proxyUrl?: string;
  proxyUsername?: string;
  proxyPassword?: string;
  isEnabled: boolean;
  fetchIntervalMinutes: number;
  maxItems: number;
  query?: string;
  lastFetchTime?: string;
  fetchStatus: number;
  errorMessage?: string;
  remark?: string;
  creationTime: string;
}

// 创建/更新RSS源DTO
export interface CreateUpdateRssSourceDto {
  name: string;
  url: string;
  proxyUrl?: string;
  proxyUsername?: string;
  proxyPassword?: string;
  isEnabled: boolean;
  fetchIntervalMinutes: number;
  maxItems: number;
  query?: string;
  remark?: string;
}

// RSS镜像条目DTO
export interface RssMirrorItemDto {
  id: number;
  rssSourceId: number;
  rssSourceName?: string;
  title: string;
  link: string;
  description?: string;
  author?: string;
  category?: string;
  publishDate?: string;
  seeders?: number;
  leechers?: number;
  downloads?: number;
  extensions?: string;
  isDownloaded: boolean;
  downloadTime?: string;
  creationTime: string;
  wordSegments?: RssWordSegmentDto[];
}

// RSS分词DTO
export interface RssWordSegmentDto {
  id?: number;
  rssMirrorItemId: number;
  word: string;
  languageType: number;
  count: number;
  partOfSpeech?: string;
  creationTime: string;
}

// 获取RSS镜像条目请求DTO
export interface GetRssMirrorItemsRequestDto extends PagedRequestDto {
  rssSourceId?: number;
  filter?: string;
  startTime?: string;
  endTime?: string;
  isDownloaded?: boolean;
  wordToken?: string;
}

// 分词统计DTO
export interface WordSegmentStatisticsDto {
  word: string;
  totalCount: number;
  itemCount: number;
  languageType: number;
}

// RSS分词（带镜像条目信息）DTO
export interface RssWordSegmentWithItemDto {
  id: number;
  rssMirrorItemId: number;
  rssMirrorItemTitle?: string;
  rssMirrorItemLink?: string;
  rssSourceId?: number;
  rssSourceName?: string;
  word: string;
  languageType: number;
  count: number;
  partOfSpeech?: string;
  creationTime: string;
}

// 获取RSS分词请求DTO
export interface GetRssWordSegmentsRequestDto extends PagedRequestDto {
  rssSourceId?: number;
  filter?: string;
  languageType?: number;
  word?: string;
}

