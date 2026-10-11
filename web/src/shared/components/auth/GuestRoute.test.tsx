import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { GuestRoute } from './GuestRoute'

const auth = vi.hoisted(() => ({
  isAuthenticated: false,
  isLoading: false,
  status: 'unauthenticated',
}))
vi.mock('@/app/providers', () => ({ useAuth: () => auth }))
vi.mock('@/shared/ui', () => ({ LoadingScreen: () => <p>Checking authentication</p> }))

describe('GuestRoute', () => {
  it('preserves a login form while submitting and after authentication fails', () => {
    const form = () => (
      <MemoryRouter>
        <GuestRoute>
          <input aria-label="Username" defaultValue="" />
        </GuestRoute>
      </MemoryRouter>
    )
    const view = render(form())
    fireEvent.change(screen.getByRole('textbox'), { target: { value: 'alice' } })
    auth.isLoading = true
    auth.status = 'loading'
    view.rerender(form())
    expect(screen.getByText('Checking authentication')).toBeTruthy()
    auth.isLoading = false
    auth.status = 'unauthenticated'
    view.rerender(form())
    expect((screen.getByRole('textbox') as HTMLInputElement).value).toBe('alice')
  })
})
