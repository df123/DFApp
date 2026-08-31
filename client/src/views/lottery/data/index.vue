<template>
  <div class="lottery-data-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>彩票数据</span>
          <el-button
            v-if="hasPermission('DFApp.Lottery.Create')"
            v-permission="'DFApp.Lottery.Create'"
            type="primary"
            icon="plus"
            @click="handleCreate"
          >
            新增
          </el-button>
        </div>
      </template>

      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
      >
        <el-table-column label="操作" width="100">
          <template #default="scope">
            <el-button
              v-if="hasPermission('DFApp.Lottery.Delete')"
              v-permission="'DFApp.Lottery.Delete'"
              type="danger"
              link
              size="small"
              @click="handleDelete(scope.row)"
            >
              删除
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="indexNo" label="期号" />
        <el-table-column prop="lotteryType" label="彩票类型" />
        <el-table-column prop="redNumbers" label="红球" />
        <el-table-column prop="blueNumber" label="蓝球" />
        <el-table-column prop="groupId" label="组号" />
        <el-table-column prop="creationTime" label="创建时间">
          <template #default="scope">
            {{ formatDateTime(scope.row.creationTime) }}
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </el-card>

    <el-dialog v-model="createDialogVisible" title="新增彩票数据">
      <el-form ref="createFormRef" :model="createForm" :rules="createRules">
        <el-form-item prop="indexNo" label="期号">
          <el-input-number v-model="createForm.indexNo" :min="1" />
        </el-form-item>
        <el-form-item prop="lotteryType" label="彩票类型">
          <el-input v-model="createForm.lotteryType" placeholder="如：双色球" />
        </el-form-item>
        <el-form-item prop="redNumbers" label="红球">
          <el-input
            v-model="createForm.redNumbers"
            placeholder="多个号码用逗号分隔"
          />
        </el-form-item>
        <el-form-item prop="blueNumber" label="蓝球">
          <el-input
            v-model="createForm.blueNumber"
            placeholder="快乐8可留空；双色球填写一个号码"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">取消</el-button>
        <el-button
          type="primary"
          :loading="submitting"
          @click="handleCreateSubmit"
        >
          创建
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { lotteryApi } from "@/api/lottery";
import type { CreateUpdateLotteryDto, LotteryGroupDto } from "@/types/business";

interface LotteryGroupForm {
  indexNo: number;
  lotteryType: string;
  redNumbers: string;
  blueNumber: string;
}

const loading = ref(false);
const submitting = ref(false);
const tableData = ref<LotteryGroupDto[]>([]);
const total = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);

const createDialogVisible = ref(false);
const createFormRef = ref<FormInstance>();
const createForm = reactive<LotteryGroupForm>({
  indexNo: 1,
  lotteryType: "双色球",
  redNumbers: "",
  blueNumber: ""
});

const createRules: FormRules = {
  indexNo: [{ required: true, message: "请输入期号", trigger: "blur" }],
  lotteryType: [{ required: true, message: "请输入彩票类型", trigger: "blur" }],
  redNumbers: [{ required: true, message: "请输入红球", trigger: "blur" }]
};

const hasPermission = (permission: string) => {
  return true;
};

const getLotteryData = async () => {
  loading.value = true;
  try {
    const result = await lotteryApi.getLotteryGroups({
      pageIndex: currentPage.value,
      pageSize: pageSize.value
    });
    tableData.value = result.items;
    total.value = result.totalCount;
  } catch (error) {
    ElMessage.error("获取数据失败");
  } finally {
    loading.value = false;
  }
};

const handleCreate = () => {
  createDialogVisible.value = true;
};

const handleDelete = async (row: LotteryGroupDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除第 ${row.indexNo} 期第 ${row.groupId} 组数据吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );
    await lotteryApi.deleteLotteryGroupByIndexNoAndGroupId(
      row.indexNo,
      row.groupId
    );
    ElMessage.success("删除成功");
    getLotteryData();
  } catch {
    ElMessage.info("已取消删除");
  }
};

const parseNumbers = (value: string) =>
  value
    .split(/[,，\s]+/)
    .map(number => number.trim())
    .filter(Boolean);

const handleCreateSubmit = async () => {
  try {
    await createFormRef.value?.validate();

    const records: CreateUpdateLotteryDto[] = parseNumbers(
      createForm.redNumbers
    ).map(number => ({
      indexNo: createForm.indexNo,
      number,
      colorType: "0",
      lotteryType: createForm.lotteryType,
      groupId: 0
    }));

    for (const number of parseNumbers(createForm.blueNumber)) {
      records.push({
        indexNo: createForm.indexNo,
        number,
        colorType: "1",
        lotteryType: createForm.lotteryType,
        groupId: 0
      });
    }

    submitting.value = true;
    await lotteryApi.createLotteryGroup(records);
    ElMessage.success("创建成功");
    createDialogVisible.value = false;
    getLotteryData();
  } catch (error) {
    if (error !== false) {
      ElMessage.error("创建失败");
    }
  } finally {
    submitting.value = false;
  }
};

const handleSizeChange = (val: number) => {
  pageSize.value = val;
  currentPage.value = 1;
  getLotteryData();
};

const handleCurrentChange = (val: number) => {
  currentPage.value = val;
  getLotteryData();
};

const formatDateTime = (dateTime: string) => {
  return new Date(dateTime).toLocaleString();
};

onMounted(() => {
  getLotteryData();
});
</script>

<style scoped>
.lottery-data-container {
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
</style>
