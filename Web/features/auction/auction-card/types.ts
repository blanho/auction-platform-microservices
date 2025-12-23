import { Auction } from "@/types/auction";
import { SearchItem } from "@/types/search";

export type AuctionCardVariant = "default" | "compact" | "featured" | "carousel";

export type AuctionCardColorScheme = "purple" | "emerald";

export interface AuctionCardProps {
    auction: Auction | SearchItem;
    variant?: AuctionCardVariant;
    colorScheme?: AuctionCardColorScheme;
    index?: number;
    showWatchlistButton?: boolean;
    showBidButton?: boolean;
    showStats?: boolean;
    isWatched?: boolean;
    onWatchlistToggle?: (auctionId: string) => void;
}

export interface TimeDisplayProps {
    endDate: string;
    isUrgent: boolean;
    variant: AuctionCardVariant;
}
