<template>
  <div class="lottery-data-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>{{ $t("lottery.data.title") }}</span>
          <el-button
            v-if="hasPermission('DFApp.Lottery.Create')"
            v-permission="'DFApp.Lottery.Create'"
            type="primary"
            icon="plus"
            @click="handleCreate"
          >
            {{ $t("lottery.data.addButton") }}
          </el-button>
        </div>
      </template>

      <el-table
        v-loading="loading"
        :data="tableData"
        stripe
        style="width: 100%"
      >
        <el-table-column :label="$t('lottery.data.actions')" width="150">
          <template #default="scope">
            <el-button
              v-if="hasPermission('DFApp.Lottery.Edit')"
              v-permission="'DFApp.Lottery.Edit'"
              type="primary"
              link
              size="small"
              @click="handleEdit(scope.row)"
            >
              {{ $t("lottery.data.edit") }}
            </el-button>
            <el-button
              v-if="hasPermission('DFApp.Lottery.Delete')"
              v-permission="'DFApp.Lottery.Delete'"
              type="danger"
              link
              size="small"
              @click="handleDelete(scope.row)"
            >
              {{ $t("lottery.data.delete") }}
            </el-button>
          </template>
        </el-table-column>
        <el-table-column prop="indexNo" :label="$t('lottery.data.indexNo')" />
        <el-table-column
          prop="lotteryType"
          :label="$t('lottery.data.lotteryType')"
        />
        <el-table-column
          prop="redNumbers"
          :label="$t('lottery.data.redNumbers')"
        />
        <el-table-column
          prop="blueNumber"
          :label="$t('lottery.data.blueNumber')"
        />
        <el-table-column prop="groupId" :label="$t('lottery.data.groupId')" />
        <el-table-column
          prop="creationTime"
          :label="$t('lottery.data.creationTime')"
        >
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

    <!-- 编辑模态框 -->
    <el-dialog
      v-model="editDialogVisible"
      :title="$t('lottery.data.editTitle')"
    >
      <el-form ref="editFormRef" :model="editForm" :rules="editRules">
        <el-form-item prop="indexNo" :label="$t('lottery.data.indexNo')">
          <el-input v-model="editForm.indexNo" />
        </el-form-item>
        <el-form-item
          prop="lotteryType"
          :label="$t('lottery.data.lotteryType')"
        >
          <el-input v-model="editForm.lotteryType" />
        </el-form-item>
        <el-form-item prop="redNumbers" :label="$t('lottery.data.redNumbers')">
          <el-input v-model="editForm.redNumbers" />
        </el-form-item>
        <el-form-item prop="blueNumber" :label="$t('lottery.data.blueNumber')">
          <el-input v-model="editForm.blueNumber" />
        </el-form-item>
        <el-form-item prop="groupId" :label="$t('lottery.data.groupId')">
          <el-input v-model="editForm.groupId" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">
          {{ $t("lottery.data.cancel") }}
        </el-button>
        <el-button type="primary" @click="handleEditSubmit">
          {{ $t("lottery.data.save") }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 创建模态框 -->
    <el-dialog
      v-model="createDialogVisible"
      :title="$t('lottery.data.createTitle')"
    >
      <el-form ref="createFormRef" :model="createForm" :rules="createRules">
        <el-form-item prop="indexNo" :label="$t('lottery.data.indexNo')">
          <el-input v-model="createForm.indexNo" />
        </el-form-item>
        <el-form-item
          prop="lotteryType"
          :label="$t('lottery.data.lotteryType')"
        >
          <el-input v-model="createForm.lotteryType" />
        </el-form-item>
        <el-form-item prop="redNumbers" :label="$t('lottery.data.redNumbers')">
          <el-input v-model="createForm.redNumbers" />
        </el-form-item>
        <el-form-item prop="blueNumber" :label="$t('lottery.data.blueNumber')">
          <el-input v-model="createForm.blueNumber" />
        </el-form-item>
        <el-form-item prop="groupId" :label="$t('lottery.data.groupId')">
          <el-input v-model="createForm.groupId" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">
          {{ $t("lottery.data.cancel") }}
        </el-button>
        <el-button type="primary" @click="handleCreateSubmit">
          {{ $t("lottery.data.create") }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import { lotteryApi } from "@/api/lottery";
import type { LotteryGroupDto, CreateUpdateLotteryDto } from "@/types/business";

// 数据状态
const loading = ref(false);
const tableData = ref<LotteryGroupDto[]>([]);
const total = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);

// 编辑相关状态
const editDialogVisible = ref(false);
const editFormRef = ref();
const editForm = reactive<CreateUpdateLotteryDto>({
  indexNo: 0,
  number: "",
  colorType: "",
  lotteryType: "",
  groupId: 0
});

// 创建相关状态
const createDialogVisible = ref(false);
const createFormRef = ref();
const createForm = reactive<CreateUpdateLotteryDto>({
  indexNo: 0,
  number: "",
  colorType: "",
  lotteryType: "",
  groupId: 0
});

// 表单验证规则
const editRules = {
  indexNo: [{ required: true, message: "请输入期号", trigger: "blur" }]
};

const createRules = {
  indexNo: [{ required: true, message: "请输入期号", trigger: "blur" }]
};

// 权限检查
const hasPermission = (permission: string) => {
  // 这里需要根据实际的权限系统实现
  return true;
};

// 获取彩票数据
const getLotteryData = async () => {
  loading.value = true;
  try {
    const result = await lotteryApi.getLotteryGroups({
      skipCount: (currentPage.value - 1) * pageSize.value,
      maxResultCount: pageSize.value
    });
    tableData.value = result.items;
    total.value = result.totalCount;
  } catch (error) {
    ElMessage.error("获取数据失败");
  } finally {
    loading.value = false;
  }
};

// 处理创建
const handleCreate = () => {
  createDialogVisible.value = true;
};

// 处理编辑
const handleEdit = (row: LotteryGroupDto) => {
  Object.assign(editForm, row);
  editDialogVisible.value = true;
};

// 处理删除
const handleDelete = async (row: LotteryGroupDto) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除第 ${row.indexNo} 期数据吗？`,
      "删除确认",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );
    await lotteryApi.deleteLottery(row.id);
    ElMessage.success("删除成功");
    getLotteryData();
  } catch {
    // 用户取消删除
  }
};

// 处理编辑提交
const handleEditSubmit = async () => {
  try {
    await editFormRef.value.validate();
    await lotteryApi.updateLottery(editForm.id, editForm);
    ElMessage.success("更新成功");
    editDialogVisible.value = false;
    getLotteryData();
  } catch (error) {
    // 验证失败
  }
};

// 处理创建提交
const handleCreateSubmit = async () => {
  try {
    await createFormRef.value.validate();
    await lotteryApi.createLottery(createForm);
    ElMessage.success("创建成功");
    createDialogVisible.value = false;
    getLotteryData();
  } catch (error) {
    // 验证失败
  }
};

// 分页处理
const handleSizeChange = (val: number) => {
  pageSize.value = val;
  currentPage.value = 1;
  getLotteryData();
};

const handleCurrentChange = (val: number) => {
  currentPage.value = val;
  getLotteryData();
};

// 格式化日期时间
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
