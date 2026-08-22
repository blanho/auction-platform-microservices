export function capitalizeStatus(status: string): string {
  return status.replace('-', ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}
