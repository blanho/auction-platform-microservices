import { Box } from '@mui/material'
import {
  HeroSection,
  CategoriesSection,
  FeaturedAuctionsSection,
  FeaturesSection,
  CollectionsSection,
  TrustedBrandsSection,
  CTASection,
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
    </Box>
  )
}
