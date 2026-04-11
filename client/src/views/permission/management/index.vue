<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import {
  ElMessage,
  ElMessageBox,
  type FormInstance,
  type FormRules
} from "element-plus";
import { Search } from "@element-plus/icons-vue";
import { permissionApi, type PermissionGroupDto } from "@/api/permission";
import {
  roleApi,
  type RoleDto,
  type CreateRoleDto,
  type UpdateRoleDto
} from "@/api/role";
import {
  identityUserApi,
  type IdentityUserDto,
  type CreateIdentityUserDto,
  type UpdateIdentityUserDto
} from "@/api/identity-user";

defineOptions({
  name: "PermissionManagement"
});

const activeTab = ref("role");
const loading = ref(false);
const saving = ref(false);

// 角色列表
const roles = ref<RoleDto[]>([]);

// 用户列表
const users = ref<IdentityUserDto[]>([]);

// 权限对话框
const permissionDialogVisible = ref(false);
const currentProvider = ref<{
  name: string;
  displayName: string;
  type: "Role" | "User";
} | null>(null);
const permissionData = ref<{
  entityDisplayName: string;
  groups: PermissionGroupDto[];
} | null>(null);
const searchText = ref("");

// 角色编辑对话框
const roleDialogVisible = ref(false);
const roleFormRef = ref<FormInstance>();
const isEditingRole = ref(false);
const currentRole = ref<RoleDto | null>(null);
const roleForm = ref<CreateRoleDto & { id?: string }>({
  name: "",
  isDefault: false,
  isPublic: false
});
const roleFormRules: FormRules = {
  name: [{ required: true, message: "请输入角色名称", trigger: "blur" }]
};

// 用户编辑对话框
const userDialogVisible = ref(false);
const userFormRef = ref<FormInstance>();
const isEditingUser = ref(false);
const currentUser = ref<IdentityUserDto | null>(null);
const userForm = ref<
  CreateIdentityUserDto & { id?: string; confirmPassword?: string }
>({
  userName: "",
  name: "",
  surname: "",
  email: "",
  phoneNumber: "",
  password: "",
  confirmPassword: "",
  roleNames: [],
  isActive: true
});
const userFormRules: FormRules = {
  userName: [{ required: true, message: "请输入用户名", trigger: "blur" }],
  email: [
    { required: true, message: "请输入邮箱地址", trigger: "blur" },
    { type: "email", message: "请输入正确的邮箱地址", trigger: "blur" }
  ],
  password: [
    { required: true, message: "请输入密码", trigger: "blur" },
    { min: 6, message: "密码长度至少6位", trigger: "blur" }
  ],
  confirmPassword: [
    { required: true, message: "请确认密码", trigger: "blur" },
    {
      validator: (rule, value, callback) => {
        if (value !== userForm.value.password) {
          callback(new Error("两次输入的密码不一致"));
        } else {
          callback();
        }
      },
      trigger: "blur"
    }
  ]
};

// 获取角色列表
async function fetchRoles() {
  try {
    loading.value = true;
    const data = await roleApi.getAllRoles();
    roles.value = data;
  } catch (error) {
    console.error("Failed to fetch roles:", error);
    ElMessage.error("获取角色列表失败");
  } finally {
    loading.value = false;
  }
}

// 获取用户列表
async function fetchUsers() {
  try {
    loading.value = true;
    const data = await identityUserApi.getAllUsers();
    users.value = data;
  } catch (error) {
    console.error("Failed to fetch users:", error);
    ElMessage.error("获取用户列表失败");
  } finally {
    loading.value = false;
  }
}

// ===== 角色管理 =====

// 打开新增角色对话框
function openCreateRoleDialog() {
  isEditingRole.value = false;
  currentRole.value = null;
  roleForm.value = {
    name: "",
    isDefault: false,
    isPublic: false
  };
  roleDialogVisible.value = true;
}

// 打开编辑角色对话框
function openEditRoleDialog(role: RoleDto) {
  isEditingRole.value = true;
  currentRole.value = role;
  roleForm.value = {
    id: role.id,
    name: role.name,
    isDefault: role.isDefault || false,
    isPublic: role.isPublic || false
  };
  roleDialogVisible.value = true;
}

