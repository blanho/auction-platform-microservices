import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'
import vm from 'node:vm'

const source = readFileSync(new URL('../../Search.Infrastructure/Services/BidStateScript.cs', import.meta.url), 'utf8').split('"""')[1]
const initial = () => ({ status: 'Live', currentPrice: 100, reservePrice: 100 })
function apply(document, ticks, price, isRetraction = false) {
  const ctx = { _source: document }
  const params = { ticks: BigInt(ticks), price: price ?? 0, hasPrice: price !== null, isRetraction, soldStatus: 'Sold', finishedStatus: 'Finished', syncedAt: 'now' }
  vm.runInNewContext(source, { ctx, params })
  return ctx.op
}

test('older and duplicate bids cannot overwrite a newer price', () => {
  const doc = initial()
  apply(doc, 200, 300)
  assert.equal(apply(doc, 100, 200), 'noop')
  assert.equal(apply(doc, 200, 300), 'noop')
  assert.equal(doc.currentPrice, 300)
})
test('last-bid retraction restores reserve price and blocks delayed higher bids', () => {
  const doc = initial()
  apply(doc, 100, 300)
  apply(doc, 200, null, true)
  assert.equal(doc.currentPrice, 100)
  assert.equal(apply(doc, 100, 300), 'noop')
  assert.equal(apply(doc, 200, null, true), 'noop')
})
test('a later bid may raise the price after a retraction', () => {
  const doc = initial()
  apply(doc, 100, 300)
  apply(doc, 200, 200, true)
  apply(doc, 300, 250)
  assert.equal(doc.currentPrice, 250)
})
test('an older retraction cannot undo a later accepted bid', () => {
  const doc = initial()
  apply(doc, 300, 500)
  assert.equal(apply(doc, 200, 200, true), 'noop')
  assert.equal(doc.currentPrice, 500)
})
test('retraction wins a timestamp tie regardless of delivery order', () => {
  for (const reverse of [false, true]) {
    const doc = initial()
    if (reverse) { apply(doc, 100, 200, true); apply(doc, 100, 300) }
    else { apply(doc, 100, 300); apply(doc, 100, 200, true) }
    assert.equal(doc.currentPrice, 200)
  }
})
test('higher accepted bid wins a timestamp tie', () => {
  const doc = initial()
  apply(doc, 100, 300)
  apply(doc, 100, 400)
  apply(doc, 100, 350)
  assert.equal(doc.currentPrice, 400)
})
test('late bids do not change finalized auction prices', () => {
  for (const status of ['Sold', 'Finished']) {
    const doc = { ...initial(), status, currentPrice: 500 }
    assert.equal(apply(doc, 999, 1000), 'noop')
    assert.equal(apply(doc, 1000, null, true), 'noop')
    assert.equal(doc.currentPrice, 500)
  }
})
test('ordering retains 100-nanosecond tick precision', () => {
  const doc = initial()
  apply(doc, 639000000000000002n, 400)
  assert.equal(apply(doc, 639000000000000001n, 300), 'noop')
  assert.equal(doc.currentPrice, 400)
})
