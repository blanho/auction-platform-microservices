import { cn } from "@/lib/utils";

export const conditionalStyles = {
    loading: (isLoading: boolean) => cn(isLoading && "animate-spin"),
    
    disabled: (isDisabled: boolean) => cn(
        isDisabled && "opacity-50 cursor-not-allowed pointer-events-none"
    ),
    
    active: (isActive: boolean, activeClass = "border-purple-500") => cn(
        isActive ? activeClass : "hover:border-slate-300 dark:hover:border-slate-600"
    ),
    
    selected: (isSelected: boolean) => cn(
        isSelected 
            ? "bg-purple-100 dark:bg-purple-900/30 border-purple-500" 
            : "bg-white dark:bg-slate-900"
    ),

    urgent: (isUrgent: boolean) => cn(
        isUrgent ? "text-red-500" : "text-slate-900 dark:text-white"
    ),

    success: (isSuccess: boolean) => cn(
        isSuccess ? "text-green-600 dark:text-green-400" : ""
    ),

    error: (hasError: boolean) => cn(
        hasError ? "border-red-500 focus:ring-red-500/20" : ""
    ),

    visible: (isVisible: boolean) => cn(
        isVisible ? "opacity-100 visible" : "opacity-0 invisible"
    ),

    expanded: (isExpanded: boolean) => cn(
        isExpanded ? "rotate-180" : "rotate-0",
        "transition-transform duration-200"
    ),

    favorited: (isFavorited: boolean) => cn(
        isFavorited ? "text-red-500 fill-red-500" : "text-slate-400 hover:text-red-500"
    ),
} as const;

export const stateStyles = {
    price: {
        aboveReserve: "text-green-600 dark:text-green-400 font-medium",
        belowReserve: "text-amber-600 dark:text-amber-400",
        default: "text-slate-900 dark:text-white",
    },
    
    timer: {
        urgent: "text-red-500 font-bold",
        warning: "text-amber-500 font-medium", 
        normal: "text-slate-600 dark:text-slate-400",
    },

    balance: {
        sufficient: "text-green-500 font-medium",
        insufficient: "text-red-500 font-medium",
    },

    bid: {
        winning: "bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800",
        outbid: "bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800",
        neutral: "bg-slate-50 dark:bg-slate-800/50",
    },
} as const;

export function getPriceStyle(currentBid: number, reservePrice: number) {
    if (currentBid >= reservePrice) return stateStyles.price.aboveReserve;
    if (currentBid > 0) return stateStyles.price.belowReserve;
    return stateStyles.price.default;
}

export function getTimerStyle(secondsRemaining: number) {
    if (secondsRemaining <= 60) return stateStyles.timer.urgent;
    if (secondsRemaining <= 300) return stateStyles.timer.warning;
    return stateStyles.timer.normal;
}

export function getBalanceStyle(balance: number, required: number) {
    return balance >= required 
        ? stateStyles.balance.sufficient 
        : stateStyles.balance.insufficient;
}

export function getBidStyle(userBid: number, highestBid: number, isUserBid: boolean) {
    if (!isUserBid) return stateStyles.bid.neutral;
    return userBid >= highestBid ? stateStyles.bid.winning : stateStyles.bid.outbid;
}

export const gradients = {
    primary: "bg-gradient-to-r from-purple-600 to-blue-600",
    primaryHover: "bg-gradient-to-r from-purple-700 to-blue-700",
    primarySubtle: "bg-gradient-to-r from-purple-600/10 to-blue-600/10",
    gold: "bg-gradient-to-r from-amber-500 to-yellow-400",
    success: "bg-gradient-to-r from-green-500 to-emerald-500",
    danger: "bg-gradient-to-r from-red-500 to-rose-500",
    dark: "bg-gradient-to-br from-slate-900 to-slate-950",
    mesh: "gradient-mesh",
} as const;

export const gradientText = {
    primary: "bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent",
    gold: "bg-gradient-to-r from-amber-500 to-yellow-400 bg-clip-text text-transparent",
    success: "bg-gradient-to-r from-green-500 to-emerald-500 bg-clip-text text-transparent",
} as const;

export const glass = {
    default: "bg-white/80 dark:bg-slate-950/80 backdrop-blur-xl",
    subtle: "bg-white/50 dark:bg-slate-950/50 backdrop-blur-md",
    card: "bg-white/70 dark:bg-slate-900/70 backdrop-blur-xl border border-white/20 dark:border-slate-800/50",
    dark: "bg-slate-900/80 backdrop-blur-xl",
} as const;

