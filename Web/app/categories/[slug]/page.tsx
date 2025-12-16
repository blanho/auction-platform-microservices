"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import {
  Car,
  Watch,
  Gem,
  Paintbrush,
  Home,
  Smartphone,
  ShoppingBag,
  Sparkles,
  Loader2,
  Filter,
  Grid3X3,
  List,
  SlidersHorizontal,
} from "lucide-react";

import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { Slider } from "@/components/ui/slider";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Separator } from "@/components/ui/separator";
import { AuctionCard } from "@/features/auction/auction-card";
import { Pagination } from "@/components/common/pagination";
import { Auction, AuctionStatus, Category } from "@/types/auction";
import { auctionService, AuctionPagedResult } from "@/services/auction.service";
import { ROUTES } from "@/constants/routes";

const iconMap: Record<string, React.ElementType> = {
  car: Car,
  watch: Watch,
  gem: Gem,
  art: Paintbrush,
  home: Home,
  electronics: Smartphone,
  fashion: ShoppingBag,
  default: Sparkles,
};

function getIcon(iconName: string | undefined): React.ElementType {
  const key = iconName?.toLowerCase() || "default";
  return iconMap[key] || iconMap.default;
}

const SORT_OPTIONS = [
  { value: "createdAt-desc", label: "Newest First" },
  { value: "createdAt-asc", label: "Oldest First" },
  { value: "currentHighBid-desc", label: "Highest Bid" },
  { value: "currentHighBid-asc", label: "Lowest Bid" },
  { value: "auctionEnd-asc", label: "Ending Soon" },
  { value: "auctionEnd-desc", label: "Ending Later" },
];

const STATUS_OPTIONS = [
  { value: "all", label: "All Statuses" },
  { value: AuctionStatus.Live, label: "Live" },
  { value: AuctionStatus.Finished, label: "Finished" },
];

