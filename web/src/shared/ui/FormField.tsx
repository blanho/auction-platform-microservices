import {
  TextField,
  InputAdornment,
  IconButton,
} from '@mui/material'
import type { TextFieldProps } from '@mui/material'
import { Visibility, VisibilityOff } from '@mui/icons-material'
import type {
  FieldValues,
  FieldPath,
  UseFormRegister,
  FieldErrors,
  RegisterOptions,
} from 'react-hook-form'
import { useState } from 'react'

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
  const [showPassword, setShowPassword] = useState(false)

  const error = errors[name]
  const errorMessage = error?.message as string | undefined
  const hasError = !!error

  const isPasswordField = type === 'password' || showPasswordToggle
  const effectiveType = isPasswordField && showPassword ? 'text' : type

  const passwordAdornment = showPasswordToggle ? {
    endAdornment: (
      <InputAdornment position="end">
        <IconButton
          onClick={() => setShowPassword(!showPassword)}
          edge="end"
          size="small"
          tabIndex={-1}
        >
          {showPassword ? <VisibilityOff /> : <Visibility />}
        </IconButton>
      </InputAdornment>
    ),
  } : {}

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
