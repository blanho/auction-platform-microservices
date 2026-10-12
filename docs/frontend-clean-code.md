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

## Research-guided continuation

The next pass consulted published Clean Code previews and official React guidance.
The full book was not available in the workspace or through the public previews;
this review does not claim to have read it in full. The relevant sources and their
application are:

| Source | Applied principle and concrete change |
|---|---|
| [Clean Code: Meaningful Names](https://www.oreilly.com/library/view/clean-code-a/9780136083238/chapter02.xhtml) | Seller selection variables now name the auction ID or auction being acted on, rather than calling IDs dialogs. |
| [Clean Code: Functions](https://www.oreilly.com/library/view/clean-code-a/9780136083238/chapter03.xhtml) | Seller confirmation rendering, form state, and request handling form one cohesive component. Guards reduce nesting; repeated dismissal/reset logic has named handlers. |
| [React: Choosing the State Structure](https://react.dev/learn/choosing-the-state-structure) | Cancellation reason and extension input belong to the confirmation workflow, while the seller page retains selection and list state. |
| [React: You Might Not Need an Effect](https://react.dev/learn/you-might-not-need-an-effect) | Order identity is expressed with a React key to reset the content subtree, avoiding a synchronization effect that first renders with the previous order's state. |

### Behavior-preserving seller refactor

`web/src/modules/users/components/dialogs/AuctionManagementDialogs.tsx` now owns
the delete, cancel, and extend confirmation forms and their existing mutation
hooks. `MyAuctionsPage.tsx` retains list queries, filters, selection, activation,
deactivation, and page feedback. Its size decreased from 992 to 776 lines because
an existing responsibility moved, rather than through layout compression.

The dialog component remains mounted after dismissal. This preserves feedback
from pending requests that finish after the user closes a dialog. It also retains
the existing independent close callbacks, labels, styling, request payloads,
failure behavior, and dismissal rules. No generic confirmation framework or new
dependency was introduced.

Nine tests in `MyAuctionsPage.test.tsx` passed before and after extraction. They
cover all three actions succeeding/failing, input retention on failure, reset on
dismissal, and a pending cancellation completing after dismissal. They render
the real seller page and mutation/query hooks; unrelated import/export dialogs
are stubbed.

### Reproduced order navigation bugs

Two new tests in `OrderDetailPage.test.tsx` failed before the fix:

1. Dismissing an unfinished shipment for order A and navigating within the SPA to
   order B retained A's carrier/tracking fields in B's shipment form.
2. A pending shipment for A that completed after navigating to B cleared B's
   current form and could close it or display feedback for A on B's page.

`OrderDetailPage.tsx` now resolves the route ID and renders `OrderDetailContent`
with that ID as its key. The order-specific actions, dialog, and feedback state
therefore have the same lifetime as the viewed order. Both regression tests and
the sixteen existing order tests pass. The late-completion test also verifies
that the original request still targets A exactly once with its original payload.
Navigation does not cancel a request or undo server work.

The reproduction result is retained locally at
`/private/tmp/auction-frontend-order-navigation-reproduction.log`.

### Updated validation

- Baseline: 62 frontend tests passed.
- Final: 73 frontend tests across 8 files and all 3 Chromium fixture journeys
  passed, with zero failures or skips.
- Formatting, ESLint, TypeScript, and production build passed. Existing bundle
  size and SignalR annotation warnings remain.
- `npm run validate` still stops at the already deleted i18n checker. The
  unmodified committed checker passed for all 11 namespaces from a temporary
  directory against the current source, using the command documented above.
- The seven pre-existing script deletions remain outside this work. Backend and
  isolated full-system suites were not rerun for these frontend changes.

Run the same validation commands in the Reproduce section above. For focused
checks, run from `web/`:

```bash
npx vitest run src/modules/users/pages/MyAuctionsPage.test.tsx src/modules/payments/pages/OrderDetailPage.test.tsx
```

Logs for this continuation use the prefix `/private/tmp/auction-frontend-react-`
and suffixes `baseline.log`, `focused.log`, `tests.log`, `validate.log`,
`build.log`, and `browser.log`. Seller pre-extraction evidence is in
`/private/tmp/auction-frontend-seller-characterization.log`.

Additional inspection covered auth callback/registration effects, import/export
dialogs, two-factor setup, file-upload cleanup, and search filter/debounce logic.
Those areas were left unchanged. Search URL/state synchronization and overlapping
auth callback effects merit their own regression coverage before refactoring.
