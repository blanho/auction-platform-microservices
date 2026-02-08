import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { preloadNamespaces } from '@/i18n'

export function useModuleTranslation(namespaces: string | string[]) {
  const ns = Array.isArray(namespaces) ? namespaces : [namespaces]
  const [isLoaded, setIsLoaded] = useState(false)
  const translation = useTranslation(ns)

  useEffect(() => {
    let cancelled = false

    preloadNamespaces(ns).then(() => {
      if (!cancelled) {
        setIsLoaded(true)
      }
    })

    return () => {
      cancelled = true
    }
  }, [ns.join(',')])

  return {
    ...translation,
    isTranslationLoaded: isLoaded,
  }
}
