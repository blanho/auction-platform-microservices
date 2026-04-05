import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'

import commonEn from './locales/en/common.json'
import commonJa from './locales/ja/common.json'

import auctionsEn from '../modules/auctions/i18n/en.json'
import auctionsJa from '../modules/auctions/i18n/ja.json'
import authEn from '../modules/auth/i18n/en.json'
import authJa from '../modules/auth/i18n/ja.json'
import biddingEn from '../modules/bidding/i18n/en.json'
import biddingJa from '../modules/bidding/i18n/ja.json'
import homeEn from '../modules/home/i18n/en.json'
import homeJa from '../modules/home/i18n/ja.json'
import usersEn from '../modules/users/i18n/en.json'
import usersJa from '../modules/users/i18n/ja.json'
import notificationsEn from '../modules/notifications/i18n/en.json'
import notificationsJa from '../modules/notifications/i18n/ja.json'
import paymentsEn from '../modules/payments/i18n/en.json'
import paymentsJa from '../modules/payments/i18n/ja.json'
import analyticsEn from '../modules/analytics/i18n/en.json'
import analyticsJa from '../modules/analytics/i18n/ja.json'
import jobsEn from '../modules/jobs/i18n/en.json'
import jobsJa from '../modules/jobs/i18n/ja.json'
import searchEn from '../modules/search/i18n/en.json'
import searchJa from '../modules/search/i18n/ja.json'

export async function preloadNamespaces(_namespaces: string[]): Promise<void> {
  return Promise.resolve()
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
  'jobs',
  'search',
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
      en: {
        common: commonEn,
        auctions: auctionsEn,
        auth: authEn,
        bidding: biddingEn,
        home: homeEn,
        users: usersEn,
        notifications: notificationsEn,
        payments: paymentsEn,
        analytics: analyticsEn,
        jobs: jobsEn,
        search: searchEn,
      },
      ja: {
        common: commonJa,
        auctions: auctionsJa,
        auth: authJa,
        bidding: biddingJa,
        home: homeJa,
        users: usersJa,
        notifications: notificationsJa,
        payments: paymentsJa,
        analytics: analyticsJa,
        jobs: jobsJa,
        search: searchJa,
      },
    },
    fallbackLng: 'en',
    supportedLngs: ['en', 'ja'],
    defaultNS: 'common',
    ns: [
      'common',
      'auctions',
      'auth',
      'bidding',
      'home',
      'users',
      'notifications',
      'payments',
      'analytics',
      'jobs',
      'search',
    ],

    interpolation: {
      escapeValue: false,
    },

    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'i18nextLng',
    },
  })

export default i18n
