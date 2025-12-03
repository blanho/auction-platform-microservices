// Auctions page
import { MainLayout } from "@/components/layout/main-layout";
import { AuctionList } from "@/features/auction/auction-list";
import { AuctionProvider } from "@/context/auction.context";

export default function AuctionsPage() {
    return (
        <AuctionProvider>
            <MainLayout>
                <div className="space-y-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold">Auctions</h1>
                            <p className="text-muted-foreground">
                                Browse all available auctions
                            </p>
                        </div>
                    </div>
                    <AuctionList />
                </div>
            </MainLayout>
        </AuctionProvider>
    );
}
