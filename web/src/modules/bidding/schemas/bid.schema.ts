import type { TFunction } from 'i18next'
import { z } from 'zod'
import { BID_CONSTANTS } from '../constants'

export const createAutoBidSchema = (t: TFunction<'bidding'>) =>
  z
    .object({
      auctionId: z.uuid({ message: t('validation.invalidAuctionId') }),
      maxAmount: z
        .number()
        .min(
          BID_CONSTANTS.MIN_BID_AMOUNT,
          t('validation.autoBidMinimum', { amount: BID_CONSTANTS.MIN_BID_AMOUNT })
        ),
      bidIncrement: z
        .number()
        .min(
          BID_CONSTANTS.MIN_BID_AMOUNT,
          t('validation.incrementMinimum', { amount: BID_CONSTANTS.MIN_BID_AMOUNT })
        )
        .optional(),
    })
    .refine((data) => !data.bidIncrement || data.bidIncrement < data.maxAmount, {
      message: t('validation.incrementLessThanMaximum'),
      path: ['bidIncrement'],
    })

export const createUpdateAutoBidSchema = (t: TFunction<'bidding'>) =>
  z
    .object({
      maxAmount: z
        .number()
        .min(
          BID_CONSTANTS.MIN_BID_AMOUNT,
          t('validation.autoBidMinimum', { amount: BID_CONSTANTS.MIN_BID_AMOUNT })
        )
        .optional(),
      bidIncrement: z
        .number()
        .min(
          BID_CONSTANTS.MIN_BID_AMOUNT,
          t('validation.incrementMinimum', { amount: BID_CONSTANTS.MIN_BID_AMOUNT })
        )
        .optional(),
    })
    .refine(
      (data) => {
        if (data.maxAmount && data.bidIncrement) {
          return data.bidIncrement < data.maxAmount
        }
        return true
      },
      {
        message: t('validation.incrementLessThanMaximum'),
        path: ['bidIncrement'],
      }
    )

export const createRetractBidSchema = (t: TFunction<'bidding'>) =>
  z.object({
    reason: z.string().min(10, t('validation.reasonMin')).max(500, t('validation.reasonMax')),
  })
