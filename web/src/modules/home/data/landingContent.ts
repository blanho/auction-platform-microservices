export const LIVE_STATS = [
  { label: 'Active Bidders', value: 2847, prefix: '', suffix: '' },
  { label: 'Live Auctions', value: 156, prefix: '', suffix: '' },
  { label: 'Total Sales Today', value: 847500, prefix: '$', suffix: '' },
] as const

export const FEATURED_CATEGORIES = [
  {
    id: '1',
    name: 'Fine Art',
    image: '/images/categories/fine-art.jpg',
    count: 234,
    gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
  },
  {
    id: '2',
    name: 'Antiques',
    image: '/images/categories/antiques.jpg',
    count: 156,
    gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
  },
  {
    id: '3',
    name: 'Jewelry',
    image: '/images/categories/jewelry.jpg',
    count: 89,
    gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
  },
  {
    id: '4',
    name: 'Collectibles',
    image: '/images/categories/collectibles.jpg',
    count: 312,
    gradient: 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
  },
  {
    id: '5',
    name: 'Watches',
    image: '/images/categories/watches.jpg',
    count: 67,
    gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
  },
  {
    id: '6',
    name: 'Wine & Spirits',
    image: '/images/categories/wine.jpg',
    count: 45,
    gradient: 'linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)',
  },
] as const

export const FEATURED_COLLECTIONS = [
  {
    id: '1',
    name: 'Impressionist Masters',
    image: 'https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5?w=600',
    itemCount: 24,
    totalValue: '$4.2M',
  },
  {
    id: '2',
    name: 'Vintage Timepieces',
    image: 'https://images.unsplash.com/photo-1587836374828-4dbafa94cf0e?w=600',
    itemCount: 18,
    totalValue: '$1.8M',
  },
  {
    id: '3',
    name: 'Contemporary Art',
    image: 'https://images.unsplash.com/photo-1541367777708-7905fe3296c0?w=600',
    itemCount: 32,
    totalValue: '$2.9M',
  },
  {
    id: '4',
    name: 'Rare Manuscripts',
    image: 'https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=600',
    itemCount: 15,
    totalValue: '$3.1M',
  },
] as const

export const TRUSTED_BRANDS = [
  { name: "Christie's", logo: 'C' },
  { name: "Sotheby's", logo: 'S' },
  { name: 'Phillips', logo: 'P' },
  { name: 'Bonhams', logo: 'B' },
  { name: 'Heritage', logo: 'H' },
  { name: 'Artcurial', logo: 'A' },
] as const

export const PLATFORM_FEATURES = [
  {
    icon: 'Shield',
    title: 'Expert Authentication',
    description: 'Every item verified by certified specialists. Full provenance documentation included.',
    gradient: 'linear-gradient(135deg, #3B82F6 0%, #8B5CF6 100%)',
  },
  {
    icon: 'Gavel',
    title: 'Transparent Bidding',
    description: 'Real-time updates, no hidden fees. Our escrow system protects every transaction.',
    gradient: 'linear-gradient(135deg, #22C55E 0%, #10B981 100%)',
  },
  {
    icon: 'LocalShipping',
    title: 'White-Glove Delivery',
    description: 'Insured worldwide shipping with climate-controlled transport for delicate items.',
    gradient: 'linear-gradient(135deg, #F59E0B 0%, #EF4444 100%)',
  },
  {
    icon: 'TrendingUp',
    title: 'Seller Success',
    description: 'Reach 50,000+ verified collectors. Competitive rates with dedicated support.',
    gradient: 'linear-gradient(135deg, #EC4899 0%, #8B5CF6 100%)',
  },
] as const

export type LiveStat = (typeof LIVE_STATS)[number]
export type FeaturedCategory = (typeof FEATURED_CATEGORIES)[number]
export type FeaturedCollection = (typeof FEATURED_COLLECTIONS)[number]
export type TrustedBrand = (typeof TRUSTED_BRANDS)[number]
export type PlatformFeature = (typeof PLATFORM_FEATURES)[number]
