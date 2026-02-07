import { lazy, Suspense, type ComponentType } from 'react'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { MainLayout } from '@/shared/components/layouts/MainLayout'
import { AuthLayout } from '@/shared/components/layouts/AuthLayout'
import { LandingLayout } from '@/shared/components/layouts/LandingLayout'
import { LoadingScreen } from '@/shared/ui/LoadingScreen'
import { ProtectedRoute } from '@/shared/components/auth/ProtectedRoute'

const LandingPage = lazy(() =>
  import('@/modules/home/pages/LandingPageEnhanced').then((m) => ({
    default: m.LandingPageEnhanced,
  }))
)
const HowItWorksPage = lazy(() =>
  import('@/modules/home/pages/HowItWorksPage').then((m) => ({ default: m.HowItWorksPage }))
)
const AuctionsPage = lazy(() =>
  import('@/modules/auctions/pages/AuctionsListPage').then((m) => ({ default: m.AuctionsListPage }))
)
const AuctionDetailPage = lazy(() =>
  import('@/modules/auctions/pages/AuctionDetailPage').then((m) => ({
    default: m.AuctionDetailPage,
  }))
)
const AuctionFormPage = lazy(() =>
  import('@/modules/auctions/pages/AuctionFormPage').then((m) => ({ default: m.AuctionFormPage }))
)
const WatchlistPage = lazy(() =>
  import('@/modules/auctions/pages/WatchlistPage').then((m) => ({ default: m.WatchlistPage }))
)

