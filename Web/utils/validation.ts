import { VALIDATION, FILE_UPLOAD } from '@/constants/config';

/**
 * Validate email format
 */
export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

/**
 * Validate password strength
 */
export interface PasswordValidation {
  isValid: boolean;
  hasMinLength: boolean;
  hasUppercase: boolean;
  hasLowercase: boolean;
  hasNumber: boolean;
  hasSpecialChar: boolean;
}

export function validatePassword(password: string): PasswordValidation {
  return {
    isValid:
      password.length >= VALIDATION.MIN_PASSWORD_LENGTH &&
      /[A-Z]/.test(password) &&
      /[a-z]/.test(password) &&
      /[0-9]/.test(password),
    hasMinLength: password.length >= VALIDATION.MIN_PASSWORD_LENGTH,
    hasUppercase: /[A-Z]/.test(password),
    hasLowercase: /[a-z]/.test(password),
    hasNumber: /[0-9]/.test(password),
    hasSpecialChar: /[!@#$%^&*(),.?":{}|<>]/.test(password),
  };
}

/**
 * Validate file for upload
 */
export interface FileValidation {
  isValid: boolean;
  error?: string;
}

export function validateImageFile(file: File): FileValidation {
  const acceptedTypes: readonly string[] = FILE_UPLOAD.ACCEPTED_IMAGE_TYPES;
  if (!acceptedTypes.includes(file.type)) {
    return {
      isValid: false,
      error: `Invalid file type. Accepted types: ${FILE_UPLOAD.ACCEPTED_IMAGE_TYPES.join(', ')}`,
    };
  }
  
  if (file.size > FILE_UPLOAD.MAX_FILE_SIZE) {
    return {
      isValid: false,
      error: `File size exceeds ${FILE_UPLOAD.MAX_FILE_SIZE / (1024 * 1024)}MB limit`,
    };
  }
  
  return { isValid: true };
}

/**
 * Validate URL format
 */
export function isValidUrl(url: string): boolean {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
}

/**
 * Validate bid amount
 */
export function isValidBidAmount(
  amount: number,
  currentHighBid: number,
  minimumIncrement: number = 1
): { isValid: boolean; error?: string } {
  if (isNaN(amount) || amount <= 0) {
    return { isValid: false, error: 'Please enter a valid bid amount' };
  }
  
  const minimumBid = currentHighBid + minimumIncrement;
  if (amount < minimumBid) {
    return {
      isValid: false,
      error: `Bid must be at least $${minimumBid.toLocaleString()}`,
    };
  }
  
  return { isValid: true };
}

/**
 * Validate year (for vehicle auctions)
 */
export function isValidYear(year: number): boolean {
  return year >= VALIDATION.MIN_YEAR && year <= VALIDATION.MAX_YEAR;
}

/**
 * Sanitize input string (prevent XSS)
 */
export function sanitizeInput(input: string): string {
  return input
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .trim();
}

/**
 * Check if string is empty or whitespace only
 */
export function isEmpty(value: string | null | undefined): boolean {
  return !value || value.trim().length === 0;
}

/**
 * Check if value is within range
 */
export function isInRange(value: number, min: number, max: number): boolean {
  return value >= min && value <= max;
}
