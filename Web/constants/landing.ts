import { 
    Shield, 
    CreditCard, 
    Truck, 
    Award, 
    Clock, 
    HeartHandshake,
    Search,
    Gavel,
    Trophy,
    UserPlus,
    CheckCircle2,
    Zap,
    LucideIcon,
} from "lucide-react";

export interface TrustFeature {
    icon: LucideIcon;
    title: string;
    description: string;
    stat: string;
    statLabel: string;
}

export interface HowItWorksStep {
    step: string;
    icon: LucideIcon;
    title: string;
    description: string;
    features: string[];
    color: string;
    bgColor: string;
}

export interface Testimonial {
    id: number;
    name: string;
    avatar: string;
    role: string;
    location: string;
    rating: number;
    text: string;
    image: string;
    savings: string;
    verified: boolean;
}

export interface Stat {
    value: number;
    suffix: string;
    label: string;
    prefix?: string;
    description: string;
}

export interface LiveBidItem {
    item: string;
    bidder: string;
    amount: number;
}

export interface ValueProp {
    icon: LucideIcon;
    text: string;
}

export interface TrustLogo {
    name: string;
    text: string;
}

export const HERO_CONTENT = {
    BADGE_TEXT: "2,847 bidders online now",
    HEADLINE: {
        LINE_1: "The Smartest Way to",
        LINE_2: "Buy & Sell Online",
    },
    SUBHEADLINE: {
        TEXT: "Join over",
        MEMBERS: "500,000+ members",
        SAVINGS_TEXT: "who save an average of",
        SAVINGS_AMOUNT: "47% on retail prices",
        ENDING: "From luxury watches to rare collectibles — find your next treasure.",
    },
    CTA: {
        PRIMARY: "Explore Auctions",
        SECONDARY: "Create Free Account",
    },
    FLOATING_STATS: {
        SAVED: { value: "$2.4M+", label: "Saved by members this month" },
        REVIEWS: { value: "12,847", label: "reviews" },
    },
} as const;

export const VALUE_PROPS: ValueProp[] = [
    { icon: Shield, text: "Buyer Protection Guaranteed" },
    { icon: CheckCircle2, text: "100% Authentic Items" },
    { icon: Zap, text: "Real-Time Bidding" },
];

export const TRUST_LOGOS: TrustLogo[] = [
    { name: "Forbes", text: "FORBES" },
    { name: "TechCrunch", text: "TechCrunch" },
    { name: "Bloomberg", text: "Bloomberg" },
    { name: "CNBC", text: "CNBC" },
];

export const TRUST_FEATURES: TrustFeature[] = [
    {
        icon: Shield,
        title: "Buyer Protection",
        description: "Every purchase is backed by our comprehensive money-back guarantee. Shop with complete confidence.",
        stat: "$50M+",
        statLabel: "Protected",
    },
    {
        icon: CreditCard,
        title: "Secure Payments",
        description: "Bank-level 256-bit SSL encryption keeps your financial data safe. Multiple payment options available.",
        stat: "100%",
        statLabel: "Secure",
    },
    {
        icon: Award,
        title: "Verified Sellers",
        description: "Every seller undergoes rigorous verification. Background checks, identity confirmation, and quality standards.",
        stat: "15,000+",
        statLabel: "Verified",
    },
    {
        icon: Truck,
        title: "Global Shipping",
        description: "Fast, insured delivery to 150+ countries. Real-time tracking and delivery guarantees on all orders.",
        stat: "150+",
        statLabel: "Countries",
    },
    {
        icon: Clock,
        title: "24/7 Support",
        description: "Our dedicated support team is available around the clock. Average response time under 2 minutes.",
        stat: "<2 min",
        statLabel: "Response",
    },
    {
        icon: HeartHandshake,
        title: "Satisfaction Promise",
        description: "Not happy? Full refund within 14 days, no questions asked. Your satisfaction is our top priority.",
        stat: "99.2%",
        statLabel: "Satisfied",
    },
];

