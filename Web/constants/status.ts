export const AUCTION_STATUS = {
  LIVE: 'Live',
  FINISHED: 'Finished',
  RESERVE_NOT_MET: 'ReserveNotMet',
  CANCELLED: 'Cancelled',
} as const;

export type AuctionStatusType = (typeof AUCTION_STATUS)[keyof typeof AUCTION_STATUS];

export const AUCTION_STATUS_COLORS: Record<string, string> = {
  [AUCTION_STATUS.LIVE]: 'bg-green-500',
  [AUCTION_STATUS.FINISHED]: 'bg-gray-500',
  [AUCTION_STATUS.RESERVE_NOT_MET]: 'bg-yellow-500',
  [AUCTION_STATUS.CANCELLED]: 'bg-red-500',
} as const;

export const AUCTION_STATUS_VARIANTS: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  [AUCTION_STATUS.LIVE]: 'default',
  [AUCTION_STATUS.FINISHED]: 'secondary',
  [AUCTION_STATUS.RESERVE_NOT_MET]: 'outline',
  [AUCTION_STATUS.CANCELLED]: 'destructive',
} as const;

export const REPORT_STATUS = {
  PENDING: 'Pending',
  UNDER_REVIEW: 'UnderReview',
  RESOLVED: 'Resolved',
  DISMISSED: 'Dismissed',
} as const;

export type ReportStatusType = (typeof REPORT_STATUS)[keyof typeof REPORT_STATUS];

export const REPORT_PRIORITY = {
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
  CRITICAL: 'Critical',
} as const;

export type ReportPriorityType = (typeof REPORT_PRIORITY)[keyof typeof REPORT_PRIORITY];

export const REPORT_TYPE = {
  FRAUD: 'Fraud',
  FAKE_ITEM: 'FakeItem',
  NON_PAYMENT: 'NonPayment',
  HARASSMENT: 'Harassment',
  INAPPROPRIATE_CONTENT: 'InappropriateContent',
  SUSPICIOUS_ACTIVITY: 'SuspiciousActivity',
  OTHER: 'Other',
} as const;

export type ReportTypeValue = (typeof REPORT_TYPE)[keyof typeof REPORT_TYPE];

export const TRANSACTION_STATUS = {
  PENDING: 'Pending',
  COMPLETED: 'Completed',
  FAILED: 'Failed',
  CANCELLED: 'Cancelled',
} as const;

export type TransactionStatusType = (typeof TRANSACTION_STATUS)[keyof typeof TRANSACTION_STATUS];

export const TRANSACTION_TYPE = {
  DEPOSIT: 'Deposit',
  WITHDRAWAL: 'Withdrawal',
  PAYMENT: 'Payment',
  REFUND: 'Refund',
  HOLD: 'Hold',
  RELEASE: 'Release',
} as const;

export type TransactionTypeValue = (typeof TRANSACTION_TYPE)[keyof typeof TRANSACTION_TYPE];

export const USER_STATUS = {
  ACTIVE: 'active',
  INACTIVE: 'inactive',
  SUSPENDED: 'suspended',
} as const;

export type UserStatusType = (typeof USER_STATUS)[keyof typeof USER_STATUS];
