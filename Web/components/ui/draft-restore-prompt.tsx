"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faFloppyDisk, faTrash, faRotateLeft } from "@fortawesome/free-solid-svg-icons";
import { motion, AnimatePresence } from "framer-motion";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { formatRelativeTime } from "@/utils";

interface DraftRestorePromptProps {
  isOpen: boolean;
  draftDate: Date | null;
  onRestore: () => void;
  onDiscard: () => void;
}

export function DraftRestorePrompt({
  isOpen,
  draftDate,
  onRestore,
  onDiscard,
}: DraftRestorePromptProps) {
  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -20 }}
          className="mb-6"
        >
          <Card className="border-amber-200 dark:border-amber-800 bg-amber-50/50 dark:bg-amber-950/20">
            <CardContent className="p-4">
              <div className="flex items-center justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-amber-100 dark:bg-amber-900/50">
                    <FontAwesomeIcon
                      icon={faFloppyDisk}
                      className="h-4 w-4 text-amber-600 dark:text-amber-400"
                    />
                  </div>
                  <div>
                    <p className="font-medium text-amber-900 dark:text-amber-100">
                      Unsaved draft found
                    </p>
                    <p className="text-sm text-amber-700 dark:text-amber-300">
                      {draftDate ? `Saved ${formatRelativeTime(draftDate.toISOString())}` : "Recently saved"}
                    </p>
                  </div>
                </div>
                
                <div className="flex items-center gap-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={onDiscard}
                    className="text-amber-700 hover:text-amber-900 hover:bg-amber-100 dark:text-amber-300 dark:hover:text-amber-100 dark:hover:bg-amber-900/50"
                  >
                    <FontAwesomeIcon icon={faTrash} className="h-3.5 w-3.5 mr-1.5" />
                    Discard
                  </Button>
                  <Button
                    size="sm"
                    onClick={onRestore}
                    className="bg-amber-600 hover:bg-amber-700 text-white"
                  >
                    <FontAwesomeIcon icon={faRotateLeft} className="h-3.5 w-3.5 mr-1.5" />
                    Restore draft
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
