import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Bar, CartesianGrid, ComposedChart, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../shared/api/client'
import type { Dashboard, Sale, SalesResponse } from '../shared/api/types'

type PeriodChoice = 'today' | 'last7Days' | 'last30Days' | 'thisMonth' | 'lastMonth' | 'custom'
type Ranking = 'grossProfit' | 'averageCheck'
type LoadState<T> = { value: T | null; loading: boolean; error: string }
const formatMoney = (value: string | null) => value === null ? '—' : `${Number(value).toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₽`
const formatDate = (value: string) => new Date(`${value}T00:00:00`).toLocaleDateString('ru-RU', { day: '2-digit', month: 'short' })
const formatChange = (value: number | null) => value === null ? '—' : `${value > 0 ? '+' : ''}${value.toFixed(1)}%`
const isoDate = (date: Date) => [date.getFullYear(), String(date.getMonth() + 1).padStart(2, '0'), String(date.getDate()).padStart(2, '0')].join('-')
const businessToday = () => new Date(new Date().toLocaleString('en-US', { timeZone: 'Europe/Moscow' }))

function periodDates(choice: PeriodChoice): [string, string] {
  const today = businessToday()
  const from = new Date(today)
  const to = new Date(today)
  if (choice === 'last7Days') from.setDate(today.getDate() - 6)
  if (choice === 'last30Days') from.setDate(today.getDate() - 29)
  if (choice === 'thisMonth') from.setDate(1)
  if (choice === 'lastMonth') {
    from.setMonth(today.getMonth() - 1, 1)
    to.setDate(0)
  }
  return [isoDate(from), isoDate(to)]
}

export function App() {
  const [initial] = useState(() => periodDates('last30Days'))
  const [from, setFrom] = useState(initial[0])
  const [to, setTo] = useState(initial[1])
  const [choice, setChoice] = useState<PeriodChoice>('last30Days')
  const [rankingBy, setRankingBy] = useState<Ranking>('grossProfit')
  const valid = Boolean(from && to && from <= to)
  const dashboardQuery = useQuery({ queryKey: ['dashboard', from, to, rankingBy], queryFn: ({ signal }) => api.dashboard(from, to, rankingBy, signal), enabled: valid })
  const salesQuery = useQuery({ queryKey: ['sales', from, to], queryFn: ({ signal }) => api.sales(from, to, signal), enabled: valid })
  const dashboard: LoadState<Dashboard> = { value: dashboardQuery.data ?? null, loading: dashboardQuery.isPending, error: dashboardQuery.error?.message ?? '' }
  const sales: LoadState<SalesResponse> = { value: salesQuery.data ?? null, loading: salesQuery.isPending, error: salesQuery.error?.message ?? '' }

  function choosePeriod(next: PeriodChoice) {
    setChoice(next)
    if (next !== 'custom') {
      const [start, end] = periodDates(next)
      setFrom(start)
      setTo(end)
    }
  }

  const invalid = from && to && from > to
  return <div className="shell">
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark">S</span><span>Sales<span>Lab</span></span></div>
      <nav aria-label="Навигация"><span className="active">Обзор <b>⌂</b></span></nav>
      <div className="sidebar-foot"><div className="avatar">SL</div><div><strong>Sales Lab</strong><small>Аналитика продаж</small></div></div>
    </aside>
    <main>
      <header><div><p className="eyebrow">SALES PERFORMANCE</p><h1>Обзор продаж <span>✦</span></h1><p className="muted">Показатели команды за выбранный период</p></div></header>
      <section className="toolbar" aria-label="Период отчёта">
        <div className="period-tabs">
          {([['today', 'Сегодня'], ['last7Days', '7 дней'], ['last30Days', '30 дней'], ['thisMonth', 'Этот месяц'], ['lastMonth', 'Прошлый месяц'], ['custom', 'Период']] as const).map(([key, label]) =>
            <button key={key} className={choice === key ? 'selected' : ''} onClick={() => choosePeriod(key)}>{label}</button>)}
        </div>
        <div className="date-inputs">
          <input aria-label="Дата начала" type="date" value={from} onChange={event => { setFrom(event.target.value); setChoice('custom') }} />
          <span>→</span>
          <input aria-label="Дата окончания" type="date" value={to} onChange={event => { setTo(event.target.value); setChoice('custom') }} />
        </div>
      </section>
      {invalid ? <div className="error">Дата окончания должна быть не раньше даты начала.</div> :
        <div aria-busy={dashboard.loading}>
          {dashboard.loading && <div className="loading">Загружаем показатели…</div>}
          {dashboard.error && <ErrorBlock message={dashboard.error} retry={() => dashboardQuery.refetch()} />}
          {dashboard.value && <DashboardView data={dashboard.value} rankingBy={rankingBy} setRankingBy={setRankingBy} />}
          <SalesView state={sales} retry={() => salesQuery.refetch()} />
        </div>}
    </main>
  </div>
}

