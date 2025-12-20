export const PLATFORM_FEE_PERCENTAGE = 5;
export const BID_PLATFORM_FEE_PERCENTAGE = 2.5;

export const PAYMENT_METHODS = [
  {
    id: "wallet",
    name: "Wallet Balance",
    description: "Pay using your auction wallet balance",
    iconName: "Wallet" as const,
  },
  {
    id: "card",
    name: "Credit/Debit Card",
    description: "Pay securely with Stripe",
    iconName: "CreditCard" as const,
  },
] as const;

export type PaymentMethodId = (typeof PAYMENT_METHODS)[number]["id"];

export const PLATFORM_STATS = [
  { label: "Active Users", value: "50,000+", iconName: "Users" as const },
  { label: "Auctions Completed", value: "100,000+", iconName: "Award" as const },
  { label: "Countries Served", value: "25+", iconName: "Globe" as const },
  { label: "Items Listed", value: "500,000+", iconName: "TrendingUp" as const },
] as const;

export const PLATFORM_VALUES = [
  {
    title: "Trust & Transparency",
    description:
      "We believe in building trust through transparent processes. Every transaction is secure, and every listing is verified.",
    iconName: "Shield" as const,
  },
  {
    title: "User-First Approach",
    description:
      "Our platform is designed with you in mind. We continuously improve based on user feedback to deliver the best experience.",
    iconName: "Heart" as const,
  },
  {
    title: "Innovation",
    description:
      "We leverage cutting-edge technology to provide real-time bidding, secure payments, and a seamless user experience.",
    iconName: "Zap" as const,
  },
  {
    title: "Fair Marketplace",
    description:
      "We ensure a level playing field for all buyers and sellers with clear rules, dispute resolution, and fraud protection.",
    iconName: "Target" as const,
  },
] as const;

export const LEADERSHIP_TEAM = [
  {
    name: "John Smith",
    role: "CEO & Founder",
    bio: "20+ years in e-commerce and marketplace development",
  },
  {
    name: "Sarah Johnson",
    role: "CTO",
    bio: "Former tech lead at major tech companies",
  },
  {
    name: "Michael Chen",
    role: "Head of Operations",
    bio: "Expert in logistics and customer experience",
  },
  {
    name: "Emily Davis",
    role: "Head of Trust & Safety",
    bio: "Background in fraud prevention and security",
  },
] as const;

export const FAQS = [
  {
    question: "How do I place a bid?",
    answer:
      "To place a bid, navigate to any live auction, enter your bid amount (must be higher than the current bid), and click 'Place Bid'. You must be logged in to bid.",
  },
  {
    question: "What happens when I win an auction?",
    answer:
      "When you win an auction, you'll receive a notification and email. Payment must be completed within 3 days. Once payment is confirmed, the seller will ship your item.",
  },
  {
    question: "How do I sell an item?",
    answer:
      "Click 'Sell' or 'Create Auction' from the navigation. Fill out the item details, set your starting price and auction duration, then submit. Your listing will be reviewed before going live.",
  },
  {
    question: "What payment methods are accepted?",
    answer:
      "We accept major credit cards, debit cards, and bank transfers. All payments are processed securely through our payment partner.",
  },
  {
    question: "How does buyer protection work?",
    answer:
      "All purchases are covered by our Buyer Protection program. If an item doesn't arrive or isn't as described, you can file a claim within 30 days for a full refund.",
  },
  {
    question: "Can I cancel a bid?",
    answer:
      "Bids are binding contracts. You can only retract a bid if you made an obvious error (like bidding $1,000 instead of $100). Contact support immediately if this happens.",
  },
  {
    question: "How do withdrawals work?",
    answer:
      "Sellers can request withdrawals from their wallet balance. Processing typically takes 3-5 business days depending on your payment method.",
  },
  {
    question: "What are the seller fees?",
    answer:
      "We charge a 5% commission on successful sales. There are no listing fees. Additional features like featured listings may have extra costs.",
  },
] as const;

export const CONTACT_OPTIONS = [
  {
    iconName: "MessageCircle" as const,
    title: "Live Chat",
    description: "Get instant help from our support team",
    action: "Start Chat",
    available: "24/7",
  },
  {
    iconName: "Mail" as const,
    title: "Email Support",
    description: "support@auctionhub.com",
    action: "Send Email",
    available: "1-2 business days",
  },
  {
    iconName: "Phone" as const,
    title: "Phone Support",
    description: "1-800-AUCTION",
    action: "Call Now",
    available: "Mon-Fri 9AM-6PM",
  },
] as const;

export const HELP_CATEGORIES = [
  {
    iconName: "Gavel" as const,
    title: "Bidding & Buying",
    description: "How to bid, win, and pay for items",
  },
  {
    iconName: "Package" as const,
    title: "Selling",
    description: "Creating listings and managing sales",
  },
  {
    iconName: "CreditCard" as const,
    title: "Payments",
    description: "Payment methods, fees, and withdrawals",
  },
  {
    iconName: "Shield" as const,
    title: "Safety & Security",
    description: "Account protection and buyer guarantees",
  },
  {
    iconName: "Clock" as const,
    title: "Shipping",
    description: "Delivery times and tracking",
  },
  {
    iconName: "FileQuestion" as const,
    title: "Account Issues",
    description: "Profile, settings, and verification",
  },
] as const;

export const PLATFORM_INFO = {
  name: "Auction Platform",
  foundedYear: 2020,
  supportEmail: "support@auctionhub.com",
  supportPhone: "1-800-AUCTION",
} as const;
