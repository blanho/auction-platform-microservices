export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'] as const

export const ACCEPTED_DOCUMENT_TYPES = [
  'application/pdf',
  'text/csv',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
] as const

export const ALL_ACCEPTED_TYPES = [...ACCEPTED_IMAGE_TYPES, ...ACCEPTED_DOCUMENT_TYPES] as const

export const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024

export const MAX_FILES_PER_UPLOAD = 10
