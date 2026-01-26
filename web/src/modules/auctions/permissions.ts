import { Permission } from '@/shared/permissions'

export const AUCTION_PERMISSIONS = {
  VIEW: 'auctions:view' as Permission,
  CREATE: 'auctions:create' as Permission,
  EDIT: 'auctions:edit' as Permission,
  DELETE: 'auctions:delete' as Permission,
  MANAGE: 'auctions:manage' as Permission,
}
