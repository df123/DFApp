<template>
    <div class="user-management-container">
        <el-card>
            <template #header>
                <div class="card-header">
                    <span class="card-title">用户管理</span>
                    <el-button v-permission="['DFApp.UserManagement.Create']" type="primary" @click="handleCreate">
                        <el-icon>
                            <Plus />
                        </el-icon>
                        新增用户
                    </el-button>
                </div>
            </template>

            <!-- 数据表格 -->
            <pure-table ref="tableRef" :loading="loading" :data="tableData" :columns="columns" :pagination="pagination"
                @page-size-change="handleSizeChange" @page-current-change="handleCurrentChange">
                <template #operation="{ row }">
                    <el-button v-permission="['DFApp.UserManagement.Update']" size="small" type="primary" link
                        @click="handleEdit(row)">
                        编辑
                    </el-button>
                    <el-button v-permission="['DFApp.UserManagement.ChangePassword']" size="small" type="warning" link
                        @click="handleChangePassword(row)">
                        修改密码
                    </el-button>
                    <el-button v-permission="['DFApp.UserManagement.Delete']" size="small" type="danger" link
                        @click="handleDelete(row)">
                        删除
                    </el-button>
                </template>
            </pure-table>
        </el-card>

        <!-- 新增/编辑对话框 -->
        <el-dialog v-model="dialogVisible" :title="dialogTitle" width="600px" :before-close="handleClose">
            <el-form ref="formRef" :model="formData" :rules="formRules" label-width="120px">
                <el-form-item label="用户名" prop="userName">
                    <el-input v-model="formData.userName" placeholder="请输入用户名" maxlength="50" show-word-limit />
                </el-form-item>
                <el-form-item label="邮箱" prop="email">
                    <el-input v-model="formData.email" placeholder="请输入邮箱" maxlength="256" show-word-limit />
                </el-form-item>
                <el-form-item v-if="!currentEditId" label="密码" prop="password">
                    <el-input v-model="formData.password" type="password" placeholder="请输入密码" maxlength="100"
                        show-password />
                </el-form-item>
                <el-form-item label="是否激活">
                    <el-switch v-model="formData.isActive" />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="handleClose">取消</el-button>
                <el-button type="primary" :loading="submitting" @click="handleSubmit">
                    确定
                </el-button>
            </template>
        </el-dialog>

        <!-- 修改密码对话框 -->
        <el-dialog v-model="passwordDialogVisible" title="修改密码" width="500px" :before-close="handlePasswordClose">
            <el-form ref="passwordFormRef" :model="passwordFormData" :rules="passwordFormRules" label-width="120px">
                <el-form-item label="用户名">
                    <el-input v-model="passwordFormData.userName" disabled />
                </el-form-item>
                <el-form-item label="新密码" prop="newPassword">
                    <el-input v-model="passwordFormData.newPassword" type="password" placeholder="请输入新密码"
                        maxlength="100" show-password />
                </el-form-item>
                <el-form-item label="确认密码" prop="confirmPassword">
                    <el-input v-model="passwordFormData.confirmPassword" type="password" placeholder="请再次输入新密码"
                        maxlength="100" show-password />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="handlePasswordClose">取消</el-button>
                <el-button type="primary" :loading="passwordSubmitting" @click="handlePasswordSubmit">
                    确定
                </el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import {
    ElMessage,
    ElMessageBox,
    type FormInstance,
    type FormRules
} from "element-plus";
import { Plus } from "@element-plus/icons-vue";
import { http } from "@/utils/http";

// 响应式数据
const loading = ref(false);
const submitting = ref(false);
const passwordSubmitting = ref(false);
const dialogVisible = ref(false);
const passwordDialogVisible = ref(false);
const tableRef = ref();
const formRef = ref<FormInstance>();
const passwordFormRef = ref<FormInstance>();

// 分页数据
const pagination = reactive({
    pageSize: 10,
    currentPage: 1,
    total: 0
});

// 表格数据
const tableData = ref<any[]>([]);

// 表单数据
const formData = reactive({
    userName: "",
    email: "",
    password: "",
    isActive: true
});

// 修改密码表单数据
const passwordFormData = reactive({
    userId: "",
    userName: "",
    newPassword: "",
    confirmPassword: ""
});

// 当前编辑的ID
const currentEditId = ref<string | null>(null);

// 计算属性
const dialogTitle = computed(() =>
    currentEditId.value ? "编辑用户" : "新增用户"
);

// 表单验证规则
const formRules: FormRules = {
    userName: [
        { required: true, message: "请输入用户名", trigger: "blur" },
        { max: 50, message: "用户名长度不能超过50个字符", trigger: "blur" }
    ],
    email: [
        { required: true, message: "请输入邮箱", trigger: "blur" },
        { type: "email", message: "邮箱格式不正确", trigger: "blur" }
    ],
    password: [
        { required: true, message: "请输入密码", trigger: "blur" },
        { min: 6, max: 100, message: "密码长度必须在6-100个字符之间", trigger: "blur" }
    ]
};

