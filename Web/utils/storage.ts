import { STORAGE_KEYS } from '@/constants/config';

/**
 * Safe localStorage operations with JSON support
 */
export const storage = {
  /**
   * Get an item from localStorage
   */
  get<T>(key: string, defaultValue: T | null = null): T | null {
    if (typeof window === 'undefined') return defaultValue;
    
    try {
      const item = localStorage.getItem(key);
      return item ? JSON.parse(item) : defaultValue;
    } catch {
      console.warn(`Error reading from localStorage key "${key}"`);
      return defaultValue;
    }
  },
  
  /**
   * Set an item in localStorage
   */
  set<T>(key: string, value: T): boolean {
    if (typeof window === 'undefined') return false;
    
    try {
      localStorage.setItem(key, JSON.stringify(value));
      return true;
    } catch {
      console.warn(`Error writing to localStorage key "${key}"`);
      return false;
    }
  },
  
  /**
   * Remove an item from localStorage
   */
  remove(key: string): boolean {
    if (typeof window === 'undefined') return false;
    
    try {
      localStorage.removeItem(key);
      return true;
    } catch {
      console.warn(`Error removing localStorage key "${key}"`);
      return false;
    }
  },
  
  /**
   * Clear all localStorage
   */
  clear(): boolean {
    if (typeof window === 'undefined') return false;
    
    try {
      localStorage.clear();
      return true;
    } catch {
      console.warn('Error clearing localStorage');
      return false;
    }
  },
};

export const watchlistStorage = {
  get: (): string[] => storage.get<string[]>(STORAGE_KEYS.WATCHLIST, []) ?? [],
  
  add: (auctionId: string): void => {
    const watchlist = watchlistStorage.get();
    if (!watchlist.includes(auctionId)) {
      storage.set(STORAGE_KEYS.WATCHLIST, [...watchlist, auctionId]);
    }
  },
  
  remove: (auctionId: string): void => {
    const watchlist = watchlistStorage.get();
    storage.set(
      STORAGE_KEYS.WATCHLIST,
      watchlist.filter(id => id !== auctionId)
    );
  },
  
  has: (auctionId: string): boolean => {
    return watchlistStorage.get().includes(auctionId);
  },
  
  toggle: (auctionId: string): boolean => {
    if (watchlistStorage.has(auctionId)) {
      watchlistStorage.remove(auctionId);
      return false;
    } else {
      watchlistStorage.add(auctionId);
      return true;
    }
  },
  
  clear: (): void => {
    storage.remove(STORAGE_KEYS.WATCHLIST);
  },
};

export const recentSearchesStorage = {
  get: (): string[] => storage.get<string[]>(STORAGE_KEYS.RECENT_SEARCHES, []) ?? [],
  
  add: (term: string, maxItems: number = 10): void => {
    const searches = recentSearchesStorage.get();
    const filtered = searches.filter(s => s !== term);
    storage.set(STORAGE_KEYS.RECENT_SEARCHES, [term, ...filtered].slice(0, maxItems));
  },
  
  clear: (): void => {
    storage.remove(STORAGE_KEYS.RECENT_SEARCHES);
  },
};
