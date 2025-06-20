'use client'

import Link from 'next/link'
import { useMembers } from '@/hooks/useMembers'
import { MemberCategory } from '@/types/member'
import { generateSlug } from '@/utils/slug'

export default function StaffHallOfFame() {
  const { getMembersByCategory, loading, error } = useMembers()
  const staffMembers = getMembersByCategory(MemberCategory.Staff)

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-green-700 text-white shadow-lg">
        <div className="container mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold text-center">
            Staff Hall of Fame
          </h1>
          <p className="text-xl text-center mt-2">
            Honoring outstanding teachers, administrators, and staff members
          </p>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        {/* Introduction */}
        <section className="mb-12">
          <div className="bg-white rounded-lg shadow-md p-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">
              Distinguished Staff Members
            </h2>
            <p className="text-lg text-gray-600 leading-relaxed">
              The Staff Hall of Fame recognizes the exceptional contributions of teachers,
              administrators, and staff members who have dedicated their careers to serving
              the students and community of Duanesburg Central School. These individuals
              have left an indelible mark on the school's history and continue to inspire
              future generations.
            </p>
          </div>
        </section>

        {/* Loading State */}
        {loading && (
          <section className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-green-600"></div>
            <p className="mt-4 text-gray-600 text-lg">Loading staff members...</p>
          </section>
        )}

        {/* Error State */}
        {error && (
          <section className="text-center py-12">
            <div className="bg-red-50 border border-red-200 rounded-lg p-8 max-w-md mx-auto">
              <p className="text-red-600 text-lg mb-4">Error loading staff members: {error}</p>
              <button
                onClick={() => window.location.reload()}
                className="px-6 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
              >
                Retry
              </button>
            </div>
          </section>
        )}

        {/* Staff Members Grid */}
        {!loading && !error && (
          <section>
            {staffMembers.length === 0 ? (
              <div className="text-center py-12">
                <p className="text-gray-600 text-lg">No staff members found.</p>
              </div>
            ) : (
              <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
                {staffMembers.map((member) => (
                  <Link
                    key={member.id}
                    href={`/staff-hall-of-fame/${generateSlug(member.name)}`}
                    className="group"
                  >
                    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-all duration-300 border-l-4 border-green-500 group-hover:border-green-600">
                      <h3 className="text-xl font-bold text-gray-800 mb-2 group-hover:text-green-600 transition-colors">
                        {member.name}
                      </h3>
                      {member.graduationYear && (
                        <p className="text-gray-600 mb-2">
                          Class of {member.graduationYear}
                        </p>
                      )}
                      <p className="text-sm text-gray-500 mb-3">
                        Inducted {member.inductionYear}
                      </p>
                      {member.achievements.length > 0 && (
                        <p className="text-sm text-gray-600 mb-3">
                          {member.achievements[0]}
                        </p>
                      )}
                      <div className="mt-4 text-green-600 group-hover:text-green-700 transition-colors">
                        View Profile →
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </section>
        )}

        {/* Statistics */}
        {!loading && !error && staffMembers.length > 0 && (
          <section className="mt-12">
            <div className="bg-white rounded-lg shadow-md p-8">
              <h3 className="text-2xl font-bold text-gray-800 mb-6 text-center">
                Staff Hall of Fame Statistics
              </h3>
              <div className="grid md:grid-cols-3 gap-8 text-center">
                <div>
                  <div className="text-4xl font-bold text-green-600 mb-2">{staffMembers.length}</div>
                  <div className="text-gray-600">Inducted Members</div>
                </div>
                <div>
                  <div className="text-4xl font-bold text-green-600 mb-2">
                    {Math.round(staffMembers.reduce((acc, member) => acc + (new Date().getFullYear() - member.inductionYear), 0) / staffMembers.length)}
                  </div>
                  <div className="text-gray-600">Average Years Since Induction</div>
                </div>
                <div>
                  <div className="text-4xl font-bold text-green-600 mb-2">
                    {Math.min(...staffMembers.map(m => m.inductionYear))} - {Math.max(...staffMembers.map(m => m.inductionYear))}
                  </div>
                  <div className="text-gray-600">Induction Period</div>
                </div>
              </div>
            </div>
          </section>
        )}
      </main>
    </div>
  )
}