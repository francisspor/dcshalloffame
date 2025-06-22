'use client'

import { useState } from 'react'
import { useSession, signOut } from 'next-auth/react'
import AdminProtected from '@/components/AdminProtected'
import MemberTable from '@/components/MemberTable'
import { useMembers } from '@/hooks/useMembers'
import { MemberCategory } from '@/types/member'
import Link from 'next/link'

export default function AdminDashboard() {
  const { data: session } = useSession()
  const { members, loading, error, refreshMembers } = useMembers()
  const [activeTab, setActiveTab] = useState<'overview' | 'staff' | 'alumni'>('overview')

  const staffMembers = members.filter(m => m.category === MemberCategory.Staff)
  const alumniMembers = members.filter(m => m.category === MemberCategory.Alumni)

  const handleSignOut = () => {
    signOut({ callbackUrl: '/' })
  }

  return (
    <AdminProtected>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <header className="bg-blue-900 text-white shadow-lg">
          <div className="container mx-auto px-4 py-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-bold">Hall of Fame Admin</h1>
                <p className="text-blue-200">Manage Hall of Fame members</p>
              </div>
              <div className="flex items-center space-x-4">
                <div className="text-right">
                  <p className="text-sm text-blue-200">Signed in as</p>
                  <p className="font-medium">{session?.user?.name}</p>
                </div>
                <button
                  onClick={handleSignOut}
                  className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition-colors"
                >
                  Sign Out
                </button>
              </div>
            </div>
          </div>
        </header>

        {/* Navigation */}
        <nav className="bg-white shadow-sm border-b">
          <div className="container mx-auto px-4">
            <div className="flex space-x-8">
              <button
                onClick={() => setActiveTab('overview')}
                className={`py-4 px-2 border-b-2 font-medium text-sm ${activeTab === 'overview'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Overview
              </button>
              <button
                onClick={() => setActiveTab('staff')}
                className={`py-4 px-2 border-b-2 font-medium text-sm ${activeTab === 'staff'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Staff Members ({staffMembers.length})
              </button>
              <button
                onClick={() => setActiveTab('alumni')}
                className={`py-4 px-2 border-b-2 font-medium text-sm ${activeTab === 'alumni'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Alumni Members ({alumniMembers.length})
              </button>
            </div>
          </div>
        </nav>

        {/* Main Content */}
        <main className="container mx-auto px-4 py-8">
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <div className="space-y-6">
              <div className="grid md:grid-cols-3 gap-6">
                <div className="bg-white rounded-lg shadow-md p-6">
                  <h3 className="text-lg font-semibold text-gray-800 mb-2">Total Members</h3>
                  <p className="text-3xl font-bold text-blue-600">{members.length}</p>
                </div>
                <div className="bg-white rounded-lg shadow-md p-6">
                  <h3 className="text-lg font-semibold text-gray-800 mb-2">Staff Members</h3>
                  <p className="text-3xl font-bold text-green-600">{staffMembers.length}</p>
                </div>
                <div className="bg-white rounded-lg shadow-md p-6">
                  <h3 className="text-lg font-semibold text-gray-800 mb-2">Alumni Members</h3>
                  <p className="text-3xl font-bold text-purple-600">{alumniMembers.length}</p>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-md p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-800">Quick Actions</h3>
                </div>
                <div className="grid md:grid-cols-2 gap-4">
                  <Link
                    href="/admin/members/new"
                    className="p-4 border-2 border-dashed border-gray-300 rounded-lg text-center hover:border-blue-500 hover:text-blue-600 transition-colors"
                  >
                    <div className="text-2xl mb-2">+</div>
                    <div className="font-medium">Add New Member</div>
                  </Link>
                  <button
                    onClick={refreshMembers}
                    className="p-4 border-2 border-dashed border-gray-300 rounded-lg text-center hover:border-blue-500 hover:text-blue-600 transition-colors"
                  >
                    <div className="text-2xl mb-2">↻</div>
                    <div className="font-medium">Refresh Data</div>
                  </button>
                </div>
              </div>

              {/* Recent Members */}
              <div className="bg-white rounded-lg shadow-md p-6">
                <h3 className="text-lg font-semibold text-gray-800 mb-4">Recent Members</h3>
                {loading ? (
                  <div className="text-center py-8">
                    <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                    <p className="mt-2 text-gray-600">Loading...</p>
                  </div>
                ) : error ? (
                  <div className="text-red-600">Error loading recent members: {error}</div>
                ) : members.length === 0 ? (
                  <p className="text-gray-500 text-center py-8">No members found.</p>
                ) : (
                  <div className="space-y-3">
                    {members
                      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                      .slice(0, 5)
                      .map((member) => (
                        <div key={member.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                          <div>
                            <div className="font-medium text-gray-900">{member.name}</div>
                            <div className="text-sm text-gray-500">
                              {member.category === MemberCategory.Staff ? 'Staff' : 'Alumni'} •
                              Inducted {member.inductionYear}
                            </div>
                          </div>
                          <div className="flex space-x-2">
                            <Link
                              href={`/admin/members/${member.id}/edit`}
                              className="text-blue-600 hover:text-blue-900 text-sm"
                            >
                              Edit
                            </Link>
                            <Link
                              href={member.category === MemberCategory.Staff
                                ? `/staff-hall-of-fame/${member.name.toLowerCase().replace(/\s+/g, '-')}`
                                : `/alumni-hall-of-fame/${member.name.toLowerCase().replace(/\s+/g, '-')}`
                              }
                              className="text-green-600 hover:text-green-900 text-sm"
                            >
                              View
                            </Link>
                          </div>
                        </div>
                      ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Staff Members Tab */}
          {activeTab === 'staff' && (
            <div className="space-y-6">
              <div className="flex items-center justify-between">
                <h2 className="text-2xl font-bold text-gray-800">Staff Members</h2>
                <Link
                  href="/admin/members/new?category=staff"
                  className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
                >
                  Add Staff Member
                </Link>
              </div>

              <MemberTable
                members={staffMembers}
                category={MemberCategory.Staff}
                loading={loading}
                error={error}
              />
            </div>
          )}

          {/* Alumni Members Tab */}
          {activeTab === 'alumni' && (
            <div className="space-y-6">
              <div className="flex items-center justify-between">
                <h2 className="text-2xl font-bold text-gray-800">Alumni Members</h2>
                <Link
                  href="/admin/members/new?category=alumni"
                  className="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 transition-colors"
                >
                  Add Alumni Member
                </Link>
              </div>

              <MemberTable
                members={alumniMembers}
                category={MemberCategory.Alumni}
                loading={loading}
                error={error}
              />
            </div>
          )}
        </main>
      </div>
    </AdminProtected>
  )
}