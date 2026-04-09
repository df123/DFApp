<template>
  <div class="gasoline-price-container">
    <el-card>
      <template #header>
        <div class="card-header">
          <span class="card-title">油价列表</span>
          <el-button
            type="primary"
            :loading="refreshing"
            @click="handleRefresh"
          >
            <el-icon><Refresh /></el-icon>
            刷新油价
          </el-button>
        </div>
      </template>

      <div class="filter-bar">
        <el-form :inline="true" :model="filterForm">
          <el-form-item label="省份">
            <el-select
              v-model="filterForm.province"
              placeholder="请选择省份"
              clearable
              style="width: 200px"
              @change="handleSearch"
            >
              <el-option label="全部" value="" />
              <el-option label="北京" value="北京" />
              <el-option label="上海" value="上海" />
              <el-option label="广东" value="广东" />
              <el-option label="山东" value="山东" />
              <el-option label="浙江" value="浙江" />
              <el-option label="江苏" value="江苏" />
              <el-option label="河北" value="河北" />
              <el-option label="河南" value="河南" />
              <el-option label="湖北" value="湖北" />
              <el-option label="湖南" value="湖南" />
              <el-option label="四川" value="四川" />
              <el-option label="重庆" value="重庆" />
            </el-select>
          </el-form-item>
          <el-form-item label="日期范围">
            <el-date-picker
              v-model="dateRange"
              type="daterange"
              range-separator="至"
              start-placeholder="开始日期"
              end-placeholder="结束日期"
              @change="handleSearch"
            />
          </el-form-item>
        </el-form>
      </div>

      <el-table
        v-loading="loading"
        :data="priceList"
        stripe
        border
        style="width: 100%"
      >
        <el-table-column prop="province" label="省份" min-width="100" />
        <el-table-column prop="date" label="日期" min-width="120">
          <template #default="{ row }">
            {{ formatDate(row.date) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price0H"
          label="0号柴油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price0H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price89H"
          label="89号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price89H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price90H"
          label="90号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price90H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price92H"
          label="92号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price92H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price93H"
          label="93号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price93H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price95H"
          label="95号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price95H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price97H"
          label="97号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price97H) }}
          </template>
        </el-table-column>
        <el-table-column
          prop="price98H"
          label="98号汽油"
          min-width="110"
          align="right"
        >
          <template #default="{ row }">
            {{ formatPrice(row.price98H) }}
          </template>
        </el-table-column>
        <el-table-column prop="creationTime" label="创建时间" min-width="180">
          <template #default="{ row }">
            {{ formatDateTime(row.creationTime) }}
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="handleSizeChange"
        @current-change="handlePageChange"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { Refresh } from "@element-plus/icons-vue";
import { gasolinePriceApi } from "@/api/electric-vehicle";
import type { GasolinePriceDto } from "@/types/api";

const filterForm = ref({
  province: ""
});

const dateRange = ref<[Date, Date] | null>(null);

const priceList = ref<GasolinePriceDto[]>([]);
const loading = ref(false);
const refreshing = ref(false);

const currentPage = ref(1);
const pageSize = ref(20);
const total = ref(0);

const loadPrices = async () => {
  loading.value = true;
  try {
    const params: any = {
      skipCount: (currentPage.value - 1) * pageSize.value,
      maxResultCount: pageSize.value
    };

    if (filterForm.value.province) {
      params.province = filterForm.value.province;
    }

    if (dateRange.value && dateRange.value.length === 2) {
      params.startDate = dateRange.value[0].toISOString();
      params.endDate = dateRange.value[1].toISOString();
    }

    const result = await gasolinePriceApi.getPrices(params);
    priceList.value = result.items || [];
    total.value = result.totalCount || 0;
  } catch (error) {
    console.error("加载油价失败:", error);
    ElMessage.error("加载油价失败");
  } finally {
    loading.value = false;
  }
};

const handleSearch = () => {
  currentPage.value = 1;
  loadPrices();
};

const handleRefresh = async () => {
  refreshing.value = true;
  try {
    await gasolinePriceApi.refreshPrices();
    ElMessage.success("油价刷新成功");
    loadPrices();
  } catch (error) {
    console.error("刷新油价失败:", error);
    ElMessage.error("刷新油价失败");
  } finally {
    refreshing.value = false;
  }
};

const handleSizeChange = (size: number) => {
  pageSize.value = size;
  currentPage.value = 1;
  loadPrices();
};

const handlePageChange = (page: number) => {
  currentPage.value = page;
  loadPrices();
};

const formatDate = (date: string) => {
  if (!date) return "-";
  return new Date(date).toLocaleDateString("zh-CN");
};

const formatDateTime = (dateTime: string) => {
  if (!dateTime) return "-";
  return new Date(dateTime).toLocaleString("zh-CN");
};

const formatPrice = (price: number | undefined) => {
  if (price === undefined || price === null) return "-";
  return `￥${price.toFixed(2)}/升`;
};

onMounted(() => {
  loadPrices();
});
</script>

<style scoped>
.gasoline-price-container {
  padding: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
}

.filter-bar {
  margin-bottom: 20px;
}
</style>
