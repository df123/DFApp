<template>
  <div class="compound-lottery-container">
    <div class="card-header">
      <el-button
        type="primary"
        :disabled="!canCalculate"
        :loading="calculating"
        @click="handleCalculate"
      >
        <el-icon><Operation /></el-icon>
        计算并保存
      </el-button>
    </div>

    <el-form :model="formData" label-width="80px" class="compound-form">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="彩票类型" required>
            <el-select
              v-model="formData.LotteryType"
              placeholder="请选择彩票类型"
              @change="onLotteryTypeChange"
            >
              <el-option label="双色球" value="ssq" />
              <el-option label="快乐8" value="kl8" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="期号" required>
            <el-input
              v-model="formData.Period"
              placeholder="请输入期号"
              type="number"
            />
          </el-form-item>
        </el-col>
      </el-row>

      <!-- 双色球复式投注 -->
      <div v-if="formData.LotteryType === 'ssq'" class="lottery-section">
        <h4>双色球复式投注</h4>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="红球号码" required>
              <el-input
                v-model="redNumbersText"
                type="textarea"
                :rows="4"
                placeholder="请输入红球号码，用空格或逗号分隔（最少6个）"
                @input="parseRedNumbers"
              />
              <div class="number-hint">
                已选择：{{ formData.RedNumbers.length }} 个红球
                <span v-if="formData.RedNumbers.length < 6" class="error-text">
                  （最少需要6个）
                </span>
              </div>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="蓝球号码" required>
              <el-input
                v-model="blueNumbersText"
                type="textarea"
                :rows="4"
                placeholder="请输入蓝球号码，用空格或逗号分隔（最少1个）"
                @input="parseBlueNumbers"
              />
              <div class="number-hint">
                已选择：{{ formData.BlueNumbers.length }} 个蓝球
                <span v-if="formData.BlueNumbers.length < 1" class="error-text">
                  （最少需要1个）
                </span>
              </div>
            </el-form-item>
          </el-col>
        </el-row>
      </div>

      <!-- 快乐8复式投注 -->
      <div v-if="formData.LotteryType === 'kl8'" class="lottery-section">
        <h4>快乐8复式投注</h4>
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="玩法类型" required>
              <el-select
                v-model="formData.PlayType"
                placeholder="请选择玩法类型"
              >
                <el-option
                  v-for="playType in playTypes"
                  :key="playType.value"
                  :label="playType.label"
                  :value="playType.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="投注金额">
              <div class="bet-amount">每注 2 元</div>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="号码" required>
          <el-input
            v-model="kl8NumbersText"
            type="textarea"
            :rows="6"
            placeholder="请输入快乐8号码，用空格或逗号分隔"
            @input="parseKL8Numbers"
          />
          <div class="number-hint">
            已选择：{{ formData.KL8Numbers.length }} 个号码
            <span
              v-if="formData.KL8Numbers.length < requiredKL8Count"
              class="error-text"
            >
              （{{ formData.PlayType || "选一" }}最少需要{{
                requiredKL8Count
              }}个）
            </span>
          </div>
        </el-form-item>
      </div>
    </el-form>

    <!-- 验证错误信息 -->
    <div v-if="validationErrors.length > 0" class="validation-errors">
      <h5>验证错误：</h5>
      <ul>
        <li
          v-for="(error, index) in validationErrors"
          :key="index"
          class="error-text"
        >
          {{ error }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, nextTick } from "vue";
import { ElMessage } from "element-plus";
import { Operation } from "@element-plus/icons-vue";
import { lotteryApi } from "@/api/lottery";

// 快乐8玩法类型
const playTypes = [
  { value: 1, label: "选一" },
  { value: 2, label: "选二" },
  { value: 3, label: "选三" },
  { value: 4, label: "选四" },
  { value: 5, label: "选五" },
  { value: 6, label: "选六" },
  { value: 7, label: "选七" },
  { value: 8, label: "选八" },
  { value: 9, label: "选九" },
  { value: 10, label: "选十" }
];

