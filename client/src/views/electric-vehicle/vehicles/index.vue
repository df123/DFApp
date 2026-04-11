<template>
  <div class="vehicles-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">车辆管理</span>
          <el-button type="primary" size="small" @click="handleCreate">
            <el-icon><Plus /></el-icon>
            新增车辆
          </el-button>
        </div>
      </template>

      <el-table :data="tableData" :loading="loading" stripe border>
        <el-table-column prop="name" label="车辆名称" width="150" />
        <el-table-column prop="brand" label="品牌" width="120" />
        <el-table-column prop="model" label="型号" width="120" />
        <el-table-column prop="licensePlate" label="车牌号" width="120" />
        <el-table-column prop="batteryCapacity" label="电池容量" width="100">
          <template #default="{ row }">
            {{ row.batteryCapacity ? row.batteryCapacity + " kWh" : "-" }}
          </template>
        </el-table-column>
        <el-table-column prop="totalMileage" label="总里程" width="120">
          <template #default="{ row }">
            {{ row.totalMileage.toFixed(0) }} km
          </template>
        </el-table-column>
        <el-table-column prop="remark" label="备注" show-overflow-tooltip />
        <el-table-column label="操作" width="150" fixed="right">
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
        <el-form-item label="车辆名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入车辆名称" />
        </el-form-item>
        <el-form-item label="品牌" prop="brand">
          <el-input v-model="formData.brand" placeholder="请输入品牌" />
        </el-form-item>
        <el-form-item label="型号" prop="model">
          <el-input v-model="formData.model" placeholder="请输入型号" />
        </el-form-item>
        <el-form-item label="车牌号" prop="licensePlate">
          <el-input
            v-model="formData.licensePlate"
            placeholder="请输入车牌号"
          />
        </el-form-item>
        <el-form-item label="电池容量" prop="batteryCapacity">
          <el-input-number
            v-model="formData.batteryCapacity"
            :min="0"
            :precision="1"
            :step="0.1"
            style="width: 100%"
          />
          <span style="margin-left: 10px">kWh</span>
        </el-form-item>
        <el-form-item label="总里程" prop="totalMileage">
          <el-input-number
            v-model="formData.totalMileage"
            :min="0"
            :precision="0"
            style="width: 100%"
          />
          <span style="margin-left: 10px">km</span>
        </el-form-item>
        <el-form-item label="购买日期">
          <el-date-picker
            v-model="formData.purchaseDate"
            type="date"
            placeholder="选择日期"
            value-format="YYYY-MM-DD"
            style="width: 100%"
          />
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
import { electricVehicleApi } from "@/api/electric-vehicle";
import type {
  ElectricVehicleDto,
  CreateUpdateElectricVehicleDto,
  PagedResultDto
} from "@/types/api";

const loading = ref(false);
const submitting = ref(false);
const dialogVisible = ref(false);
const formRef = ref<FormInstance>();
const tableData = ref<ElectricVehicleDto[]>([]);

const pagination = reactive({
  pageSize: 10,
  currentPage: 1,
  total: 0
});

const formData = ref<CreateUpdateElectricVehicleDto>({
  name: "",
  brand: "",
  model: "",
  licensePlate: "",
  batteryCapacity: undefined,
  totalMileage: 0,
  purchaseDate: "",
  remark: ""
});

const currentEditId = ref<string | null>(null);

const dialogTitle = ref("新增车辆");

const formRules: FormRules = {
  name: [{ required: true, message: "请输入车辆名称", trigger: "blur" }]
};

const loadTableData = async () => {
  loading.value = true;
  try {
    const result = await electricVehicleApi.getVehicles({
      pageIndex: pagination.currentPage,
      pageSize: pagination.pageSize
    });
    tableData.value = result.items;
    pagination.total = result.totalCount;
  } catch (error) {
    console.error("加载数据失败:", error);
    ElMessage.error("加载数据失败");
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
    name: "",
    brand: "",
    model: "",
    licensePlate: "",
    batteryCapacity: undefined,
    totalMileage: 0,
    purchaseDate: "",
    remark: ""
  });
  dialogTitle.value = "新增车辆";
  dialogVisible.value = true;
};

const handleEdit = (row: ElectricVehicleDto) => {
  currentEditId.value = row.id;
  Object.assign(formData.value, {
    name: row.name,
    brand: row.brand || "",
    model: row.model || "",
    licensePlate: row.licensePlate || "",
    batteryCapacity: row.batteryCapacity,
    totalMileage: row.totalMileage,
    purchaseDate: row.purchaseDate || "",
    remark: row.remark || ""
  });
  dialogTitle.value = "编辑车辆";
  dialogVisible.value = true;
};

const handleDelete = async (row: ElectricVehicleDto) => {
  try {
    await ElMessageBox.confirm(`确定要删除车辆"${row.name}"吗？`, "删除确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    await electricVehicleApi.deleteVehicle(row.id);
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
      await electricVehicleApi.updateVehicle(
        currentEditId.value,
        formData.value
      );
      ElMessage.success("更新成功");
    } else {
      await electricVehicleApi.createVehicle(formData.value);
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
  loadTableData();
});
</script>

<style scoped>
.vehicles-container {
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
