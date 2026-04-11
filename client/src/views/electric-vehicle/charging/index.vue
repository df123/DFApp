<template>
  <div class="charging-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">充电记录</span>
          <el-button type="primary" size="small" @click="handleCreate">
            <el-icon><Plus /></el-icon>
            新增
          </el-button>
        </div>
      </template>

      <el-table :data="tableData" :loading="loading" stripe border>
        <el-table-column prop="chargingDate" label="日期" width="120">
          <template #default="{ row }">
            {{ row.chargingDate ? row.chargingDate.split("T")[0] : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="amount" label="金额" width="140">
          <template #default="{ row }">
            ￥{{ row.amount.toFixed(2) }}
          </template>
        </el-table-column>
        <el-table-column prop="energy" label="电量" width="140">
          <template #default="{ row }">
            {{ row.energy ? row.energy.toFixed(4) + " kWh" : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="currentMileage" label="当前里程" width="160">
          <template #default="{ row }">
            {{
              row.currentMileage ? row.currentMileage.toFixed(1) + " km" : "-"
            }}
          </template>
        </el-table-column>
        <el-table-column prop="mileageDiff" label="里程差值" width="140">
          <template #default="{ row }">
            {{ row.mileageDiff ? row.mileageDiff.toFixed(1) + " km" : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="vehicle.name" label="车辆" min-width="120" />
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button
              size="small"
              type="primary"
              link
              @click="handleEdit(row)"
            >
              编辑
            </el-button>
            <el-button
              size="small"
              type="danger"
              link
              @click="handleDelete(row)"
            >
              删除
            </el-button>
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
        <el-form-item label="日期" prop="chargingDate">
          <el-date-picker
            v-model="formData.chargingDate"
            type="date"
            placeholder="请选择日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="电量" prop="energy">
          <el-input-number
            v-model="formData.energy"
            :min="0"
            :precision="4"
            placeholder="请输入电量"
            style="width: 100%"
          />
          <span style="margin-left: 10px">kWh</span>
        </el-form-item>
        <el-form-item label="金额" prop="amount">
          <el-input-number
            v-model="formData.amount"
            :min="0"
            :precision="2"
            placeholder="请输入金额"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="当前总里程" prop="currentMileage">
          <el-input-number
            v-model="formData.currentMileage"
            :min="0"
            :precision="1"
            placeholder="请输入当前总里程"
            style="width: 100%"
          />
          <span style="margin-left: 10px">km</span>
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
  electricVehicleChargingRecordApi
} from "@/api/electric-vehicle";
import type {
  ElectricVehicleDto,
  ElectricVehicleChargingRecordDto,
  CreateUpdateElectricVehicleChargingRecordDto,
  PagedResultDto
} from "@/types/api";

const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const formRef = ref<FormInstance>();
const tableData = ref<ElectricVehicleChargingRecordDto[]>([]);
const vehicles = ref<ElectricVehicleDto[]>([]);

const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

const formData = ref<CreateUpdateElectricVehicleChargingRecordDto>({
  chargingDate: "",
  stationName: "",
  chargingDuration: undefined,
  energy: undefined,
  amount: 0,
  currentMileage: undefined,
  startSOC: undefined,
  endSOC: undefined,
  isBelongToSelf: true,
  vehicleId: "",
  remark: ""
});

const currentEditId = ref<string | null>(null);

const dialogTitle = ref("新增充电记录");

const formRules: FormRules = {
  chargingDate: [{ required: true, message: "请选择日期", trigger: "change" }],
  amount: [
    { required: true, message: "请输入金额", trigger: "blur" },
    { type: "number", min: 0.01, message: "金额必须大于0", trigger: "blur" }
  ],
  vehicleId: [{ required: true, message: "请选择车辆", trigger: "change" }]
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
    const result = await electricVehicleChargingRecordApi.getChargingRecords({
      pageIndex: pagination.currentPage,
      pageSize: pagination.pageSize
    });
    tableData.value = result.items;
    pagination.total = result.totalCount;

    tableData.value.sort(
      (a, b) =>
        new Date(b.chargingDate).getTime() - new Date(a.chargingDate).getTime()
    );

    tableData.value.forEach((item, index) => {
      if (index < tableData.value.length - 1) {
        const nextItem = tableData.value[index + 1];
        if (item.currentMileage && nextItem.currentMileage) {
          item.mileageDiff = item.currentMileage - nextItem.currentMileage;
        }
      } else {
        item.mileageDiff = undefined;
      }
    });
  } catch (error) {
    console.error("加载充电数据失败:", error);
    ElMessage.error("加载充电数据失败");
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
    chargingDate: new Date().toISOString().split("T")[0],
    stationName: "",
    chargingDuration: undefined,
    energy: undefined,
    amount: 0,
    currentMileage: undefined,
    startSOC: undefined,
    endSOC: undefined,
    isBelongToSelf: true,
    vehicleId: vehicles.value.length === 1 ? vehicles.value[0].id : undefined,
    remark: ""
  });
  dialogTitle.value = "新增充电记录";
  dialogVisible.value = true;
};

const handleEdit = (row: ElectricVehicleChargingRecordDto) => {
  currentEditId.value = row.id;
  Object.assign(formData.value, {
    chargingDate: row.chargingDate,
    stationName: row.stationName || "",
    chargingDuration: row.chargingDuration,
    energy: row.energy,
    amount: row.amount,
    currentMileage: row.currentMileage,
    startSOC: row.startSOC,
    endSOC: row.endSOC,
    isBelongToSelf: row.isBelongToSelf,
    vehicleId: row.vehicleId,
    remark: row.remark || ""
  });
  dialogTitle.value = "编辑充电记录";
  dialogVisible.value = true;
};

const handleDelete = async (row: ElectricVehicleChargingRecordDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除充电记录吗？`, "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await electricVehicleChargingRecordApi.deleteChargingRecord(row.id);
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
      await electricVehicleChargingRecordApi.updateChargingRecord(
        currentEditId.value,
        formData.value
      );
      ElMessage.success("更新成功");
    } else {
      await electricVehicleChargingRecordApi.createChargingRecord(
        formData.value
      );
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
.charging-container {
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