export const HOW_IT_WORKS_CONTENT = {
    LABEL: "How It Works",
    TITLE: "Start Winning in 4 Simple Steps",
    DESCRIPTION: "From sign-up to delivery, we've made the entire auction experience seamless. Join over 500,000 happy members who save big every day.",
    CTA: {
        TITLE: "Ready to start winning?",
        SUBTITLE: "Join 500,000+ members saving up to 70% on premium items",
        BUTTON: "Create Free Account",
    },
} as const;

export const HOW_IT_WORKS_STEPS: HowItWorksStep[] = [
    {
        step: "01",
        icon: UserPlus,
        title: "Create Your Free Account",
        description: "Sign up in under 60 seconds. No credit card required, no hidden fees. Start exploring thousands of auctions immediately.",
        features: [
            "Free forever membership",
            "Instant access to all auctions",
            "Personalized recommendations",
        ],
        color: "from-blue-500 to-indigo-600",
        bgColor: "bg-blue-50 dark:bg-blue-950/30",
    },
    {
        step: "02",
        icon: Search,
        title: "Find Your Perfect Item",
        description: "Browse curated collections from verified sellers worldwide. Use smart filters to discover exactly what you're looking for.",
        features: [
            "50,000+ active listings",
            "AI-powered search",
            "Save favorites & set alerts",
        ],
        color: "from-purple-500 to-pink-600",
        bgColor: "bg-purple-50 dark:bg-purple-950/30",
    },
    {
        step: "03",
        icon: Gavel,
        title: "Place Your Winning Bid",
        description: "Bid with confidence using our real-time auction system. Set auto-bid to never miss out, even while you sleep.",
        features: [
            "Real-time bid updates",
            "Smart auto-bidding",
            "Outbid notifications",
        ],
        color: "from-orange-500 to-red-600",
        bgColor: "bg-orange-50 dark:bg-orange-950/30",
    },
    {
        step: "04",
        icon: Trophy,
        title: "Win & Receive Your Item",
        description: "Secure checkout with buyer protection. Track your delivery in real-time and receive your item with our satisfaction guarantee.",
        features: [
            "Secure escrow payments",
            "Insured global shipping",
            "14-day money-back guarantee",
        ],
        color: "from-green-500 to-emerald-600",
        bgColor: "bg-green-50 dark:bg-green-950/30",
    },
];

export const STATS_CONTENT = {
    TITLE: "Trusted by Hundreds of Thousands",
    DESCRIPTION: "Numbers that speak for themselves. Join the fastest-growing auction community.",
} as const;

export const STATS: Stat[] = [
    { value: 500000, suffix: "+", label: "Active Members", description: "Trusted buyers & sellers" },
    { value: 2.4, suffix: "M", label: "Items Sold", prefix: "$", description: "In total transaction value" },
    { value: 47, suffix: "%", label: "Average Savings", description: "Compared to retail prices" },
    { value: 99.2, suffix: "%", label: "Satisfaction Rate", description: "Based on 50K+ reviews" },
];

export const TESTIMONIALS_CONTENT = {
    LABEL: "Success Stories",
    TITLE: "Hear From Our Community",
    DESCRIPTION: "Join over 500,000 members who are saving money and finding treasures every day.",
    AUTOPLAY_INTERVAL: 6000,
} as const;

