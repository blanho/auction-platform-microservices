"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUndo, faXmark, faTrash, faCheck } from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";
import { motion, AnimatePresence } from "framer-motion";

const UNDO_TIMEOUT_MS = 5000;

interface UndoToastProps {
  message: string;
  onUndo: () => void | Promise<void>;
  onConfirm: () => void | Promise<void>;
  onDismiss?: () => void;
  variant?: "delete" | "remove" | "archive";
}

interface UndoToastState {
  id: string;
  message: string;
  onUndo: () => void | Promise<void>;
  onConfirm: () => void | Promise<void>;
  onDismiss?: () => void;
  variant: "delete" | "remove" | "archive";
}

let toastQueue: UndoToastState[] = [];
let listeners: Array<(toasts: UndoToastState[]) => void> = [];

function notifyListeners() {
  listeners.forEach((listener) => listener([...toastQueue]));
}

export function showUndoToast(options: UndoToastProps) {
  const id = Math.random().toString(36).substring(2, 9);
  const toast: UndoToastState = {
    id,
    message: options.message,
    onUndo: options.onUndo,
    onConfirm: options.onConfirm,
    onDismiss: options.onDismiss,
    variant: options.variant ?? "delete",
  };

  toastQueue = [...toastQueue, toast];
  notifyListeners();

  return id;
}

export function dismissUndoToast(id: string) {
  toastQueue = toastQueue.filter((t) => t.id !== id);
  notifyListeners();
}

function UndoToastItem({
  toast,
  onComplete,
}: {
  toast: UndoToastState;
  onComplete: (id: string, action: "undo" | "confirm" | "timeout") => void;
}) {
  const [progress, setProgress] = useState(100);
  const [isProcessing, setIsProcessing] = useState(false);
  const startTimeRef = useRef<number>(Date.now());
  const animationRef = useRef<number>();

  useEffect(() => {
    startTimeRef.current = Date.now();

    const animate = () => {
      const elapsed = Date.now() - startTimeRef.current;
      const remaining = Math.max(0, 100 - (elapsed / UNDO_TIMEOUT_MS) * 100);
      setProgress(remaining);

      if (remaining > 0) {
        animationRef.current = requestAnimationFrame(animate);
      } else {
        onComplete(toast.id, "timeout");
      }
    };

    animationRef.current = requestAnimationFrame(animate);

    return () => {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
    };
  }, [toast.id, onComplete]);

  const handleUndo = useCallback(async () => {
    if (isProcessing) return;
    setIsProcessing(true);

    if (animationRef.current) {
      cancelAnimationFrame(animationRef.current);
    }

    try {
      await toast.onUndo();
      onComplete(toast.id, "undo");
    } catch {
      setIsProcessing(false);
    }
  }, [toast, onComplete, isProcessing]);

  const handleConfirmNow = useCallback(async () => {
    if (isProcessing) return;
    setIsProcessing(true);

    if (animationRef.current) {
      cancelAnimationFrame(animationRef.current);
    }

    try {
      await toast.onConfirm();
      onComplete(toast.id, "confirm");
    } catch {
      setIsProcessing(false);
    }
  }, [toast, onComplete, isProcessing]);

  const getVariantIcon = () => {
    switch (toast.variant) {
      case "archive":
        return faCheck;
      case "remove":
        return faXmark;
      default:
        return faTrash;
    }
  };

  const getVariantColor = () => {
    switch (toast.variant) {
      case "archive":
        return "text-amber-500";
      case "remove":
        return "text-orange-500";
      default:
        return "text-red-500";
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 50, scale: 0.9 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      exit={{ opacity: 0, y: 20, scale: 0.9 }}
      className={cn(
        "relative overflow-hidden rounded-lg border bg-white shadow-lg dark:bg-slate-900",
        "min-w-[320px] max-w-[400px]"
      )}
    >
      <div className="absolute bottom-0 left-0 h-1 bg-slate-200 dark:bg-slate-700 w-full">
        <motion.div
          className="h-full bg-purple-500"
          initial={{ width: "100%" }}
          animate={{ width: `${progress}%` }}
          transition={{ duration: 0.1, ease: "linear" }}
        />
      </div>

      <div className="p-4">
        <div className="flex items-center gap-3">
          <div
            className={cn(
              "flex h-10 w-10 items-center justify-center rounded-full",
              "bg-slate-100 dark:bg-slate-800"
            )}
          >
            <FontAwesomeIcon
              icon={getVariantIcon()}
              className={cn("h-4 w-4", getVariantColor())}
            />
          </div>

          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-slate-900 dark:text-white truncate">
              {toast.message}
            </p>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              {Math.ceil((progress / 100) * 5)}s to undo
            </p>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={handleUndo}
              disabled={isProcessing}
              className={cn(
                "flex items-center gap-1.5 rounded-md px-3 py-1.5",
                "text-sm font-medium text-purple-600 dark:text-purple-400",
                "bg-purple-50 hover:bg-purple-100 dark:bg-purple-900/30 dark:hover:bg-purple-900/50",
                "transition-colors",
                isProcessing && "opacity-50 cursor-not-allowed"
              )}
            >
              <FontAwesomeIcon icon={faUndo} className="h-3 w-3" />
              <span>Undo</span>
            </button>

            <button
              onClick={handleConfirmNow}
              disabled={isProcessing}
              className={cn(
                "p-1.5 rounded-md text-slate-400 hover:text-slate-600",
                "hover:bg-slate-100 dark:hover:bg-slate-800",
                "transition-colors",
                isProcessing && "opacity-50 cursor-not-allowed"
              )}
              title="Dismiss"
            >
              <FontAwesomeIcon icon={faXmark} className="h-4 w-4" />
            </button>
          </div>
        </div>
      </div>
    </motion.div>
  );
}

export function UndoToastContainer() {
  const [toasts, setToasts] = useState<UndoToastState[]>([]);

  useEffect(() => {
    const listener = (newToasts: UndoToastState[]) => {
      setToasts(newToasts);
    };

    listeners.push(listener);

    return () => {
      listeners = listeners.filter((l) => l !== listener);
    };
  }, []);

  const handleComplete = useCallback(
    async (id: string, action: "undo" | "confirm" | "timeout") => {
      const toast = toasts.find((t) => t.id === id);
      if (!toast) return;

      if (action === "timeout") {
        await toast.onConfirm();
      }

      toast.onDismiss?.();
      dismissUndoToast(id);
    },
    [toasts]
  );

  return (
    <div className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2">
      <AnimatePresence mode="popLayout">
        {toasts.map((toast) => (
          <UndoToastItem
            key={toast.id}
            toast={toast}
            onComplete={handleComplete}
          />
        ))}
      </AnimatePresence>
    </div>
  );
}
