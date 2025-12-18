"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { UseFormReturn, FieldValues, Path, PathValue } from "react-hook-form";

const DRAFT_PREFIX = "auction_draft_";
const AUTO_SAVE_DELAY_MS = 1000;

interface DraftData<T> {
  values: T;
  savedAt: number;
  version: number;
}

interface UseFormDraftOptions<T extends FieldValues> {
  form: UseFormReturn<T>;
  key: string;
  exclude?: (keyof T)[];
  onDraftRestored?: () => void;
}

interface UseFormDraftReturn {
  hasDraft: boolean;
  draftDate: Date | null;
  restoreDraft: () => void;
  discardDraft: () => void;
  clearDraft: () => void;
  isRestorePromptOpen: boolean;
  setIsRestorePromptOpen: (open: boolean) => void;
}

const CURRENT_VERSION = 1;

function getStorageKey(key: string): string {
  return `${DRAFT_PREFIX}${key}`;
}

function saveDraft<T>(key: string, values: T): void {
  try {
    const data: DraftData<T> = {
      values,
      savedAt: Date.now(),
      version: CURRENT_VERSION,
    };
    localStorage.setItem(getStorageKey(key), JSON.stringify(data));
  } catch {
  }
}

function loadDraft<T>(key: string): DraftData<T> | null {
  try {
    const stored = localStorage.getItem(getStorageKey(key));
    if (!stored) return null;
    
    const data = JSON.parse(stored) as DraftData<T>;
    
    if (data.version !== CURRENT_VERSION) {
      localStorage.removeItem(getStorageKey(key));
      return null;
    }
    
    const oneWeekAgo = Date.now() - 7 * 24 * 60 * 60 * 1000;
    if (data.savedAt < oneWeekAgo) {
      localStorage.removeItem(getStorageKey(key));
      return null;
    }
    
    return data;
  } catch {
    return null;
  }
}

function deleteDraft(key: string): void {
  try {
    localStorage.removeItem(getStorageKey(key));
  } catch {
  }
}

export function useFormDraft<T extends FieldValues>({
  form,
  key,
  exclude = [],
  onDraftRestored,
}: UseFormDraftOptions<T>): UseFormDraftReturn {
  const [hasDraft, setHasDraft] = useState(false);
  const [draftDate, setDraftDate] = useState<Date | null>(null);
  const [isRestorePromptOpen, setIsRestorePromptOpen] = useState(false);
  const saveTimeoutRef = useRef<NodeJS.Timeout>(undefined);
  const draftDataRef = useRef<DraftData<T> | null>(null);
  const initialCheckDone = useRef(false);

  useEffect(() => {
    if (initialCheckDone.current) return;
    initialCheckDone.current = true;

    const draft = loadDraft<T>(key);
    if (draft) {
      draftDataRef.current = draft;
      Promise.resolve().then(() => {
        setHasDraft(true);
        setDraftDate(new Date(draft.savedAt));
        setIsRestorePromptOpen(true);
      });
    }
  }, [key]);

  useEffect(() => {
    const subscription = form.watch((values) => {
      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }

      saveTimeoutRef.current = setTimeout(() => {
        const filteredValues = { ...values } as T;
        exclude.forEach((field) => {
          delete filteredValues[field as keyof T];
        });

        const hasContent = Object.values(filteredValues).some((v) => {
          if (typeof v === "string") return v.trim().length > 0;
          if (typeof v === "number") return v > 0;
          if (Array.isArray(v)) return v.length > 0;
          return v !== undefined && v !== null && v !== false;
        });

        if (hasContent) {
          saveDraft(key, filteredValues);
          setHasDraft(true);
          setDraftDate(new Date());
        }
      }, AUTO_SAVE_DELAY_MS);
    });

    return () => {
      subscription.unsubscribe();
      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }
    };
  }, [form, key, exclude]);

  const restoreDraft = useCallback(() => {
    const draft = draftDataRef.current || loadDraft<T>(key);
    if (!draft) return;

    Object.entries(draft.values).forEach(([field, value]) => {
      if (!exclude.includes(field as keyof T) && value !== undefined) {
        form.setValue(field as Path<T>, value as PathValue<T, Path<T>>, {
          shouldValidate: false,
          shouldDirty: true,
        });
      }
    });

    setIsRestorePromptOpen(false);
    onDraftRestored?.();
  }, [form, key, exclude, onDraftRestored]);

  const discardDraft = useCallback(() => {
    deleteDraft(key);
    setHasDraft(false);
    setDraftDate(null);
    draftDataRef.current = null;
    setIsRestorePromptOpen(false);
  }, [key]);

  const clearDraft = useCallback(() => {
    deleteDraft(key);
    setHasDraft(false);
    setDraftDate(null);
    draftDataRef.current = null;
  }, [key]);

  return {
    hasDraft,
    draftDate,
    restoreDraft,
    discardDraft,
    clearDraft,
    isRestorePromptOpen,
    setIsRestorePromptOpen,
  };
}
