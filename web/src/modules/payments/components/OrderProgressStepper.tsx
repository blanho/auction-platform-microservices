import { palette } from '@/shared/theme/tokens'
import { formatDate } from '@/shared/utils/formatters'
import { CheckCircle, Inventory, LocalShipping, Payment, ShoppingBag } from '@mui/icons-material'
import {
  Step,
  StepConnector,
  stepConnectorClasses,
  StepLabel,
  Stepper,
  styled,
  Typography,
} from '@mui/material'
import type { StepIconProps } from '@mui/material/StepIcon'
import { useTranslation } from 'react-i18next'
import type { Order } from '../types'
import { getOrderActiveStep } from '../utils'

const OrderProgressConnector = styled(StepConnector)(({ theme }) => ({
  [`&.${stepConnectorClasses.alternativeLabel}`]: {
    top: 22,
  },
  [`&.${stepConnectorClasses.active}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: `linear-gradient(95deg, ${palette.brand.primary} 0%, #A16207 100%)`,
    },
  },
  [`&.${stepConnectorClasses.completed}`]: {
    [`& .${stepConnectorClasses.line}`]: {
      backgroundImage: `linear-gradient(95deg, ${palette.semantic.success} 0%, #16A34A 100%)`,
    },
  },
  [`& .${stepConnectorClasses.line}`]: {
    height: 3,
    border: 0,
    backgroundColor: theme.palette.mode === 'dark' ? theme.palette.grey[800] : palette.neutral[200],
    borderRadius: 1,
  },
}))

const OrderProgressStepIconRoot = styled('div')<{
  ownerState: { completed?: boolean; active?: boolean; error?: boolean }
}>(({ ownerState }) => ({
  backgroundColor: palette.neutral[200],
  zIndex: 1,
  color: palette.neutral[500],
  width: 50,
  height: 50,
  display: 'flex',
  borderRadius: '50%',
  justifyContent: 'center',
  alignItems: 'center',
  ...(ownerState.active && {
    backgroundImage: `linear-gradient(136deg, ${palette.brand.primary} 0%, #A16207 100%)`,
    boxShadow: `0 4px 10px 0 ${palette.brand.muted}`,
    color: 'white',
  }),
  ...(ownerState.completed && {
    backgroundImage: `linear-gradient(136deg, ${palette.semantic.success} 0%, #16A34A 100%)`,
    color: 'white',
  }),
  ...(ownerState.error && {
    backgroundImage: `linear-gradient(136deg, ${palette.semantic.error} 0%, #DC2626 100%)`,
    color: 'white',
  }),
}))

interface OrderStepIconProps extends StepIconProps {
  icon: React.ReactNode
}

function OrderStepIcon({ icon, ...props }: Readonly<OrderStepIconProps>) {
  const { active = false, completed = false, error, className } = props
  return (
    <OrderProgressStepIconRoot ownerState={{ completed, active, error }} className={className}>
      {icon}
    </OrderProgressStepIconRoot>
  )
}

type OrderProgress = Pick<
  Order,
  'status' | 'createdAt' | 'paidAt' | 'shippedAt' | 'deliveredAt' | 'completedAt'
>

export function OrderProgressStepper({ order }: Readonly<{ order: OrderProgress }>) {
  const { t } = useTranslation('payments')
  const steps = [
    {
      label: t('orderDetail.steps.placed'),
      icon: <ShoppingBag fontSize="small" />,
      date: order.createdAt,
    },
    {
      label: t('orderDetail.steps.payment'),
      icon: <Payment fontSize="small" />,
      date: order.paidAt,
    },
    {
      label: t('orderDetail.steps.shipped'),
      icon: <LocalShipping fontSize="small" />,
      date: order.shippedAt,
    },
    {
      label: t('orderDetail.steps.delivered'),
      icon: <Inventory fontSize="small" />,
      date: order.deliveredAt,
    },
    {
      label: t('orderDetail.steps.completed'),
      icon: <CheckCircle fontSize="small" />,
      date: order.completedAt,
    },
  ]

  const activeStep = getOrderActiveStep(order.status)
  return (
    <Stepper alternativeLabel activeStep={activeStep} connector={<OrderProgressConnector />}>
      {steps.map((step, index) => (
        <Step key={step.label}>
          <StepLabel
            slots={{
              stepIcon: OrderStepIcon,
            }}
            slotProps={{
              stepIcon: {
                icon: step.icon,
              },
            }}
          >
            <Typography
              sx={{
                fontWeight: index <= activeStep ? 600 : 400,
                color: index <= activeStep ? palette.neutral[900] : palette.neutral[500],
              }}
            >
              {step.label}
            </Typography>
            {step.date && (
              <Typography variant="caption" color="text.secondary">
                {formatDate(step.date)}
              </Typography>
            )}
          </StepLabel>
        </Step>
      ))}
    </Stepper>
  )
}
