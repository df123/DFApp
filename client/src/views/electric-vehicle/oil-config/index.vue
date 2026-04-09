<template>
  <div class="oil-config-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">油车参数配置</span>
        </div>
      </template>

      <el-form :model="configForm" label-width="120px" style="max-width: 600px">
        <el-form-item label="所在省份">
          <el-select v-model="configForm.province" placeholder="请选择省份">
            <el-option label="北京" value="北京" />
            <el-option label="上海" value="上海" />
            <el-option label="广东" value="广东" />
            <el-option label="山东" value="山东" />
            <el-option label="浙江" value="浙江" />
            <el-option label="江苏" value="江苏" />
          </el-select>
        </el-form-item>

        <el-form-item label="汽油标号">
          <el-select
            v-model="configForm.gasolineGrade"
            placeholder="请选择汽油标号"
          >
            <el-option label="92号" :value="92" />
            <el-option label="95号" :value="95" />
            <el-option label="98号" :value="98" />
          </el-select>
        </el-form-item>

        <el-form-item label="百公里油耗">
          <el-input-number
            v-model="configForm.fuelConsumption"
            :min="0"
            :max="50"
            :precision="1"
            :step="0.1"
          />
          <span style="margin-left: 10px">升</span>
        </el-form-item>

        <el-form-item label="API Key">
          <el-input
            v-model="configForm.apiKey"
            placeholder="请输入探书API Key"
            type="password"
            show-password
          />
          <div style="margin-top: 5px; font-size: 12px; color: #909399">
            获取地址：https://www.tanshuapi.com/
          </div>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="saving" @click="handleSave">
            保存配置
          </el-button>
          <el-button :loading="refreshing" @click="handleRefreshPrice">
            刷新油价
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { configurationApi } from "@/api/configuration";
import { gasolinePriceApi } from "@/api/electric-vehicle";
import type { GasolineGrade } from "@/types/api";

const configForm = ref({
  province: "山东",
  gasolineGrade: 95,
  fuelConsumption: 8,
  apiKey: ""
});

const saving = ref(false);
const refreshing = ref(false);

const loadConfig = async () => {
  try {
    const result = await configurationApi.getConfigurations({
      moduleName: "DFApp.ElectricVehicle",
      maxResultCount: 100
    });
    result.items.forEach((config: any) => {
      if (config.configurationName === "OilProvince") {
        configForm.value.province = config.configurationValue;
      } else if (config.configurationName === "OilGasolineGrade") {
        configForm.value.gasolineGrade = parseInt(
          config.configurationValue
        ) as GasolineGrade;
      } else if (config.configurationName === "OilFuelConsumption") {
        configForm.value.fuelConsumption = parseFloat(
          config.configurationValue
        );
      } else if (config.configurationName === "GasPriceApiKey") {
        configForm.value.apiKey = config.configurationValue;
      }
    });
  } catch (error) {
    console.error("加载配置失败:", error);
  }
};

const handleSave = async () => {
  saving.value = true;
  try {
    const configs = [
      {
        configurationName: "OilProvince",
        value: configForm.value.province,
        remark: "油车所在省份"
      },
      {
        configurationName: "OilGasolineGrade",
        value: configForm.value.gasolineGrade.toString(),
        remark: "汽油标号"
      },
      {
        configurationName: "OilFuelConsumption",
        value: configForm.value.fuelConsumption.toString(),
        remark: "百公里油耗（升）"
      },
      {
        configurationName: "GasPriceApiKey",
        value: configForm.value.apiKey,
        remark: "油价API Key"
      }
    ];

    const existingConfigs = await configurationApi.getConfigurations({
      moduleName: "DFApp.ElectricVehicle",
      maxResultCount: 100
    });
    const existingMap = new Map(
      existingConfigs.items.map((c: any) => [c.configurationName, c])
    );

    for (const config of configs) {
      const existing = existingMap.get(config.configurationName);
      if (existing) {
        await configurationApi.updateConfiguration(existing.id, {
          configurationName: config.configurationName,
          moduleName: "DFApp.ElectricVehicle",
          configurationValue: config.value,
          remark: config.remark
        });
      } else {
        await configurationApi.createConfiguration({
          configurationName: config.configurationName,
          moduleName: "DFApp.ElectricVehicle",
          configurationValue: config.value,
          remark: config.remark
        });
      }
    }

    ElMessage.success("配置保存成功");
  } catch (error) {
    console.error("保存配置失败:", error);
    ElMessage.error("保存配置失败");
  } finally {
    saving.value = false;
  }
};

const handleRefreshPrice = async () => {
  refreshing.value = true;
  try {
    await gasolinePriceApi.refreshPrices();
    ElMessage.success("油价刷新成功");
  } catch (error) {
    console.error("刷新油价失败:", error);
    ElMessage.error("刷新油价失败");
  } finally {
    refreshing.value = false;
  }
};

onMounted(() => {
  loadConfig();
});
</script>

<style scoped>
.oil-config-container {
  padding: 20px;
}
</style>
