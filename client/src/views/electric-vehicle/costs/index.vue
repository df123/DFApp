<template>
  <div class="costs-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">成本记录</span>
          <el-button type="primary" size="small" @click="handleCreate">
            <el-icon><Plus /></el-icon>
            新增
          </el-button>
        </div>
      </template>

      <el-table :data="tableData" :loading="loading" stripe border>
        <el-table-column prop="costDate" label="日期" width="120">
          <template #default="{ row }">
            {{ row.costDate ? row.costDate.split("T")[0] : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="amount" label="金额" width="120">
          <template #default="{ row }">
            ￥{{ row.amount.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column prop="isBelongToSelf" label="归属" width="100">
          <template #default="{ row }">
            {{ row.isBelongToSelf ? "个人" : "家庭" }}
          </template>
        </el-table-column>
        <el-table-column prop="costType" label="类型" width="100">
          <template #default="{ row }">
            {{ getCostTypeName(row.costType) }}
          </template>
        </el-table-column>
        <el-table-column prop="vehicle.name" label="车辆" width="120" />
        <el-table-column prop="remark" label="备注" show-overflow-tooltip>
          <template #default="{ row }">
            {{ formatRemark(row.remark) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.costType !== 1"
              size="small"
              type="primary"
              link
              @click="handleEdit(row)"
            >
              编辑
            </el-button>
            <el-button
              v-if="row.costType !== 1"
              size="small"
              type="danger"
              link
              @click="handleDelete(row)"
            >
              删除
            </el-button>
            <el-text v-if="row.costType === 1" type="info" size="small">
              自动同步
            </el-text>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="pagination.currentPage"
        v-model:page-size="pagination.pageSize"
        :total="pagination.total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        style="margin-top: 20px"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </el-card>

    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="600px"
      :before-close="handleClose"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="100px"
      >
        <el-form-item label="日期" prop="costDate">
          <el-date-picker
            v-model="formData.costDate"
            type="date"
            placeholder="请选择日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="金额" prop="amount">
          <el-input-number
            v-model="formData.amount"
            :min="0"
            :precision="2"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="归属" prop="isBelongToSelf">
          <el-radio-group v-model="formData.isBelongToSelf">
            <el-radio :label="true">个人</el-radio>
            <el-radio :label="false">家庭</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="类型" prop="costType">
          <el-select
            v-model="formData.costType"
            placeholder="请选择类型"
            style="width: 100%"
          >
            <el-option label="保养" :value="2" />
            <el-option label="保险" :value="3" />
            <el-option label="停车" :value="4" />
            <el-option label="维修" :value="5" />
            <el-option label="其他" :value="6" />
          </el-select>
        </el-form-item>
        <el-form-item label="车辆" prop="vehicleId">
          <el-select
            v-model="formData.vehicleId"
            placeholder="请选择车辆"
            style="width: 100%"
          >
            <el-option
              v-for="vehicle in vehicles"
              :key="vehicle.id"
              :label="vehicle.name"
              :value="vehicle.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="备注">
          <el-input
            v-model="formData.remark"
            type="textarea"
            :rows="2"
            placeholder="请输入备注"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import {
  electricVehicleApi,
  electricVehicleCostApi
} from "@/api/electric-vehicle";
import type {
  ElectricVehicleDto,
  ElectricVehicleCostDto,
  CreateUpdateElectricVehicleCostDto,
  PagedResultDto
} from "@/types/api";

const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const formRef = ref<FormInstance>();
const tableData = ref<ElectricVehicleCostDto[]>([]);
const vehicles = ref<ElectricVehicleDto[]>([]);

const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

const formData = ref<CreateUpdateElectricVehicleCostDto>({
  costDate: "",
  amount: 0,
  isBelongToSelf: true,
  costType: 2,
  vehicleId: "",
  remark: ""
});

const currentEditId = ref<string | null>(null);

const dialogTitle = ref("新增成本");

const formRules: FormRules = {
  costDate: [{ required: true, message: "请选择日期", trigger: "change" }],
  amount: [
    { required: true, message: "请输入金额", trigger: "blur" },
    { type: "number", min: 0.01, message: "金额必须大于0", trigger: "blur" }
  ],
  isBelongToSelf: [
    { required: true, message: "请选择归属", trigger: "change" }
  ],
  costType: [{ required: true, message: "请选择类型", trigger: "change" }],
  vehicleId: [{ required: true, message: "请选择车辆", trigger: "change" }]
};

const getCostTypeName = (type: number) => {
  const map = {
    1: "充电",
    2: "保养",
    3: "保险",
    4: "停车",
    5: "维修",
    6: "其他"
  };
  return map[type] || "未知";
};

const formatRemark = (remark: string | undefined) => {
  if (!remark) return "-";
  if (remark.startsWith("ChargingRecord:")) {
    const parts = remark.split("|");
    if (parts.length > 1) {
      return parts[1];
    }
  }
  return remark;
};

const loadVehicles = async () => {
  try {
    const result = await electricVehicleApi.getVehicles({
      pageSize: 1000
    });
    vehicles.value = result.items;
  } catch (error) {
    console.error("加载车辆列表失败:", error);
    ElMessage.error("加载车辆列表失败");
  }
};

const loadTableData = async () => {
  loading.value = true;
  try {
    const result = await electricVehicleCostApi.getCosts({
      pageIndex: pagination.currentPage,
      pageSize: pagination.pageSize
    });
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载成本数据失败:", error);
    ElMessage.error("加载成本数据失败");
  } finally {
    loading.value = false;
  }
};

const handleSizeChange = (size: number) => {
  pagination.pageSize = size;
  pagination.currentPage = 1;
  loadTableData();
};

const handleCurrentChange = (page: number) => {
  pagination.currentPage = page;
  loadTableData();
};

const handleCreate = () => {
  currentEditId.value = null;
  Object.assign(formData.value, {
    costDate: new Date().toISOString().split("T")[0],
    amount: 0,
    isBelongToSelf: true,
    costType: 2,
    vehicleId: vehicles.value.length === 1 ? vehicles.value[0].id : undefined,
    remark: ""
  });
  dialogTitle.value = "新增成本";
  dialogVisible.value = true;
};

const handleEdit = (row: ElectricVehicleCostDto) => {
  if (row.costType === 1) {
    ElMessage.warning("充电类型的成本记录不能直接编辑，请通过充电记录进行管理");
    return;
  }

  currentEditId.value = row.id;
  Object.assign(formData.value, {
    costDate: row.costDate,
    amount: row.amount,
    isBelongToSelf: row.isBelongToSelf,
    costType: row.costType,
    vehicleId: row.vehicleId,
    remark: row.remark || ""
  });
  dialogTitle.value = "编辑成本";
  dialogVisible.value = true;
};

const handleDelete = async (row: ElectricVehicleCostDto) => {
  try {
    if (row.costType === 1) {
      ElMessage.warning(
        "充电类型的成本记录不能直接删除，请通过充电记录进行管理"
      );
      return;
    }

    await ElMessageBox.confirm(`确定要删除成本记录吗？`, "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await electricVehicleCostApi.deleteCost(row.id);
    ElMessage.success("删除成功");
    loadTableData();
  } catch (error) {
    if (error !== "cancel") {
      console.error("删除失败:", error);
      ElMessage.error("删除失败");
    }
  }
};

const handleClose = () => {
  dialogVisible.value = false;
  formRef.value?.clearValidate();
};

const handleSubmit = async () => {
  if (!formRef.value) return;

  const valid = await formRef.value.validate();
  if (!valid) return;

  submitting.value = true;
  try {
    if (currentEditId.value) {
      await electricVehicleCostApi.updateCost(
        currentEditId.value,
        formData.value
      );
      ElMessage.success("更新成功");
    } else {
      await electricVehicleCostApi.createCost(formData.value);
      ElMessage.success("创建成功");
    }

    dialogVisible.value = false;
    loadTableData();
  } catch (error) {
    console.error("保存失败:", error);
    ElMessage.error("保存失败");
  } finally {
    submitting.value = false;
  }
};

onMounted(() => {
  loadVehicles();
  loadTableData();
});
</script>

<style scoped>
.costs-container {
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
}
</style>
