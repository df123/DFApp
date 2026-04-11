<template>
  <div class="lottery-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">彩票购买</span>
          <div class="header-actions">
            <el-button-group>
              <el-button type="primary" @click="handleSingleBet">
                单式投注
              </el-button>
              <el-button
                v-if="hasCreatePermission"
                type="primary"
                @click="handleCompoundBet"
              >
                复式投注
              </el-button>
            </el-button-group>
          </div>
        </div>
      </template>

      <!-- 数据表格 -->
      <div class="table-container">
        <el-table
          v-loading="loading"
          :data="tableData"
          style="width: 100%"
          stripe
          @sort-change="handleSortChange"
        >
          <el-table-column
            prop="indexNo"
            label="期号"
            sortable="custom"
            width="120"
          />
          <el-table-column
            prop="lotteryType"
            label="彩票类型"
            sortable="custom"
            width="120"
          />
          <el-table-column prop="redNumbers" label="红球号码" min-width="150">
            <template #default="scope">
              <span class="red-number">{{ scope.row.redNumbers }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="blueNumber" label="蓝球号码" min-width="100">
            <template #default="scope">
              <span class="blue-number">{{ scope.row.blueNumber }}</span>
            </template>
          </el-table-column>
          <el-table-column
            prop="groupId"
            label="组号"
            sortable="custom"
            min-width="100"
          />
          <el-table-column
            prop="creationTime"
            label="创建时间"
            sortable="custom"
            min-width="180"
          >
            <template #default="scope">
              {{ formatDateTime(scope.row.creationTime) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" fixed="right" width="120">
            <template #default="scope">
              <el-button
                v-if="hasEditPermission"
                size="small"
                type="primary"
                link
                @click="handleEdit(scope.row)"
              >
                编辑
              </el-button>
              <el-button
                v-if="hasDeletePermission"
                size="small"
                type="danger"
                link
                @click="handleDelete(scope.row.indexNo, scope.row.groupId)"
              >
                删除组
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="currentPage"
          v-model:page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="totalCount"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>

      <!-- 单式投注弹窗 -->
      <el-dialog
        v-model="singleDialogVisible"
        title="单式投注"
        width="600px"
        :close-on-click-modal="false"
      >
        <div class="dialog-content">
          <el-row>
            <div class="botton-area select-area">
              <el-select
                v-model="formLotteryTypeValue"
                class="m-2"
                placeholder="彩票类型"
              >
                <el-option
                  v-for="item in formLotteryTypeItems"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </div>
          </el-row>
          <el-row>
            <div class="botton-area input-area">
              <el-input
                v-model="formData.indexNo"
                class="m-2"
                placeholder="期号"
              />
            </div>
          </el-row>
          <el-row>
            <div class="botton-area input-area">
              <el-input
                v-model="numberInputValue"
                class="m-2"
                type="textarea"
                :rows="5"
                :placeholder="numberInputPlaceholder"
              />
            </div>
          </el-row>
        </div>
        <template #footer>
          <el-button @click="singleDialogVisible = false">取消</el-button>
          <el-button
            type="primary"
            :loading="formLoading"
            @click="handleFormSubmit"
          >
            确认
          </el-button>
        </template>
      </el-dialog>

      <!-- 复式投注弹窗 -->
      <el-dialog
        v-model="compoundDialogVisible"
        title="复式投注"
        width="800px"
        :close-on-click-modal="false"
      >
        <CompoundLotteryInput
          ref="compoundInputRef"
          @created="handleCompoundCreated"
        />
      </el-dialog>

      <!-- 编辑对话框 -->
      <el-dialog
        v-model="formDialogVisible"
        :title="formDialogTitle"
        width="600px"
        :close-on-click-modal="false"
      >
        <div class="dialog-content">
          <el-row>
            <div class="botton-area select-area">
              <el-select
                v-model="formLotteryTypeValue"
                class="m-2"
                placeholder="彩票类型"
              >
                <el-option
                  v-for="item in formLotteryTypeItems"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                />
              </el-select>
            </div>
          </el-row>
          <el-row>
            <div class="botton-area input-area">
              <el-input
                v-model="formData.indexNo"
                class="m-2"
                placeholder="期号"
              />
            </div>
          </el-row>
          <el-row>
            <div class="botton-area input-area">
              <el-input
                v-model="numberInputValue"
                class="m-2"
                type="textarea"
                :rows="5"
                :placeholder="numberInputPlaceholder"
              />
            </div>
          </el-row>
        </div>
        <template #footer>
          <el-button @click="formDialogVisible = false">取消</el-button>
          <el-button
            type="primary"
            :loading="formLoading"
            @click="handleFormSubmit"
          >
            确认
          </el-button>
        </template>
      </el-dialog>

      <!-- 删除确认对话框 -->
      <el-dialog v-model="deleteDialogVisible" title="确认删除" width="400px">
        <span>确定要删除整组记录吗？此操作不可恢复。</span>
        <template #footer>
          <el-button @click="deleteDialogVisible = false">取消</el-button>
          <el-button
            type="danger"
            :loading="deleteLoading"
            @click="confirmDelete"
          >
            确认删除组
          </el-button>
        </template>
      </el-dialog>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed, nextTick, watch } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { lotteryApi } from "@/api/lottery";
import CompoundLotteryInput from "./components/CompoundLotteryInput.vue";
import type { PagedRequestDto, PagedResultDto } from "@/types/api";
import type {
  LotteryGroupDto,
  CreateUpdateLotteryDto,
  ConstsDto
} from "@/types/business";

// 对话框相关数据
const formLotteryTypeValue = ref("kl8");
const formLotteryTypeItems = ref<{ value: string; label: string }[]>([]);

// 响应式数据
const loading = ref(false);
const tableData = ref<LotteryGroupDto[]>([]);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(10);
const sortField = ref("indexNo");
const sortOrder = ref("desc");
const compoundInputRef = ref();

// 弹窗状态
const singleDialogVisible = ref(false);
const compoundDialogVisible = ref(false);

// 表单相关状态
const formRef = ref<FormInstance>();
const formDialogVisible = ref(false);
const formLoading = ref(false);
const formDialogTitle = ref("");
const isEditMode = ref(false);
const currentEditId = ref<number | null>(null);

// 表单数据
const formData = reactive({
  indexNo: "",
  reds: "",
  blues: "",
  numbers: ""
});

// 计算属性：根据彩票类型动态绑定输入框的值
const numberInputValue = computed({
  get: () =>
    formLotteryTypeValue.value === "ssq" ? formData.numbers : formData.reds,
  set: value => {
    if (formLotteryTypeValue.value === "ssq") {
      formData.numbers = value;
    } else {
      formData.reds = value;
    }
  }
});

// 计算属性：根据彩票类型动态设置输入框的占位符
const numberInputPlaceholder = computed(() => {
  return formLotteryTypeValue.value === "ssq"
    ? "红球+蓝球 (例如: 01 02 03 04 05 06+07 或每行一组: 01 02 03 04 05 06+07)"
    : "红球";
});

// 表单验证规则
const formRules: FormRules = {
  indexNo: [
    { required: true, message: "请输入期号", trigger: "blur" },
    { type: "number", min: 1, message: "期号必须大于0", trigger: "blur" }
  ],
  lotteryType: [
    { required: true, message: "请选择彩票类型", trigger: "change" }
  ],
  number: [{ required: true, message: "请输入号码", trigger: "blur" }]
};

// 删除相关状态
const deleteDialogVisible = ref(false);
const deleteLoading = ref(false);
const currentDeleteIndexNo = ref<number | null>(null);
const currentDeleteGroupId = ref<number | null>(null);

// 权限控制（这里需要根据项目实际的权限系统进行调整）
const hasCreatePermission = computed(() => true); // 替换为实际的权限检查
const hasEditPermission = computed(() => true); // 替换为实际的权限检查
const hasDeletePermission = computed(() => true); // 替换为实际的权限检查

// 获取表格数据
const fetchTableData = async () => {
  loading.value = true;
  try {
    const params: PagedRequestDto = {
      pageIndex: currentPage.value,
      pageSize: pageSize.value,
      sorting: `${sortField.value} ${sortOrder.value}`
    };

    const result: PagedResultDto<LotteryGroupDto> =
      await lotteryApi.getLotteryGroups(params);
    tableData.value = result.items || [];
    totalCount.value = result.totalCount || 0;
  } catch (error) {
    console.error("获取彩票数据失败:", error);
    ElMessage.error("获取数据失败");
  } finally {
    loading.value = false;
  }
};

// 分页处理
const handleSizeChange = (newSize: number) => {
  pageSize.value = newSize;
  currentPage.value = 1;
  fetchTableData();
};

const handleCurrentChange = (newPage: number) => {
  currentPage.value = newPage;
  fetchTableData();
};

// 排序处理
const handleSortChange = (sort: any) => {
  if (sort.prop) {
    sortField.value = sort.prop;
    sortOrder.value = sort.order === "ascending" ? "asc" : "desc";
    fetchTableData();
  }
};

// 加载最新期号的函数
const loadLatestIndexNo = async () => {
  if (formLotteryTypeValue.value) {
    try {
      // 获取彩票类型的中文名称
      const lotteryType = formLotteryTypeItems.value.find(
        item => item.value === formLotteryTypeValue.value
      );

      if (lotteryType) {
        // 调用API获取最新期号
        const latestIndexNo = await lotteryApi.getLatestIndexNoByType(
          lotteryType.label
        );

        // 如果有最新期号，则填充到期号输入框
        if (latestIndexNo > 0) {
          formData.indexNo = latestIndexNo.toString();
        }
      }
    } catch (error) {
      console.error("获取最新期号失败:", error);
      // 静默失败，不影响用户操作
    }
  }
};

// 操作处理
const handleSingleBet = async () => {
  resetForm();
  singleDialogVisible.value = true;

  // 等待下一个tick确保弹窗已打开且彩票类型选项已加载
  await nextTick();
  loadLatestIndexNo();
};

const handleCompoundBet = async () => {
  compoundDialogVisible.value = true;
  // 重置复式投注表单
  nextTick(() => {
    compoundInputRef.value?.resetForm();
  });
};

const handleEdit = (row: LotteryGroupDto) => {
  resetForm();
  isEditMode.value = true;
  formDialogTitle.value = "编辑彩票记录";
  currentEditId.value = row.id!;

  // 预填充表单数据
  // 注意：由于表格显示的是分组后的数据，而编辑需要的是单条记录
  // 这里需要根据实际情况获取单条记录的数据，目前先简单处理
  Object.assign(formData, {
    indexNo: row.indexNo,
    lotteryType: row.lotteryType,
    number: row.redNumbers || "", // 这里需要根据实际情况调整
    colorType: "0", // 默认为红球
    groupId: row.groupId
  });

  formDialogVisible.value = true;
};

const handleDelete = (indexNo: number, groupId: number) => {
  currentDeleteIndexNo.value = indexNo;
  currentDeleteGroupId.value = groupId;
  deleteDialogVisible.value = true;
};

const confirmDelete = async () => {
  if (
    currentDeleteIndexNo.value === null ||
    currentDeleteIndexNo.value === undefined ||
    currentDeleteGroupId.value === null ||
    currentDeleteGroupId.value === undefined
  ) {
    console.error("IndexNo or Group ID is null or undefined");
    return;
  }

  deleteLoading.value = true;
  try {
    await lotteryApi.deleteLotteryGroupByIndexNoAndGroupId(
      currentDeleteIndexNo.value,
      currentDeleteGroupId.value
    );
    ElMessage.success("删除成功");
    deleteDialogVisible.value = false;
    fetchTableData();
  } catch (error) {
    console.error("删除失败:", error);
    ElMessage.error("删除失败");
  } finally {
    deleteLoading.value = false;
    currentDeleteIndexNo.value = null;
    currentDeleteGroupId.value = null;
  }
};

// 表单提交处理
const handleFormSubmit = async () => {
  try {
    formLoading.value = true;

    // 获取彩票类型信息
    let lotteryType = formLotteryTypeItems.value.find(
      item => item.value === formLotteryTypeValue.value
    );
    if (!lotteryType) {
      ElMessage.error("未找到对应的彩票类型");
      return;
    }

    let createDtos: CreateUpdateLotteryDto[] = [];

    // 处理双色球数据（红球+蓝球格式）
    if (formLotteryTypeValue.value === "ssq") {
      // 验证输入
      if (
        !isValidString(formData.indexNo) ||
        !isValidString(formData.numbers)
      ) {
        ElMessage.error("请输入期号和号码");
        return;
      }

      // 解析红球+蓝球格式
      let numberGroups = formData.numbers
        .split(/\n+/)
        .filter(group => group.trim() !== "");

      numberGroups.forEach((group, groupIndex) => {
        // 检查是否包含+号（红球+蓝球格式）
        if (group.includes("+")) {
          // 处理红球+蓝球格式：01 02 03 04 05 06+07
          let parts = group.split("+");
          if (parts.length >= 2) {
            let redPart = parts[0].trim();
            let bluePart = parts[1].trim();

            // 处理红球
            let redNumbers = redPart
              .split(/[\s,]+/)
              .filter(item => item.trim() !== "");
            redNumbers.forEach(element => {
              createDtos.push({
                indexNo: parseInt(formData.indexNo),
                number: element,
                colorType: "0",
                lotteryType: lotteryType.label,
                groupId: groupIndex + 1
              });
            });

            // 处理蓝球
            let blueNumbers = bluePart
              .split(/[\s,]+/)
              .filter(item => item.trim() !== "");
            blueNumbers.forEach(element => {
              createDtos.push({
                indexNo: parseInt(formData.indexNo),
                number: element,
                colorType: "1",
                lotteryType: lotteryType.label,
                groupId: groupIndex + 1
              });
            });
          }
        } else {
          // 处理普通格式：将最后一个数字作为蓝球，其余作为红球
          let numbers = group
            .split(/[\s,]+/)
            .filter(item => item.trim() !== "");
          if (numbers.length > 0) {
            // 前n-1个作为红球
            for (let i = 0; i < numbers.length - 1; i++) {
              createDtos.push({
                indexNo: parseInt(formData.indexNo),
                number: numbers[i],
                colorType: "0",
                lotteryType: lotteryType.label,
                groupId: groupIndex + 1
              });
            }

            // 最后一个作为蓝球
            createDtos.push({
              indexNo: parseInt(formData.indexNo),
              number: numbers[numbers.length - 1],
              colorType: "1",
              lotteryType: lotteryType.label,
              groupId: groupIndex + 1
            });
          }
        }
      });
    } else {
      // 处理其他彩票类型
      // 验证输入
      if (!isValidString(formData.indexNo) || !isValidString(formData.reds)) {
        ElMessage.error("请输入期号和号码");
        return;
      }

      // 处理红球数据
      let redGroups = formData.reds
        .split(/\n+/)
        .filter(group => group.trim() !== "");
      redGroups.forEach((group, groupIndex) => {
        let groupItems = group
          .split(/[\s,]+/)
          .filter(item => item.trim() !== "");
        groupItems.forEach(element => {
          createDtos.push({
            indexNo: parseInt(formData.indexNo),
            number: element,
            colorType: "0",
            lotteryType: lotteryType.label,
            groupId: groupIndex + 1
          });
        });
      });
    }

    // 批量创建彩票记录
    for (const dto of createDtos) {
      if (isEditMode.value && currentEditId.value) {
        // 编辑模式（这里需要根据实际情况调整，目前只支持创建）
        await lotteryApi.createLottery(dto);
      } else {
        // 创建模式
        await lotteryApi.createLottery(dto);
      }
    }

    ElMessage.success(isEditMode.value ? "更新成功" : "创建成功");
    singleDialogVisible.value = false;
    formDialogVisible.value = false;
    fetchTableData();
  } catch (error) {
    console.error("表单提交失败:", error);
    ElMessage.error(isEditMode.value ? "更新失败" : "创建失败");
  } finally {
    formLoading.value = false;
  }
};

// 字符串验证函数
function isValidString(input: string | null | undefined): boolean {
  if (input !== null && input !== undefined && input.trim() !== "") {
    return true;
  } else {
    return false;
  }
}

// 重置表单
const resetForm = () => {
  formRef.value?.clearValidate();
  Object.assign(formData, {
    indexNo: "",
    reds: "",
    blues: "",
    numbers: ""
  });
  formLotteryTypeValue.value = "kl8";
  currentEditId.value = null;
};

// 格式化日期时间
const formatDateTime = (dateTime: string) => {
  if (!dateTime) return "";
  return new Date(dateTime).toLocaleString("zh-CN");
};

// 复式投注创建成功处理
const handleCompoundCreated = (result: any) => {
  const totalCombinations =
    result?.TotalCombinations || result?.totalCombinations || 0;
  ElMessage.success(`复式投注创建成功，共 ${totalCombinations} 种组合`);
  compoundDialogVisible.value = false;
  fetchTableData();
};

// 监听彩票类型变化，自动填充最新期号
watch(formLotteryTypeValue, async newValue => {
  if (newValue && (singleDialogVisible.value || formDialogVisible.value)) {
    try {
      // 获取彩票类型的中文名称
      const lotteryType = formLotteryTypeItems.value.find(
        item => item.value === newValue
      );

      if (lotteryType) {
        // 调用API获取最新期号
        const latestIndexNo = await lotteryApi.getLatestIndexNoByType(
          lotteryType.label
        );

        // 如果有最新期号，则填充到期号输入框
        if (latestIndexNo > 0) {
          formData.indexNo = latestIndexNo.toString();
        }
      }
    } catch (error) {
      console.error("获取最新期号失败:", error);
      // 静默失败，不影响用户操作
    }
  }
});

// 生命周期
onMounted(async () => {
  // 获取彩票常量
  try {
    const consts: ConstsDto[] = await lotteryApi.getLotteryConsts();
    if (consts && Array.isArray(consts) && consts.length > 0) {
      consts.forEach((item: any) => {
        formLotteryTypeItems.value.push({
          value: item.lotteryTypeEng,
          label: item.lotteryType
        });
      });
    }
  } catch (error) {
    console.error("获取彩票常量失败:", error);
    // 设置默认值
    formLotteryTypeItems.value = [
      { value: "ssq", label: "双色球" },
      { value: "kl8", label: "快乐8" }
    ];
  }

  fetchTableData();
});
</script>

<style scoped>
.lottery-container {
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

.table-container {
  width: 100%;
  overflow-x: auto;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.red-number {
  font-weight: bold;
  color: #f56c6c;
}

.blue-number {
  font-weight: bold;
  color: #409eff;
}

.botton-area {
  display: flex;
  width: 100%;
  margin-left: unset;
  font-weight: bold;
}

.select-area {
  width: 100%;
}

.input-area {
  width: 100%;
}

.margin-left-12 {
  width: 100%;
  margin-right: 1.1rem;
  margin-left: 0.375rem;
}

.m-1 {
  margin: 0.25rem;
}

.m-2 {
  margin: 0.5rem;
}

.dialog-content {
  padding: 10px 0;
}
</style>
