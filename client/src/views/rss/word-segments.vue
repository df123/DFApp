<template>
  <div ref="containerRef" class="word-segments-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">RSS 分词统计</span>
        </div>
      </template>

      <!-- 筛选条件 -->
      <el-form :model="queryParams" :inline="true" class="filter-form">
        <el-form-item label="关键词">
          <el-input
            v-model="queryParams.filter"
            placeholder="搜索分词"
            clearable
            style="width: 200px"
            @clear="handleQuery"
            @keyup.enter="handleQuery"
          />
        </el-form-item>
        <el-form-item label="语言类型">
          <el-select
            v-model="queryParams.languageType"
            placeholder="全部"
            clearable
            style="width: 120px"
            @change="handleQuery"
          >
            <el-option label="中文" :value="0" />
            <el-option label="英文" :value="1" />
            <el-option label="日文" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleQuery">搜索</el-button>
          <el-button @click="handleReset">重置</el-button>
        </el-form-item>
      </el-form>

      <!-- 筛选条件提示 -->
      <el-alert
        v-if="filterDescription"
        :title="filterDescription"
        :type="filterDescriptionType"
        :closable="false"
        show-icon
        class="filter-alert"
      />

      <!-- 表格区域 - 左右布局 -->
      <el-row :gutter="20" class="tables-row">
        <!-- 左侧：统计表格 -->
        <el-col :span="10">
          <div class="table-panel">
            <div class="panel-header">全部分词统计</div>
            <el-table
              v-loading="statisticsLoading"
              :data="statisticsData"
              :height="tableHeight - 80"
              stripe
              :default-sort="{ prop: 'totalCount', order: 'descending' }"
            >
              <el-table-column type="index" label="排名" width="60" />
              <el-table-column
                prop="word"
                label="分词"
                min-width="100"
                show-overflow-tooltip
              />
              <el-table-column
                prop="languageType"
                label="语言"
                width="60"
                align="center"
              >
                <template #default="scope">
                  <el-tag
                    :type="
                      scope.row.languageType === 0
                        ? 'danger'
                        : scope.row.languageType === 1
                          ? 'success'
                          : 'warning'
                    "
                    size="small"
                  >
                    {{
                      scope.row.languageType === 0
                        ? "中"
                        : scope.row.languageType === 1
                          ? "英"
                          : "日"
                    }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column
                prop="totalCount"
                label="总次数"
                width="100"
                sortable
              />
              <el-table-column
                prop="itemCount"
                label="条目数"
                width="100"
                sortable
              />
              <el-table-column label="操作" width="70" fixed="right">
                <template #default="scope">
                  <el-button
                    size="small"
                    type="primary"
                    link
                    @click="viewItems(scope.row.word)"
                  >
                    查看
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <!-- 统计表格分页 -->
            <div class="pagination-container">
              <el-pagination
                v-model:current-page="statisticsPagination.page"
                v-model:page-size="statisticsPagination.pageSize"
                :page-sizes="[20, 50, 100, 200]"
                :total="statisticsPagination.total"
                layout="total, sizes, prev, pager, next"
                @size-change="handleStatisticsSizeChange"
                @current-change="handleStatisticsPageChange"
              />
            </div>
          </div>
        </el-col>

        <!-- 右侧：详情列表 -->
        <el-col :span="14">
          <div class="table-panel">
            <div class="panel-header">分词详情列表</div>
            <el-table
              v-loading="tableLoading"
              :data="tableData"
              :height="tableHeight - 80"
              stripe
              @sort-change="handleSortChange"
            >
              <el-table-column
                prop="word"
                label="分词"
                width="100"
                show-overflow-tooltip
              />
              <el-table-column
                prop="languageType"
                label="语言"
                width="60"
                align="center"
              >
                <template #default="scope">
                  <el-tag
                    :type="
                      scope.row.languageType === 0
                        ? 'danger'
                        : scope.row.languageType === 1
                          ? 'success'
                          : 'warning'
                    "
                    size="small"
                  >
                    {{
                      scope.row.languageType === 0
                        ? "中"
                        : scope.row.languageType === 1
                          ? "英"
                          : "日"
                    }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column
                prop="rssSourceName"
                label="RSS源"
                width="110"
                show-overflow-tooltip
              />
              <el-table-column
                prop="rssMirrorItemTitle"
                label="条目标题"
                min-width="200"
                show-overflow-tooltip
              />
              <el-table-column prop="count" label="次数" width="70" />
              <el-table-column
                prop="creationTime"
                label="创建时间"
                width="120"
                sortable="custom"
              >
                <template #default="scope">
                  {{
                    new Date(scope.row.creationTime).toLocaleDateString("zh-CN")
                  }}
                </template>
              </el-table-column>
              <el-table-column label="操作" width="180" fixed="right">
                <template #default="scope">
                  <el-button
                    size="small"
                    type="primary"
                    link
                    @click="openLink(scope.row.rssMirrorItemLink)"
                  >
                    打开
                  </el-button>
                  <el-button
                    size="small"
                    type="success"
                    link
                    @click="downloadToAria2(scope.row.rssMirrorItemLink)"
                  >
                    下载
                  </el-button>
                  <el-button
                    size="small"
                    type="info"
                    link
                    @click="viewWord(scope.row.word)"
                  >
                    查看
                  </el-button>
                </template>
              </el-table-column>
            </el-table>

            <!-- 分页 -->
            <div class="pagination-container">
              <el-pagination
                v-model:current-page="pagination.page"
                v-model:page-size="pagination.pageSize"
                :page-sizes="[10, 20, 50, 100]"
                :total="pagination.total"
                layout="total, sizes, prev, pager, next"
                @size-change="handleSizeChange"
                @current-change="handlePageChange"
              />
            </div>
          </div>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from "vue";
import { ElMessage } from "element-plus";
import { rssWordSegmentApi } from "@/api/rssWordSegment";
import { aria2Api } from "@/api/aria2";
import type {
  RssWordSegmentWithItemDto,
  GetRssWordSegmentsRequestDto,
  WordSegmentStatisticsDto
} from "@/types/business";

// 查询参数
const queryParams = reactive<GetRssWordSegmentsRequestDto>({
  filter: "",
  languageType: undefined,
  pageIndex: 1,
  pageSize: 20,
  sorting: ""
});

// 下载选项
const videoOnly = ref(true);
const enableKeywordFilter = ref(true);

// 筛选条件提示
const filterDescription = ref("");
const filterDescriptionType = ref<"info" | "success" | "warning" | "error">(
  "info"
);

// 分页信息
const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0
});

