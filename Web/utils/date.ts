import { format, formatDistance, formatRelative, isValid, parseISO } from 'date-fns';

/**
 * Safely parse a date string to Date object
 */
export function parseDate(dateString: string | Date | null | undefined): Date | null {
  if (!dateString) return null;
  
  if (dateString instanceof Date) {
    return isValid(dateString) ? dateString : null;
  }
  
  try {
    const parsed = parseISO(dateString);
    return isValid(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

/**
 * Format a date to a readable string
 */
export function formatDate(
  date: string | Date | null | undefined,
  formatString: string = 'PPP'
): string {
  const parsed = parseDate(date);
  if (!parsed) return 'Invalid date';
  
  try {
    return format(parsed, formatString);
  } catch {
    return 'Invalid date';
  }
}

/**
 * Format a date to relative time (e.g., "2 hours ago")
 */
export function formatRelativeTime(date: string | Date | null | undefined): string {
  const parsed = parseDate(date);
  if (!parsed) return 'Invalid date';
  
  try {
    return formatDistance(parsed, new Date(), { addSuffix: true });
  } catch {
    return 'Invalid date';
  }
}

/**
 * Format a date relative to now with day context
 */
export function formatRelativeDate(date: string | Date | null | undefined): string {
  const parsed = parseDate(date);
  if (!parsed) return 'Invalid date';
  
  try {
    return formatRelative(parsed, new Date());
  } catch {
    return 'Invalid date';
  }
}

/**
 * Get time remaining until a date
 */
export interface TimeRemaining {
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
  total: number;
  isExpired: boolean;
}

export function getTimeRemaining(endDate: string | Date | null | undefined): TimeRemaining {
  const parsed = parseDate(endDate);
  
  if (!parsed) {
    return { days: 0, hours: 0, minutes: 0, seconds: 0, total: 0, isExpired: true };
  }
  
  const total = parsed.getTime() - Date.now();
  
  if (total <= 0) {
    return { days: 0, hours: 0, minutes: 0, seconds: 0, total: 0, isExpired: true };
  }
  
  return {
    days: Math.floor(total / (1000 * 60 * 60 * 24)),
    hours: Math.floor((total / (1000 * 60 * 60)) % 24),
    minutes: Math.floor((total / (1000 / 60)) % 60),
    seconds: Math.floor((total / 1000) % 60),
    total,
    isExpired: false,
  };
}

/**
 * Check if a date is in the past
 */
export function isDatePast(date: string | Date | null | undefined): boolean {
  const parsed = parseDate(date);
  if (!parsed) return true;
  return parsed.getTime() < Date.now();
}

/**
 * Check if a date is in the future
 */
export function isDateFuture(date: string | Date | null | undefined): boolean {
  const parsed = parseDate(date);
  if (!parsed) return false;
  return parsed.getTime() > Date.now();
}

/**
 * Format date for input fields (datetime-local)
 */
export function formatDateForInput(date: string | Date | null | undefined): string {
  const parsed = parseDate(date);
  if (!parsed) return '';
  
  try {
    return format(parsed, "yyyy-MM-dd'T'HH:mm");
  } catch {
    return '';
  }
}
