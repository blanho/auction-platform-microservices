"use client";

import { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";
import Image from "next/image";
import Link from "next/link";
import { ExternalLink } from "lucide-react";

import { Auction, AuctionStatus } from "@/types/auction";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { DataTableColumnHeader } from "@/components/ui/data-table-column-header";
import { AuctionActions } from "./auction-actions";

function getStatusBadgeVariant(status: AuctionStatus) {
  switch (status) {
    case AuctionStatus.Live:
      return "default";
    case AuctionStatus.Finished:
      return "secondary";
    case AuctionStatus.ReserveNotMet:
      return "destructive";
    case AuctionStatus.Cancelled:
      return "outline";
    case AuctionStatus.Inactive:
      return "secondary";
    default:
      return "outline";
  }
}

function formatCurrency(value: number | undefined) {
  if (value === undefined || value === null) return "-";
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

export const createAuctionColumns = (
  onActionComplete?: () => void
): ColumnDef<Auction>[] => [
  {
    id: "select",
    header: ({ table }) => (
      <Checkbox
        checked={
          table.getIsAllPageRowsSelected() ||
          (table.getIsSomePageRowsSelected() && "indeterminate")
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label="Select all"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label="Select row"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "imageUrl",
    header: "Image",
    cell: ({ row }) => {
      const imageUrl = row.getValue("imageUrl") as string | undefined;
      return (
        <div className="relative h-12 w-16 overflow-hidden rounded-md bg-muted">
          {imageUrl ? (
            <Image
              src={imageUrl}
              alt={row.getValue("title") as string}
              fill
              className="object-cover"
              sizes="64px"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-xs text-muted-foreground">
              No image
            </div>
          )}
        </div>
      );
    },
    enableSorting: false,
  },
  {
    accessorKey: "title",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Title" />
    ),
    cell: ({ row }) => {
      return (
        <div className="flex flex-col">
          <Link
            href={`/auctions/${row.original.id}`}
            className="font-medium hover:underline max-w-[200px] truncate"
          >
            {row.getValue("title")}
          </Link>
          <span className="text-xs text-muted-foreground">
            {row.original.make} {row.original.model} ({row.original.year})
          </span>
        </div>
      );
    },
  },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" />
    ),
    cell: ({ row }) => {
      const status = row.getValue("status") as AuctionStatus;
      return (
        <Badge variant={getStatusBadgeVariant(status)}>
          {status}
        </Badge>
      );
    },
    filterFn: (row, id, value) => {
      return value.includes(row.getValue(id));
    },
  },
  {
    accessorKey: "reservePrice",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Reserve Price" />
    ),
    cell: ({ row }) => formatCurrency(row.getValue("reservePrice")),
  },
  {
    accessorKey: "currentHighBid",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Current Bid" />
    ),
    cell: ({ row }) => {
      const currentBid = row.getValue("currentHighBid") as number | undefined;
      const reservePrice = row.original.reservePrice;
      const isAboveReserve = currentBid && currentBid >= reservePrice;
      
      return (
        <span className={isAboveReserve ? "text-green-600 font-medium" : ""}>
          {formatCurrency(currentBid)}
        </span>
      );
    },
  },
  {
    accessorKey: "auctionEnd",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="End Date" />
    ),
    cell: ({ row }) => {
      const date = new Date(row.getValue("auctionEnd"));
      const isPast = date < new Date();
      return (
        <span className={isPast ? "text-muted-foreground" : ""}>
          {format(date, "MMM d, yyyy HH:mm")}
        </span>
      );
    },
  },
  {
    accessorKey: "mileage",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Mileage" />
    ),
    cell: ({ row }) => {
      const mileage = row.getValue("mileage") as number;
      return mileage.toLocaleString() + " mi";
    },
  },
  {
    accessorKey: "color",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Color" />
    ),
    cell: ({ row }) => (
      <span className="capitalize">{row.getValue("color")}</span>
    ),
  },
  {
    accessorKey: "createdAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Created" />
    ),
    cell: ({ row }) => {
      return format(new Date(row.getValue("createdAt")), "MMM d, yyyy");
    },
  },
  {
    id: "actions",
    header: "Actions",
    cell: ({ row }) => {
      return (
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon" asChild className="h-8 w-8">
            <Link href={`/auctions/${row.original.id}`}>
              <ExternalLink className="h-4 w-4" />
              <span className="sr-only">View</span>
            </Link>
          </Button>
          <AuctionActions
            auction={row.original}
            onActionComplete={onActionComplete}
          />
        </div>
      );
    },
    enableSorting: false,
    enableHiding: false,
  },
];
