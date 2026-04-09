import Axios, {
  type AxiosInstance,
  type AxiosRequestConfig,
  type CustomParamsSerializer
} from "axios";

import type {
  PureHttpError,
  RequestMethods,
  PureHttpResponse,
  PureHttpRequestConfig
} from "./types.d";
import { stringify } from "qs";
import NProgress from "../progress";

// 获取CSRF令牌的函数
function getCsrfToken(): string | null {
  const match = document.cookie.match(/XSRF-TOKEN=([\w-]+)/);
  return match ? match[1] : null;
}

// 刷新CSRF令牌的函数，可在登录成功后调用
export async function refreshCsrfToken(httpInstance: PureHttp = http) {
  try {
    // 尝试从API配置端点获取CSRF令牌
    await httpInstance.get("/api/abp/application-configuration");
    console.log("CSRF token refreshed successfully");
  } catch (error) {
    console.warn("Failed to refresh CSRF token:", error);
    throw error;
  }
}

// 相关配置请参考：www.axios-js.com/zh-cn/docs/#axios-request-config-1
const defaultConfig: AxiosRequestConfig = {
  // 请求超时时间
  timeout: 10000,
  headers: {
    Accept: "application/json, text/plain, */*",
    "Content-Type": "application/json",
    "X-Requested-With": "XMLHttpRequest"
  },
  // 数组格式参数序列化（https://github.com/axios/axios/issues/5142）
  paramsSerializer: {
    serialize: stringify as unknown as CustomParamsSerializer
  }
};

class PureHttp {
  constructor() {
    this.httpInterceptorsRequest();
    this.httpInterceptorsResponse();
  }

  /** 初始化配置对象 */
  private static initConfig: PureHttpRequestConfig = {};

  /** 保存当前`Axios`实例对象 */
  private static axiosInstance: AxiosInstance = Axios.create(defaultConfig);

  /** 请求拦截 */
  private httpInterceptorsRequest(): void {
    PureHttp.axiosInstance.interceptors.request.use(
      async (config: PureHttpRequestConfig): Promise<any> => {
        // 开启进度条动画
        NProgress.start();
        // 优先判断post/get等方法是否传入回调，否则执行初始化设置等回调
        if (typeof config.beforeRequestCallback === "function") {
          config.beforeRequestCallback(config);
          return config;
        }
        if (PureHttp.initConfig.beforeRequestCallback) {
          PureHttp.initConfig.beforeRequestCallback(config);
          return config;
        }

        /** 请求白名单，放置一些不需要`token`的接口（通过设置请求白名单，防止`token`过期后再请求造成的死循环问题） */
        const whiteList = ["/api/app/account/login"];
        if (whiteList.some(url => config.url?.endsWith(url))) {
          return config;
        }

        // 从 sessionStorage 获取 token
        const token = sessionStorage.getItem("access_token");
        if (token) {
          config.headers["Authorization"] = `Bearer ${token}`;
        }

        return config;
      },
      error => {
        return Promise.reject(error);
      }
    );
  }

