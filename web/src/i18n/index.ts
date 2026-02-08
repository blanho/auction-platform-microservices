import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'

import commonEn from './locales/en/common.json'
import commonJa from './locales/ja/common.json'

const moduleTranslationLoaders: Record<string, Record<string, () => Promise<{ default: Record<string, unknown> }>>> = {
  en: {
    auctions: () => import('../modules/auctions/i18n/en.json'),
    auth: () => import('../modules/auth/i18n/en.json'),
    bidding: () => import('../modules/bidding/i18n/en.json'),
    home: () => import('../modules/home/i18n/en.json'),
    users: () => import('../modules/users/i18n/en.json'),
    notifications: () => import('../modules/notifications/i18n/en.json'),
    payments: () => import('../modules/payments/i18n/en.json'),
    analytics: () => import('../modules/analytics/i18n/en.json'),
  },
  ja: {
    auctions: () => import('../modules/auctions/i18n/ja.json'),
    auth: () => import('../modules/auth/i18n/ja.json'),
    bidding: () => import('../modules/bidding/i18n/ja.json'),
    home: () => import('../modules/home/i18n/ja.json'),
    users: () => import('../modules/users/i18n/ja.json'),
    notifications: () => import('../modules/notifications/i18n/ja.json'),
    payments: () => import('../modules/payments/i18n/ja.json'),
    analytics: () => import('../modules/analytics/i18n/ja.json'),
  },
}

const loadedNamespaces = new Set<string>()

async function loadNamespace(lng: string, ns: string): Promise<void> {
  const cacheKey = `${lng}:${ns}`
  if (loadedNamespaces.has(cacheKey)) {
    return
  }

  const langLoaders = moduleTranslationLoaders[lng]
  if (!langLoaders) {
    return
  }

  const loader = langLoaders[ns]
  if (!loader) {
    return
  }

  const translationModule = await loader()
  i18n.addResourceBundle(lng, ns, translationModule.default, true, true)
  loadedNamespaces.add(cacheKey)
}

export async function preloadNamespaces(namespaces: string[]): Promise<void> {
  const currentLng = i18n.language || 'en'
  await Promise.all(namespaces.map((ns) => loadNamespace(currentLng, ns)))
}

export const moduleNamespaces = [
  'auctions',
  'auth',
  'bidding',
  'home',
  'users',
  'notifications',
  'payments',
  'analytics',
] as const

export const supportedLanguages = [
  { code: 'en', label: 'English', flag: '🇺🇸' },
  { code: 'ja', label: '日本語', flag: '🇯🇵' },
] as const

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { common: commonEn },
      ja: { common: commonJa },
    },
    fallbackLng: 'en',
    supportedLngs: ['en', 'ja'],
    defaultNS: 'common',
    ns: ['common'],
    partialBundledLanguages: true,

    interpolation: {
      escapeValue: false,
    },

    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'i18nextLng',
    },
  })

i18n.on('languageChanged', (lng) => {
  const namespacesToReload = [...new Set(
    [...loadedNamespaces].map((key) => key.split(':')[1])
  )]

  namespacesToReload.forEach((ns) => {
    if (ns !== 'common') {
      loadNamespace(lng, ns)
    }
  })
})

export default i18n