function ErrorBlock({ message, retry }: { message: string; retry: () => void }) {
  return <div className="error" role="alert">{message} <button onClick={retry}>Повторить</button></div>
}

function Kpi({ title, value, change, tone, icon, changeUnit = '%' }: { title: string; value: string; change: number | null; tone: string; icon: string; changeUnit?: '%' | 'п.п.' }) {
  return <article className={`card kpi ${tone}`}>
    <div className="kpi-top"><span>{title}</span><b>{icon}</b></div>
    <strong>{value}</strong>
    <small className={change !== null && change > 0 ? 'up' : ''}>{changeUnit === '%' ? formatChange(change) : change === null ? '—' : `${change > 0 ? '+' : ''}${change.toFixed(1)} п.п.`} <em>к прошлому периоду</em></small>
  </article>
}

function DashboardView({ data, rankingBy, setRankingBy }: { data: Dashboard; rankingBy: Ranking; setRankingBy: (value: Ranking) => void }) {
  const { kpis, comparison } = data
  const chart = data.series.map(day => ({ date: formatDate(day.date), revenue: Number(day.revenue), profit: Number(day.grossProfit), count: day.salesCount }))
  const leader = data.ranking.find(row => row.manager.id === kpis.bestManager?.id)
  return <>
    <section className="kpis" aria-label="Ключевые показатели">
      <Kpi title="Выручка" value={formatMoney(kpis.revenue)} change={comparison.change.revenuePercent} tone="violet" icon="◈" />
      <Kpi title="Валовая прибыль" value={formatMoney(kpis.grossProfit)} change={comparison.change.grossProfitPercent} tone="orange" icon="↗" />
      <Kpi title="Продажи" value={kpis.salesCount.toLocaleString('ru-RU')} change={comparison.change.salesCountPercent} tone="blue" icon="▦" />
      <Kpi title="Средний чек" value={formatMoney(kpis.averageCheck)} change={comparison.change.averageCheckPercent} tone="green" icon="◎" />
      <Kpi title="Маржинальность" value={kpis.margin === null ? '—' : `${(kpis.margin * 100).toFixed(1)}%`} change={comparison.change.marginPoints} tone="violet" icon="◉" changeUnit="п.п." />
    </section>
    {kpis.salesCount === 0 && <p className="empty-note">За этот период оплаченных продаж нет. История ниже может содержать отменённые и возвращённые продажи.</p>}
    <section className="grid-main">
      <article className="card chart-card">
        <div className="card-head"><div><h2>Динамика продаж</h2><p className="muted">Выручка, прибыль и количество по дням</p></div></div>
        <div className="chart" role="img" aria-label="Ежедневная выручка, валовая прибыль и число продаж">
          <ResponsiveContainer width="100%" height="100%">
            <ComposedChart data={chart}><CartesianGrid vertical={false} stroke="#eeedf4" /><XAxis dataKey="date" minTickGap={24} tick={{ fill: '#9ca3af', fontSize: 10 }} /><YAxis yAxisId="money" tick={{ fill: '#9ca3af', fontSize: 10 }} width={45} /><YAxis yAxisId="count" orientation="right" tick={{ fill: '#9ca3af', fontSize: 10 }} width={30} /><Tooltip formatter={(value, name) => name === 'Продажи' ? String(value) : formatMoney(String(value))} /><Bar yAxisId="money" dataKey="revenue" name="Выручка" fill="#8978e5" radius={[3, 3, 0, 0]} /><Bar yAxisId="money" dataKey="profit" name="Прибыль" fill="#f2b26f" radius={[3, 3, 0, 0]} /><Line yAxisId="count" dataKey="count" name="Продажи" stroke="#4bb99a" dot={false} strokeWidth={2} /></ComposedChart>
          </ResponsiveContainer>
        </div>
      </article>
      <article className="card best-card">
        <p className="eyebrow">ЛИДЕР ПЕРИОДА</p><div className="best-avatar">{kpis.bestManager?.initials ?? '—'}</div>
        <h2>{kpis.bestManager?.name ?? 'Нет данных'}</h2><p className="muted">{kpis.bestManager?.team ?? '—'}</p>
        <div className="best-metric"><span>Валовая прибыль</span><strong>{formatMoney(leader?.metrics.grossProfit ?? null)}</strong></div>
      </article>
    </section>
    <section className="grid-two">
      <article className="card">
        <div className="card-head"><div><h2>Рейтинг менеджеров</h2><p className="muted">Сортировка по <select aria-label="Сортировка рейтинга" value={rankingBy} onChange={event => setRankingBy(event.target.value as Ranking)}><option value="grossProfit">валовой прибыли</option><option value="averageCheck">среднему чеку</option></select></p></div><span className="count">{data.ranking.length} чел.</span></div>
        <div className="ranking">{data.ranking.map(row => <div className="rank-row" key={row.manager.id}><span className="rank">{row.rank ?? '—'}</span><div className="mini-avatar">{row.manager.initials}</div><div className="person"><strong>{row.manager.name}</strong><small>{row.manager.team}</small></div><strong className="rank-value">{row.rank === null ? 'Нет продаж' : formatMoney(rankingBy === 'grossProfit' ? row.metrics.grossProfit : row.metrics.averageCheck)}</strong></div>)}</div>
      </article>
      <article className="card">
        <div className="card-head"><div><h2>Категории</h2><p className="muted">Вклад в выручку</p></div></div>
        {data.categories.length ? <div className="categories">{data.categories.map(category => <div className="category" key={category.id}><div><span>{category.name}</span><strong>{formatMoney(category.revenue)}</strong></div><div className="progress"><i style={{ width: `${Math.min(100, Number(category.revenue) / Math.max(Number(kpis.revenue), 1) * 100)}%` }} /></div></div>)}</div> : <p className="muted">Категорий с продажами нет.</p>}
      </article>
    </section>
    <section className="card sales-card"><div className="card-head"><div><h2>Товары с наибольшей прибылью</h2><p className="muted">Пять лидеров периода</p></div></div>
      {data.topProducts.length ? <div className="ranking">{data.topProducts.map((product, index) => <div className="rank-row" key={product.id}><span className="rank">{index + 1}</span><div className="person"><strong>{product.name}</strong><small>{product.quantity} шт.</small></div><strong className="rank-value">{formatMoney(product.grossProfit)}</strong></div>)}</div> : <p className="muted">Товаров с продажами нет.</p>}
    </section>
  </>
}

