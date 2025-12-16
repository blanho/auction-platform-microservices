"use client";

import { useState, useCallback } from "react";
import { useSession } from "next-auth/react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faHeart, faSpinner } from "@fortawesome/free-solid-svg-icons";
import { motion, AnimatePresence } from "framer-motion";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { bookmarkService } from "@/services/bookmark.service";

interface WishlistButtonProps {
  auctionId: string;
  initialIsWishlisted?: boolean;
  variant?: "icon" | "button" | "compact";
  size?: "sm" | "md" | "lg";
  className?: string;
  onToggle?: (isWishlisted: boolean) => void;
}

export function WishlistButton({
  auctionId,
  initialIsWishlisted = false,
  variant = "icon",
  size = "md",
  className,
  onToggle,
}: WishlistButtonProps) {
  const { data: session } = useSession();
  const [isWishlisted, setIsWishlisted] = useState(initialIsWishlisted);
  const [isLoading, setIsLoading] = useState(false);

  const sizeClasses = {
    sm: "w-3 h-3",
    md: "w-4 h-4",
    lg: "w-5 h-5",
  };

  const handleToggle = useCallback(async () => {
    if (!session) {
      toast.error("Please sign in to add to wishlist");
      return;
    }

    setIsLoading(true);
    try {
      const result = await bookmarkService.toggleWishlist(auctionId);
      setIsWishlisted(result.isBookmarked);
      toast.success(result.message);
      onToggle?.(result.isBookmarked);
    } catch {
      toast.error("Failed to update wishlist");
    } finally {
      setIsLoading(false);
    }
  }, [auctionId, session, onToggle]);

  if (variant === "icon") {
    return (
      <button
        onClick={handleToggle}
        disabled={isLoading}
        className={cn(
          "relative p-2 rounded-full transition-all duration-200",
          "hover:bg-pink-100 dark:hover:bg-pink-900/30",
          "focus:outline-none focus:ring-2 focus:ring-pink-500 focus:ring-offset-2",
          isWishlisted && "text-pink-500",
          !isWishlisted && "text-slate-400 hover:text-pink-500",
          className
        )}
        aria-label={isWishlisted ? "Remove from wishlist" : "Add to wishlist"}
      >
        <AnimatePresence mode="wait">
          {isLoading ? (
            <motion.div
              key="loading"
              initial={{ opacity: 0, scale: 0.5 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.5 }}
            >
              <FontAwesomeIcon icon={faSpinner} className={cn(sizeClasses[size], "animate-spin")} />
            </motion.div>
          ) : (
            <motion.div
              key={isWishlisted ? "filled" : "outline"}
              initial={{ scale: 0.5 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.5 }}
              transition={{ type: "spring", stiffness: 500, damping: 30 }}
            >
              <FontAwesomeIcon
                icon={faHeart}
                className={cn(
                  sizeClasses[size],
                  isWishlisted ? "text-pink-500" : "text-slate-300 dark:text-slate-600"
                )}
              />
            </motion.div>
          )}
        </AnimatePresence>
        {isWishlisted && (
          <motion.div
            initial={{ scale: 0 }}
            animate={{ scale: [0, 1.5, 0] }}
            transition={{ duration: 0.5 }}
            className="absolute inset-0 rounded-full bg-pink-500/20"
          />
        )}
      </button>
    );
  }

  if (variant === "compact") {
    return (
      <Button
        variant="ghost"
        size="sm"
        onClick={handleToggle}
        disabled={isLoading}
        className={cn(
          isWishlisted ? "text-pink-500 hover:text-pink-600" : "text-slate-500 hover:text-pink-500",
          className
        )}
      >
        {isLoading ? (
          <FontAwesomeIcon icon={faSpinner} className="w-4 h-4 animate-spin" />
        ) : (
          <FontAwesomeIcon 
            icon={faHeart} 
            className={cn("w-4 h-4", isWishlisted ? "text-pink-500" : "text-slate-400")} 
          />
        )}
      </Button>
    );
  }

  return (
    <Button
      variant={isWishlisted ? "default" : "outline"}
      onClick={handleToggle}
      disabled={isLoading}
      className={cn(
        isWishlisted && "bg-pink-500 hover:bg-pink-600 border-pink-500",
        !isWishlisted && "border-pink-300 text-pink-500 hover:bg-pink-50 dark:hover:bg-pink-950",
        className
      )}
    >
      {isLoading ? (
        <FontAwesomeIcon icon={faSpinner} className="w-4 h-4 mr-2 animate-spin" />
      ) : (
        <FontAwesomeIcon icon={faHeart} className="w-4 h-4 mr-2" />
      )}
      {isWishlisted ? "In Wishlist" : "Add to Wishlist"}
    </Button>
  );
}
