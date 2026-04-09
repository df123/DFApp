<template>
  <div class="lottery-data-fetch-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">彩票数据获取测试</span>
        </div>
      </template>

      <!-- 快速操作区域 -->
      <div class="quick-actions">
        <el-row :gutter="20">
          <el-col :span="6">
            <el-button
              type="primary"
              :loading="ssqLoading"
              style="width: 100%"
              @click="fetchSSQLatest"
            >
              获取双色球最新数据
            </el-button>
          </el-col>
          <el-col :span="6">
            <el-button
              type="success"
              :loading="kl8Loading"
              style="width: 100%"
              @click="fetchKL8Latest"
            >
              获取快乐8最新数据
            </el-button>
          </el-col>
          <el-col :span="6">
            <el-button
              type="warning"
              :loading="ssqTestLoading"
              style="width: 100%"
              @click="testSSQConnection"
            >
              测试双色球API连接
            </el-button>
          </el-col>
          <el-col :span="6">
            <el-button
              type="info"
              :loading="kl8TestLoading"
              style="width: 100%"
              @click="testKL8Connection"
            >
              测试快乐8API连接
            </el-button>
          </el-col>
        </el-row>
      </div>

      <!-- 自定义查询表单 -->
      <el-divider content-position="left">自定义查询</el-divider>
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="120px"
        class="custom-form"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="彩票类型" prop="lotteryType">
              <el-select
                v-model="formData.lotteryType"
                placeholder="请选择彩票类型"
              >
                <el-option label="双色球" value="ssq" />
                <el-option label="快乐8" value="kl8" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="页码" prop="pageNo">
              <el-input-number
                v-model="formData.pageNo"
                :min="1"
                :max="100"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="开始日期" prop="dayStart">
              <el-date-picker
                v-model="formData.dayStart"
                type="date"
                placeholder="选择开始日期"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="结束日期" prop="dayEnd">
              <el-date-picker
                v-model="formData.dayEnd"
                type="date"
                placeholder="选择结束日期"
                format="YYYY-MM-DD"
                value-format="YYYY-MM-DD"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="24">
            <el-form-item label="保存到数据库">
              <el-switch v-model="formData.saveToDatabase" />
              <span class="form-tip">开启后将自动保存获取到的数据到数据库</span>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="24" style="text-align: center">
            <el-button
              type="primary"
              :loading="customLoading"
              style="width: 200px"
              @click="fetchCustomData"
            >
              开始获取数据
            </el-button>
          </el-col>
        </el-row>
      </el-form>

      <!-- 结果展示区域 -->
      <el-divider content-position="left">获取结果</el-divider>
      <div class="result-container">
        <el-table
          v-loading="resultLoading"
          :data="resultList"
          style="width: 100%"
          stripe
        >
          <el-table-column prop="time" label="时间" width="180" />
          <el-table-column prop="type" label="操作类型" width="120" />
          <el-table-column prop="lotteryType" label="彩票类型" width="100" />
          <el-table-column prop="success" label="是否成功" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.success ? 'success' : 'danger'">
                {{ scope.row.success ? "成功" : "失败" }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="message" label="消息" min-width="200" />
          <el-table-column prop="dataCount" label="数据条数" width="100" />
          <el-table-column prop="savedCount" label="保存条数" width="100" />
          <el-table-column
            prop="responseTime"
            label="响应时间(ms)"
            width="120"
          />
          <el-table-column prop="statusCode" label="HTTP状态码" width="120" />
          <el-table-column label="操作" width="120">
            <template #default="scope">
              <el-button
                size="small"
                type="primary"
                link
                @click="showDetail(scope.row)"
              >
                查看详情
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-card>

    <!-- 详情对话框 -->
    <el-dialog
      v-model="detailDialogVisible"
      title="请求详情"
      width="80%"
      :close-on-click-modal="false"
    >
      <div class="detail-content">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="请求时间">
            {{ currentDetail.time }}
          </el-descriptions-item>
          <el-descriptions-item label="操作类型">
            {{ currentDetail.type }}
          </el-descriptions-item>
          <el-descriptions-item label="彩票类型">
            {{ currentDetail.lotteryType }}
          </el-descriptions-item>
          <el-descriptions-item label="是否成功">
            <el-tag :type="currentDetail.success ? 'success' : 'danger'">
              {{ currentDetail.success ? "成功" : "失败" }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="消息">
            {{ currentDetail.message }}
          </el-descriptions-item>
          <el-descriptions-item label="数据条数">
            {{ currentDetail.dataCount }}
          </el-descriptions-item>
          <el-descriptions-item label="保存条数">
            {{ currentDetail.savedCount }}
          </el-descriptions-item>
          <el-descriptions-item label="响应时间">
            {{ currentDetail.responseTime }}ms
          </el-descriptions-item>
          <el-descriptions-item label="HTTP状态码">
            {{ currentDetail.statusCode }}
          </el-descriptions-item>
          <el-descriptions-item label="请求URL" :span="2">
            <el-input
              v-model="currentDetail.requestUrl"
              readonly
              type="textarea"
              :rows="2"
            />
          </el-descriptions-item>
        </el-descriptions>

        <div v-if="currentDetail.data" class="data-preview">
          <h4>数据预览</h4>
          <el-input
            v-model="currentDetail.dataPreview"
            readonly
            type="textarea"
            :rows="10"
          />
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, type FormInstance, type FormRules } from "element-plus";
import { lotteryDataFetchApi } from "@/api/lotteryDataFetch";
import type {
  LotteryDataFetchRequestDto,
  LotteryDataFetchResponseDto
} from "@/api/lotteryDataFetch";

// 表单相关
const formRef = ref<FormInstance>();
const formData = reactive<LotteryDataFetchRequestDto>({
  dayStart: "",
  dayEnd: "",
  pageNo: 1,
  lotteryType: "ssq",
  saveToDatabase: false
});

const formRules: FormRules = {
  lotteryType: [
    { required: true, message: "请选择彩票类型", trigger: "change" }
  ],
  dayStart: [{ required: true, message: "请选择开始日期", trigger: "change" }],
  dayEnd: [{ required: true, message: "请选择结束日期", trigger: "change" }]
};

// 加载状态
const ssqLoading = ref(false);
const kl8Loading = ref(false);
const ssqTestLoading = ref(false);
const kl8TestLoading = ref(false);
const customLoading = ref(false);
const resultLoading = ref(false);

// 结果数据
const resultList = ref<any[]>([]);

// 详情对话框
const detailDialogVisible = ref(false);
const currentDetail = ref<any>({});

// 获取双色球最新数据
const fetchSSQLatest = async () => {
  ssqLoading.value = true;
  try {
    const result = await lotteryDataFetchApi.fetchSSQLatestData();
    addResult("获取双色球最新数据", "ssq", result);
    ElMessage.success("双色球最新数据获取完成");
  } catch (error) {
    console.error("获取双色球最新数据失败:", error);
    ElMessage.error("获取双色球最新数据失败");
  } finally {
    ssqLoading.value = false;
  }
};

// 获取快乐8最新数据
const fetchKL8Latest = async () => {
  kl8Loading.value = true;
  try {
    const result = await lotteryDataFetchApi.fetchKL8LatestData();
    addResult("获取快乐8最新数据", "kl8", result);
    ElMessage.success("快乐8最新数据获取完成");
  } catch (error) {
    console.error("获取快乐8最新数据失败:", error);
    ElMessage.error("获取快乐8最新数据失败");
  } finally {
    kl8Loading.value = false;
  }
};

// 测试双色球API连接
const testSSQConnection = async () => {
  ssqTestLoading.value = true;
  try {
    const result = await lotteryDataFetchApi.testLotteryApiConnection("ssq");
    addResult("测试双色球API连接", "ssq", result);
    ElMessage.success("双色球API连接测试完成");
  } catch (error) {
    console.error("测试双色球API连接失败:", error);
    ElMessage.error("测试双色球API连接失败");
  } finally {
    ssqTestLoading.value = false;
  }
};

// 测试快乐8API连接
const testKL8Connection = async () => {
  kl8TestLoading.value = true;
  try {
    const result = await lotteryDataFetchApi.testLotteryApiConnection("kl8");
    addResult("测试快乐8API连接", "kl8", result);
    ElMessage.success("快乐8API连接测试完成");
  } catch (error) {
    console.error("测试快乐8API连接失败:", error);
    ElMessage.error("测试快乐8API连接失败");
  } finally {
    kl8TestLoading.value = false;
  }
};

// 自定义数据获取
const fetchCustomData = async () => {
  if (!formRef.value) return;

  const valid = await formRef.value.validate();
  if (!valid) return;

  customLoading.value = true;
  try {
    const result = await lotteryDataFetchApi.fetchLotteryData(formData);
    addResult("自定义数据获取", formData.lotteryType, result);
    ElMessage.success("自定义数据获取完成");
  } catch (error) {
    console.error("自定义数据获取失败:", error);
    ElMessage.error("自定义数据获取失败");
  } finally {
    customLoading.value = false;
  }
};

// 添加结果到列表
const addResult = (
  type: string,
  lotteryType: string,
  result: LotteryDataFetchResponseDto
) => {
  const resultItem = {
    time: new Date().toLocaleString(),
    type,
    lotteryType,
    success: result.success,
    message: result.message,
    dataCount: result.data?.total || 0,
    savedCount: result.savedCount,
    responseTime: result.responseTime,
    statusCode: result.statusCode,
    requestUrl: result.requestUrl,
    data: result.data,
    dataPreview: result.data
      ? JSON.stringify(result.data, null, 2).substring(0, 1000) + "..."
      : ""
  };

  resultList.value.unshift(resultItem);

  // 限制结果列表长度
  if (resultList.value.length > 50) {
    resultList.value = resultList.value.slice(0, 50);
  }
};

// 显示详情
const showDetail = (row: any) => {
  currentDetail.value = { ...row };
  detailDialogVisible.value = true;
};

// 生命周期
onMounted(() => {
  // 设置默认日期
  const today = new Date();
  const yesterday = new Date(today);
  yesterday.setDate(yesterday.getDate() - 1);

  formData.dayStart = yesterday.toISOString().split("T")[0];
  formData.dayEnd = today.toISOString().split("T")[0];
});
</script>

<style scoped>
.lottery-data-fetch-container {
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

.quick-actions {
  margin-bottom: 20px;
}

.custom-form {
  margin-bottom: 20px;
}

.form-tip {
  margin-left: 10px;
  font-size: 12px;
  color: #909399;
}

.result-container {
  margin-top: 20px;
}

.detail-content {
  max-height: 70vh;
  overflow-y: auto;
}

.data-preview {
  margin-top: 20px;
}

.data-preview h4 {
  margin-bottom: 10px;
  color: #303133;
}
</style>