export const shadows = {
    sm: "shadow-sm",
    md: "shadow-md",
    lg: "shadow-lg",
    xl: "shadow-xl",
    "2xl": "shadow-2xl",
    soft: "shadow-soft",
    medium: "shadow-medium",
    strong: "shadow-strong",
    intense: "shadow-intense",
    glow: {
        purple: "shadow-lg shadow-purple-500/25 dark:shadow-purple-500/20",
        blue: "shadow-lg shadow-blue-500/25 dark:shadow-blue-500/20",
        gold: "shadow-lg shadow-amber-500/25 dark:shadow-amber-500/20",
        success: "shadow-lg shadow-green-500/25 dark:shadow-green-500/20",
        danger: "shadow-lg shadow-red-500/25 dark:shadow-red-500/20",
    },
} as const;

export const buttons = {
    primary: cn(
        "inline-flex items-center justify-center gap-2",
        "bg-gradient-to-r from-purple-600 to-blue-600",
        "hover:from-purple-700 hover:to-blue-700",
        "text-white font-medium",
        "shadow-lg shadow-purple-500/25 hover:shadow-purple-500/40",
        "transition-all duration-300",
        "focus-ring"
    ),
    secondary: cn(
        "inline-flex items-center justify-center gap-2",
        "bg-slate-100 dark:bg-slate-800",
        "hover:bg-slate-200 dark:hover:bg-slate-700",
        "text-slate-900 dark:text-slate-100 font-medium",
        "transition-all duration-300",
        "focus-ring"
    ),
    outline: cn(
        "inline-flex items-center justify-center gap-2",
        "border-2 border-purple-500",
        "text-purple-600 dark:text-purple-400",
        "hover:bg-purple-500 hover:text-white",
        "font-medium transition-all duration-300",
        "focus-ring"
    ),
    ghost: cn(
        "inline-flex items-center justify-center gap-2",
        "hover:bg-slate-100 dark:hover:bg-slate-800",
        "text-slate-700 dark:text-slate-300 font-medium",
        "transition-all duration-300",
        "focus-ring"
    ),
    danger: cn(
        "inline-flex items-center justify-center gap-2",
        "bg-gradient-to-r from-red-500 to-rose-500",
        "hover:from-red-600 hover:to-rose-600",
        "text-white font-medium",
        "shadow-lg shadow-red-500/25 hover:shadow-red-500/40",
        "transition-all duration-300",
        "focus-ring"
    ),
    success: cn(
        "inline-flex items-center justify-center gap-2",
        "bg-gradient-to-r from-green-500 to-emerald-500",
        "hover:from-green-600 hover:to-emerald-600",
        "text-white font-medium",
        "shadow-lg shadow-green-500/25 hover:shadow-green-500/40",
        "transition-all duration-300",
        "focus-ring"
    ),
} as const;

export const inputs = {
    default: cn(
        "w-full",
        "bg-white dark:bg-slate-900",
        "border border-slate-200 dark:border-slate-700",
        "focus:border-purple-500 dark:focus:border-purple-400",
        "focus:ring-2 focus:ring-purple-500/20 dark:focus:ring-purple-400/20",
        "placeholder:text-slate-400 dark:placeholder:text-slate-500",
        "transition-all duration-300",
        "rounded-lg"
    ),
    search: cn(
        "w-full pl-10",
        "bg-slate-50 dark:bg-slate-900",
        "border border-slate-200 dark:border-slate-700",
        "focus:border-purple-500 dark:focus:border-purple-400",
        "focus:ring-2 focus:ring-purple-500/20",
        "placeholder:text-slate-400",
        "transition-all duration-300",
        "rounded-xl"
    ),
    glass: cn(
        "w-full",
        "bg-white/10 backdrop-blur-sm",
        "border border-white/20",
        "focus:border-white/40",
        "text-white placeholder:text-white/50",
        "transition-all duration-300",
        "rounded-xl"
    ),
} as const;

export const cards = {
    default: cn(
        "bg-white dark:bg-slate-900",
        "rounded-2xl",
        "border border-slate-200/80 dark:border-slate-800/80"
    ),
    elevated: cn(
        "bg-white dark:bg-slate-900",
        "rounded-2xl",
        "border border-slate-200/80 dark:border-slate-800/80",
        "shadow-md hover:shadow-lg",
        "transition-all duration-300"
    ),
    interactive: cn(
        "bg-white dark:bg-slate-900",
        "rounded-2xl",
        "border border-slate-200/80 dark:border-slate-800/80",
        "shadow-sm hover:shadow-lg",
        "hover:-translate-y-1",
        "transition-all duration-300",
        "cursor-pointer"
    ),
    glass: cn(
        "bg-white/70 dark:bg-slate-900/70",
        "backdrop-blur-xl",
        "rounded-2xl",
        "border border-white/20 dark:border-slate-800/50",
        "shadow-md"
    ),
    gradient: cn(
        "bg-gradient-to-br from-purple-600 to-blue-600",
        "rounded-2xl",
        "text-white",
        "shadow-lg shadow-purple-500/25"
    ),
    auction: cn(
        "group",
        "bg-white dark:bg-slate-900",
        "rounded-2xl",
        "border border-slate-200/80 dark:border-slate-800/80",
        "shadow-sm hover:shadow-xl",
        "hover:-translate-y-1",
        "transition-all duration-300",
        "overflow-hidden"
    ),
} as const;

