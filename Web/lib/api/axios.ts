import axios, {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig
} from "axios";

const isServer = typeof window === "undefined";

const getBaseUrl = () => {
  if (isServer) {
    return process.env.GATEWAY_URL || process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:6001";
  }
  return "/api/proxy";
};

const apiClient: AxiosInstance = axios.create({
  baseURL: getBaseUrl(),
  timeout: 30000,
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: true
});

apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
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
