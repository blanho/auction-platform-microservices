/**
 * Format a number as currency
 */
export function formatCurrency(
  amount: number | null | undefined,
  currency: string = 'USD',
  locale: string = 'en-US'
): string {
  if (amount == null || isNaN(amount)) return '$0';
  
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount);
}

/**
 * Format a number with thousand separators
 */
export function formatNumber(
  value: number | null | undefined,
  locale: string = 'en-US'
): string {
  if (value == null || isNaN(value)) return '0';
  
  return new Intl.NumberFormat(locale).format(value);
}

/**
 * Format a number as a compact representation (e.g., 1.2K, 3.4M)
 */
export function formatCompactNumber(
  value: number | null | undefined,
  locale: string = 'en-US'
): string {
  if (value == null || isNaN(value)) return '0';
  
  return new Intl.NumberFormat(locale, {
    notation: 'compact',
    compactDisplay: 'short',
  }).format(value);
}

/**
 * Format a percentage
 */
export function formatPercentage(
  value: number | null | undefined,
  decimals: number = 1
): string {
  if (value == null || isNaN(value)) return '0%';
  
  return `${value.toFixed(decimals)}%`;
}

/**
 * Format file size in human-readable format
 */
export function formatFileSize(bytes: number | null | undefined): string {
  if (bytes == null || isNaN(bytes) || bytes === 0) return '0 B';
  
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  const k = 1024;
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${units[i]}`;
}

/**
 * Format mileage with appropriate suffix
 */
export function formatMileage(mileage: number | null | undefined): string {
  if (mileage == null || isNaN(mileage)) return '0 mi';
  
  return `${formatNumber(mileage)} mi`;
}

/**
 * Truncate text with ellipsis
 */
export function truncateText(
  text: string | null | undefined,
  maxLength: number,
  suffix: string = '...'
): string {
  if (!text) return '';
  if (text.length <= maxLength) return text;
  
  return text.slice(0, maxLength - suffix.length).trim() + suffix;
}

/**
 * Capitalize first letter of a string
 */
export function capitalize(text: string | null | undefined): string {
  if (!text) return '';
  return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase();
}

/**
 * Convert string to title case
 */
export function toTitleCase(text: string | null | undefined): string {
  if (!text) return '';
  return text
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

/**
 * Convert camelCase or PascalCase to readable text
 */
export function camelToReadable(text: string | null | undefined): string {
  if (!text) return '';
  return text
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim();
}

/**
 * Generate initials from a name
 */
export function getInitials(name: string | null | undefined, maxLength: number = 2): string {
  if (!name) return '';
  
  return name
    .split(' ')
    .filter(Boolean)
    .map(word => word.charAt(0).toUpperCase())
    .slice(0, maxLength)
    .join('');
}

/**
 * Pluralize a word based on count
 */
export function pluralize(
  count: number,
  singular: string,
  plural?: string
): string {
  const pluralForm = plural || `${singular}s`;
  return count === 1 ? singular : pluralForm;
}