// 保存角色
async function saveRole() {
  if (!roleFormRef.value) return;

  try {
    await roleFormRef.value.validate();

    if (isEditingRole.value && roleForm.value.id) {
      // 编辑模式
      await roleApi.updateRole(roleForm.value.id, {
        name: roleForm.value.name,
        isDefault: roleForm.value.isDefault,
        isPublic: roleForm.value.isPublic
      });
      ElMessage.success("角色更新成功");
    } else {
      // 新增模式
      await roleApi.createRole({
        name: roleForm.value.name,
        isDefault: roleForm.value.isDefault,
        isPublic: roleForm.value.isPublic
      });
      ElMessage.success("角色创建成功");
    }

    roleDialogVisible.value = false;
    fetchRoles();
  } catch (error) {
    if (error !== false) {
      console.error("Failed to save role:", error);
      ElMessage.error(isEditingRole.value ? "更新角色失败" : "创建角色失败");
    }
  }
}

// 删除角色
async function deleteRole(role: RoleDto) {
  try {
    await ElMessageBox.confirm(
      `确定要删除角色 "${role.name}" 吗？`,
      "确认删除",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await roleApi.deleteRole(role.id);
    ElMessage.success("角色删除成功");
    fetchRoles();
  } catch (error) {
    if (error !== "cancel") {
      console.error("Failed to delete role:", error);
      ElMessage.error("删除角色失败");
    }
  }
}

// ===== 用户管理 =====

// 打开新增用户对话框
function openCreateUserDialog() {
  isEditingUser.value = false;
  currentUser.value = null;
  userForm.value = {
    userName: "",
    name: "",
    surname: "",
    email: "",
    phoneNumber: "",
    password: "",
    confirmPassword: "",
    roleNames: [],
    isActive: true
  };
  userFormRef.value?.resetFields();
  userDialogVisible.value = true;
}

// 打开编辑用户对话框
function openEditUserDialog(user: IdentityUserDto) {
  isEditingUser.value = true;
  currentUser.value = user;
  userForm.value = {
    id: user.id,
    userName: user.userName,
    name: user.name,
    surname: user.surname,
    email: user.email,
    phoneNumber: user.phoneNumber,
    roleNames: user.roleNames || [],
    isActive: user.isActive
  };
  userDialogVisible.value = true;
}

// 保存用户
async function saveUser() {
  if (!userFormRef.value) return;

  try {
    await userFormRef.value.validate();

    if (isEditingUser.value && userForm.value.id) {
      // 编辑模式
      await identityUserApi.updateUser(userForm.value.id, {
        userName: userForm.value.userName,
        name: userForm.value.name,
        surname: userForm.value.surname,
        email: userForm.value.email,
        phoneNumber: userForm.value.phoneNumber,
        roleNames: userForm.value.roleNames,
        isActive: userForm.value.isActive
      });
      ElMessage.success("用户更新成功");
    } else {
      // 新增模式
      await identityUserApi.createUser({
        userName: userForm.value.userName,
        name: userForm.value.name,
        surname: userForm.value.surname,
        email: userForm.value.email,
        phoneNumber: userForm.value.phoneNumber,
        password: userForm.value.password,
        roleNames: userForm.value.roleNames,
        isActive: userForm.value.isActive
      });
      ElMessage.success("用户创建成功");
    }

    userDialogVisible.value = false;
    fetchUsers();
  } catch (error) {
    if (error !== false) {
      console.error("Failed to save user:", error);
      ElMessage.error(isEditingUser.value ? "更新用户失败" : "创建用户失败");
    }
  }
}

// 删除用户
async function deleteUser(user: IdentityUserDto) {
  try {
    await ElMessageBox.confirm(
      `确定要删除用户 "${user.userName}" 吗？`,
      "确认删除",
      {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }
    );

    await identityUserApi.deleteUser(user.id);
    ElMessage.success("用户删除成功");
    fetchUsers();
  } catch (error) {
    if (error !== "cancel") {
      console.error("Failed to delete user:", error);
      ElMessage.error("删除用户失败");
    }
  }
}

// ===== 权限管理 =====

// 打开权限对话框
async function openPermissionDialog(provider: {
  name: string;
  displayName: string;
  type: "Role" | "User";
}) {
  try {
    loading.value = true;
    currentProvider.value = provider;
    searchText.value = "";

    const data = await permissionApi.getPermissions(
      provider.type,
      provider.name
    );
    permissionData.value = data;
    permissionDialogVisible.value = true;
  } catch (error) {
    console.error("Failed to fetch permissions:", error);
    ElMessage.error("获取权限列表失败");
  } finally {
    loading.value = false;
  }
}

// 关闭权限对话框
function closePermissionDialog() {
  permissionDialogVisible.value = false;
  currentProvider.value = null;
  permissionData.value = null;
  searchText.value = "";
}

// 保存权限
async function savePermissions() {
  if (!permissionData.value || !currentProvider.value) return;

  try {
    await ElMessageBox.confirm("确定要保存权限更改吗？", "确认", {
      confirmButtonText: "确定",
      cancelButtonText: "取消",
      type: "warning"
    });

    saving.value = true;

    // 收集所有权限
    const permissions: { name: string; isGranted: boolean }[] = [];
    permissionData.value.groups.forEach(group => {
      group.permissions.forEach(permission => {
        permissions.push({
          name: permission.name,
          isGranted: permission.isGranted
        });
      });
    });

    // 根据提供者类型调用不同的更新方法
    if (currentProvider.value.type === "Role") {
      await permissionApi.updateRolePermissions(
        currentProvider.value.name,
        permissions
      );
    } else {
      await permissionApi.updateUserPermissions(
        currentProvider.value.name,
        permissions
      );
    }

    ElMessage.success("权限保存成功");
    closePermissionDialog();
  } catch (error) {
    if (error !== "cancel") {
      console.error("Failed to save permissions:", error);
      ElMessage.error("保存权限失败");
    }
  } finally {
    saving.value = false;
  }
}

// 切换权限状态
function togglePermission(
  group: PermissionGroupDto,
  permissionName: string,
  granted: boolean
) {
  const permission = group.permissions.find(p => p.name === permissionName);
  if (permission && !isPermissionDisabled(permission)) {
    permission.isGranted = granted;

    // 如果是启用权限，需要启用所有父权限
    if (granted && permission.parentName) {
      enableParentPermission(group, permission.parentName);
    }

    // 如果是禁用权限，需要禁用所有子权限
    if (!granted) {
      disableChildPermissions(group, permissionName);
    }
  }
}

// 启用父权限
function enableParentPermission(group: PermissionGroupDto, parentName: string) {
  const parent = group.permissions.find(p => p.name === parentName);
  if (parent) {
    parent.isGranted = true;
    if (parent.parentName) {
      enableParentPermission(group, parent.parentName);
    }
  }
}

// 禁用子权限
function disableChildPermissions(
  group: PermissionGroupDto,
  parentName: string
) {
  group.permissions.forEach(permission => {
    if (permission.parentName === parentName) {
      permission.isGranted = false;
      disableChildPermissions(group, permission.name);
    }
  });
}

// 全选/取消全选某个分组
function toggleGroup(group: PermissionGroupDto, granted: boolean) {
  group.permissions.forEach(permission => {
    if (!isPermissionDisabled(permission)) {
      permission.isGranted = granted;
    }
  });
}

// 全选/取消全选所有权限
function toggleAllPermissions(granted: boolean) {
  if (!permissionData.value) return;
  permissionData.value.groups.forEach(group => {
    toggleGroup(group, granted);
  });
}

// 计算每个分组的已选权限数
function getGroupGrantedCount(group: PermissionGroupDto): number {
  return group.permissions.filter(p => p.isGranted).length;
}

// 计算所有已选权限数
function getTotalGrantedCount(): number {
  if (!permissionData.value) return 0;
  return permissionData.value.groups.reduce(
    (sum, group) => sum + getGroupGrantedCount(group),
    0
  );
}

// 检查分组是否全部选中
function isGroupAllGranted(group: PermissionGroupDto): boolean {
  return group.permissions.every(p => p.isGranted);
}

// 检查分组是否部分选中
function isGroupIndeterminate(group: PermissionGroupDto): boolean {
  const grantedCount = getGroupGrantedCount(group);
  return grantedCount > 0 && grantedCount < group.permissions.length;
}

// 过滤权限
const filteredGroups = computed(() => {
  if (!permissionData.value) return [];

  const search = searchText.value.toLowerCase().trim();

  if (!search) {
    return permissionData.value.groups;
  }

  return permissionData.value.groups.map(group => ({
    ...group,
    permissions: group.permissions.filter(
      p =>
        p.displayName.toLowerCase().includes(search) ||
        p.name.toLowerCase().includes(search)
    )
  }));
});

// 检查权限是否被禁用
function isPermissionDisabled(permission: any): boolean {
  // 如果权限未被授予，则不禁用
  if (!permission.isGranted) {
    return false;
  }
  // 如果没有授予者信息，则不禁用
  if (
    !permission.grantedProviders ||
    permission.grantedProviders.length === 0
  ) {
    return false;
  }
  // 如果所有授予者都不是当前provider，则禁用（表示该权限是通过其他provider授予的，如通过角色授予给用户）
  return permission.grantedProviders.every(
    (p: any) => p.providerName !== currentProvider.value?.type
  );
}

// 获取权限显示名称（包含授予者信息）
function getPermissionDisplayName(permission: any): string {
  if (!isPermissionDisabled(permission)) {
    return permission.displayName;
  }

  // 获取非当前provider的授予者
  const otherProviders = (permission.grantedProviders || [])
    .filter((p: any) => p.providerName !== currentProvider.value?.type)
    .map((p: any) => {
      // 将provider名称转换为更友好的显示
      if (p.providerName === "Role") return `角色: ${p.providerKey}`;
      if (p.providerName === "User") return `用户`;
      return p.providerName;
    });

  if (otherProviders.length > 0) {
    return `${permission.displayName} (通过 ${otherProviders.join(", ")} 授予)`;
  }

  return permission.displayName;
}

// 计算权限的深度（用于缩进显示）
function getPermissionDepth(
  group: PermissionGroupDto,
  permissionName: string
): number {
  const permission = group.permissions.find(p => p.name === permissionName);
  if (!permission) return 0;

  let depth = 0;
  let current = permission;
  while (current.parentName) {
    depth++;
    const parent = group.permissions.find(p => p.name === current.parentName);
    if (!parent) break;
    current = parent;
  }
  return depth;
}

// 获取所有权限的总数
function getAllPermissionsCount(): number {
  if (!permissionData.value) return 0;
  return permissionData.value.groups.reduce(
    (sum, group) => sum + group.permissions.length,
    0
  );
}

onMounted(() => {
  fetchRoles();
  fetchUsers();
});
</script>

<template>
  <div class="permission-management">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>权限管理</span>
        </div>
      </template>

      <el-tabs v-model="activeTab" class="permission-tabs">
        <!-- 角色权限管理 -->
        <el-tab-pane label="角色管理" name="role">
          <div class="tab-header">
            <el-button type="primary" @click="openCreateRoleDialog">
              新增角色
            </el-button>
          </div>

          <el-table v-loading="loading" :data="roles" style="width: 100%">
            <el-table-column prop="name" label="角色名称" />
            <el-table-column label="是否默认" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isDefault ? 'success' : 'info'" size="small">
                  {{ row.isDefault ? "是" : "否" }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="是否公开" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isPublic ? 'success' : 'info'" size="small">
                  {{ row.isPublic ? "是" : "否" }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="300" align="center">
              <template #default="{ row }">
                <el-button
                  type="primary"
                  size="small"
                  @click="
                    openPermissionDialog({
                      name: row.name,
                      displayName: row.name,
                      type: 'Role'
                    })
                  "
                >
                  管理权限
                </el-button>
                <el-button
                  type="warning"
                  size="small"
                  @click="openEditRoleDialog(row)"
                >
                  编辑
                </el-button>
                <el-button type="danger" size="small" @click="deleteRole(row)">
                  删除
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>

        <!-- 用户权限管理 -->
        <el-tab-pane label="用户管理" name="user">
          <div class="tab-header">
            <el-button type="primary" @click="openCreateUserDialog">
              新增用户
            </el-button>
          </div>

          <el-table v-loading="loading" :data="users" style="width: 100%">
            <el-table-column prop="userName" label="用户名" />
            <el-table-column label="姓名" width="200">
              <template #default="{ row }">
                {{
                  row.name || row.surname
                    ? `${row.name || ""} ${row.surname || ""}`.trim()
                    : "-"
                }}
              </template>
            </el-table-column>
            <el-table-column prop="email" label="邮箱" />
            <el-table-column label="角色" width="200">
              <template #default="{ row }">
                <el-tag
                  v-for="roleName in row.roleNames || []"
                  :key="roleName"
                  type="primary"
                  size="small"
                  style="margin-right: 5px"
                >
                  {{ roleName }}
                </el-tag>
                <span v-if="!row.roleNames || row.roleNames.length === 0"
                  >-</span
                >
              </template>
            </el-table-column>
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="row.isActive ? 'success' : 'danger'">
                  {{ row.isActive ? "激活" : "禁用" }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="350" align="center">
              <template #default="{ row }">
                <el-button
                  type="primary"
                  size="small"
                  @click="
                    openPermissionDialog({
                      name: row.id,
                      displayName: row.userName,
                      type: 'User'
                    })
                  "
                >
                  管理权限
                </el-button>
                <el-button
                  type="warning"
                  size="small"
                  @click="openEditUserDialog(row)"
                >
                  编辑
                </el-button>
                <el-button type="danger" size="small" @click="deleteUser(row)">
                  删除
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
      </el-tabs>
    </el-card>

    <!-- 角色编辑对话框 -->
    <el-dialog
      v-model="roleDialogVisible"
      :title="isEditingRole ? '编辑角色' : '新增角色'"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="roleFormRef"
        :model="roleForm"
        :rules="roleFormRules"
        label-width="100px"
      >
        <el-form-item label="角色名称" prop="name">
          <el-input v-model="roleForm.name" placeholder="请输入角色名称" />
        </el-form-item>
        <el-form-item label="是否默认">
          <el-switch v-model="roleForm.isDefault" />
        </el-form-item>
        <el-form-item label="是否公开">
          <el-switch v-model="roleForm.isPublic" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveRole">保存</el-button>
      </template>
    </el-dialog>

    <!-- 用户编辑对话框 -->
    <el-dialog
      v-model="userDialogVisible"
      :title="isEditingUser ? '编辑用户' : '新增用户'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="userFormRef"
        :model="userForm"
        :rules="userFormRules"
        label-width="100px"
      >
        <el-form-item label="用户名" prop="userName">
          <el-input v-model="userForm.userName" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item label="姓名" prop="name">
          <el-input v-model="userForm.name" placeholder="请输入姓名" />
        </el-form-item>
        <el-form-item label="姓氏" prop="surname">
          <el-input v-model="userForm.surname" placeholder="请输入姓氏" />
        </el-form-item>
        <el-form-item label="邮箱地址" prop="email" required>
          <el-input v-model="userForm.email" placeholder="请输入邮箱地址" />
        </el-form-item>
        <el-form-item label="手机号" prop="phoneNumber">
          <el-input v-model="userForm.phoneNumber" placeholder="请输入手机号" />
        </el-form-item>
        <el-form-item v-if="!isEditingUser" label="密码" prop="password">
          <el-input
            v-model="userForm.password"
            type="password"
            placeholder="请输入密码"
            show-password
          />
        </el-form-item>
        <el-form-item
          v-if="!isEditingUser"
          label="确认密码"
          prop="confirmPassword"
        >
          <el-input
            v-model="userForm.confirmPassword"
            type="password"
            placeholder="请再次输入密码"
            show-password
          />
        </el-form-item>
        <el-form-item label="角色" prop="roleNames">
          <el-select
            v-model="userForm.roleNames"
            multiple
            placeholder="请选择角色"
            style="width: 100%"
          >
            <el-option
              v-for="role in roles"
              :key="role.name"
              :label="role.name"
              :value="role.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="状态">
          <el-switch v-model="userForm.isActive" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="userDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveUser">保存</el-button>
      </template>
    </el-dialog>

    <!-- 权限编辑对话框 -->
    <el-dialog
      v-model="permissionDialogVisible"
      :title="`权限管理 - ${currentProvider?.displayName || ''}`"
      width="80%"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <div v-if="permissionData" class="permission-dialog">
        <!-- 搜索和全选 -->
        <div class="permission-toolbar">
          <el-input
            v-model="searchText"
            placeholder="搜索权限..."
            clearable
            style="width: 300px"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>

          <div class="select-all-actions">
            <el-checkbox
              :model-value="
                getTotalGrantedCount() > 0 &&
                getTotalGrantedCount() === getAllPermissionsCount()
              "
              :indeterminate="
                getTotalGrantedCount() > 0 &&
                getTotalGrantedCount() < getAllPermissionsCount()
              "
              @change="toggleAllPermissions($event as boolean)"
            >
              全选所有权限
            </el-checkbox>
          </div>
        </div>

        <!-- 权限分组 -->
        <el-tabs type="border-card" class="permission-groups-tabs">
          <el-tab-pane
            v-for="group in filteredGroups"
            :key="group.name"
            :label="`${group.displayName} (${getGroupGrantedCount(group)})`"
          >
            <div class="permission-group-content">
              <div class="group-header">
                <el-checkbox
                  :model-value="isGroupAllGranted(group)"
                  :indeterminate="isGroupIndeterminate(group)"
                  @change="toggleGroup(group, $event as boolean)"
                >
                  全选本分组
                </el-checkbox>
              </div>

              <el-divider />

              <div class="permissions-list">
                <div
                  v-for="permission in group.permissions"
                  :key="permission.name"
                  class="permission-item"
                  :style="{
                    marginLeft: `${getPermissionDepth(group, permission.name) * 20}px`
                  }"
                >
                  <el-checkbox
                    :model-value="permission.isGranted"
                    :disabled="isPermissionDisabled(permission)"
                    @change="
                      togglePermission(
                        group,
                        permission.name,
                        $event as boolean
                      )
                    "
                  >
                    {{ getPermissionDisplayName(permission) }}
                  </el-checkbox>
                </div>
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>

        <!-- 统计信息 -->
        <div class="permission-summary">
          <el-tag type="info">
            已选择 {{ getTotalGrantedCount() }} 个权限
          </el-tag>
        </div>
      </div>

      <template #footer>
        <el-button @click="closePermissionDialog">取消</el-button>
        <el-button type="primary" :loading="saving" @click="savePermissions">
          保存
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.permission-management {
  padding: 20px;

  .card-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-weight: bold;
  }

  .permission-tabs {
    margin-top: 20px;
  }

  .tab-header {
    display: flex;
    justify-content: flex-end;
    margin-bottom: 16px;
  }
}

.permission-dialog {
  .permission-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px;
    margin-bottom: 20px;
    background-color: #f5f7fa;
    border-radius: 4px;

    .select-all-actions {
      display: flex;
      gap: 10px;
    }
  }

  .permission-groups-tabs {
    max-height: 600px;
    overflow-y: auto;

    :deep(.el-tabs__content) {
      max-height: 550px;
      overflow-y: auto;
    }
  }

  .permission-group-content {
    .group-header {
      padding: 10px 0;
    }

    .permissions-list {
      max-height: 450px;
      padding: 10px 0;
      overflow-y: auto;

      .permission-item {
        padding: 8px 0;
        border-bottom: 1px solid #f0f0f0;

        &:last-child {
          border-bottom: none;
        }

        // 为禁用的权限（通过角色授予的）添加样式
        :deep(.el-checkbox.is-disabled) {
          .el-checkbox__label {
            font-style: italic;
            color: #909399;
          }
        }
      }
    }
  }

  .permission-summary {
    display: flex;
    justify-content: flex-end;
    padding: 10px;
    margin-top: 20px;
    background-color: #f5f7fa;
    border-radius: 4px;
  }
}
</style>
