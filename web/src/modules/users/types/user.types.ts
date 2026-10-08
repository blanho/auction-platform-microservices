import type { SELLER_APPLICATION_STATUS } from '../constants/seller-application-status'

export type SellerApplicationStatus =
  (typeof SELLER_APPLICATION_STATUS)[keyof typeof SELLER_APPLICATION_STATUS]

export interface UserProfile {
  id: string
  email: string
  username: string
  fullName?: string
  avatarUrl?: string
  phoneNumber?: string
  bio?: string
  location?: string
  emailConfirmed: boolean
  phoneNumberConfirmed: boolean
  twoFactorEnabled: boolean
  roles: string[]
  createdAt: string
  lastLoginAt?: string
}

export interface SellerStatus {
  isSeller: boolean
  applicationStatus?: SellerApplicationStatus
  appliedAt?: string
  approvedAt?: string
  rejectedAt?: string
  rejectionReason?: string
}
