"use client";

import { useState, useEffect } from "react";
import { Loader2, Bell, BellOff, Check } from "lucide-react";
import { toast } from "sonner";

import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";

interface NotificationCategory {
  id: string;
  name: string;
  description: string;
  types: NotificationType[];
}

interface NotificationType {
  id: string;
  label: string;
  description: string;
  email: boolean;
  push: boolean;
  inApp: boolean;
}

const DEFAULT_NOTIFICATION_CATEGORIES: NotificationCategory[] = [
  {
    id: "bidding",
    name: "Bidding Activity",
    description: "Notifications related to your bids",
    types: [
      {
        id: "outbid",
        label: "Outbid Alerts",
        description: "When someone places a higher bid than yours",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "bid_accepted",
        label: "Bid Accepted",
        description: "When your bid is accepted",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "auction_won",
        label: "Auction Won",
        description: "When you win an auction",
        email: true,
        push: true,
        inApp: true,
      },
    ],
  },
  {
    id: "auctions",
    name: "Auction Updates",
    description: "Notifications about auctions you're interested in",
    types: [
      {
        id: "ending_soon",
        label: "Ending Soon",
        description: "When watched auctions are about to end",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "price_drop",
        label: "Price Alerts",
        description: "When items in your wishlist have new activity",
        email: true,
        push: false,
        inApp: true,
      },
      {
        id: "new_in_category",
        label: "New Listings",
        description: "When new items are listed in your favorite categories",
        email: false,
        push: false,
        inApp: true,
      },
    ],
  },
  {
    id: "selling",
    name: "Selling Activity",
    description: "Notifications for sellers",
    types: [
      {
        id: "new_bid",
        label: "New Bids",
        description: "When someone bids on your auctions",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "auction_sold",
        label: "Item Sold",
        description: "When your auction ends with a winning bid",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "question_asked",
        label: "Questions",
        description: "When buyers ask questions about your items",
        email: true,
        push: true,
        inApp: true,
      },
    ],
  },
  {
    id: "orders",
    name: "Orders & Shipping",
    description: "Updates about your purchases and sales",
    types: [
      {
        id: "order_created",
        label: "Order Confirmation",
        description: "When an order is created",
        email: true,
        push: false,
        inApp: true,
      },
      {
        id: "order_shipped",
        label: "Shipping Updates",
        description: "When items are shipped or delivered",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "review_received",
        label: "Reviews",
        description: "When you receive a review",
        email: true,
        push: false,
        inApp: true,
      },
    ],
  },
  {
    id: "account",
    name: "Account & Security",
    description: "Important account notifications",
    types: [
      {
        id: "security_alerts",
        label: "Security Alerts",
        description: "Login attempts and password changes",
        email: true,
        push: true,
        inApp: true,
      },
      {
        id: "account_updates",
        label: "Account Updates",
        description: "Changes to your account settings",
        email: true,
        push: false,
        inApp: true,
      },
    ],
  },
  {
    id: "marketing",
    name: "Marketing & Promotions",
    description: "Stay updated with deals and offers",
    types: [
      {
        id: "promotions",
        label: "Promotions",
        description: "Special offers and discounts",
        email: false,
        push: false,
        inApp: true,
      },
      {
        id: "newsletter",
        label: "Newsletter",
        description: "Weekly digest and platform updates",
        email: false,
        push: false,
        inApp: false,
      },
    ],
  },
];

export function NotificationPreferences() {
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [categories, setCategories] = useState<NotificationCategory[]>(
    DEFAULT_NOTIFICATION_CATEGORIES
  );
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    setIsLoading(false);
  }, []);

  const togglePreference = (
    categoryId: string,
    typeId: string,
    channel: "email" | "push" | "inApp"
  ) => {
    setCategories((prev) =>
      prev.map((category) => {
        if (category.id !== categoryId) return category;
        return {
          ...category,
          types: category.types.map((type) => {
            if (type.id !== typeId) return type;
            return { ...type, [channel]: !type[channel] };
          }),
        };
      })
    );
    setHasChanges(true);
  };

  const handleToggleAllInCategory = (
    categoryId: string,
    channel: "email" | "push" | "inApp",
    value: boolean
  ) => {
    setCategories((prev) =>
      prev.map((category) => {
        if (category.id !== categoryId) return category;
        return {
          ...category,
          types: category.types.map((type) => ({ ...type, [channel]: value })),
        };
      })
    );
    setHasChanges(true);
  };

  const handleSave = async () => {
    setIsSaving(true);
    try {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      toast.success("Notification preferences saved");
      setHasChanges(false);
    } catch {
      toast.error("Failed to save preferences");
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold text-zinc-900 dark:text-white">
            Notification Preferences
          </h2>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            Manage how and when you receive notifications
          </p>
        </div>
        {hasChanges && (
          <Button
            onClick={handleSave}
            disabled={isSaving}
            className="bg-amber-500 hover:bg-amber-600"
          >
            {isSaving ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin mr-2" />
                Saving...
              </>
            ) : (
              <>
                <Check className="h-4 w-4 mr-2" />
                Save Changes
              </>
            )}
          </Button>
        )}
      </div>

      <div className="grid gap-4 text-sm">
        <div className="flex items-center gap-4 justify-end px-4">
          <span className="w-16 text-center text-zinc-500">Email</span>
          <span className="w-16 text-center text-zinc-500">Push</span>
          <span className="w-16 text-center text-zinc-500">In-App</span>
        </div>
      </div>

      {categories.map((category) => (
        <Card key={category.id}>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
                  <Bell className="h-5 w-5 text-amber-600" />
                </div>
                <div>
                  <CardTitle className="text-lg">{category.name}</CardTitle>
                  <CardDescription>{category.description}</CardDescription>
                </div>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {category.types.map((type, index) => (
              <div key={type.id}>
                {index > 0 && <Separator className="mb-4" />}
                <div className="flex items-center justify-between">
                  <div className="flex-1 pr-4">
                    <Label className="text-base font-medium">{type.label}</Label>
                    <p className="text-sm text-zinc-500 dark:text-zinc-400">
                      {type.description}
                    </p>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="w-16 flex justify-center">
                      <Switch
                        checked={type.email}
                        onCheckedChange={() =>
                          togglePreference(category.id, type.id, "email")
                        }
                        aria-label={`${type.label} email notifications`}
                      />
                    </div>
                    <div className="w-16 flex justify-center">
                      <Switch
                        checked={type.push}
                        onCheckedChange={() =>
                          togglePreference(category.id, type.id, "push")
                        }
                        aria-label={`${type.label} push notifications`}
                      />
                    </div>
                    <div className="w-16 flex justify-center">
                      <Switch
                        checked={type.inApp}
                        onCheckedChange={() =>
                          togglePreference(category.id, type.id, "inApp")
                        }
                        aria-label={`${type.label} in-app notifications`}
                      />
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ))}

      <Card className="border-dashed">
        <CardContent className="flex items-center justify-between py-4">
          <div className="flex items-center gap-3">
            <BellOff className="h-5 w-5 text-zinc-400" />
            <div>
              <p className="font-medium text-zinc-900 dark:text-white">
                Disable All Notifications
              </p>
              <p className="text-sm text-zinc-500">
                Turn off all non-essential notifications
              </p>
            </div>
          </div>
          <Button
            variant="outline"
            onClick={() => {
              setCategories((prev) =>
                prev.map((category) => ({
                  ...category,
                  types: category.types.map((type) => ({
                    ...type,
                    email: category.id === "account",
                    push: false,
                    inApp: category.id === "account",
                  })),
                }))
              );
              setHasChanges(true);
            }}
          >
            Disable All
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
