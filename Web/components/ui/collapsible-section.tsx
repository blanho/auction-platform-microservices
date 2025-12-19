"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChevronDown, faChevronUp } from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface CollapsibleSectionProps {
  children: React.ReactNode;
  previewHeight?: number;
  showMoreLabel?: string;
  showLessLabel?: string;
  defaultExpanded?: boolean;
  className?: string;
}

export function CollapsibleSection({
  children,
  previewHeight = 200,
  showMoreLabel = "Show More",
  showLessLabel = "Show Less",
  defaultExpanded = false,
  className,
}: CollapsibleSectionProps) {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  return (
    <div className={cn("relative", className)}>
      <AnimatePresence mode="wait">
        {isExpanded ? (
          <motion.div
            key="expanded"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            {children}
          </motion.div>
        ) : (
          <motion.div
            key="collapsed"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="relative overflow-hidden"
            style={{ maxHeight: previewHeight }}
          >
            {children}
            <div className="absolute inset-x-0 bottom-0 h-32 bg-linear-to-t from-white via-white/90 to-transparent dark:from-slate-950 dark:via-slate-950/90" />
          </motion.div>
        )}
      </AnimatePresence>

      <div className={cn(
        "flex justify-center",
        isExpanded ? "mt-6" : "relative -mt-4 z-10"
      )}>
        <Button
          variant="outline"
          onClick={() => setIsExpanded(!isExpanded)}
          className="gap-2 rounded-full px-6 bg-white dark:bg-slate-900 shadow-lg hover:shadow-xl transition-shadow border-slate-200 dark:border-slate-700"
        >
          <span>{isExpanded ? showLessLabel : showMoreLabel}</span>
          <FontAwesomeIcon
            icon={isExpanded ? faChevronUp : faChevronDown}
            className="w-3 h-3"
          />
        </Button>
      </div>
    </div>
  );
}

interface CollapsibleHomeSectionProps {
  children: React.ReactNode;
  sectionId: string;
  showMoreLabel?: string;
}

export function CollapsibleHomeSection({
  children,
  sectionId,
  showMoreLabel = "Explore More",
}: CollapsibleHomeSectionProps) {
  const [isVisible, setIsVisible] = useState(false);

  if (isVisible) {
    return <section id={sectionId}>{children}</section>;
  }

  return (
    <div className="py-8 bg-slate-50 dark:bg-slate-950">
      <div className="container mx-auto px-4">
        <div className="flex justify-center">
          <Button
            variant="outline"
            onClick={() => setIsVisible(true)}
            className="gap-2 rounded-full px-8 py-6 text-base bg-white dark:bg-slate-900 shadow-lg hover:shadow-xl transition-all hover:scale-105 border-slate-200 dark:border-slate-700"
          >
            <span>{showMoreLabel}</span>
            <FontAwesomeIcon icon={faChevronDown} className="w-4 h-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}
