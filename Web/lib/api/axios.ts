import axios, {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig
} from "axios";
import { getSession } from "next-auth/react";

const baseURL = process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:6001";

const apiClient: AxiosInstance = axios.create({
  baseURL,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: true
});

apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    if (typeof window !== "undefined") {
      const session = await getSession();
      if (session?.accessToken && config.headers) {
        config.headers.Authorization = `Bearer ${session.accessToken}`;
      }
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
        const currentPath = window.location.pathname;
        const protectedRoutes = [
          "/profile",
          "/auctions/create",
          "/auctions/my",
          "/notifications",
          "/admin"
        ];
        const isProtectedRoute = protectedRoutes.some((route) =>
          currentPath.startsWith(route)
        );

        if (isProtectedRoute) {
          window.location.href = `/auth/signin?callbackUrl=${encodeURIComponent(
            currentPath
          )}`;
        }
      }
    }

    if (error.response?.status === 403) {
      console.error("Access forbidden:", error.response.data);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