export const badges = {
    primary: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-medium",
        "bg-purple-100 dark:bg-purple-900/30",
        "text-purple-700 dark:text-purple-300"
    ),
    success: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-medium",
        "bg-green-100 dark:bg-green-900/30",
        "text-green-700 dark:text-green-300"
    ),
    warning: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-medium",
        "bg-amber-100 dark:bg-amber-900/30",
        "text-amber-700 dark:text-amber-300"
    ),
    danger: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-medium",
        "bg-red-100 dark:bg-red-900/30",
        "text-red-700 dark:text-red-300"
    ),
    live: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-1 rounded-full",
        "text-xs font-bold",
        "bg-green-500 text-white",
        "animate-pulse"
    ),
    new: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-bold",
        "bg-gradient-to-r from-purple-600 to-blue-600",
        "text-white"
    ),
    hot: cn(
        "inline-flex items-center gap-1.5",
        "px-2.5 py-0.5 rounded-full",
        "text-xs font-bold",
        "bg-gradient-to-r from-orange-500 to-red-500",
        "text-white"
    ),
} as const;

export const text = {
    heading: {
        h1: "text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight",
        h2: "text-3xl md:text-4xl font-bold tracking-tight",
        h3: "text-2xl md:text-3xl font-bold",
        h4: "text-xl md:text-2xl font-semibold",
        h5: "text-lg font-semibold",
        h6: "text-base font-semibold",
    },
    body: {
        lg: "text-lg text-slate-600 dark:text-slate-400 leading-relaxed",
        default: "text-base text-slate-600 dark:text-slate-400",
        sm: "text-sm text-slate-500 dark:text-slate-400",
        xs: "text-xs text-slate-500 dark:text-slate-500",
    },
    label: "text-sm font-medium text-slate-700 dark:text-slate-300",
    caption: "text-xs text-slate-500 dark:text-slate-400",
    overline: "text-xs font-semibold uppercase tracking-wider text-slate-500 dark:text-slate-400",
} as const;

export const links = {
    primary: cn(
        "text-purple-600 dark:text-purple-400",
        "hover:text-purple-700 dark:hover:text-purple-300",
        "underline-offset-4 hover:underline",
        "transition-colors"
    ),
    subtle: cn(
        "text-slate-600 dark:text-slate-400",
        "hover:text-slate-900 dark:hover:text-white",
        "transition-colors"
    ),
    nav: cn(
        "text-slate-600 dark:text-slate-300",
        "hover:text-purple-600 dark:hover:text-purple-400",
        "font-medium",
        "transition-colors"
    ),
} as const;

export const effects = {
    hover: {
        lift: "hover:-translate-y-1 hover:shadow-lg transition-all duration-300",
        scale: "hover:scale-[1.02] transition-transform duration-200",
        glow: "hover:shadow-lg hover:shadow-purple-500/25 transition-shadow duration-300",
        brighten: "hover:brightness-110 transition-all duration-200",
    },
    focus: {
        ring: "focus:outline-none focus:ring-2 focus:ring-purple-500/50 focus:ring-offset-2 focus:ring-offset-white dark:focus:ring-offset-slate-950",
        outline: "focus:outline-none focus:border-purple-500 dark:focus:border-purple-400",
    },
    active: {
        scale: "active:scale-[0.98] transition-transform",
        press: "active:translate-y-0.5 transition-transform",
    },
} as const;

export const layout = {
    section: "py-16 md:py-24 lg:py-32",
    sectionSm: "py-12 md:py-16 lg:py-20",
    container: "container mx-auto px-4",
    containerTight: "max-w-5xl mx-auto px-4",
    containerWide: "max-w-7xl mx-auto px-4",
    stack: {
        xs: "space-y-2",
        sm: "space-y-4",
        md: "space-y-6",
        lg: "space-y-8",
        xl: "space-y-12",
    },
    row: {
        xs: "space-x-2",
        sm: "space-x-4",
        md: "space-x-6",
        lg: "space-x-8",
    },
    grid: {
        cols2: "grid grid-cols-1 sm:grid-cols-2 gap-4 md:gap-6",
        cols3: "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6",
        cols4: "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6",
        cols6: "grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4",
        auto: "grid grid-cols-[repeat(auto-fill,minmax(280px,1fr))] gap-4 md:gap-6",
    },
} as const;

