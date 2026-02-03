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
  applicationStatus?: 'pending' | 'approved' | 'rejected'
  appliedAt?: string
  approvedAt?: string
  rejectedAt?: string
  rejectionReason?: string
}