const LoginPage = lazy(() =>
  import('@/modules/auth/pages/LoginPage').then((m) => ({ default: m.LoginPage }))
)
const RegisterPage = lazy(() =>
  import('@/modules/auth/pages/RegisterPage').then((m) => ({ default: m.RegisterPage }))
)
const ForgotPasswordPage = lazy(() =>
  import('@/modules/auth/pages/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage }))
)
const ResetPasswordPage = lazy(() =>
  import('@/modules/auth/pages/ResetPasswordPage').then((m) => ({ default: m.ResetPasswordPage }))
)
const ConfirmEmailPage = lazy(() =>
  import('@/modules/auth/pages/ConfirmEmailPage').then((m) => ({ default: m.ConfirmEmailPage }))
)

const DashboardPage = lazy(() =>
  import('@/modules/analytics/pages/UserDashboardPage').then((m) => ({
    default: m.UserDashboardPage,
  }))
)
const ReportsPage = lazy(() =>
  import('@/modules/analytics/pages/ReportsPage').then((m) => ({ default: m.ReportsPage }))
)
const AdminDashboardPage = lazy(() =>
  import('@/modules/analytics/pages/AdminDashboardPage').then((m) => ({
    default: m.AdminDashboardPage,
  }))
)
const AuditLogsPage = lazy(() =>
  import('@/modules/analytics/pages/AuditLogsPage').then((m) => ({ default: m.AuditLogsPage }))
)
const PlatformSettingsPage = lazy(() =>
  import('@/modules/analytics/pages/PlatformSettingsPage').then((m) => ({
    default: m.PlatformSettingsPage,
  }))
)
const JobsPage = lazy(() =>
  import('@/modules/jobs/pages/JobsPage').then((m) => ({
    default: m.JobsPage,
  }))
)
const JobDetailPage = lazy(() =>
  import('@/modules/jobs/pages/JobDetailPage').then((m) => ({
    default: m.JobDetailPage,
  }))
)

const ProfilePage = lazy(() =>
  import('@/modules/users/pages/ProfilePage').then((m) => ({ default: m.ProfilePage }))
)
const SettingsPage = lazy(() =>
  import('@/modules/users/pages/SettingsPage').then((m) => ({ default: m.SettingsPage }))
)
const MyAuctionsPage = lazy(() =>
  import('@/modules/users/pages/MyAuctionsPage').then((m) => ({ default: m.MyAuctionsPage }))
)
const UsersManagementPage = lazy(() =>
  import('@/modules/users/pages/UsersManagementPage').then((m) => ({
    default: m.UsersManagementPage,
  }))
)
const RolePermissionsPage = lazy(() =>
  import('@/modules/users/pages/RolePermissionsPage').then((m) => ({
    default: m.RolePermissionsPage,
  }))
)
const SellerApplyPage = lazy(() =>
  import('@/modules/users/pages/SellerApplyPage').then((m) => ({ default: m.SellerApplyPage }))
)
const SellerProfilePage = lazy(() =>
  import('@/modules/users/pages/SellerProfilePage').then((m) => ({ default: m.SellerProfilePage }))
)
const MyReviewsPage = lazy(() =>
  import('@/modules/users/pages/MyReviewsPage').then((m) => ({ default: m.MyReviewsPage }))
)
const AuctionListingPage = lazy(() =>
  import('@/modules/auctions/pages/AuctionListingPage').then((m) => ({ default: m.AuctionListingPage }))
)

const MyBidsPage = lazy(() =>
  import('@/modules/bidding/pages/MyBidsPage').then((m) => ({ default: m.MyBidsPage }))
)
const WinningBidsPage = lazy(() =>
  import('@/modules/bidding/pages/WinningBidsPage').then((m) => ({ default: m.WinningBidsPage }))
)
const BidHistoryPage = lazy(() =>
  import('@/modules/bidding/pages/BidHistoryPage').then((m) => ({ default: m.BidHistoryPage }))
)
const AutoBidManagementPage = lazy(() =>
  import('@/modules/bidding/pages/AutoBidManagementPage').then((m) => ({
    default: m.AutoBidManagementPage,
  }))
)

const SearchPage = lazy(() =>
  import('@/modules/search/pages/SearchPage').then((m) => ({ default: m.SearchPage }))
)
const NotificationsPage = lazy(() =>
  import('@/modules/notifications/pages/NotificationsPage').then((m) => ({
    default: m.NotificationsPage,
  }))
)
const TemplatesManagementPage = lazy(() =>
  import('@/modules/notifications/pages/TemplatesManagementPage').then((m) => ({
    default: m.TemplatesManagementPage,
  }))
)
const RecordsDashboardPage = lazy(() =>
  import('@/modules/notifications/pages/RecordsDashboardPage').then((m) => ({
    default: m.RecordsDashboardPage,
  }))
)
const BroadcastNotificationPage = lazy(() =>
  import('@/modules/notifications/pages/BroadcastNotificationPage').then((m) => ({
    default: m.BroadcastNotificationPage,
  }))
)
const AllNotificationsPage = lazy(() =>
  import('@/modules/notifications/pages/AllNotificationsPage').then((m) => ({
    default: m.AllNotificationsPage,
  }))
)
const NotificationStatsPage = lazy(() =>
  import('@/modules/notifications/pages/NotificationStatsPage').then((m) => ({
    default: m.NotificationStatsPage,
  }))
)

const WalletPage = lazy(() =>
  import('@/modules/payments/pages/WalletPage').then((m) => ({ default: m.WalletPage }))
)
const OrdersPage = lazy(() =>
  import('@/modules/payments/pages/OrdersPage').then((m) => ({ default: m.OrdersPage }))
)
const OrderDetailPage = lazy(() =>
  import('@/modules/payments/pages/OrderDetailPage').then((m) => ({ default: m.OrderDetailPage }))
)
const CheckoutPage = lazy(() =>
  import('@/modules/payments/pages/CheckoutPage').then((m) => ({ default: m.CheckoutPage }))
)
const AdminOrdersPage = lazy(() =>
  import('@/modules/payments/pages/AdminOrdersPage').then((m) => ({ default: m.AdminOrdersPage }))
)
const PaymentSuccessPage = lazy(() =>
  import('@/modules/payments/pages/PaymentSuccessPage').then((m) => ({
    default: m.PaymentSuccessPage,
  }))
)
const PaymentCancelPage = lazy(() =>
  import('@/modules/payments/pages/PaymentCancelPage').then((m) => ({
    default: m.PaymentCancelPage,
  }))
)
const TransactionDetailPage = lazy(() =>
  import('@/modules/payments/pages/TransactionDetailPage').then((m) => ({
    default: m.TransactionDetailPage,
  }))
)
const PaymentMethodsPage = lazy(() =>
  import('@/modules/payments/pages/PaymentMethodsPage').then((m) => ({
    default: m.PaymentMethodsPage,
  }))
)

const CategoriesManagementPage = lazy(() =>
  import('@/modules/auctions/pages/CategoriesManagementPage').then((m) => ({
    default: m.CategoriesManagementPage,
  }))
)
const BrandsManagementPage = lazy(() =>
  import('@/modules/auctions/pages/BrandsManagementPage').then((m) => ({
    default: m.BrandsManagementPage,
  }))
)

const NotFoundPage = lazy(() =>
  import('@/shared/components/errors/NotFoundPage').then((m) => ({ default: m.NotFoundPage }))
)

const withSuspense = (Component: React.LazyExoticComponent<ComponentType<object>>) => (
  <Suspense fallback={<LoadingScreen />}>
    <Component />
  </Suspense>
)

export const router = createBrowserRouter([
  {
    element: <LandingLayout />,
    children: [
      { index: true, element: withSuspense(LandingPage) },
      { path: '/how-it-works', element: withSuspense(HowItWorksPage) },
    ],
  },
  {
    path: '/login',
    element: withSuspense(LoginPage),
  },
  {
    path: '/register',
    element: withSuspense(RegisterPage),
  },
  {
    path: '/forgot-password',
    element: withSuspense(ForgotPasswordPage),
  },
  {
    path: '/reset-password',
    element: withSuspense(ResetPasswordPage),
  },
  {
    path: '/confirm-email',
    element: withSuspense(ConfirmEmailPage),
  },
  {
    element: <MainLayout />,
    children: [
      { path: '/auctions', element: withSuspense(AuctionsPage) },
      { path: '/browse', element: withSuspense(AuctionListingPage) },
      { path: '/auctions/:id', element: withSuspense(AuctionDetailPage) },
      { path: '/sellers/:sellerId', element: withSuspense(SellerProfilePage) },
      { path: '/search', element: withSuspense(SearchPage) },
      {
        path: '/auctions/create',
        element: <ProtectedRoute>{withSuspense(AuctionFormPage)}</ProtectedRoute>,
      },
      {
        path: '/auctions/:id/edit',
        element: <ProtectedRoute>{withSuspense(AuctionFormPage)}</ProtectedRoute>,
      },
      {
        path: '/watchlist',
        element: <ProtectedRoute>{withSuspense(WatchlistPage)}</ProtectedRoute>,
      },
      {
        path: '/dashboard',
        element: <ProtectedRoute>{withSuspense(DashboardPage)}</ProtectedRoute>,
      },
      {
        path: '/reports',
        element: <ProtectedRoute>{withSuspense(ReportsPage)}</ProtectedRoute>,
      },
      {
        path: '/profile',
        element: <ProtectedRoute>{withSuspense(ProfilePage)}</ProtectedRoute>,
      },
      {
        path: '/settings',
        element: <ProtectedRoute>{withSuspense(SettingsPage)}</ProtectedRoute>,
      },
      {
        path: '/my-auctions',
        element: <ProtectedRoute>{withSuspense(MyAuctionsPage)}</ProtectedRoute>,
      },
      {
        path: '/my-reviews',
        element: <ProtectedRoute>{withSuspense(MyReviewsPage)}</ProtectedRoute>,
      },
      {
        path: '/my-bids',
        element: <ProtectedRoute>{withSuspense(MyBidsPage)}</ProtectedRoute>,
      },
      {
        path: '/winning-bids',
        element: <ProtectedRoute>{withSuspense(WinningBidsPage)}</ProtectedRoute>,
      },
      {
        path: '/bid-history',
        element: <ProtectedRoute>{withSuspense(BidHistoryPage)}</ProtectedRoute>,
      },
      {
        path: '/auto-bids',
        element: <ProtectedRoute>{withSuspense(AutoBidManagementPage)}</ProtectedRoute>,
      },
      {
        path: '/notifications',
        element: <ProtectedRoute>{withSuspense(NotificationsPage)}</ProtectedRoute>,
      },
      {
        path: '/wallet',
        element: <ProtectedRoute>{withSuspense(WalletPage)}</ProtectedRoute>,
      },
      {
        path: '/orders',
        element: <ProtectedRoute>{withSuspense(OrdersPage)}</ProtectedRoute>,
      },
      {
        path: '/orders/:orderId',
        element: <ProtectedRoute>{withSuspense(OrderDetailPage)}</ProtectedRoute>,
      },
      {
        path: '/checkout/:auctionId',
        element: <ProtectedRoute>{withSuspense(CheckoutPage)}</ProtectedRoute>,
      },
      {
        path: '/payment/success',
        element: <ProtectedRoute>{withSuspense(PaymentSuccessPage)}</ProtectedRoute>,
      },
      {
        path: '/payment/cancel',
        element: <ProtectedRoute>{withSuspense(PaymentCancelPage)}</ProtectedRoute>,
      },
      {
        path: '/wallet/transactions/:transactionId',
        element: <ProtectedRoute>{withSuspense(TransactionDetailPage)}</ProtectedRoute>,
      },
      {
        path: '/wallet/payment-methods',
        element: <ProtectedRoute>{withSuspense(PaymentMethodsPage)}</ProtectedRoute>,
      },
      {
        path: '/become-seller',
        element: <ProtectedRoute>{withSuspense(SellerApplyPage)}</ProtectedRoute>,
      },
      {
        path: '/admin/dashboard',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(AdminDashboardPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/orders',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(AdminOrdersPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/audit-logs',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(AuditLogsPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/settings',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(PlatformSettingsPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/jobs',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(JobsPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/jobs/:jobId',
        element: (
          <ProtectedRoute permissions={['admin:access']}>
            {withSuspense(JobDetailPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/users',
        element: (
          <ProtectedRoute permissions={['users:manage']}>
            {withSuspense(UsersManagementPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/roles',
        element: (
          <ProtectedRoute permissions={['users:manage-roles']}>
            {withSuspense(RolePermissionsPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/categories',
        element: (
          <ProtectedRoute permissions={['auctions:manage']}>
            {withSuspense(CategoriesManagementPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/brands',
        element: (
          <ProtectedRoute permissions={['auctions:manage']}>
            {withSuspense(BrandsManagementPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/notifications/templates',
        element: (
          <ProtectedRoute permissions={['notifications:manage']}>
            {withSuspense(TemplatesManagementPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/notifications/records',
        element: (
          <ProtectedRoute permissions={['notifications:manage']}>
            {withSuspense(RecordsDashboardPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/notifications/broadcast',
        element: (
          <ProtectedRoute permissions={['notifications:manage']}>
            {withSuspense(BroadcastNotificationPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/notifications/all',
        element: (
          <ProtectedRoute permissions={['notifications:manage']}>
            {withSuspense(AllNotificationsPage)}
          </ProtectedRoute>
        ),
      },
      {
        path: '/admin/notifications/stats',
        element: (
          <ProtectedRoute permissions={['notifications:manage']}>
            {withSuspense(NotificationStatsPage)}
          </ProtectedRoute>
        ),
      },
    ],
  },
  { path: '/404', element: withSuspense(NotFoundPage) },
  { path: '*', element: <Navigate to="/404" replace /> },
])
