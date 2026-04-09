import { http } from "@/utils/http";
import type { PagedRequestDto, PagedResultDto } from "../types/api";
import type {
  FileUploadInfoDto,
  CreateUpdateFileUploadInfoDto,
  CustomFileTypeDto
} from "../types/business";

// 文件上传 API
export class FileUploadApi {
  private baseUrl = "/api";

  // GET /api/app/file-upload-info
  async getFileUploadInfos(
    params?: PagedRequestDto
  ): Promise<PagedResultDto<FileUploadInfoDto>> {
    return http.get(`${this.baseUrl}/app/file-upload-info`, { params });
  }

  // POST /api/app/file-upload-info
  async createFileUploadInfo(
    request: CreateUpdateFileUploadInfoDto
  ): Promise<FileUploadInfoDto> {
    return http.post(`${this.baseUrl}/app/file-upload-info`, { data: request });
  }

  // PUT /api/app/file-upload-info/{id}
  async updateFileUploadInfo(
    id: number,
    request: CreateUpdateFileUploadInfoDto
  ): Promise<FileUploadInfoDto> {
    return http.request("put", `${this.baseUrl}/app/file-upload-info/${id}`, {
      data: request
    });
  }

  // DELETE /api/app/file-upload-info/{id}
  async deleteFileUploadInfo(id: number): Promise<void> {
    return http.request("delete", `${this.baseUrl}/app/file-upload-info/${id}`);
  }

  // GET /api/app/file-upload-info/configuration-value
  async getCustomFileTypeConfig(
    configurationName?: string
  ): Promise<CustomFileTypeDto> {
    return http.get(
      `${this.baseUrl}/app/file-upload-info/configuration-value`,
      {
        params: { configurationName }
      }
    );
  }

  // 文件上传辅助方法 (使用 FormData)
  async uploadFile(
    file: File,
    onProgress?: (progress: number) => void
  ): Promise<any> {
    const formData = new FormData();
    formData.append("file", file);

    // 计算文件 SHA1
    const fileSha1 = await this.calculateFileSHA1(file);

    const config = {
      headers: {
        "Content-Type": "multipart/form-data",
        FileSHA1: fileSha1 // 将SHA1值添加到请求头中
      },
      onUploadProgress: (progressEvent: any) => {
        if (onProgress && progressEvent.total) {
          const progress = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          onProgress(progress);
        }
      }
    };

    return http.request("post", `/api/FileUploadInfo/upload`, {
      data: formData,
      ...config
    });
  }

  // 下载文件
  async downloadFile(id: number): Promise<string> {
    return `${window.location.origin}/api/FileUploadInfo?id=${id}`;
  }

  // 计算文件 SHA1
  async calculateFileSHA1(file: File): Promise<string> {
    // 动态导入 crypto-js
    const CryptoJS = await import("crypto-js");

    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = async event => {
        try {
          const buffer = event.target?.result as ArrayBuffer;
          const wordArray = this.arrayBufferToWordArray(buffer, CryptoJS);
          const hash = CryptoJS.SHA1(wordArray).toString(CryptoJS.enc.Hex);
          resolve(hash);
        } catch (error) {
          reject(error);
        }
      };

      reader.onerror = () => reject(new Error("文件读取失败"));
      reader.readAsArrayBuffer(file);
    });
  }

  // ArrayBuffer 转 WordArray (CryptoJS 格式)
  private arrayBufferToWordArray(buffer: ArrayBuffer, CryptoJS: any): any {
    const byteArray = new Uint8Array(buffer);
    const wordArray = [];
    for (let i = 0; i < byteArray.length; i += 4) {
      const word =
        (byteArray[i] << 24) |
        (byteArray[i + 1] << 16) |
        (byteArray[i + 2] << 8) |
        byteArray[i + 3];
      wordArray.push(word);
    }
    return CryptoJS.lib.WordArray.create(wordArray, byteArray.length);
  }

  // 获取 Cookie
  private getCookie(name: string): string | undefined {
    const cookieName = name + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const cookieArray = decodedCookie.split(";");

    for (let i = 0; i < cookieArray.length; i++) {
      let cookie = cookieArray[i];
      while (cookie.charAt(0) === " ") {
        cookie = cookie.substring(1);
      }
      if (cookie.indexOf(cookieName) === 0) {
        return cookie.substring(cookieName.length, cookie.length);
      }
    }
    return undefined;
  }
}

// 导出实例
export const fileUploadApi = new FileUploadApi();

export default fileUploadApi;
