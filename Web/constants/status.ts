import { AuctionStatus } from "@/types/auction";
import { BidStatus } from "@/types/bid";

export const AUCTION_STATUS = {
  LIVE: 'Live',
  FINISHED: 'Finished',
  RESERVED_NOT_MET: 'ReservedNotMet',
  INACTIVE: 'Inactive',
  SCHEDULED: 'Scheduled',
  RESERVED_FOR_BUYNOW: 'ReservedForBuyNow',
} as const;

export type AuctionStatusType = (typeof AUCTION_STATUS)[keyof typeof AUCTION_STATUS];

export const AUCTION_STATUS_COLORS: Record<string, string> = {
  [AUCTION_STATUS.LIVE]: 'bg-green-500',
  [AUCTION_STATUS.FINISHED]: 'bg-gray-500',
  [AUCTION_STATUS.RESERVED_NOT_MET]: 'bg-yellow-500',
  [AUCTION_STATUS.INACTIVE]: 'bg-slate-500',
  [AUCTION_STATUS.SCHEDULED]: 'bg-blue-500',
  [AUCTION_STATUS.RESERVED_FOR_BUYNOW]: 'bg-purple-500',
} as const;

export const AUCTION_STATUS_VARIANTS: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  [AUCTION_STATUS.LIVE]: 'default',
  [AUCTION_STATUS.FINISHED]: 'secondary',
  [AUCTION_STATUS.RESERVED_NOT_MET]: 'outline',
  [AUCTION_STATUS.INACTIVE]: 'secondary',
  [AUCTION_STATUS.SCHEDULED]: 'outline',
  [AUCTION_STATUS.RESERVED_FOR_BUYNOW]: 'default',
} as const;

export interface AuctionStatusStyleConfig {
  gradient: string;
  bgGlow: string;
  pulse: boolean;
}

export const AUCTION_STATUS_STYLES: Record<AuctionStatus, AuctionStatusStyleConfig> = {
  [AuctionStatus.Live]: {
    gradient: "from-emerald-500 to-green-600",
    bgGlow: "shadow-emerald-500/25",
    pulse: true,
  },
  [AuctionStatus.Finished]: {
    gradient: "from-slate-500 to-slate-600",
    bgGlow: "",
    pulse: false,
  },
  [AuctionStatus.ReservedNotMet]: {
    gradient: "from-amber-500 to-orange-600",
    bgGlow: "shadow-amber-500/25",
    pulse: false,
  },
  [AuctionStatus.Inactive]: {
    gradient: "from-slate-400 to-slate-500",
    bgGlow: "",
    pulse: false,
  },
  [AuctionStatus.Scheduled]: {
    gradient: "from-blue-500 to-indigo-600",
    bgGlow: "shadow-blue-500/25",
    pulse: false,
  },
  [AuctionStatus.ReservedForBuyNow]: {
    gradient: "from-purple-500 to-violet-600",
    bgGlow: "shadow-purple-500/25",
    pulse: true,
  },
};

export function getAuctionStatusStyle(status: AuctionStatus): AuctionStatusStyleConfig {
  return AUCTION_STATUS_STYLES[status] || AUCTION_STATUS_STYLES[AuctionStatus.Inactive];
}

export interface BidStatusStyleConfig {
  label: string;
  className: string;
}

export const BID_STATUS_STYLES: Record<BidStatus, BidStatusStyleConfig> = {
  [BidStatus.Accepted]: {
    label: "Accepted",
    className: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
  },
  [BidStatus.AcceptedBelowReserve]: {
    label: "Below Reserve",
    className: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200"
  },
  [BidStatus.TooLow]: {
    label: "Too Low",
    className: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
  },
  [BidStatus.Pending]: {
    label: "Pending",
    className: "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200"
  },
  [BidStatus.Rejected]: {
    label: "Rejected",
    className: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
  }
};

export function getBidStatusStyle(status: BidStatus): BidStatusStyleConfig {
  return BID_STATUS_STYLES[status] || BID_STATUS_STYLES[BidStatus.Pending];
}

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
