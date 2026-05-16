import { http } from "@/utils/http";
import type {
  SendPasswordResetCodeDto,
  VerifyPasswordResetTokenDto,
  VerifyPasswordResetTokenResultDto,
  ResetPasswordDto
} from "@/types/api";

export type UserResult = {
  success: boolean;
  data: {
    /** 头像 */
    avatar: string;
    /** 用户名 */
    username: string;
    /** 昵称 */
    nickname: string;
    /** 当前登录用户的角色 */
    roles: Array<string>;
    /** 按钮级别权限 */
    permissions: Array<string>;
    /** `token` */
    accessToken: string;
    /** 用于调用刷新`accessToken`的接口时所需的`token` */
    refreshToken: string;
    /** `accessToken`的过期时间（格式'xxxx/xx/xx xx:xx:xx'） */
    expires: Date;
  };
};

export type RefreshTokenResult = {
  success: boolean;
  data: {
    /** `token` */
    accessToken: string;
    /** 用于调用刷新`accessToken`的接口时所需的`token` */
    refreshToken: string;
    /** `accessToken`的过期时间（格式'xxxx/xx/xx xx:xx:xx'） */
    expires: Date;
  };
};

/** 登录 */
export const getLogin = (data?: object) => {
  return http.request<UserResult>("post", "/api/app/account/login", { data });
};

/** 刷新`token` */
export const refreshTokenApi = (data?: object) => {
  return http.request<RefreshTokenResult>("post", "/refresh-token", { data });
};

/** 发送重置密码验证码 */
export const sendPasswordResetCode = (data: SendPasswordResetCodeDto) => {
  return http.request<void>(
    "post",
    "/api/app/account/send-password-reset-code",
    {
      data
    }
  );
};

/** 验证重置密码令牌 */
export const verifyPasswordResetToken = (data: VerifyPasswordResetTokenDto) => {
  return http.request<VerifyPasswordResetTokenResultDto>(
    "post",
    "/api/app/account/verify-password-reset-token",
    { data }
  );
};

/** 重置密码 */
export const resetPassword = (data: ResetPasswordDto) => {
  return http.request<void>("post", "/api/app/account/reset-password", {
    data
  });
};
