import { useState, useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import {
  createAuctionSchema,
  updateAuctionSchema,
  type CreateAuctionFormData,
  type UpdateAuctionFormData,
} from '../schemas'
import { useAuction, useCreateAuction, useUpdateAuction } from './useAuctions'
import type { CreateAuctionRequest, UpdateAuctionRequest } from '../types'
import type { FileAttachment } from '@/shared/types/storage.types'
import { getDefaultCreateValues } from '../utils'

export function useAuctionForm(id: string | undefined) {
  const navigate = useNavigate()
  const isEditMode = Boolean(id)

  const [enableBuyNow, setEnableBuyNow] = useState(false)

  const {
    data: existingAuction,
    isLoading: isFetchingAuction,
    error: fetchError,
  } = useAuction(id ?? '')

  const createMutation = useCreateAuction()
  const updateMutation = useUpdateAuction()

  const schema = useMemo(
    () => (isEditMode ? updateAuctionSchema : createAuctionSchema),
    [isEditMode]
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
      } as UpdateAuctionFormData)

      if (existingAuction.buyNowPrice) {
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setEnableBuyNow(true)
      }
    }
  }, [existingAuction, isEditMode, form])

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
      const formData = data as CreateAuctionFormData
      const createData: CreateAuctionRequest = {
        title: formData.title,
        description: formData.description,
        condition: formData.condition,
        yearManufactured: formData.yearManufactured,
        reservePrice: formData.reservePrice,
        buyNowPrice: enableBuyNow ? formData.buyNowPrice : undefined,
        auctionEnd: new Date(formData.auctionEnd).toISOString(),
        categoryId: formData.categoryId,
        brandId: formData.brandId || undefined,
        currency: formData.currency,
        isFeatured: formData.isFeatured,
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
