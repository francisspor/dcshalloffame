'use client'

import { useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { useSession } from 'next-auth/react'
import AdminProtected from '@/components/AdminProtected'
import MemberForm from '@/components/MemberForm'
import SuccessNotification from '@/components/SuccessNotification'
import { MemberCategory, HallOfFameMember } from '@/types/member'
import { apiService } from '@/services/api'
import Link from 'next/link'

export default function AddMemberPage() {
  const { data: session } = useSession()
  const router = useRouter()
  const searchParams = useSearchParams()
  const defaultCategory = searchParams.get('category') === 'alumni' ? MemberCategory.Alumni : MemberCategory.Staff

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [showSuccess, setShowSuccess] = useState(false)
  const [successMessage, setSuccessMessage] = useState('')

  const handleSubmit = async (memberData: Omit<HallOfFameMember, 'id' | 'createdAt' | 'updatedAt'>) => {
    setLoading(true)
    setError('')

    try {
      const response = await apiService.createMember(memberData)

      if (response.error) {
        setError(response.error)
      } else {
        setSuccessMessage(`${memberData.name} has been successfully added to the Hall of Fame!`)
        setShowSuccess(true)
        // Redirect after a short delay to show the success message
        setTimeout(() => {
          router.push('/admin')
        }, 1500)
      }
    } catch (err) {
      setError('An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    router.push('/admin')
  }

  return (
    <AdminProtected>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <header className="bg-blue-900 text-white shadow-lg">
          <div className="container mx-auto px-4 py-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-bold">Add New Member</h1>
                <p className="text-blue-200">Create a new Hall of Fame member</p>
              </div>
              <div className="flex items-center space-x-4">
                <div className="text-right">
                  <p className="text-sm text-blue-200">Signed in as</p>
                  <p className="font-medium">{session?.user?.name}</p>
                </div>
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
            <MemberForm
              onSubmit={handleSubmit}
              onCancel={handleCancel}
              loading={loading}
              error={error}
              submitLabel="Create Member"
              title="Add New Member"
              subtitle="Fill in the details below to create a new Hall of Fame member"
            />
          </div>
        </main>

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