export const TESTIMONIALS: Testimonial[] = [
    {
        id: 1,
        name: "Sarah Mitchell",
        avatar: "SM",
        role: "Luxury Watch Collector",
        location: "New York, USA",
        rating: 5,
        text: "I've saved over $15,000 on my watch collection in just 6 months. The authentication process gives me complete confidence in every purchase. This platform has completely changed how I collect.",
        image: "https://randomuser.me/api/portraits/women/1.jpg",
        savings: "$15,000+ saved",
        verified: true,
    },
    {
        id: 2,
        name: "James Chen",
        avatar: "JC",
        role: "Tech Entrepreneur",
        location: "San Francisco, USA",
        rating: 5,
        text: "As a busy professional, the auto-bid feature is a game changer. I've won 12 auctions while in meetings. The real-time notifications keep me informed without constant monitoring.",
        image: "https://randomuser.me/api/portraits/men/2.jpg",
        savings: "12 auctions won",
        verified: true,
    },
    {
        id: 3,
        name: "Emma Thompson",
        avatar: "ET",
        role: "Interior Designer",
        location: "London, UK",
        rating: 5,
        text: "Finding unique vintage pieces for my clients used to take weeks. Now I source rare items in days. The quality and authenticity verification is unmatched in the industry.",
        image: "https://randomuser.me/api/portraits/women/3.jpg",
        savings: "200+ items sourced",
        verified: true,
    },
    {
        id: 4,
        name: "Marcus Rodriguez",
        avatar: "MR",
        role: "Classic Car Enthusiast",
        location: "Miami, USA",
        rating: 5,
        text: "Found my dream 1965 Mustang at 40% below market value. The escrow system protected a $45,000 transaction flawlessly. Couldn't be happier with the entire experience.",
        image: "https://randomuser.me/api/portraits/men/4.jpg",
        savings: "40% below market",
        verified: true,
    },
    {
        id: 5,
        name: "Aisha Patel",
        avatar: "AP",
        role: "Art Investor",
        location: "Dubai, UAE",
        rating: 5,
        text: "The global shipping and insurance options make international art acquisition stress-free. I've built a significant collection with complete peace of mind on every transaction.",
        image: "https://randomuser.me/api/portraits/women/5.jpg",
        savings: "Global collector",
        verified: true,
    },
];

export const LIVE_TICKER_CONTENT = {
    BADGE: "Live Feed",
    INITIAL_STATS: {
        totalBids: 12847,
        activeBidders: 2847,
    },
    UPDATE_INTERVAL: 4000,
};

export const INITIAL_BIDS: (LiveBidItem & { id: string; timeAgo: string })[] = [
    { id: "1", item: "Rolex Submariner Date", bidder: "Alex M.", amount: 12500, timeAgo: "2s ago" },
    { id: "2", item: "1967 Ford Mustang GT", bidder: "Sarah K.", amount: 45000, timeAgo: "5s ago" },
    { id: "3", item: "Banksy Original Print", bidder: "John D.", amount: 8900, timeAgo: "8s ago" },
    { id: "4", item: "Charizard PSA 10", bidder: "Mike T.", amount: 32000, timeAgo: "12s ago" },
    { id: "5", item: "Cartier Love Bracelet", bidder: "Emma L.", amount: 5600, timeAgo: "15s ago" },
    { id: "6", item: "MacBook Pro M4 Max", bidder: "Chris P.", amount: 2800, timeAgo: "20s ago" },
    { id: "7", item: "Gibson Les Paul 1959", bidder: "David R.", amount: 165000, timeAgo: "25s ago" },
    { id: "8", item: "Hermès Birkin 25", bidder: "Anna S.", amount: 28000, timeAgo: "30s ago" },
];

export const NEW_BIDS_POOL: LiveBidItem[] = [
    { item: "Ferrari 488 Pista", bidder: "Tom H.", amount: 285000 },
    { item: "Michael Jordan Jersey", bidder: "Kevin W.", amount: 4200 },
    { item: "Patek Philippe Nautilus", bidder: "Rachel B.", amount: 89000 },
    { item: "Air Jordan 1 Chicago", bidder: "Jason M.", amount: 12000 },
    { item: "Leica M6 Classic", bidder: "Lisa P.", amount: 4890 },
    { item: "Tiffany Engagement Ring", bidder: "Mark Z.", amount: 24000 },
];

export const CTA_CONTENT = {
    BADGE: "Limited Time: Free Premium Access",
    TITLE: "Stop Overpaying. Start Winning.",
    DESCRIPTION: "Join 500,000+ smart shoppers who save an average of 47% on every purchase. Your next great deal is waiting.",
    FEATURES: [
        "No membership fees",
        "$50M+ buyer protection",
        "Free global shipping",
    ],
    BUTTON: "Start Bidding Now — It's Free",
} as const;
