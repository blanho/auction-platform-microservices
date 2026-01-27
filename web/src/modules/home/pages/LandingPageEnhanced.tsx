import { Box } from '@mui/material'
import {
  HeroSection,
  CategoriesSection,
  FeaturedAuctionsSection,
  FeaturesSection,
  CollectionsSection,
  TrustedBrandsSection,
  CTASection,
  Footer,
} from '../components'
import { colors } from '@/shared/theme/tokens'

export const LandingPageEnhanced = () => {
  return (
    <Box sx={{ bgcolor: colors.background.primary, minHeight: '100vh', overflow: 'hidden' }}>
      <HeroSection />
      <CategoriesSection />
      <FeaturedAuctionsSection />
      <FeaturesSection />
      <CollectionsSection />
      <TrustedBrandsSection />
      <CTASection />
      <Footer />

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
      `}</style>
    </Box>
  )
}