export default function CategoryDetailPage() {
  const params = useParams();
  const slug = params?.slug as string;

  const [category, setCategory] = useState<Category | null>(null);
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [viewMode, setViewMode] = useState<"grid" | "list">("grid");

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(12);
  const [sortBy, setSortBy] = useState("createdAt-desc");
  const [statusFilter, setStatusFilter] = useState("all");
  const [priceRange, setPriceRange] = useState<[number, number]>([0, 100000]);

  useEffect(() => {
    const fetchCategory = async () => {
      try {
        const categories = await auctionService.getCategories();
        const found = categories.find((c) => c.slug === slug);
        setCategory(found || null);
      } catch {
      }
    };
    fetchCategory();
  }, [slug]);

  useEffect(() => {
    const fetchAuctions = async () => {
      if (!slug) return;

      setIsLoading(true);
      try {
        const [orderBy, descending] = sortBy.split("-");
        const result: AuctionPagedResult = await auctionService.getAuctions({
          category: slug,
          status: statusFilter === "all" ? undefined : statusFilter,
          pageNumber,
          pageSize,
          orderBy,
          descending: descending === "desc",
        });

        setAuctions(result.items);
        setTotalCount(result.totalCount);
        setTotalPages(result.totalPages);
      } catch {
      } finally {
        setIsLoading(false);
      }
    };
    fetchAuctions();
  }, [slug, pageNumber, pageSize, sortBy, statusFilter]);

  const Icon = category ? getIcon(category.icon) : Sparkles;

  return (
    <MainLayout>
      <div className="space-y-6">
        <Breadcrumb>
          <BreadcrumbList>
            <BreadcrumbItem>
              <BreadcrumbLink href={ROUTES.HOME}>Home</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbLink href="/categories">Categories</BreadcrumbLink>
            </BreadcrumbItem>
            <BreadcrumbSeparator />
            <BreadcrumbItem>
              <BreadcrumbPage>{category?.name || slug}</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <div className="flex flex-col md:flex-row md:items-center gap-4 p-6 bg-zinc-50 dark:bg-zinc-900 rounded-xl">
          <div className="w-16 h-16 rounded-xl bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
            <Icon className="w-8 h-8 text-amber-600" />
          </div>
          <div className="flex-1">
            <h1 className="text-3xl font-bold text-zinc-900 dark:text-white">
              {category?.name || slug}
            </h1>
            {category?.description && (
              <p className="text-zinc-600 dark:text-zinc-400 mt-1">
                {category.description}
              </p>
            )}
            <div className="flex items-center gap-2 mt-2">
              <Badge variant="secondary">
                {totalCount.toLocaleString()} auctions
              </Badge>
            </div>
          </div>
        </div>

        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-2">
            <Sheet>
              <SheetTrigger asChild>
                <Button variant="outline" size="sm" className="sm:hidden">
                  <SlidersHorizontal className="h-4 w-4 mr-2" />
                  Filters
                </Button>
              </SheetTrigger>
              <SheetContent side="left">
                <SheetHeader>
                  <SheetTitle>Filters</SheetTitle>
                  <SheetDescription>
                    Narrow down your search results
                  </SheetDescription>
                </SheetHeader>
                <div className="space-y-6 mt-6">
                  <div className="space-y-3">
                    <Label>Status</Label>
                    {STATUS_OPTIONS.map((option) => (
                      <div
                        key={option.value}
                        className="flex items-center space-x-2"
                      >
                        <Checkbox
                          id={`mobile-status-${option.value}`}
                          checked={statusFilter === option.value}
                          onCheckedChange={() => setStatusFilter(option.value)}
                        />
                        <label
                          htmlFor={`mobile-status-${option.value}`}
                          className="text-sm"
                        >
                          {option.label}
                        </label>
                      </div>
                    ))}
                  </div>
                  <Separator />
                  <div className="space-y-3">
                    <Label>Price Range</Label>
                    <Slider
                      min={0}
                      max={100000}
                      step={1000}
                      value={priceRange}
                      onValueChange={(value) =>
                        setPriceRange(value as [number, number])
                      }
                    />
                    <div className="flex justify-between text-sm text-zinc-500">
                      <span>${priceRange[0].toLocaleString()}</span>
                      <span>${priceRange[1].toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              </SheetContent>
            </Sheet>

            <div className="hidden sm:flex items-center gap-2">
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-[150px]">
                  <Filter className="h-4 w-4 mr-2" />
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  {STATUS_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <Select value={sortBy} onValueChange={setSortBy}>
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Sort by" />
              </SelectTrigger>
              <SelectContent>
                {SORT_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <div className="hidden sm:flex border rounded-lg">
              <Button
                variant={viewMode === "grid" ? "secondary" : "ghost"}
                size="icon"
                onClick={() => setViewMode("grid")}
              >
                <Grid3X3 className="h-4 w-4" />
              </Button>
              <Button
                variant={viewMode === "list" ? "secondary" : "ghost"}
                size="icon"
                onClick={() => setViewMode("list")}
              >
                <List className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
          </div>
        ) : auctions.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <Icon className="h-16 w-16 text-zinc-300 dark:text-zinc-700 mb-4" />
            <h3 className="text-lg font-semibold text-zinc-900 dark:text-white">
              No auctions found
            </h3>
            <p className="text-zinc-600 dark:text-zinc-400 mt-1 max-w-md">
              There are no auctions in this category yet. Check back later or
              browse other categories.
            </p>
            <Button asChild className="mt-4">
              <Link href="/categories">Browse Categories</Link>
            </Button>
          </div>
        ) : (
          <>
            <div
              className={
                viewMode === "grid"
                  ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
                  : "space-y-4"
              }
            >
              {auctions.map((auction) => (
                <AuctionCard
                  key={auction.id}
                  auction={auction}
                />
              ))}
            </div>

            {totalPages > 1 && (
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            )}
          </>
        )}
      </div>
    </MainLayout>
  );
}