// 表单数据
const formData = reactive({
  Period: 0,
  LotteryType: "",
  RedNumbers: [] as string[],
  BlueNumbers: [] as string[],
  KL8Numbers: [] as string[],
  PlayType: undefined as number | undefined
});

// 文本输入
const redNumbersText = ref("");
const blueNumbersText = ref("");
const kl8NumbersText = ref("");

// 状态
const calculating = ref(false);
const validationErrors = ref<string[]>([]);

// 计算属性
const requiredKL8Count = computed(() => {
  return formData.PlayType || 1;
});

const canCalculate = computed(() => {
  if (!formData.Period || !formData.LotteryType) {
    return false;
  }

  if (formData.LotteryType === "ssq") {
    return formData.RedNumbers.length >= 6 && formData.BlueNumbers.length >= 1;
  } else if (formData.LotteryType === "kl8") {
    return formData.KL8Numbers.length >= requiredKL8Count.value;
  }

  return false;
});

// 方法
const parseRedNumbers = () => {
  const numbers = redNumbersText.value
    .split(/[\s,]+/)
    .map(n => n.trim())
    .filter(n => n && !isNaN(Number(n)))
    .map(n => Number(n).toString().padStart(2, "0")); // 格式化为两位数
  formData.RedNumbers = [...new Set(numbers)]; // 去重
};

const parseBlueNumbers = () => {
  const numbers = blueNumbersText.value
    .split(/[\s,]+/)
    .map(n => n.trim())
    .filter(n => n && !isNaN(Number(n)))
    .map(n => Number(n).toString().padStart(2, "0")); // 格式化为两位数
  formData.BlueNumbers = [...new Set(numbers)]; // 去重
};

const parseKL8Numbers = () => {
  const numbers = kl8NumbersText.value
    .split(/[\s,]+/)
    .map(n => n.trim())
    .filter(n => n && !isNaN(Number(n)))
    .map(n => Number(n).toString().padStart(2, "0")); // 格式化为两位数
  formData.KL8Numbers = [...new Set(numbers)]; // 去重
};

const onLotteryTypeChange = () => {
  // 清空之前的输入
  redNumbersText.value = "";
  blueNumbersText.value = "";
  kl8NumbersText.value = "";
  formData.RedNumbers = [];
  formData.BlueNumbers = [];
  formData.KL8Numbers = [];
  formData.PlayType = undefined;
  validationErrors.value = [];

  // 获取最新期号并加一
  loadLatestIndexNo();
};

// 加载最新期号并加一
const loadLatestIndexNo = async () => {
  if (formData.LotteryType) {
    try {
      // 获取彩票类型的中文名称
      const lotteryTypeMap: { [key: string]: string } = {
        ssq: "双色球",
        kl8: "快乐8"
      };

      const lotteryTypeName = lotteryTypeMap[formData.LotteryType];
      if (lotteryTypeName) {
        // 调用API获取最新期号
        const latestIndexNo =
          await lotteryApi.getLatestIndexNoByType(lotteryTypeName);

        // 如果有最新期号，则加一后填充到期号输入框
        if (latestIndexNo > 0) {
          formData.Period = latestIndexNo + 1;
        } else {
          // 如果没有最新期号，设置为1
          formData.Period = 1;
        }
      }
    } catch (error) {
      console.error("获取最新期号失败:", error);
      // 静默失败，不影响用户操作，设置默认期号
      formData.Period = 1;
    }
  }
};

