import type { LeadTimeReport } from '../api.ts'

interface Props {
  report: LeadTimeReport | null
}

export function LeadTimePanel({ report }: Props) {
  if (!report) return null

  return (
    <section className="card">
      <h2>Lead time</h2>
      <p className="headline">
        {report.averageLeadTimeDays === null ? '—' : `${report.averageLeadTimeDays} days`}
        <span className="muted"> average, submission to completion ({report.completedCount} completed)</span>
      </p>
      {report.byArea.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Area</th>
              <th className="num">Completed</th>
              <th className="num">Avg days</th>
            </tr>
          </thead>
          <tbody>
            {report.byArea.map((a) => (
              <tr key={a.area}>
                <td>{a.area}</td>
                <td className="num">{a.completedCount}</td>
                <td className="num">{a.averageLeadTimeDays}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