// 表格数据
const tableData = ref<RssWordSegmentWithItemDto[]>([]);
const tableLoading = ref(false);

// 统计数据
const statisticsData = ref<WordSegmentStatisticsDto[]>([]);
const statisticsLoading = ref(false);

// 统计分页信息
const statisticsPagination = reactive({
  page: 1,
  pageSize: 50,
  total: 0
});

// 容器引用
const containerRef = ref<HTMLElement>();

// 动态计算表格高度
const tableHeight = ref(500);

// 计算表格高度
const calculateTableHeight = () => {
  if (!containerRef.value) return;

  const containerHeight = containerRef.value.clientHeight;
  // 减去卡片内边距、header、表单等固定部分的高度
  // 卡片内边距: 20px * 2 = 40px
  // Card header: 约 60px
  // 表单: 约 52px
  // 栅格间距: 20px
  // 面板header: 约 40px
  // 分页: 约 52px
  // 底部间距: 20px

  const fixedHeight = 40 + 60 + 52 + 20 + 40 + 52 + 20;
  tableHeight.value = Math.max(300, containerHeight - fixedHeight);
};

// 监听窗口大小变化
const handleResize = () => {
  calculateTableHeight();
};

// 加载统计数据
const loadStatistics = async () => {
  statisticsLoading.value = true;
  try {
    const input: GetRssWordSegmentsRequestDto = {
      pageIndex: statisticsPagination.page,
      pageSize: statisticsPagination.pageSize,
      filter: queryParams.filter,
      languageType: queryParams.languageType,
      sorting: "totalCount desc"
    };

    const result = await rssWordSegmentApi.getStatistics(input);
    statisticsData.value = result.items;
    statisticsPagination.total = result.totalCount;
    updateFilterDescription();
  } catch (error) {
    console.error("加载统计数据失败:", error);
    ElMessage.error("加载统计数据失败");
  } finally {
    statisticsLoading.value = false;
  }
};

// 加载表格数据
const loadTableData = async () => {
  tableLoading.value = true;
  try {
    queryParams.pageIndex = pagination.page;
    queryParams.pageSize = pagination.pageSize;

    const result = await rssWordSegmentApi.getList(queryParams);
    tableData.value = result.items;
    pagination.total = result.totalCount;
    updateFilterDescription();
  } catch (error) {
    console.error("加载数据失败:", error);
    ElMessage.error("加载数据失败");
  } finally {
    tableLoading.value = false;
  }
};

