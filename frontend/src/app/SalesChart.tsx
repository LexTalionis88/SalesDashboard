import {
  Bar,
  CartesianGrid,
  ComposedChart,
  Line,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts'
import type { Dashboard } from '../shared/api/types'

type SalesChartProps = { readonly series: Dashboard['series'] }

const formatMoney = (value: string) => `${Number(value).toLocaleString('ru-RU', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2
})} ₽`

export default function SalesChart({ series }: SalesChartProps) {
  const chart = series.map(day => ({
    date: new Date(`${day.date}T00:00:00`).toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: 'short'
    }),
    revenue: Number(day.revenue),
    profit: Number(day.grossProfit),
    count: day.salesCount
  }))

  return <div
    className="chart"
    role="img"
    aria-label="Ежедневная выручка, валовая прибыль и число продаж"
  >
    <ResponsiveContainer width="100%" height="100%">
      <ComposedChart data={chart}>
        <CartesianGrid vertical={false} stroke="#eeedf4" />
        <XAxis
          dataKey="date"
          minTickGap={24}
          tick={{ fill: '#9ca3af', fontSize: 10 }}
        />
        <YAxis yAxisId="money" tick={{ fill: '#9ca3af', fontSize: 10 }} width={45} />
        <YAxis
          yAxisId="count"
          orientation="right"
          tick={{ fill: '#9ca3af', fontSize: 10 }}
          width={30}
        />
        <Tooltip
          formatter={(value, name) => name === 'Продажи'
            ? String(value)
            : formatMoney(String(value))}
        />
        <Bar
          yAxisId="money"
          dataKey="revenue"
          name="Выручка"
          fill="#8978e5"
          radius={[3, 3, 0, 0]}
        />
        <Bar
          yAxisId="money"
          dataKey="profit"
          name="Прибыль"
          fill="#f2b26f"
          radius={[3, 3, 0, 0]}
        />
        <Line
          yAxisId="count"
          dataKey="count"
          name="Продажи"
          stroke="#4bb99a"
          dot={false}
          strokeWidth={2}
        />
      </ComposedChart>
    </ResponsiveContainer>
  </div>
}
