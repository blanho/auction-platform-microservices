import { useState, useCallback } from 'react'
import type { UseFormTrigger, FieldValues, Path } from 'react-hook-form'

type StepFieldMap<T extends FieldValues> = Record<number, Path<T>[]>

interface UseMultiStepFormOptions<T extends FieldValues> {
  totalSteps: number
  trigger: UseFormTrigger<T>
  stepFields: StepFieldMap<T>
}

interface UseMultiStepFormReturn {
  activeStep: number
  isFirstStep: boolean
  isLastStep: boolean
  goToNext: () => Promise<void>
  goToPrev: () => void
}

/**
 * Manages multi-step form progression with per-step validation.
 * Decouples stepper logic from form content, keeping both testable and focused.
 */
export function useMultiStepForm<T extends FieldValues>({
  totalSteps,
  trigger,
  stepFields,
}: UseMultiStepFormOptions<T>): UseMultiStepFormReturn {
  const [activeStep, setActiveStep] = useState(0)

  const goToNext = useCallback(async () => {
    const fields = stepFields[activeStep] ?? []
    const isValid = await trigger(fields)
    if (isValid) {
      setActiveStep((prev) => Math.min(prev + 1, totalSteps - 1))
    }
  }, [activeStep, trigger, stepFields, totalSteps])

  const goToPrev = useCallback(() => {
    setActiveStep((prev) => Math.max(prev - 1, 0))
  }, [])

  return {
    activeStep,
    isFirstStep: activeStep === 0,
    isLastStep: activeStep === totalSteps - 1,
    goToNext,
    goToPrev,
  }
}