// 更新筛选提示
const updateFilterDescription = () => {
  if (queryParams.word) {
    filterDescription.value = `📌 正在查看分词："${queryParams.word}"（精确匹配，共 ${pagination.total} 条记录）`;
    filterDescriptionType.value = "success";
  } else if (queryParams.filter) {
    const langText =
      queryParams.languageType != null
        ? `，语言：${queryParams.languageType === 0 ? "中文" : queryParams.languageType === 1 ? "英文" : "日文"}`
        : "";
    filterDescription.value = `🔍 正在搜索包含"${queryParams.filter}"的分词${langText}（左侧统计 ${statisticsPagination.total} 个，右侧详情 ${pagination.total} 条）`;
    filterDescriptionType.value = "info";
  } else if (queryParams.languageType != null) {
    const langText =
      queryParams.languageType === 0
        ? "中文"
        : queryParams.languageType === 1
          ? "英文"
          : "日文";
    filterDescription.value = `🌐 筛选语言：${langText}（左侧统计 ${statisticsPagination.total} 个，右侧详情 ${pagination.total} 条）`;
    filterDescriptionType.value = "info";
  } else {
    filterDescription.value = `📊 显示全部分词（左侧统计 ${statisticsPagination.total} 个，右侧详情 ${pagination.total} 条）`;
    filterDescriptionType.value = "info";
  }
};

// 查询
const handleQuery = () => {
  pagination.page = 1;
  statisticsPagination.page = 1;
  queryParams.word = ""; // 清空精确筛选
  loadTableData();
  loadStatistics();
  updateFilterDescription();
};

// 重置
const handleReset = () => {
  queryParams.filter = "";
  queryParams.languageType = undefined;
  queryParams.word = "";
  queryParams.sorting = "";
  pagination.page = 1;
  statisticsPagination.page = 1;
  loadTableData();
  loadStatistics();
  updateFilterDescription();
};

// 统计表格分页变化
const handleStatisticsPageChange = (page: number) => {
  statisticsPagination.page = page;
  loadStatistics();
};

const handleStatisticsSizeChange = (size: number) => {
  statisticsPagination.pageSize = size;
  statisticsPagination.page = 1;
  loadStatistics();
};

// 详情表格分页变化
const handlePageChange = (page: number) => {
  pagination.page = page;
  loadTableData();
  updateFilterDescription();
};

const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.page = 1;
  loadTableData();
  updateFilterDescription();
};

// 排序变化
const handleSortChange = (sortData: any) => {
  if (sortData.prop) {
    const order = sortData.order === "ascending" ? "" : "desc";
    queryParams.sorting = `${sortData.prop} ${order}`;
  } else {
    queryParams.sorting = "";
  }
  loadTableData();
};

// 查看包含该词的条目
const viewItems = (word: string) => {
  queryParams.word = word;
  queryParams.filter = "";
  pagination.page = 1;
  loadTableData();
  updateFilterDescription();
};

// 查看分词
const viewWord = (word: string) => {
  queryParams.word = word;
  queryParams.filter = "";
  pagination.page = 1;
  loadTableData();
  updateFilterDescription();
};

// 打开链接
const openLink = (url?: string) => {
  if (url) {
    window.open(url, "_blank");
  }
};

// 下载到Aria2
const downloadToAria2 = async (link: string) => {
  if (!link) {
    ElMessage.warning("链接地址为空");
    return;
  }

  try {
    const request = {
      urls: [link],
      videoOnly: videoOnly.value,
      enableKeywordFilter: enableKeywordFilter.value
    };
    await aria2Api.addDownload(request);
    ElMessage.success("已添加到下载队列");
  } catch (error) {
    console.error("添加到下载队列失败:", error);
    ElMessage.error("添加到下载队列失败");
  }
};

// 生命周期
onMounted(() => {
  calculateTableHeight();
  window.addEventListener("resize", handleResize);
  loadStatistics();
  loadTableData();
  updateFilterDescription();
});

onUnmounted(() => {
  window.removeEventListener("resize", handleResize);
});
</script>

<style scoped>
.word-segments-container {
  height: 100%;
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.filter-form {
  margin-bottom: 16px;
}

.filter-alert {
  margin-bottom: 16px;
}

.tables-row {
  margin-top: 10px;
}

.table-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.panel-header {
  padding-bottom: 8px;
  margin-bottom: 12px;
  font-size: 14px;
  font-weight: 600;
  color: #303133;
  border-bottom: 1px solid #ebeef5;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}
</style>
