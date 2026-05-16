// 认证相关类型，基于 ABP 和 OpenIddict/OIDC 标准

// 用户信息 (从 ABP currentUser 扩展)
export interface UserInfo {
  id: string;
  userName: string;
  name?: string;
  surname?: string;
  email?: string;
  emailVerified?: boolean;
  phoneNumber?: string;
  phoneNumberVerified?: boolean;
  tenantId?: string | null;
  roles?: string[];
  permissions?: string[];
  token?: {
    accessToken: string;
    refreshToken?: string;
    expiresIn: number;
    tokenType: string;
  };
}

// 登录请求
export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
  returnUrl?: string;
  singleSignIn?: boolean;
}

// 登录响应
export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
  expiresIn: number;
  tokenType: string;
  success: boolean;
  error?: string;
}

// OIDC 配置设置
export interface OidcSettings {
  authority: string; // 认证服务器 URL
  clientId: string;
  clientSecret?: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  responseType: "code" | "id_token" | "code id_token";
  scope: string; // e.g., 'openid profile email'
  automaticSilentRenew: boolean;
  silentRedirectUri: string;
  revokeAccessTokenOnSignout: boolean;
  filterProtocolClaims: boolean;
  loadUserInfo: boolean;
  metadata?: {
    issuer: string;
    authorizationEndpoint: string;
    tokenEndpoint: string;
    userInfoEndpoint: string;
    jwksUri: string;
    endSessionEndpoint: string;
  };
}

// 令牌响应 (OIDC)
export interface TokenResponse {
  access_token: string;
  id_token?: string;
  refresh_token?: string;
  expires_in: number;
  token_type: string;
  scope?: string;
}

// 认证状态
export interface AuthState {
  isAuthenticated: boolean;
  user?: UserInfo;
  token?: string;
  expiresAt?: number;
  loading: boolean;
  error?: string;
}

// ABP 认证配置 (从 ApplicationConfigurationDto.auth 扩展)
export interface AbpAuthConfig {
  isEnabled: boolean;
  useLocalStorage: boolean;
  tokenExpiration: number;
  loginUrl: string;
  logoutUrl: string;
  clientId: string;
  scopes: string[];
  // 其他 ABP 特定配置
}

// 权限检查
export interface PermissionDto {
  name: string;
  granted: boolean;
  parentName?: string;
  children?: PermissionDto[];
}

// 角色信息
export interface RoleDto {
  id: string;
  name: string;
  displayName?: string;
  isStatic: boolean;
  isPublic: boolean;
  allowedForChildTenants: boolean;
}
