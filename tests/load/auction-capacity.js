import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { Counter, Rate } from 'k6/metrics';

const base = (__ENV.BASE_URL || 'http://localhost:3000/api').replace(/\/$/, '');
const mode = __ENV.MODE || 'browse';
const accepted = new Counter('accepted_bids');
const rejected = new Counter('business_rejected_bids');
const unexpected = new Rate('unexpected_responses');
const runId = __ENV.RUN_ID || String(Date.now());
const tokens = (__ENV.BIDDER_TOKENS || '').split(',').filter(Boolean);

if (!['browse', 'hot-auction'].includes(mode)) throw new Error('MODE must be browse or hot-auction');
if (mode === 'hot-auction' && (__ENV.ALLOW_BID_WRITES !== 'true' || !__ENV.AUCTION_ID || !tokens.length)) {
  throw new Error('Hot-auction requires ALLOW_BID_WRITES=true, AUCTION_ID and BIDDER_TOKENS');
}

export const options = {
  scenarios: mode === 'browse'
    ? { browse: { executor: 'constant-vus', vus: Number(__ENV.VUS || 10), duration: __ENV.DURATION || '2m' } }
    : { bids: {
        executor: 'constant-arrival-rate', rate: Number(__ENV.BIDS_PER_SECOND || 5),
        timeUnit: '1s', duration: __ENV.DURATION || '2m',
        preAllocatedVUs: 20, maxVUs: 100,
      } },
  thresholds: {
    unexpected_responses: ['rate<0.01'],
    http_req_duration: [`p(95)<${__ENV.P95_MS || 500}`],
    checks: ['rate>0.99'],
    ...(mode === 'hot-auction' ? { accepted_bids: ['count>0'], dropped_iterations: ['count==0'] } : {}),
  },
};

export default function () {
  if (mode === 'browse') {
    const page = (__VU % 5) + 1;
    const response = http.get(`${base}/auctions?status=Live&page=${page}&pageSize=20`, {
      tags: { name: 'browse_live' }, timeout: '10s',
    });
    unexpected.add(response.status !== 200);
    check(response, { 'browse succeeded': r => r.status === 200 });
    sleep(1);
    return;
  }

  const iteration = exec.scenario.iterationInTest;
  const response = http.post(`${base}/bids`, JSON.stringify({
    auctionId: __ENV.AUCTION_ID,
    amount: Number(__ENV.START_AMOUNT || 100) + (iteration + 1) * Number(__ENV.BID_INCREMENT || 10),
  }), {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${tokens[(__VU - 1) % tokens.length]}`,
      'Idempotency-Key': `load-${runId}-${iteration}`,
    },
    tags: { name: 'place_bid' }, timeout: '10s',
    responseCallback: http.expectedStatuses(201, 400, 409),
  });
  if (response.status === 201) accepted.add(1);
  if (response.status === 400 || response.status === 409) rejected.add(1);
  const expected = [201, 400, 409].includes(response.status);
  unexpected.add(!expected);
  check(response, { 'bid returned success or business rejection': () => expected });
}
