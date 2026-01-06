"use client";

import Image from "next/image";
import { useState } from "react";
import type { AuctionDto } from "@repo/types";
import { Badge, Separator } from "@repo/ui";

interface AuctionDetailProps {
  auction: AuctionDto;
}

export function AuctionDetail({ auction }: AuctionDetailProps) {
  const [selectedImage, setSelectedImage] = useState(0);

  return (
    <div className="space-y-6">
      <div className="space-y-4">
        <div className="relative aspect-square overflow-hidden rounded-lg">
          <Image
            src={auction.imageUrls[selectedImage] || "/placeholder.jpg"}
            alt={auction.title}
            fill
            className="object-cover"
            priority
          />
        </div>
        {auction.imageUrls.length > 1 && (
          <div className="flex gap-2 overflow-x-auto pb-2">
            {auction.imageUrls.map((url, index) => (
              <button
                key={url}
                onClick={() => setSelectedImage(index)}
                className={`relative h-20 w-20 flex-shrink-0 overflow-hidden rounded-md border-2 ${
                  index === selectedImage ? "border-primary" : "border-transparent"
                }`}
              >
                <Image
                  src={url}
                  alt={`${auction.title} ${index + 1}`}
                  fill
                  className="object-cover"
                />
              </button>
            ))}
          </div>
        )}
      </div>

      <div>
        <div className="flex items-start justify-between gap-4">
          <h1 className="text-2xl font-bold">{auction.title}</h1>
          <Badge variant={auction.status === "Live" ? "default" : "secondary"}>
            {auction.status}
          </Badge>
        </div>
        <div className="mt-2 flex items-center gap-2 text-sm text-muted-foreground">
          {auction.category && <span>{auction.category.name}</span>}
          {auction.brand && (
            <>
              <span>•</span>
              <span>{auction.brand.name}</span>
            </>
          )}
        </div>
      </div>

      <Separator />

      <div>
        <h2 className="mb-2 font-semibold">Description</h2>
        <p className="whitespace-pre-wrap text-muted-foreground">
          {auction.description}
        </p>
      </div>

      {auction.item && (
        <>
          <Separator />
          <div>
            <h2 className="mb-2 font-semibold">Item Details</h2>
            <dl className="grid gap-2 text-sm sm:grid-cols-2">
              {auction.item.condition && (
                <div>
                  <dt className="text-muted-foreground">Condition</dt>
                  <dd className="font-medium">{auction.item.condition}</dd>
                </div>
              )}
              {auction.item.model && (
                <div>
                  <dt className="text-muted-foreground">Model</dt>
                  <dd className="font-medium">{auction.item.model}</dd>
                </div>
              )}
              {auction.item.year && (
                <div>
                  <dt className="text-muted-foreground">Year</dt>
                  <dd className="font-medium">{auction.item.year}</dd>
                </div>
              )}
              {auction.item.color && (
                <div>
                  <dt className="text-muted-foreground">Color</dt>
                  <dd className="font-medium">{auction.item.color}</dd>
                </div>
              )}
            </dl>
          </div>
        </>
      )}

      <Separator />

      <div>
        <h2 className="mb-2 font-semibold">Seller</h2>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-muted font-semibold">
            {auction.seller?.userName?.[0]?.toUpperCase() || "S"}
          </div>
          <div>
            <p className="font-medium">{auction.seller?.userName || "Seller"}</p>
            <p className="text-sm text-muted-foreground">
              Member since {new Date(auction.seller?.createdAt || Date.now()).getFullYear()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
