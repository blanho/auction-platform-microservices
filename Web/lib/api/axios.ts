import axios, {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig
} from "axios";

const baseURL = process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:6001";

const apiClient: AxiosInstance = axios.create({
  baseURL,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json"
  }
});

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token =
      typeof window !== "undefined"
        ? localStorage.getItem("accessToken")
        : null;

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    const correlationId = crypto.randomUUID();
    if (config.headers) {
      config.headers["X-Correlation-Id"] = correlationId;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && originalRequest) {
      if (typeof window !== "undefined") {
        window.location.href = "/auth/login";
      }
    }

    if (error.response?.status === 403) {
      console.error("Access forbidden:", error.response.data);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
