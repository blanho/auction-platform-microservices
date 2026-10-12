# Frontend Clean Code follow-up

This focused follow-up stays on `fix/auction-reliability-and-test-coverage` and
preserves the earlier reliability changes. The broader module review inventory
remains in [clean-code-review.md](clean-code-review.md). This is not an exhaustive
rewrite of every frontend file.

## Behavior-preserving refactors

- `web/src/modules/payments/hooks/useOrderDetailActions.ts` owns shipment form
  state, shipping/delivery mutations, clipboard effects, and user feedback.
  `OrderDetailPage.tsx` retains rendering, order lookup, and action visibility.
  Existing query hooks still own request execution and cache invalidation.
- Order detail and `MyAuctionsPage.tsx` reuse the existing `useSnackbar` hook.
  Twelve seller feedback handlers no longer repeat snackbar state construction.
  Messages, severity, dismissal, timeout, and screen position remain unchanged.
- Order detail resolves its status presentation once per render.

No service, API contract, database schema, authorization policy, retry policy,
translation, or payment integration changed. Shipment inputs still remain after
failure or dialog dismissal and clear after successful submission.

Sixteen page-level tests in `OrderDetailPage.test.tsx` passed against the original
page before extraction and after extraction. They use the real React Query hooks
and rendered controls, with API and authentication boundaries stubbed. They cover:

- Buyer/seller/admin action visibility across paid, shipped, completed, and
  cancelled orders, including case-insensitive identifiers and role names.
- Required shipment fields and disabled submission while saving.
- One request from repeated clicks while saving, followed by authoritative
  shipment data appearing without a page reload.
- Failed shipment preserving inputs and allowing retry; dismissal retaining an
  unfinished shipment.
- Delivery success/failure and accurate clipboard success/failure feedback.

These tests verify frontend action visibility; backend authorization remains
responsible for enforcing access.

## Reproduced bug and intentional fix

Creating, editing, deleting, or extending an auction invalidated public lists but
did not invalidate seller lists. With cached data considered fresh, the seller
page continued showing the previous title, membership, or ending time after a
successful mutation.

`useMyAuctions.test.tsx` exercises all seven mutations against both filtered and
unfiltered seller queries. Before the fix, eight cases failed (create, update,
delete, and extend, each with two query variants); six passed (activate,
deactivate, cancel). Cancellation was investigated and was **not** a reproduced
defect: React Query's partial object matching already made `my({})` match the
filtered variants.

`auctionKeys.myLists()` now names the seller-list prefix explicitly. All seven
mutations use it. Existing concrete query keys retain their values, and existing
public-list/detail invalidations remain intact. All fourteen cases pass with
real QueryClient instances and `staleTime: Infinity`, so periodic stale refetches
cannot conceal missing invalidation.

The pre-fix result is retained locally at
`/private/tmp/auction-frontend-seller-reproduction.log`.

## Browser coverage

`web/e2e/order-fulfillment.spec.ts` runs the actual app with disposable API fixtures.
A seller submits shipment details; the tracking number appears without a reload.
A buyer then opens that order and confirms delivery; the delivery control
disappears after authoritative state refresh. The test checks payloads, one request
per action, role-appropriate controls, and absence of browser runtime errors.

Both existing buy-now completion/failure journeys also pass. External requests
are aborted by the fixtures; no real payment provider is contacted.

## Validation results

| Check | Result |
|---|---|
| Existing baseline frontend tests | 32 passed |
| Final frontend behavioral tests | 62 passed across 7 files; zero failed/skipped |
| Chromium fixture journeys | 3 passed; zero failed/skipped |
| Prettier, ESLint, TypeScript | Passed after correcting new missing-brace lint violations |
| Production build | Passed; existing large-chunk and SignalR annotation warnings remain |
| `npm run validate` | Blocked at i18n: the checker was already deleted in the working tree |
| Last committed i18n checker against current source | Passed for 11 namespaces using an unmodified temporary copy |
| Backend suite | Not rerun for these frontend-only changes |
| Isolated full-system runner | Not rerun; its scripts were already deleted in the working tree |

Pre-existing deletions of these files were preserved and are not part of this
refactor:

- `scripts/prepare-system-compose.py`
- `scripts/run-backend-tests.sh`
- `scripts/run-system-tests.sh`
- `scripts/tests/test_prepare_system_compose.py`
- `scripts/tests/test_verify_test_results.py`
- `scripts/verify-test-results.py`
- `web/scripts/check-i18n.mjs`

They are still referenced by testing instructions and/or CI. Resolving those
deletions is separate work; this change does not remove the corresponding checks
or claim the standard validation command passes.

## Reproduce

From `web/`, with the existing dependencies installed:

```bash
npm test
npm run format:check
npm run lint
npm run typecheck
npm run validate
npm run build
PLAYWRIGHT_BROWSERS_PATH=/private/tmp/auction-playwright npm run test:e2e
```

The browser command uses the Chromium installation retained from the earlier
review. On another machine, run `npx playwright install chromium` and omit
`PLAYWRIGHT_BROWSERS_PATH` unless using a custom installation directory.

Until the deleted checker is restored or its removal resolved, reproduce the
independent i18n check without changing the working tree:

```bash
check_root=$(mktemp -d /private/tmp/auction-i18n-review.XXXXXX)
mkdir "$check_root/scripts"
ln -s "$PWD/src" "$check_root/src"
ln -s "$PWD/node_modules" "$check_root/node_modules"
git show HEAD:web/scripts/check-i18n.mjs > "$check_root/scripts/check-i18n.mjs"
node "$check_root/scripts/check-i18n.mjs"
```

Validation logs are under `/private/tmp/auction-frontend-clean-code-` with suffixes
`tests.log`, `validate.log`, `build.log`, and `browser.log`.

## Remaining work

Large seller, authentication, and analytics pages can receive further cohesive
component extraction when accompanied by relevant behavioral coverage. This
phase does not force arbitrary file-size limits or add generic action wrappers.
Browser fixtures do not prove cross-service behavior, real shipping integrations,
Stripe behavior, or production readiness. Existing dependency audit findings and
bundle-size warnings were not addressed by this refactor.
