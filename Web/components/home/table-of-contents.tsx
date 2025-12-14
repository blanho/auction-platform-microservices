"use client";

import { useState, useEffect, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faList,
  faXmark,
  faChevronUp,
  faHome,
  faLayerGroup,
  faStar,
  faClock,
  faWandSparkles,
  faChartLine,
  faFire,
  faHeart,
  faTrophy,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { cn } from "@/lib/utils";

interface TocItem {
  id: string;
  label: string;
  icon: IconDefinition;
}

const TOC_ITEMS: TocItem[] = [
  { id: "hero", label: "Home", icon: faHome },
  { id: "categories", label: "Categories", icon: faLayerGroup },
  { id: "featured", label: "Featured", icon: faStar },
  { id: "ending-soon", label: "Ending Soon", icon: faClock },
  { id: "new-arrivals", label: "New Arrivals", icon: faWandSparkles },
  { id: "stats", label: "Stats", icon: faChartLine },
  { id: "trending", label: "Trending", icon: faFire },
  { id: "personalization", label: "For You", icon: faHeart },
  { id: "top-auctions", label: "Top", icon: faTrophy },
];

export function TableOfContents() {
  const [activeSection, setActiveSection] = useState<string>("hero");
  const [isExpanded, setIsExpanded] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const [isVisible, setIsVisible] = useState(false);
  const [progress, setProgress] = useState(0);

  const handleScroll = useCallback(() => {
    const scrollTop = window.scrollY;
    const docHeight =
      document.documentElement.scrollHeight - window.innerHeight;
    const scrollProgress = (scrollTop / docHeight) * 100;
    setProgress(scrollProgress);

    setIsVisible(scrollTop > 400);

    const sections = TOC_ITEMS.map((item) => {
      const element = document.getElementById(item.id);
      if (element) {
        const rect = element.getBoundingClientRect();
        return {
          id: item.id,
          top: rect.top,
          bottom: rect.bottom,
        };
      }
      return null;
    }).filter(Boolean);

    const viewportCenter = window.innerHeight / 3;

    for (const section of sections) {
      if (section && section.top <= viewportCenter && section.bottom > 0) {
        setActiveSection(section.id);
        break;
      }
    }
  }, []);

  useEffect(() => {
    window.addEventListener("scroll", handleScroll, { passive: true });
    handleScroll();

    return () => window.removeEventListener("scroll", handleScroll);
  }, [handleScroll]);

  const scrollToSection = (id: string) => {
    const element = document.getElementById(id);
    if (element) {
      const headerOffset = 80;
      const elementPosition = element.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.scrollY - headerOffset;

      window.scrollTo({
        top: offsetPosition,
        behavior: "smooth",
      });
    }
    setIsOpen(false);
    setIsExpanded(false);
  };

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  if (!isVisible) return null;

  return (
    <>
      <motion.div
        initial={{ opacity: 0, x: 20 }}
        animate={{ opacity: 1, x: 0 }}
        exit={{ opacity: 0, x: 20 }}
        transition={{ duration: 0.3 }}
        onMouseEnter={() => setIsExpanded(true)}
        onMouseLeave={() => setIsExpanded(false)}
        className="fixed right-0 top-1/2 -translate-y-1/2 z-50 hidden lg:block"
      >
        <div className="relative flex items-center">
          <AnimatePresence>
            {isExpanded && (
              <motion.div
                initial={{ opacity: 0, x: 10, width: 0 }}
                animate={{ opacity: 1, x: 0, width: "auto" }}
                exit={{ opacity: 0, x: 10, width: 0 }}
                transition={{ duration: 0.2 }}
                className="mr-1 overflow-hidden"
              >
                <div className="backdrop-blur-md bg-white/90 dark:bg-slate-900/90 border border-slate-200/50 dark:border-slate-700/50 rounded-l-xl py-2 px-1 shadow-lg">
                  <nav className="space-y-0.5">
                    {TOC_ITEMS.map((item) => {
                      const isActive = activeSection === item.id;
                      return (
                        <motion.button
                          key={item.id}
                          onClick={() => scrollToSection(item.id)}
                          whileHover={{ x: -2 }}
                          className={cn(
                            "block w-full text-right px-2 py-1 rounded-lg text-xs font-medium transition-all whitespace-nowrap",
                            isActive
                              ? "text-purple-600 dark:text-purple-400"
                              : "text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-white"
                          )}
                        >
                          {item.label}
                        </motion.button>
                      );
                    })}
                  </nav>
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          <div className="backdrop-blur-md bg-white/90 dark:bg-slate-900/90 border-l border-y border-slate-200/50 dark:border-slate-700/50 rounded-l-xl py-2 px-1.5 shadow-lg">
            <div className="absolute left-0 top-2 bottom-2 w-0.5 bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
              <motion.div
                className="w-full bg-gradient-to-b from-purple-500 to-blue-500 rounded-full origin-top"
                style={{ height: `${progress}%` }}
              />
            </div>

            <nav className="space-y-0.5 ml-2">
              {TOC_ITEMS.map((item) => {
                const isActive = activeSection === item.id;
                return (
                  <motion.button
                    key={item.id}
                    onClick={() => scrollToSection(item.id)}
                    whileHover={{ scale: 1.15 }}
                    whileTap={{ scale: 0.95 }}
                    className={cn(
                      "block w-7 h-7 rounded-lg flex items-center justify-center transition-all",
                      isActive
                        ? "bg-gradient-to-br from-purple-500 to-blue-600 text-white shadow-md shadow-purple-500/30"
                        : "text-slate-400 dark:text-slate-500 hover:text-slate-700 dark:hover:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800"
                    )}
                    title={item.label}
                  >
                    <FontAwesomeIcon icon={item.icon} className="w-3 h-3" />
                  </motion.button>
                );
              })}

              <div className="pt-1 mt-1 border-t border-slate-200/50 dark:border-slate-700/50">
                <motion.button
                  onClick={scrollToTop}
                  whileHover={{ scale: 1.15 }}
                  whileTap={{ scale: 0.95 }}
                  className="block w-7 h-7 rounded-lg flex items-center justify-center text-slate-400 dark:text-slate-500 hover:text-purple-600 dark:hover:text-purple-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-all"
                  title="Back to top"
                >
                  <FontAwesomeIcon icon={faChevronUp} className="w-3 h-3" />
                </motion.button>
              </div>
            </nav>
          </div>
        </div>
      </motion.div>

      <div className="lg:hidden fixed right-3 bottom-20 z-50 flex flex-col gap-2">
        <AnimatePresence>
          {isOpen && (
            <motion.div
              initial={{ opacity: 0, y: 10, scale: 0.95 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 10, scale: 0.95 }}
              transition={{ duration: 0.15 }}
              className="backdrop-blur-md bg-white/95 dark:bg-slate-900/95 rounded-xl shadow-xl border border-slate-200/50 dark:border-slate-700/50 p-2 mb-1 max-h-[50vh] overflow-y-auto"
            >
              <div className="grid grid-cols-3 gap-1">
                {TOC_ITEMS.map((item) => {
                  const isActive = activeSection === item.id;
                  return (
                    <motion.button
                      key={item.id}
                      onClick={() => scrollToSection(item.id)}
                      whileTap={{ scale: 0.95 }}
                      className={cn(
                        "flex flex-col items-center gap-1 p-2 rounded-lg text-[10px] font-medium transition-all",
                        isActive
                          ? "bg-gradient-to-br from-purple-500/20 to-blue-500/20 text-purple-700 dark:text-purple-300"
                          : "text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                      )}
                    >
                      <span
                        className={cn(
                          "w-8 h-8 rounded-lg flex items-center justify-center transition-all",
                          isActive
                            ? "bg-gradient-to-br from-purple-500 to-blue-600 text-white"
                            : "bg-slate-100 dark:bg-slate-800"
                        )}
                      >
                        <FontAwesomeIcon icon={item.icon} className="w-3.5 h-3.5" />
                      </span>
                      {item.label}
                    </motion.button>
                  );
                })}
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        <div className="flex gap-1.5 justify-end">
          <motion.button
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={scrollToTop}
            className="w-10 h-10 rounded-full backdrop-blur-md bg-white/90 dark:bg-slate-800/90 shadow-lg border border-slate-200/50 dark:border-slate-700/50 flex items-center justify-center text-slate-500 dark:text-slate-400 hover:text-purple-600 dark:hover:text-purple-400 transition-colors"
          >
            <FontAwesomeIcon icon={faChevronUp} className="w-3.5 h-3.5" />
          </motion.button>

          <motion.button
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={() => setIsOpen(!isOpen)}
            className={cn(
              "w-10 h-10 rounded-full shadow-lg flex items-center justify-center transition-all",
              isOpen
                ? "bg-gradient-to-br from-purple-500 to-blue-600 text-white"
                : "backdrop-blur-md bg-white/90 dark:bg-slate-800/90 border border-slate-200/50 dark:border-slate-700/50 text-slate-500 dark:text-slate-400"
            )}
          >
            <FontAwesomeIcon
              icon={isOpen ? faXmark : faList}
              className="w-3.5 h-3.5"
            />
          </motion.button>
        </div>
      </div>

      <div className="fixed bottom-0 left-0 right-0 h-0.5 bg-slate-200/30 dark:bg-slate-800/30 z-40 lg:hidden">
        <motion.div
          className="h-full bg-gradient-to-r from-purple-500 to-blue-500"
          style={{ width: `${progress}%` }}
        />
      </div>
    </>
  );
}
