import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './system-tests',
  workers: 1,
  forbidOnly: Boolean(process.env.CI),
  timeout: 180000,
  expect: { timeout: 15000 },
  reporter: [['list'], ['html', { outputFolder: 'playwright-system-report', open: 'never' }]],
  outputDir: 'system-test-results',
  use: {
    baseURL: process.env.SYSTEM_TEST_URL || 'http://localhost:3000',
    trace: 'retain-on-failure',
    ...devices['Desktop Chrome'],
  },
})
