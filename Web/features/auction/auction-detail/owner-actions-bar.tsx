"use client";

import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser, faPlay, faPause, faEdit } from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { ActivateAuctionDialog } from "@/features/auction/activate-auction-dialog";
import { DeactivateAuctionDialog } from "@/features/auction/deactivate-auction-dialog";
import { DeleteAuctionDialog } from "@/features/auction/delete-auction-dialog";
import { AuctionStatus } from "@/types";

interface OwnerActionsBarProps {
    auctionId: string;
    auctionTitle: string;
    status: AuctionStatus;
    onActionSuccess: () => void;
}

export function OwnerActionsBar({
    auctionId,
    auctionTitle,
    status,
    onActionSuccess,
}: OwnerActionsBarProps) {
    const router = useRouter();

    return (
        <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-6 flex items-center justify-between p-4 rounded-xl bg-gradient-to-r from-purple-50 to-blue-50 dark:from-purple-950/30 dark:to-blue-950/30 border border-purple-200/50 dark:border-purple-800/50"
        >
            <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-gradient-to-r from-purple-600 to-blue-600 flex items-center justify-center">
                    <FontAwesomeIcon icon={faUser} className="w-5 h-5 text-white" />
                </div>
                <span className="text-sm font-medium text-purple-700 dark:text-purple-300">
                    You are the owner of this auction
                </span>
            </div>
            <div className="flex gap-2">
                {status === AuctionStatus.Inactive && (
                    <ActivateAuctionDialog
                        auctionId={auctionId}
                        auctionTitle={auctionTitle}
                        onSuccess={onActionSuccess}
                        trigger={
                            <Button
                                variant="outline"
                                size="sm"
                                className="border-green-300 text-green-600 hover:bg-green-50 dark:border-green-700 dark:hover:bg-green-950"
                            >
                                <FontAwesomeIcon icon={faPlay} className="mr-2 w-3 h-3" />
                                Activate
                            </Button>
                        }
                    />
                )}
                {status === AuctionStatus.Live && (
                    <DeactivateAuctionDialog
                        auctionId={auctionId}
                        auctionTitle={auctionTitle}
                        onSuccess={onActionSuccess}
                        trigger={
                            <Button
                                variant="outline"
                                size="sm"
                                className="border-orange-300 text-orange-600 hover:bg-orange-50 dark:border-orange-700 dark:hover:bg-orange-950"
                            >
                                <FontAwesomeIcon icon={faPause} className="mr-2 w-3 h-3" />
                                Deactivate
                            </Button>
                        }
                    />
                )}
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => router.push(`/auctions/${auctionId}/edit`)}
                    className="border-slate-300 hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-800"
                >
                    <FontAwesomeIcon icon={faEdit} className="mr-2 w-3 h-3" />
                    Edit
                </Button>
                <DeleteAuctionDialog
                    auctionId={auctionId}
                    auctionTitle={auctionTitle}
                    redirectAfterDelete="/auctions"
                />
            </div>
        </motion.div>
    );
}
