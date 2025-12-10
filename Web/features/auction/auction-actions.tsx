"use client";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import {
  MoreVertical,
  Edit,
  PlayCircle,
  PauseCircle,
  Trash2,
  Eye
} from "lucide-react";
import Link from "next/link";
import { Auction, AuctionStatus } from "@/types/auction";
import { ActivateAuctionDialog } from "./activate-auction-dialog";
import { DeactivateAuctionDialog } from "./deactivate-auction-dialog";
import { DeleteAuctionDialog } from "./delete-auction-dialog";

interface AuctionActionsProps {
  auction: Auction;
  onActionComplete?: () => void;
}

export function AuctionActions({ auction, onActionComplete }: AuctionActionsProps) {
  const isInactive = auction.status === AuctionStatus.Inactive;
  const canDeactivate = auction.status === AuctionStatus.Live;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-8 w-8">
          <MoreVertical className="h-4 w-4" />
          <span className="sr-only">Actions</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem asChild>
          <Link href={`/auctions/${auction.id}`} className="flex items-center">
            <Eye className="mr-2 h-4 w-4" />
            View Details
          </Link>
        </DropdownMenuItem>

        <DropdownMenuItem asChild>
          <Link href={`/auctions/${auction.id}/edit`} className="flex items-center">
            <Edit className="mr-2 h-4 w-4" />
            Edit
          </Link>
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        {isInactive && (
          <ActivateAuctionDialog
            auctionId={auction.id}
            auctionTitle={auction.title}
            onSuccess={onActionComplete}
            trigger={
              <DropdownMenuItem
                onSelect={(e) => e.preventDefault()}
                className="text-green-600 focus:text-green-600"
              >
                <PlayCircle className="mr-2 h-4 w-4" />
                Activate
              </DropdownMenuItem>
            }
          />
        )}

        {canDeactivate && (
          <DeactivateAuctionDialog
            auctionId={auction.id}
            auctionTitle={auction.title}
            onSuccess={onActionComplete}
            trigger={
              <DropdownMenuItem
                onSelect={(e) => e.preventDefault()}
                className="text-orange-600 focus:text-orange-600"
              >
                <PauseCircle className="mr-2 h-4 w-4" />
                Deactivate
              </DropdownMenuItem>
            }
          />
        )}

        <DropdownMenuSeparator />

        <DeleteAuctionDialog
          auctionId={auction.id}
          auctionTitle={auction.title}
          onSuccess={onActionComplete}
          trigger={
            <DropdownMenuItem
              onSelect={(e) => e.preventDefault()}
              className="text-red-600 focus:text-red-600"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </DropdownMenuItem>
          }
        />
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
