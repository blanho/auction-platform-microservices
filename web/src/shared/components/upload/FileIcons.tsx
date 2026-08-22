import {
  CloudUpload as CloudUploadIcon,
  Description as DocIcon,
  InsertDriveFile as FileIcon,
  Image as ImageIcon,
  PictureAsPdf as PdfIcon,
} from '@mui/icons-material'

export function getFileIcon(contentType: string) {
  if (contentType.startsWith('image/')) {
    return <ImageIcon />
  }
  if (contentType === 'application/pdf') {
    return <PdfIcon />
  }
  if (contentType.includes('spreadsheet') || contentType === 'text/csv') {
    return <DocIcon />
  }
  return <FileIcon />
}

export function getUploadIcon() {
  return <CloudUploadIcon />
}
