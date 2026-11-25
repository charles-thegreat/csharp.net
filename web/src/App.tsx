import { useEffect, useState } from 'react'
import { api, errorMessage, statusLabel, STATUSES, type LeadTimeReport, type Proposal, type ProposalStatus, type Workflow } from './api.ts'
import { LeadTimePanel } from './components/LeadTimePanel.tsx'
import { ProposalForm } from './components/ProposalForm.tsx'
import { ProposalList } from './components/ProposalList.tsx'

export default function App() {
  const [proposals, setProposals] = useState<Proposal[]>([])
  const [leadTime, setLeadTime] = useState<LeadTimeReport | null>(null)
  const [workflow, setWorkflow] = useState<Workflow | null>(null)
  const [statusFilter, setStatusFilter] = useState<ProposalStatus | ''>('')
  const [error, setError] = useState<string | null>(null)
  // Bumped after any change to tell the effect below to refetch.
  const [version, setVersion] = useState(0)
  const reload = () => setVersion((v) => v + 1)

  // The transition rules never change while the page is open, so fetch them once.
  useEffect(() => {
    api.getWorkflow().then(setWorkflow).catch((e) => setError(errorMessage(e)))
  }, [])

  useEffect(() => {
    // If the filter changes again before this request returns, ignore the stale response.
    let ignore = false
    Promise.all([api.listProposals(statusFilter || undefined), api.getLeadTime()])
      .then(([list, report]) => {
        if (ignore) return
        setProposals(list)
        setLeadTime(report)
        setError(null)
      })
      .catch((e) => {
        if (!ignore) setError(errorMessage(e))
      })
    return () => {
      ignore = true
    }
  }, [statusFilter, version])

  return (
    <main>
      <header>
        <h1>Kaizen Tracker</h1>
        <p className="muted">Submit improvement proposals and track them to completion.</p>
      </header>

      {error && (
        <p className="error" role="alert">
          {error} <button type="button" onClick={() => setError(null)}>Dismiss</button>
        </p>
      )}

      <div className="columns">
        <ProposalForm onCreated={reload} />
        <LeadTimePanel report={leadTime} />
      </div>

      <section className="card">
        <div className="toolbar">
          <h2>Proposals</h2>
          <label>
            Status{' '}
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as ProposalStatus | '')}>
              <option value="">All</option>
              {STATUSES.map((s) => (
                <option key={s} value={s}>
                  {statusLabel(s)}
                </option>
              ))}
            </select>
          </label>
        </div>
        <ProposalList proposals={proposals} workflow={workflow} onChanged={reload} onError={setError} />
      </section>
    </main>
  )
}