function SalesView({ state, retry }: { state: LoadState<SalesResponse>; retry: () => void }) {
  return <section className="card sales-card">
    <div className="card-head"><div><h2>Последние продажи</h2><p className="muted">Все статусы за выбранный период</p></div></div>
    {state.loading && <p className="muted">Загружаем продажи…</p>}
    {state.error && <ErrorBlock message={state.error} retry={retry} />}
    {state.value && (state.value.items.length ? <div className="sales-table">
      <div className="table-row table-head"><span>ДАТА</span><span>КЛИЕНТ / ТОВАРЫ</span><span>МЕНЕДЖЕР</span><span>СУММА</span><span>ПРИБЫЛЬ</span><span>СТАТУС</span></div>
      {state.value.items.map(sale => <SaleRow key={sale.id} sale={sale} />)}
    </div> : <p className="muted">Продаж за этот период нет.</p>)}
  </section>
}

function SaleRow({ sale }: { sale: Sale }) {
  const status = sale.status === 'Paid' ? 'Оплачена' : sale.status === 'Refunded' ? 'Возврат' : 'Отменена'
  return <div className="table-row">
    <span>{new Date(sale.soldAt).toLocaleDateString('ru-RU', { timeZone: 'Europe/Moscow' })}</span>
    <span><strong>{sale.customer.company}</strong><small>{sale.items.map(item => `${item.productName} × ${item.quantity}`).join(', ')}</small></span>
    <span className="manager-cell"><i className="mini-avatar">{sale.manager.initials}</i>{sale.manager.name}</span>
    <strong>{formatMoney(sale.amount)}</strong><strong>{formatMoney(sale.grossProfit)}</strong><span className={`status ${sale.status.toLowerCase()}`}>{status}</span>
  </div>
}
