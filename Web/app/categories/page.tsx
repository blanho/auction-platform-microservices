"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import {
  Car,
  Watch,
  Gem,
  Paintbrush,
  Home,
  Smartphone,
  ShoppingBag,
  Sparkles,
  ChevronRight,
  Search,
  Loader2,
} from "lucide-react";

import { MainLayout } from "@/components/layout/main-layout";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Category } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { useDebounce } from "@/hooks/use-debounce";
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

const colorMap: Record<string, { bg: string; text: string; border: string }> = {
  car: {
    bg: "bg-blue-50 dark:bg-blue-950",
    text: "text-blue-600 dark:text-blue-400",
    border: "border-blue-200 dark:border-blue-800",
  },
  watch: {
    bg: "bg-amber-50 dark:bg-amber-950",
    text: "text-amber-600 dark:text-amber-400",
    border: "border-amber-200 dark:border-amber-800",
  },
  gem: {
    bg: "bg-purple-50 dark:bg-purple-950",
    text: "text-purple-600 dark:text-purple-400",
    border: "border-purple-200 dark:border-purple-800",
  },
  art: {
    bg: "bg-pink-50 dark:bg-pink-950",
    text: "text-pink-600 dark:text-pink-400",
    border: "border-pink-200 dark:border-pink-800",
  },
  home: {
    bg: "bg-green-50 dark:bg-green-950",
    text: "text-green-600 dark:text-green-400",
    border: "border-green-200 dark:border-green-800",
  },
  electronics: {
    bg: "bg-cyan-50 dark:bg-cyan-950",
    text: "text-cyan-600 dark:text-cyan-400",
    border: "border-cyan-200 dark:border-cyan-800",
  },
  fashion: {
    bg: "bg-rose-50 dark:bg-rose-950",
    text: "text-rose-600 dark:text-rose-400",
    border: "border-rose-200 dark:border-rose-800",
  },
  default: {
    bg: "bg-indigo-50 dark:bg-indigo-950",
    text: "text-indigo-600 dark:text-indigo-400",
    border: "border-indigo-200 dark:border-indigo-800",
  },
};

function getIcon(iconName: string | undefined): React.ElementType {
  const key = iconName?.toLowerCase() || "default";
  return iconMap[key] || iconMap.default;
}

function getColors(iconName: string | undefined) {
  const key = iconName?.toLowerCase() || "default";
  return colorMap[key] || colorMap.default;
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const debouncedSearch = useDebounce(searchTerm, 300);

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const data = await auctionService.getCategories();
        setCategories(data);
      } catch (error) {
        console.error("Failed to fetch categories:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchCategories();
  }, []);

  const filteredCategories = categories.filter((category) =>
    category.name.toLowerCase().includes(debouncedSearch.toLowerCase())
  );

  const totalAuctions = categories.reduce(
    (sum, cat) => sum + cat.auctionCount,
    0
  );

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
              <BreadcrumbPage>Categories</BreadcrumbPage>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>

        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-zinc-900 dark:text-white">
              Browse Categories
            </h1>
            <p className="text-zinc-600 dark:text-zinc-400 mt-1">
              Explore {totalAuctions.toLocaleString()} auctions across{" "}
              {categories.length} categories
            </p>
          </div>
          <div className="relative w-full md:w-80">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
            <Input
              placeholder="Search categories..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
          </div>
        ) : filteredCategories.length === 0 ? (
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <Search className="h-12 w-12 text-zinc-400 mb-4" />
              <h3 className="text-lg font-semibold text-zinc-900 dark:text-white">
                No categories found
              </h3>
              <p className="text-zinc-600 dark:text-zinc-400 mt-1">
                Try adjusting your search term
              </p>
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {filteredCategories.map((category, index) => {
              const Icon = getIcon(category.icon);
              const colors = getColors(category.icon);

              return (
                <motion.div
                  key={category.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                >
                  <Link href={`/categories/${category.slug}`}>
                    <Card
                      className={`group cursor-pointer transition-all duration-300 hover:shadow-lg hover:shadow-amber-500/10 hover:border-amber-400 ${colors.border}`}
                    >
                      <CardHeader className="pb-3">
                        <div className="flex items-start justify-between">
                          <div
                            className={`w-14 h-14 rounded-xl ${colors.bg} flex items-center justify-center transition-transform group-hover:scale-110`}
                          >
                            <Icon className={`w-7 h-7 ${colors.text}`} />
                          </div>
                          <ChevronRight className="h-5 w-5 text-zinc-400 group-hover:text-amber-500 transition-colors" />
                        </div>
                        <CardTitle className="text-lg mt-3">
                          {category.name}
                        </CardTitle>
                        {category.description && (
                          <CardDescription className="line-clamp-2">
                            {category.description}
                          </CardDescription>
                        )}
                      </CardHeader>
                      <CardContent className="pt-0">
                        <div className="flex items-center justify-between">
                          <Badge variant="secondary">
                            {category.auctionCount.toLocaleString()} auctions
                          </Badge>
                          {category.auctionCount > 50 && (
                            <Badge
                              variant="outline"
                              className="text-amber-600 border-amber-300"
                            >
                              Popular
                            </Badge>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                </motion.div>
              );
            })}
          </div>
        )}
      </div>
    </MainLayout>
  );
}