  /** 响应拦截 */
  private httpInterceptorsResponse(): void {
    const instance = PureHttp.axiosInstance;
    instance.interceptors.response.use(
      (response: PureHttpResponse) => {
        const $config = response.config;
        // 关闭进度条动画
        NProgress.done();
        // 优先判断post/get等方法是否传入回调，否则执行初始化设置等回调
        if (typeof $config.beforeResponseCallback === "function") {
          $config.beforeResponseCallback(response);
          return response.data;
        }
        if (PureHttp.initConfig.beforeResponseCallback) {
          PureHttp.initConfig.beforeResponseCallback(response);
          return response.data;
        }
        return response.data;
      },
      async (error: PureHttpError) => {
        const $error = error;
        $error.isCancelRequest = Axios.isCancel($error);
        // 关闭进度条动画
        NProgress.done();

        // 检查是否是 anti-forgery token 失效导致的 400 错误
        // ABP框架的anti-forgery验证失败通常返回400，且响应体可能为空
        if (
          $error.response &&
          $error.response.status === 400 &&
          !($error.config as any)._retry
        ) {
          // 对于非GET请求的400错误，尝试刷新token并重试
          const method = ($error.config.method || "").toLowerCase();
          const isModifyRequest = ["post", "put", "delete", "patch"].includes(
            method
          );

          // 检查错误响应中是否包含 anti-forgery 相关信息
          const responseData = $error.response.data as any;
          const hasAntiForgeryInfo =
            responseData?.error?.details
              ?.toLowerCase()
              .includes("antiforgery") ||
            responseData?.error?.message
              ?.toLowerCase()
              .includes("antiforgery") ||
            $error.response.headers["x-csrf-error"] !== undefined;

          // 如果是修改请求且可能是anti-forgery错误，或者明确包含anti-forgery信息
          if (
            isModifyRequest &&
            (hasAntiForgeryInfo ||
              !responseData ||
              Object.keys(responseData || {}).length === 0)
          ) {
            console.log(
              "检测到可能的 Anti-forgery token 验证失败(400)，正在刷新 token 并重试请求..."
            );

            // 标记已经重试过，避免无限循环
            ($error.config as any)._retry = true;

            try {
              // 刷新 CSRF token
              await refreshCsrfToken();

              // 重新获取最新的 CSRF token 并更新请求头
              const newCsrfToken = getCsrfToken();
              if (newCsrfToken) {
                $error.config.headers["RequestVerificationToken"] =
                  newCsrfToken;
              }

              // 重试原请求
              return instance.request($error.config);
            } catch (refreshError) {
              console.error("刷新 CSRF token 失败:", refreshError);
              // 如果刷新失败，返回原始错误
              return Promise.reject($error);
            }
          }
        }

        // 所有的响应异常 区分来源为取消请求/非取消请求
        return Promise.reject($error);
      }
    );
  }

  /** 通用请求工具函数 */
  public request<T>(
    method: RequestMethods,
    url: string,
    param?: AxiosRequestConfig,
    axiosConfig?: PureHttpRequestConfig
  ): Promise<T> {
    const config = {
      method,
      url,
      ...param,
      ...axiosConfig
    } as PureHttpRequestConfig;

    // 添加CSRF令牌到请求头
    if (config.method && config.method.toLowerCase() !== "get") {
      const csrfToken = getCsrfToken();
      if (csrfToken) {
        config.headers = config.headers || {};
        config.headers["RequestVerificationToken"] = csrfToken;
      }
    }

    // 单独处理自定义请求/响应回调
    return new Promise((resolve, reject) => {
      PureHttp.axiosInstance
        .request(config)
        .then((response: undefined) => {
          resolve(response);
        })
        .catch(error => {
          reject(error);
        });
    });
  }

  /** 单独抽离的`post`工具函数 */
  public post<T, P>(
    url: string,
    params?: AxiosRequestConfig<P>,
    config?: PureHttpRequestConfig
  ): Promise<T> {
    return this.request<T>("post", url, params, config);
  }

  /** 单独抽离的`get`工具函数 */
  public get<T, P>(
    url: string,
    params?: AxiosRequestConfig<P>,
    config?: PureHttpRequestConfig
  ): Promise<T> {
    return this.request<T>("get", url, params, config);
  }

  /** 单独抽离的`put`工具函数 */
  public put<T, P>(
    url: string,
    params?: AxiosRequestConfig<P>,
    config?: PureHttpRequestConfig
  ): Promise<T> {
    return this.request<T>("put", url, params, config);
  }

  /** 单独抽离的`delete`工具函数 */
  public delete<T, P>(
    url: string,
    params?: AxiosRequestConfig<P>,
    config?: PureHttpRequestConfig
  ): Promise<T> {
    return this.request<T>("delete", url, params, config);
  }

  /** 单独抽离的`patch`工具函数 */
  public patch<T, P>(
    url: string,
    params?: AxiosRequestConfig<P>,
    config?: PureHttpRequestConfig
  ): Promise<T> {
    return this.request<T>("patch", url, params, config);
  }
}

export const http = new PureHttp();
