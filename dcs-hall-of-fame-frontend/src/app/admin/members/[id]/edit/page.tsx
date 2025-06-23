'use client'

import { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { useSession } from 'next-auth/react'
import AdminProtected from '@/components/AdminProtected'
import MemberForm from '@/components/MemberForm'
import DeleteConfirmationModal from '@/components/DeleteConfirmationModal'
import SuccessNotification from '@/components/SuccessNotification'
import { HallOfFameMember } from '@/types/member'
import { apiService } from '@/services/api'
import Link from 'next/link'

export default function EditMemberPage() {
  const { data: session } = useSession()
  const router = useRouter()
  const params = useParams()
  const memberId = params?.id as string

  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [member, setMember] = useState<HallOfFameMember | null>(null)
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [successMessage, setSuccessMessage] = useState('')

  // Load member data
  useEffect(() => {
    const loadMember = async () => {
      setLoading(true)
      setError('')

      try {
        const response = await apiService.getMemberById(memberId)

        if (response.error) {
          setError(response.error)
        } else if (response.data) {
          setMember(response.data)
        }
      } catch {
        console.error('Failed to load member data')
      } finally {
        setLoading(false)
      }
    }

    if (memberId) {
      loadMember()
    }
  }, [memberId])

  const handleSubmit = async (memberData: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'>) => {
    setSaving(true)
    setError('')

    try {
      const response = await apiService.updateMember(memberId, memberData)

      if (response.error) {
        setError(response.error)
      } else {
        setSuccessMessage(`${memberData.name} has been successfully updated!`)
        setShowSuccess(true)
        // Redirect after a short delay to show the success message
        setTimeout(() => {
          router.push('/admin')
        }, 1500)
      }
    } catch {
      console.error('Failed to update member')
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = () => {
    router.push('/admin')
  }

  const handleDeleteClick = () => {
    setShowDeleteModal(true)
  }

  const handleDeleteConfirm = async () => {
    setSaving(true)
    setError('')

    try {
      const response = await apiService.deleteMember(memberId)

      if (response.error) {
        setError(response.error)
      } else {
        setSuccessMessage(`${member?.name} has been successfully deleted!`)
        setShowSuccess(true)
        // Redirect after a short delay to show the success message
        setTimeout(() => {
          router.push('/admin')
        }, 1500)
      }
    } catch {
      console.error('Failed to delete member')
    } finally {
      setSaving(false)
      setShowDeleteModal(false)
    }
  }

  const handleDeleteCancel = () => {
    setShowDeleteModal(false)
  }

  if (loading) {
    return (
      <AdminProtected>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <p className="mt-2 text-gray-600">Loading member data...</p>
          </div>
        </div>
      </AdminProtected>
    )
  }

  if (error && !member) {
    return (
      <AdminProtected>
        <div className="min-h-screen bg-gray-50">
          <div className="container mx-auto px-4 py-8">
            <div className="max-w-4xl mx-auto">
              <div className="bg-red-50 border border-red-200 rounded-lg p-6">
                <h2 className="text-lg font-semibold text-red-800 mb-2">Error Loading Member</h2>
                <p className="text-red-600 mb-4">{error}</p>
                <Link
                  href="/admin"
                  className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition-colors"
                >
                  Back to Admin
                </Link>
              </div>
            </div>
          </div>
        </div>
      </AdminProtected>
    )
  }

  return (
    <AdminProtected>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <header className="bg-blue-900 text-white shadow-lg">
          <div className="container mx-auto px-4 py-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-bold">Edit Member</h1>
                <p className="text-blue-200">Update Hall of Fame member information</p>
                {member && (
                  <p className="text-blue-200 text-sm mt-1">Editing: {member.name}</p>
                )}
              </div>
              <div className="flex items-center space-x-4">
                <div className="text-right">
                  <p className="text-sm text-blue-200">Signed in as</p>
                  <p className="font-medium">{session?.user?.name}</p>
                </div>
                <button
                  onClick={handleDeleteClick}
                  disabled={saving}
                  className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Delete Member
                </button>
                <Link
                  href="/admin"
                  className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700 transition-colors"
                >
                  Back to Admin
                </Link>
              </div>
            </div>
          </div>
        </header>

        {/* Main Content */}
        <main className="container mx-auto px-4 py-8">
          <div className="max-w-4xl mx-auto">
            {member && (
              <MemberForm
                initialData={member}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                loading={saving}
                error={error}
                submitLabel="Save Changes"
                title="Edit Member"
                subtitle={`Update information for ${member.name}`}
              />
            )}
          </div>
        </main>

        {/* Delete Confirmation Modal */}
        <DeleteConfirmationModal
          isOpen={showDeleteModal}
          onClose={handleDeleteCancel}
          onConfirm={handleDeleteConfirm}
          title="Delete Member"
          message={`Are you sure you want to delete ${member?.name}? This action cannot be undone.`}
          confirmLabel="Delete Member"
          loading={saving}
        />

        {/* Success Notification */}
        <SuccessNotification
          isVisible={showSuccess}
          message={successMessage}
          onClose={() => setShowSuccess(false)}
        />
      </div>
    </AdminProtected>
  )
}