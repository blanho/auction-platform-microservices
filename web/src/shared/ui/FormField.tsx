import { Visibility, VisibilityOff } from '@mui/icons-material'
import type { TextFieldProps } from '@mui/material'
import { IconButton, InputAdornment, TextField } from '@mui/material'
import { useState } from 'react'
import type {
  FieldErrors,
  FieldPath,
  FieldValues,
  RegisterOptions,
  UseFormRegister,
} from 'react-hook-form'
import { useTranslation } from 'react-i18next'

type FormFieldProps<T extends FieldValues> = Omit<
  TextFieldProps,
  'name' | 'error' | 'helperText'
> & {
  name: FieldPath<T>
  register: UseFormRegister<T>
  errors: FieldErrors<T>
  rules?: RegisterOptions<T, FieldPath<T>>
  helperText?: string
  showPasswordToggle?: boolean
}

export function FormField<T extends FieldValues>({
  name,
  register,
  errors,
  rules,
  helperText,
  showPasswordToggle = false,
  type,
  InputProps,
  ...textFieldProps
}: FormFieldProps<T>) {
  const { t } = useTranslation('common')
  const [showPassword, setShowPassword] = useState(false)

  const error = errors[name]
  const errorMessage = typeof error?.message === 'string' ? error.message : undefined
  const hasError = !!error

  const isPasswordField = type === 'password' || showPasswordToggle
  const effectiveType = isPasswordField && showPassword ? 'text' : type

  const passwordAdornment = showPasswordToggle
    ? {
        endAdornment: (
          <InputAdornment position="end">
            <IconButton
              onClick={() => setShowPassword(!showPassword)}
              edge="end"
              size="small"
              tabIndex={-1}
              aria-label={t(showPassword ? 'actions.hidePassword' : 'actions.showPassword')}
            >
              {showPassword ? <VisibilityOff /> : <Visibility />}
            </IconButton>
          </InputAdornment>
        ),
      }
    : {}

  const mergedInputProps = {
    ...passwordAdornment,
    ...InputProps,
  }

  return (
    <TextField
      {...textFieldProps}
      {...register(name, rules)}
      type={effectiveType}
      error={hasError}
      helperText={errorMessage || helperText}
      InputProps={Object.keys(mergedInputProps).length > 0 ? mergedInputProps : undefined}
    />
  )
}
