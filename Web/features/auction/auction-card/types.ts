import { Auction } from "@/types/auction";
import { SearchItem } from "@/types/search";

export type AuctionCardVariant = "default" | "compact" | "featured" | "carousel";

export type AuctionCardColorScheme = "purple" | "emerald";

export interface AuctionCardProps {
    auction: Auction | SearchItem;
    variant?: AuctionCardVariant;
    colorScheme?: AuctionCardColorScheme;
    index?: number;
    showWishlistButton?: boolean;
    showBidButton?: boolean;
    showStats?: boolean;
    isWishlisted?: boolean;
    onWishlistToggle?: (auctionId: string) => void;
}

export interface TimeDisplayProps {
    endDate: string;
    isUrgent: boolean;
    variant: AuctionCardVariant;
}

export interface StatusConfig {
    bg: string;
    text: string;
    icon: typeof import("@fortawesome/free-solid-svg-icons").faBolt | null;
    pulse: boolean;
}
