export interface UserProfile {
  id: string
  email: string
  username: string
  displayName: string
  firstName?: string
  lastName?: string
  avatarUrl?: string
  phoneNumber?: string
  bio?: string
  isSeller: boolean
  isVerified: boolean
  twoFactorEnabled: boolean
  createdAt: string
  updatedAt: string
}

export interface SellerStatus {
  isSeller: boolean
  applicationStatus?: 'pending' | 'approved' | 'rejected'
  appliedAt?: string
  approvedAt?: string
  rejectedAt?: string
  rejectionReason?: string
}
