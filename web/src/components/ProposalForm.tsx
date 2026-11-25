import { useState, type FormEvent } from 'react'
import { api, errorMessage } from '../api.ts'

const AREAS = ['Assembly', 'Body Weld', 'Paint', 'Logistics']

interface Props {
  onCreated: () => void
}

export function ProposalForm({ onCreated }: Props) {
  const [title, setTitle] = useState('')
  const [area, setArea] = useState('')
  const [description, setDescription] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(null)
    try {
      await api.createProposal({ title, area, description })
      setTitle('')
      setArea('')
      setDescription('')
      onCreated()
    } catch (e) {
      setError(errorMessage(e)) // e.g. the API's validation message
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="card form" onSubmit={handleSubmit}>
      <h2>Submit a proposal</h2>
      <label>
        Title
        <input value={title} onChange={(e) => setTitle(e.target.value)} maxLength={200} required />
      </label>
      <label>
        Area
        <input value={area} onChange={(e) => setArea(e.target.value)} list="areas" maxLength={100} required />
        <datalist id="areas">
          {AREAS.map((a) => (
            <option key={a} value={a} />
          ))}
        </datalist>
      </label>
      <label>
        What's the problem, and what do you propose?
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} required />
      </label>
      {error && <p className="error">{error}</p>}
      <button type="submit" disabled={saving}>
        {saving ? 'Submitting…' : 'Submit'}
      </button>
    </form>
  )
}
