"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle, Label, Input, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, Button } from "@repo/ui";
import { useCategories } from "@repo/hooks";

export function AuctionFilters() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { data: categories } = useCategories();

  function updateFilters(key: string, value: string) {
    const params = new URLSearchParams(searchParams.toString());
    if (value) {
      params.set(key, value);
    } else {
      params.delete(key);
    }
    params.delete("page");
    router.push(`/auctions?${params.toString()}`);
  }

  function clearFilters() {
    router.push("/auctions");
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Filters</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <Label>Search</Label>
          <Input
            placeholder="Search auctions..."
            defaultValue={searchParams.get("search") || ""}
            onChange={(e) => {
              const value = e.target.value;
              const timeout = setTimeout(() => updateFilters("search", value), 300);
              return () => clearTimeout(timeout);
            }}
          />
        </div>

        <div className="space-y-2">
          <Label>Category</Label>
          <Select
            value={searchParams.get("category") || ""}
            onValueChange={(value) => updateFilters("category", value)}
          >
            <SelectTrigger>
              <SelectValue placeholder="All categories" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">All categories</SelectItem>
              {categories?.map((category) => (
                <SelectItem key={category.id} value={category.id}>
                  {category.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label>Sort By</Label>
          <Select
            value={searchParams.get("sortBy") || ""}
            onValueChange={(value) => updateFilters("sortBy", value)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Default" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="">Default</SelectItem>
              <SelectItem value="endingSoon">Ending Soon</SelectItem>
              <SelectItem value="newlyListed">Newly Listed</SelectItem>
              <SelectItem value="priceAsc">Price: Low to High</SelectItem>
              <SelectItem value="priceDesc">Price: High to Low</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="grid grid-cols-2 gap-2">
          <div className="space-y-2">
            <Label>Min Price</Label>
            <Input
              type="number"
              placeholder="$0"
              defaultValue={searchParams.get("minPrice") || ""}
              onChange={(e) => updateFilters("minPrice", e.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label>Max Price</Label>
            <Input
              type="number"
              placeholder="$∞"
              defaultValue={searchParams.get("maxPrice") || ""}
              onChange={(e) => updateFilters("maxPrice", e.target.value)}
            />
          </div>
        </div>

        <Button variant="outline" className="w-full" onClick={clearFilters}>
          Clear Filters
        </Button>
      </CardContent>
    </Card>
  );
}
