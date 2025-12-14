import { toast } from 'sonner';
import { AxiosError } from 'axios';

export interface ApiErrorResponse {
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
  status?: number;
  traceId?: string;
}

export interface ExtractedError {
  message: string;
  fieldErrors?: Record<string, string[]>;
  statusCode?: number;
}

const HTTP_STATUS_MESSAGES: Record<number, string> = {
  400: 'Bad Request: Invalid data submitted',
  401: 'Unauthorized: Please sign in to continue',
  403: 'Forbidden: You do not have permission for this action',
  404: 'Not Found: The requested resource does not exist',
  409: 'Conflict: This action conflicts with existing data',
  422: 'Validation Error: Please check your input',
  429: 'Too Many Requests: Please wait before trying again',
  500: 'Server Error: Please try again later',
  502: 'Bad Gateway: Server is temporarily unavailable',
  503: 'Service Unavailable: Please try again later',
  504: 'Gateway Timeout: Request took too long',
};

export function extractErrorMessage(error: unknown): ExtractedError {
  const defaultMessage = 'An unexpected error occurred. Please try again.';

  if (!error) {
    return { message: defaultMessage };
  }

  if (error instanceof AxiosError) {
    return extractAxiosError(error);
  }

  if (error instanceof Error) {
    return { message: error.message || defaultMessage };
  }

  if (typeof error === 'string') {
    return { message: error };
  }

  if (typeof error === 'object' && error !== null) {
    const errorObj = error as ApiErrorResponse;
    return extractApiErrorResponse(errorObj);
  }

  return { message: defaultMessage };
}

function extractAxiosError(error: AxiosError<ApiErrorResponse>): ExtractedError {
  const statusCode = error.response?.status;
  const data = error.response?.data;

  if (data) {
    const extracted = extractApiErrorResponse(data);
    return { ...extracted, statusCode };
  }

  if (statusCode && HTTP_STATUS_MESSAGES[statusCode]) {
    return {
      message: HTTP_STATUS_MESSAGES[statusCode],
      statusCode,
    };
  }

  if (error.message === 'Network Error') {
    return {
      message: 'Network Error: Please check your internet connection',
      statusCode: 0,
    };
  }

  if (error.code === 'ECONNABORTED') {
    return {
      message: 'Request Timeout: The server took too long to respond',
      statusCode: 408,
    };
  }

  return {
    message: error.message || 'An unexpected error occurred',
    statusCode,
  };
}

function extractApiErrorResponse(data: ApiErrorResponse): ExtractedError {
  if (data.message) {
    return {
      message: data.message,
      fieldErrors: data.errors,
    };
  }

  if (data.title) {
    let message = data.title;
    if (data.detail) {
      message += `: ${data.detail}`;
    }
    return {
      message,
      fieldErrors: data.errors,
    };
  }

  if (data.errors && Object.keys(data.errors).length > 0) {
    const errorMessages = Object.entries(data.errors)
      .map(([field, messages]) => {
        const fieldName = formatFieldName(field);
        return `${fieldName}: ${Array.isArray(messages) ? messages.join(', ') : messages}`;
      })
      .join('; ');

    return {
      message: errorMessages,
      fieldErrors: data.errors,
    };
  }

  if (typeof data === 'string') {
    return { message: data };
  }

  return { message: 'An unexpected error occurred' };
}

function formatFieldName(field: string): string {
  return field
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (str) => str.toUpperCase())
    .replace(/^\$\./, '')
    .trim();
}

export interface ShowErrorToastOptions {
  duration?: number;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export function showErrorToast(
  error: unknown,
  options: ShowErrorToastOptions = {}
): ExtractedError {
  const { duration = 5000, description, action } = options;
  const extracted = extractErrorMessage(error);

  toast.error(extracted.message, {
    duration,
    description: description || getErrorDescription(extracted.statusCode),
    action: action
      ? {
          label: action.label,
          onClick: action.onClick,
        }
      : undefined,
  });

  return extracted;
}

function getErrorDescription(statusCode?: number): string | undefined {
  if (!statusCode) return 'Please try again or contact support if the issue persists';

  if (statusCode === 401) return 'You may need to sign in again';
  if (statusCode === 403) return 'Contact an administrator if you need access';
  if (statusCode === 404) return 'The item may have been deleted or moved';
  if (statusCode === 429) return 'Wait a moment before trying again';
  if (statusCode >= 500) return 'Our team has been notified of this issue';

  return 'Please check your input and try again';
}

export function showSuccessToast(
  message: string,
  options: { duration?: number; description?: string } = {}
): void {
  const { duration = 3000, description } = options;
  toast.success(message, { duration, description });
}

export function showInfoToast(
  message: string,
  options: { duration?: number; description?: string } = {}
): void {
  const { duration = 3000, description } = options;
  toast.info(message, { duration, description });
}

export function showWarningToast(
  message: string,
  options: { duration?: number; description?: string } = {}
): void {
  const { duration = 4000, description } = options;
  toast.warning(message, { duration, description });
}

export function showLoadingToast(message: string): string | number {
  return toast.loading(message);
}

export function dismissToast(toastId: string | number): void {
  toast.dismiss(toastId);
}

export function updateToast(
  toastId: string | number,
  type: 'success' | 'error' | 'info' | 'warning',
  message: string,
  options: { duration?: number; description?: string } = {}
): void {
  const { duration = 3000, description } = options;

  toast.dismiss(toastId);

  switch (type) {
    case 'success':
      toast.success(message, { duration, description });
      break;
    case 'error':
      toast.error(message, { duration, description });
      break;
    case 'info':
      toast.info(message, { duration, description });
      break;
    case 'warning':
      toast.warning(message, { duration, description });
      break;
  }
}

export async function withErrorHandling<T>(
  fn: () => Promise<T>,
  options: {
    loadingMessage?: string;
    successMessage?: string;
    errorOptions?: ShowErrorToastOptions;
  } = {}
): Promise<T | null> {
  const { loadingMessage, successMessage, errorOptions } = options;

  let toastId: string | number | undefined;

  if (loadingMessage) {
    toastId = showLoadingToast(loadingMessage);
  }

  try {
    const result = await fn();

    if (toastId) {
      dismissToast(toastId);
    }

    if (successMessage) {
      showSuccessToast(successMessage);
    }

    return result;
  } catch (error) {
    if (toastId) {
      dismissToast(toastId);
    }

    showErrorToast(error, errorOptions);
    return null;
  }
}

export function isNetworkError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    return error.message === 'Network Error' || error.code === 'ERR_NETWORK';
  }
  return false;
}

export function isAuthenticationError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    return error.response?.status === 401;
  }
  return false;
}

export function isAuthorizationError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    return error.response?.status === 403;
  }
  return false;
}

export function isValidationError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    return status === 400 || status === 422;
  }
  return false;
}

export function isServerError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    return status !== undefined && status >= 500;
  }
  return false;
}
