export const PASSWORD_MIN_LENGTH = 12
export const PASSWORD_MAX_LENGTH = 100
export const USERNAME_MIN_LENGTH = 3
export const USERNAME_MAX_LENGTH = 50

export const PASSWORD_REGEX = {
  uppercase: /[A-Z]/,
  lowercase: /[a-z]/,
  number: /[0-9]/,
  specialChar: /[!@#$%^&*(),.?":{}|<>]/,
}

export const USERNAME_REGEX = /^[a-zA-Z0-9_]+$/

export const TWO_FACTOR_CODE_LENGTH = 6
