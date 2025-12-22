"use client";

import Image from "next/image";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCheckCircle,
  faInfoCircle,
  faDollarSign,
  faTruck,
  faCamera,
} from "@fortawesome/free-solid-svg-icons";

import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { formatCurrency } from "@/utils";
import { SHIPPING_TYPE_LABELS } from "@/types/auction";
import { cn } from "@/lib/utils";

import { StepProps } from "./types";

interface ReviewSectionProps {
  icon: typeof faInfoCircle;
  iconColor: string;
  title: string;
  children: React.ReactNode;
}

function ReviewSection({ icon, iconColor, title, children }: ReviewSectionProps) {
  return (
    <Card>
      <CardContent className="pt-6">
        <div className="mb-4 flex items-center gap-2">
          <FontAwesomeIcon icon={icon} className={cn("h-5 w-5", iconColor)} />
          <h3 className="font-semibold text-slate-900 dark:text-white">{title}</h3>
        </div>
        {children}
      </CardContent>
    </Card>
  );
}

interface ReviewItemProps {
  label: string;
  value: React.ReactNode;
}

function ReviewItem({ label, value }: ReviewItemProps) {
  if (!value) return null;

  return (
    <div className="flex justify-between py-2 border-b border-slate-100 dark:border-slate-800 last:border-0">
      <span className="text-slate-600 dark:text-slate-400">{label}</span>
      <span className="font-medium text-slate-900 dark:text-white">{value}</span>
    </div>
  );
}

export function ReviewStep({ form, categories, uploadedImages, mode }: StepProps) {
  const values = form.getValues();
  const category = categories.find((c) => c.id === values.categoryId);

  const formatDate = (dateString: string) => {
    if (!dateString) return "Not set";
    return new Date(dateString).toLocaleString();
  };

  const successfulImages = uploadedImages.filter(img => img.status === "success");

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
        <FontAwesomeIcon icon={faCheckCircle} className="h-5 w-5 text-green-500" />
        Review Your {mode === "edit" ? "Changes" : "Listing"}
      </div>

      <p className="text-slate-600 dark:text-slate-400">
        Please review all details before {mode === "edit" ? "saving changes" : "creating your auction"}.
      </p>

      <div className="grid gap-6 lg:grid-cols-2">
        <ReviewSection icon={faInfoCircle} iconColor="text-purple-500" title="Basic Information">
          <div className="space-y-1">
            <ReviewItem label="Title" value={values.title} />
            <ReviewItem label="Category" value={category?.name || "Not selected"} />
            <ReviewItem label="Condition" value={values.condition} />
            <ReviewItem
              label="Year"
              value={values.yearManufactured?.toString()}
            />
            <ReviewItem
              label="Featured"
              value={
                <Badge variant={values.isFeatured ? "default" : "secondary"}>
                  {values.isFeatured ? "Yes" : "No"}
                </Badge>
              }
            />
          </div>
          {values.description && (
            <div className="mt-4 pt-4 border-t border-slate-100 dark:border-slate-800">
              <p className="text-sm text-slate-600 dark:text-slate-400 mb-2">Description</p>
              <p className="text-slate-900 dark:text-white whitespace-pre-wrap line-clamp-4">
                {values.description}
              </p>
            </div>
          )}
        </ReviewSection>

        <ReviewSection icon={faDollarSign} iconColor="text-green-500" title="Pricing & Duration">
          <div className="space-y-1">
            <ReviewItem
              label="Starting Price"
              value={formatCurrency(values.reservePrice)}
            />
            <ReviewItem
              label="Buy Now Price"
              value={values.buyNowPrice ? formatCurrency(values.buyNowPrice) : "Not set"}
            />
            <ReviewItem label="Auction Ends" value={formatDate(values.auctionEnd)} />
          </div>
        </ReviewSection>

        <ReviewSection icon={faTruck} iconColor="text-orange-500" title="Shipping">
          <div className="space-y-1">
            <ReviewItem
              label="Shipping Type"
              value={SHIPPING_TYPE_LABELS[values.shippingType]}
            />
            {values.shippingCost !== undefined && values.shippingCost > 0 && (
              <ReviewItem
                label="Shipping Cost"
                value={formatCurrency(values.shippingCost)}
              />
            )}
            <ReviewItem
              label="Handling Time"
              value={`${values.handlingTime} day(s)`}
            />
            {values.shipsFrom && (
              <ReviewItem label="Ships From" value={values.shipsFrom} />
            )}
            <ReviewItem
              label="Local Pickup"
              value={
                <Badge variant={values.localPickupAvailable ? "default" : "secondary"}>
                  {values.localPickupAvailable ? "Available" : "Not available"}
                </Badge>
              }
            />
            {values.localPickupAvailable && values.localPickupAddress && (
              <ReviewItem
                label="Pickup Location"
                value={values.localPickupAddress}
              />
            )}
          </div>
        </ReviewSection>

        <ReviewSection icon={faCamera} iconColor="text-blue-500" title="Photos">
          {successfulImages.length > 0 ? (
            <div className="grid grid-cols-4 gap-2">
              {successfulImages.slice(0, 4).map((image, index) => (
                <div
                  key={image.fileId || index}
                  className={cn(
                    "relative aspect-square overflow-hidden rounded-lg border-2",
                    image.isPrimary
                      ? "border-purple-500"
                      : "border-slate-200 dark:border-slate-700"
                  )}
                >
                  <Image
                    src={image.url}
                    alt={`Image ${index + 1}`}
                    fill
                    className="object-cover"
                  />
                  {image.isPrimary && (
                    <div className="absolute left-1 top-1 rounded bg-purple-500 px-1 py-0.5 text-[10px] font-medium text-white">
                      Cover
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <p className="text-slate-500 dark:text-slate-400 text-sm">
              No images uploaded yet
            </p>
          )}
          <p className="mt-2 text-sm text-slate-500">
            {successfulImages.length} image(s) ready
          </p>
        </ReviewSection>
      </div>
    </div>
  );
}
