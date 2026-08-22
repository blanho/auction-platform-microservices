import type { FileAttachment } from '@/shared/types/storage.types'
import { zodResolver } from '@hookform/resolvers/zod'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import {
  createAuctionSchema,
  createUpdateAuctionSchema,
  type CreateAuctionFormData,
  type UpdateAuctionFormData,
} from '../schemas'
import type { CreateAuctionRequest, UpdateAuctionRequest } from '../types'
import { getDefaultCreateValues } from '../utils'
import { useAuction, useCreateAuction, useUpdateAuction } from './useAuctions'

function isCreateAuctionFormData(
  data: CreateAuctionFormData | UpdateAuctionFormData
): data is CreateAuctionFormData {
  return 'reservePrice' in data && 'auctionEnd' in data
}

export function useAuctionForm(id: string | undefined) {
  const { t } = useTranslation('auctions')
  const navigate = useNavigate()
  const isEditMode = Boolean(id)

  const [buyNowPreference, setBuyNowPreference] = useState<{
    auctionId: string | undefined
    isEnabled: boolean
  } | null>(null)

  const {
    data: existingAuction,
    isLoading: isFetchingAuction,
    error: fetchError,
  } = useAuction(id ?? '')

  const createMutation = useCreateAuction()
  const updateMutation = useUpdateAuction()

  const schema = useMemo(
    () => (isEditMode ? createUpdateAuctionSchema(t) : createAuctionSchema(t)),
    [isEditMode, t]
  )

  const form = useForm<CreateAuctionFormData | UpdateAuctionFormData>({
    resolver: zodResolver(schema),
    defaultValues: isEditMode ? undefined : getDefaultCreateValues(),
    mode: 'onBlur',
  })

  useEffect(() => {
    if (isEditMode && existingAuction) {
      form.reset({
        title: existingAuction.title,
        description: existingAuction.description,
        categoryId: existingAuction.categoryId,
        condition: existingAuction.condition,
        yearManufactured: existingAuction.yearManufactured,
      })
    }
  }, [existingAuction, isEditMode, form])

  const enableBuyNow =
    buyNowPreference && buyNowPreference.auctionId === id
      ? buyNowPreference.isEnabled
      : Boolean(existingAuction?.buyNowPrice)

  const setEnableBuyNow = useCallback(
    (isEnabled: boolean) => setBuyNowPreference({ auctionId: id, isEnabled }),
    [id]
  )

  const handleSubmit = async (
    data: CreateAuctionFormData | UpdateAuctionFormData,
    attachments: FileAttachment[]
  ) => {
    if (isEditMode && id) {
      const updateData: UpdateAuctionRequest = {
        title: data.title,
        description: data.description,
        condition: data.condition,
        yearManufactured: data.yearManufactured,
      }
      await updateMutation.mutateAsync({ id, data: updateData })
    } else {
      if (!isCreateAuctionFormData(data)) {
        throw new Error('Create auction form data is incomplete')
      }

      const createData: CreateAuctionRequest = {
        title: data.title,
        description: data.description,
        condition: data.condition,
        yearManufactured: data.yearManufactured,
        reservePrice: data.reservePrice,
        buyNowPrice: enableBuyNow ? data.buyNowPrice : undefined,
        auctionEnd: new Date(data.auctionEnd).toISOString(),
        categoryId: data.categoryId,
        brandId: data.brandId || undefined,
        currency: data.currency,
        isFeatured: data.isFeatured,
        files: attachments.map((a) => ({
          fileId: a.fileId,
          fileType: a.fileType,
          displayOrder: a.displayOrder,
          isPrimary: a.isPrimary,
        })),
      }
      await createMutation.mutateAsync(createData)
    }
    navigate('/my-auctions')
  }

  const isSubmitting = createMutation.isPending || updateMutation.isPending
  const isFetchingAuctionData = isEditMode && isFetchingAuction

  return {
    form,
    isEditMode,
    existingAuction,
    fetchError: isEditMode ? fetchError : null,
    isFetchingAuctionData,
    isSubmitting,
    enableBuyNow,
    setEnableBuyNow,
    handleSubmit,
  }
}
