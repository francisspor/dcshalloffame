'use client'

import Link from 'next/link'
import { useMembers } from '@/hooks/useMembers'
import { MemberCategory } from '@/types/member'
import { generateSlug } from '@/utils/slug'

export default function AlumniHallOfFame() {
  const { getMembersByCategory, loading, error } = useMembers()
  const alumniMembers = getMembersByCategory(MemberCategory.Alumni)

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-purple-700 text-white shadow-lg">
        <div className="container mx-auto px-4 py-8">
          <h1 className="text-4xl font-bold text-center">
            Alumni Hall of Fame
          </h1>
          <p className="text-xl text-center mt-2">
            Celebrating distinguished alumni who have achieved excellence
          </p>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        {/* Introduction */}
        <section className="mb-12">
          <div className="bg-white rounded-lg shadow-md p-8">
            <h2 className="text-2xl font-bold text-gray-800 mb-4">
              Distinguished Alumni
            </h2>
            <p className="text-lg text-gray-600 leading-relaxed">
              The Alumni Hall of Fame celebrates the remarkable achievements of DCS graduates
              who have excelled in their chosen fields and made significant contributions to
              their communities. These alumni exemplify the values and education they received
              at Duanesburg Central School.
            </p>
          </div>
        </section>

        {/* Loading State */}
        {loading && (
          <section className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div>
            <p className="mt-4 text-gray-600 text-lg">Loading alumni members...</p>
          </section>
        )}

        {/* Error State */}
        {error && (
          <section className="text-center py-12">
            <div className="bg-red-50 border border-red-200 rounded-lg p-8 max-w-md mx-auto">
              <p className="text-red-600 text-lg mb-4">Error loading alumni members: {error}</p>
              <button
                onClick={() => window.location.reload()}
                className="px-6 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 transition-colors"
              >
                Retry
              </button>
            </div>
          </section>
        )}

        {/* Alumni Members Grid */}
        {!loading && !error && (
          <section>
            {alumniMembers.length === 0 ? (
              <div className="text-center py-12">
                <p className="text-gray-600 text-lg">No alumni members found.</p>
              </div>
            ) : (
              <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
                {alumniMembers.map((member) => (
                  <Link
                    key={member.id}
                    href={`/alumni-hall-of-fame/${generateSlug(member.name)}`}
                    className="group"
                  >
                    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-all duration-300 border-l-4 border-purple-500 group-hover:border-purple-600">
                      <h3 className="text-xl font-bold text-gray-800 mb-2 group-hover:text-purple-600 transition-colors">
                        {member.name}
                      </h3>
                      {member.graduationYear && (
                        <p className="text-gray-600 mb-1">
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
                      <div className="mt-4 text-purple-600 group-hover:text-purple-700 transition-colors">
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
        {!loading && !error && alumniMembers.length > 0 && (
          <section className="mt-12">
            <div className="bg-white rounded-lg shadow-md p-8">
              <h3 className="text-2xl font-bold text-gray-800 mb-6 text-center">
                Alumni Hall of Fame Statistics
              </h3>
              <div className="grid md:grid-cols-4 gap-8 text-center">
                <div>
                  <div className="text-4xl font-bold text-purple-600 mb-2">{alumniMembers.length}</div>
                  <div className="text-gray-600">Inducted Alumni</div>
                </div>
                <div>
                  <div className="text-4xl font-bold text-purple-600 mb-2">
                    {alumniMembers.filter(m => m.graduationYear).length > 0
                      ? `${Math.min(...alumniMembers.filter(m => m.graduationYear).map(m => m.graduationYear!))}-${Math.max(...alumniMembers.filter(m => m.graduationYear).map(m => m.graduationYear!))}`
                      : 'N/A'
                    }
                  </div>
                  <div className="text-gray-600">Graduation Years</div>
                </div>
                <div>
                  <div className="text-4xl font-bold text-purple-600 mb-2">
                    {Math.round(alumniMembers.reduce((acc, member) => acc + (new Date().getFullYear() - member.inductionYear), 0) / alumniMembers.length)}
                  </div>
                  <div className="text-gray-600">Average Years Since Induction</div>
                </div>
                <div>
                  <div className="text-4xl font-bold text-purple-600 mb-2">
                    {Math.min(...alumniMembers.map(m => m.inductionYear))} - {Math.max(...alumniMembers.map(m => m.inductionYear))}
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