const validateInput = (): string[] => {
  const errors: string[] = [];

  if (!formData.Period || formData.Period <= 0) {
    errors.push("期号必须大于0");
  }

  if (!formData.LotteryType) {
    errors.push("请选择彩票类型");
  }

  if (formData.LotteryType === "ssq") {
    if (formData.RedNumbers.length < 6) {
      errors.push("双色球复式投注红球数量不能少于6个");
    }

    if (formData.BlueNumbers.length < 1) {
      errors.push("双色球复式投注蓝球数量不能少于1个");
    }

    // 验证红球范围 (1-33)
    formData.RedNumbers.forEach(red => {
      const num = Number(red);
      if (num < 1 || num > 33) {
        errors.push(`红球号码 ${red} 超出范围 (1-33)`);
      }
    });

    // 验证蓝球范围 (1-16)
    formData.BlueNumbers.forEach(blue => {
      const num = Number(blue);
      if (num < 1 || num > 16) {
        errors.push(`蓝球号码 ${blue} 超出范围 (1-16)`);
      }
    });

    // 检查红蓝球重复
    const duplicate = formData.RedNumbers.find(red =>
      formData.BlueNumbers.includes(red)
    );
    if (duplicate) {
      errors.push(`红球和蓝球不能重复 (${duplicate})`);
    }
  } else if (formData.LotteryType === "kl8") {
    if (!formData.PlayType) {
      errors.push("请选择快乐8玩法类型");
    }

    if (formData.KL8Numbers.length < requiredKL8Count.value) {
      errors.push(`快乐8复式投注号码数量不能少于${requiredKL8Count.value}个`);
    }

    // 验证号码范围 (1-80)
    formData.KL8Numbers.forEach(number => {
      const num = Number(number);
      if (num < 1 || num > 80) {
        errors.push(`快乐8号码 ${number} 超出范围 (1-80)`);
      }
    });
  }

  return errors;
};

const handleCalculate = async () => {
  validationErrors.value = validateInput();
  if (validationErrors.value.length > 0) {
    return;
  }

  calculating.value = true;
  try {
    const result = await lotteryApi.calculateCompoundCombination(formData);
    ElMessage.success("计算并保存完成");
    // 清空表单
    resetForm();
    // 触发父组件刷新事件
    emit("created", result);
  } catch (error) {
    console.error("计算并保存失败:", error);
    ElMessage.error("计算并保存失败");
  } finally {
    calculating.value = false;
  }
};

const resetForm = () => {
  formData.Period = 0;
  formData.LotteryType = "";
  formData.RedNumbers = [];
  formData.BlueNumbers = [];
  formData.KL8Numbers = [];
  formData.PlayType = undefined;
  redNumbersText.value = "";
  blueNumbersText.value = "";
  kl8NumbersText.value = "";
  validationErrors.value = [];
};

// 监听玩法类型变化
watch(
  () => formData.PlayType,
  () => {
    validationErrors.value = [];
  }
);

// 监听号码输入变化
watch(
  [
    () => formData.RedNumbers,
    () => formData.BlueNumbers,
    () => formData.KL8Numbers
  ],
  () => {
    validationErrors.value = [];
  }
);

// 定义事件
const emit = defineEmits<{
  created: [result: any];
}>();

// 暴露方法给父组件
defineExpose({
  resetForm
});
</script>

<style scoped>
.compound-lottery-container {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  flex-direction: row-reverse;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.compound-form {
  margin-bottom: 20px;
}

.lottery-section {
  padding: 20px;
  margin-top: 20px;
  background-color: #fafafa;
  border: 1px solid #ebeef5;
  border-radius: 4px;
}

.lottery-section h4 {
  margin: 0 0 20px;
  font-weight: 600;
  color: #409eff;
}

.number-hint {
  margin-top: 8px;
  font-size: 14px;
  color: #606266;
}

.error-text {
  font-weight: bold;
  color: #f56c6c;
}

.validation-errors {
  padding: 15px;
  margin-top: 20px;
  background-color: #fef0f0;
  border: 1px solid #fde2e2;
  border-radius: 4px;
}

.validation-errors h5 {
  margin: 0 0 10px;
  font-weight: 600;
  color: #f56c6c;
}

.validation-errors ul {
  padding-left: 20px;
  margin: 0;
}

.bet-amount {
  padding: 10px;
  font-weight: 600;
  color: #606266;
  text-align: center;
  background-color: #f5f7fa;
  border-radius: 4px;
}
</style>