// 修改密码表单验证规则
const passwordFormRules: FormRules = {
    newPassword: [
        { required: true, message: "请输入新密码", trigger: "blur" },
        { min: 6, max: 100, message: "密码长度必须在6-100个字符之间", trigger: "blur" }
    ],
    confirmPassword: [
        { required: true, message: "请再次输入新密码", trigger: "blur" },
        {
            validator: (rule, value, callback) => {
                if (value !== passwordFormData.newPassword) {
                    callback(new Error("两次输入的密码不一致"));
                } else {
                    callback();
                }
            },
            trigger: "blur"
        }
    ]
};

// 表格列配置
const columns = ref([
    {
        label: "ID",
        prop: "id",
        width: 250
    },
    {
        label: "用户名",
        prop: "userName",
        minWidth: 120
    },
    {
        label: "邮箱",
        prop: "email",
        minWidth: 180
    },
    {
        label: "是否激活",
        prop: "isActive",
        width: 100,
        formatter: (row: any) => (row.isActive ? "是" : "否")
    },
    {
        label: "创建时间",
        prop: "creationTime",
        width: 180,
        formatter: (row: any) => {
            return row.creationTime
                ? new Date(row.creationTime).toLocaleString()
                : "-";
        }
    },
    {
        label: "修改时间",
        prop: "lastModificationTime",
        width: 180,
        formatter: (row: any) => {
            return row.lastModificationTime
                ? new Date(row.lastModificationTime).toLocaleString()
                : "-";
        }
    },
    {
        label: "操作",
        slot: "operation",
        fixed: "right",
        width: 180
    }
]);

// 方法
const loadTableData = async () => {
    loading.value = true;
    try {
        const params = {
            pageIndex: pagination.currentPage,
            pageSize: pagination.pageSize,
            sorting: "creationTime desc"
        };

        const result = await http.get<any, any>(
            "/api/app/user-management",
            { params }
        );
        tableData.value = result.items;
        pagination.total = result.totalCount;
    } catch (error) {
        console.error("加载用户数据失败:", error);
        ElMessage.error("加载用户数据失败");
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
    Object.assign(formData, {
        userName: "",
        email: "",
        password: "",
        isActive: true
    });
    dialogVisible.value = true;
};

const handleEdit = (row: any) => {
    currentEditId.value = row.id;
    Object.assign(formData, {
        userName: row.userName,
        email: row.email,
        password: "",
        isActive: row.isActive
    });
    dialogVisible.value = true;
};

const handleChangePassword = (row: any) => {
    Object.assign(passwordFormData, {
        userId: row.id,
        userName: row.userName,
        newPassword: "",
        confirmPassword: ""
    });
    passwordDialogVisible.value = true;
};

const handleDelete = async (row: any) => {
    try {
        await ElMessageBox.confirm(
            `确定要删除用户 "${row.userName}" 吗？`,
            "删除确认",
            {
                confirmButtonText: "确定",
                cancelButtonText: "取消",
                type: "warning"
            }
        );

        await http.delete(`/api/app/user-management/${row.id}`);
        ElMessage.success("删除成功");
        loadTableData();
    } catch (error) {
        if (error !== "cancel") {
            console.error("删除用户失败:", error);
            ElMessage.error("删除用户失败");
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
            await http.put(`/api/app/user-management/${currentEditId.value}`, {
                data: {
                    userName: formData.userName,
                    email: formData.email,
                    isActive: formData.isActive
                }
            });
            ElMessage.success("更新成功");
        } else {
            await http.post("/api/app/user-management", {
                data: {
                    userName: formData.userName,
                    email: formData.email,
                    password: formData.password,
                    isActive: formData.isActive
                }
            });
            ElMessage.success("创建成功");
        }

        dialogVisible.value = false;
        loadTableData();
    } catch (error) {
        console.error("保存用户失败:", error);
        ElMessage.error("保存用户失败");
    } finally {
        submitting.value = false;
    }
};

const handlePasswordClose = () => {
    passwordDialogVisible.value = false;
    passwordFormRef.value?.clearValidate();
};

const handlePasswordSubmit = async () => {
    if (!passwordFormRef.value) return;

    const valid = await passwordFormRef.value.validate();
    if (!valid) return;

    passwordSubmitting.value = true;
    try {
        await http.post("/api/app/user-management/change-password", {
            data: {
                userId: passwordFormData.userId,
                newPassword: passwordFormData.newPassword
            }
        });
        ElMessage.success("密码修改成功");
        passwordDialogVisible.value = false;
    } catch (error) {
        console.error("修改密码失败:", error);
        ElMessage.error("修改密码失败");
    } finally {
        passwordSubmitting.value = false;
    }
};

// 生命周期
onMounted(() => {
    loadTableData();
});
</script>

<style scoped>
.user-management-container {
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
