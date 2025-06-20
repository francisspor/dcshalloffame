'use client'

import { useSession } from 'next-auth/react'
import { useRouter } from 'next/navigation'
import { useEffect, useState } from 'react'

interface AdminProtectedProps {
  children: React.ReactNode
}

export default function AdminProtected({ children }: AdminProtectedProps) {
  const { data: session, status } = useSession()
  const router = useRouter()
  const [isAuthorized, setIsAuthorized] = useState(false)

  useEffect(() => {
    if (status === 'loading') return

    if (!session) {
      router.push('/admin/login')
      return
    }

    // Check if user is in the duanesburgboe group
    const checkAuthorization = async () => {
      try {
        const response = await fetch('/api/auth/check-group', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ email: session.user.email })
        })

        if (response.ok) {
          setIsAuthorized(true)
        } else {
          router.push('/admin/login')
        }
      } catch (error) {
        console.error('Error checking authorization:', error)
        router.push('/admin/login')
      }
    }

    checkAuthorization()
  }, [session, status, router])

  if (status === 'loading' || !isAuthorized) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600">Checking authorization...</p>
        </div>
      </div>
    )
  }

  return <>{children}</>
}