export const animations = {
    fadeIn: "animate-fade-in",
    fadeInUp: "animate-fade-in-up",
    fadeInDown: "animate-fade-in-down",
    scaleIn: "animate-scale-in",
    slideInRight: "animate-slide-in-right",
    slideInLeft: "animate-slide-in-left",
    float: "animate-float",
    pulse: "animate-pulse",
    pulseGlow: "animate-pulse-glow",
    bounce: "animate-bounce",
    bounceSub: "animate-bounce-subtle",
    spin: "animate-spin",
    spinSlow: "animate-spin-slow",
    shimmer: "animate-shimmer",
    marquee: "animate-marquee",
} as const;

export const skeleton = {
    base: "bg-slate-200 dark:bg-slate-800 animate-pulse rounded",
    text: "h-4 w-full bg-slate-200 dark:bg-slate-800 animate-pulse rounded",
    title: "h-6 w-3/4 bg-slate-200 dark:bg-slate-800 animate-pulse rounded",
    avatar: "rounded-full bg-slate-200 dark:bg-slate-800 animate-pulse",
    image: "aspect-square w-full bg-slate-200 dark:bg-slate-800 animate-pulse rounded-xl",
    card: "w-full h-48 bg-slate-200 dark:bg-slate-800 animate-pulse rounded-2xl",
} as const;

export const dividers = {
    horizontal: "h-px bg-gradient-to-r from-transparent via-slate-300 dark:via-slate-700 to-transparent",
    vertical: "w-px h-full bg-gradient-to-b from-transparent via-slate-300 dark:via-slate-700 to-transparent",
    solid: "h-px bg-slate-200 dark:bg-slate-800",
    dashed: "border-t border-dashed border-slate-200 dark:border-slate-800",
} as const;

export const statusColors = {
    live: {
        bg: "bg-green-500",
        text: "text-green-600 dark:text-green-400",
        badge: "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300",
    },
    ending: {
        bg: "bg-amber-500",
        text: "text-amber-600 dark:text-amber-400",
        badge: "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300",
    },
    ended: {
        bg: "bg-slate-400",
        text: "text-slate-500 dark:text-slate-400",
        badge: "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400",
    },
    upcoming: {
        bg: "bg-blue-500",
        text: "text-blue-600 dark:text-blue-400",
        badge: "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300",
    },
    sold: {
        bg: "bg-purple-500",
        text: "text-purple-600 dark:text-purple-400",
        badge: "bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300",
    },
} as const;

export const iconSizes = {
    xs: "w-3 h-3",
    sm: "w-4 h-4",
    md: "w-5 h-5",
    lg: "w-6 h-6",
    xl: "w-8 h-8",
    "2xl": "w-10 h-10",
} as const;

export const iconContainers = {
    sm: cn(
        "w-8 h-8 rounded-lg",
        "flex items-center justify-center",
        "bg-slate-100 dark:bg-slate-800"
    ),
    md: cn(
        "w-10 h-10 rounded-xl",
        "flex items-center justify-center",
        "bg-slate-100 dark:bg-slate-800"
    ),
    lg: cn(
        "w-12 h-12 rounded-xl",
        "flex items-center justify-center",
        "bg-slate-100 dark:bg-slate-800"
    ),
    gradient: cn(
        "w-10 h-10 rounded-xl",
        "flex items-center justify-center",
        "bg-gradient-to-br from-purple-600 to-blue-600",
        "text-white",
        "shadow-lg shadow-purple-500/25"
    ),
} as const;

export const scrollbar = {
    hide: "scrollbar-hide",
    thin: "scrollbar-thin",
} as const;

export function getStatusStyles(status: string) {
    const normalizedStatus = status.toLowerCase();
    if (normalizedStatus === "live" || normalizedStatus === "active") {
        return statusColors.live;
    }
    if (normalizedStatus === "ending" || normalizedStatus === "ending soon") {
        return statusColors.ending;
    }
    if (normalizedStatus === "ended" || normalizedStatus === "closed") {
        return statusColors.ended;
    }
    if (normalizedStatus === "upcoming" || normalizedStatus === "scheduled") {
        return statusColors.upcoming;
    }
    if (normalizedStatus === "sold" || normalizedStatus === "completed") {
        return statusColors.sold;
    }
    return statusColors.ended;
}

export function getAnimationDelay(index: number, baseDelay = 100) {
    return { animationDelay: `${index * baseDelay}ms` };
}

export type GradientVariant = keyof typeof gradients;
export type ButtonVariant = keyof typeof buttons;
export type CardVariant = keyof typeof cards;
export type BadgeVariant = keyof typeof badges;
export type TextSize = keyof typeof text.body;
export type HeadingLevel = keyof typeof text.heading;
export type IconSize = keyof typeof iconSizes;
export type StatusType = keyof typeof statusColors;
