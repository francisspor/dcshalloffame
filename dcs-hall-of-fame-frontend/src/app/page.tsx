'use client'

import Link from 'next/link'
import { useStatistics } from '@/hooks/useMembers'

export default function Home() {
  const { statistics, loading, error } = useStatistics()

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Main Content */}
      <main className="container mx-auto px-4 py-8">
        {/* Welcome Section */}
        <section className="mb-12">
          <div className="bg-white rounded-lg shadow-md p-8">
            <h1 className="text-4xl font-bold text-gray-800 mb-4 text-center">
              Welcome to the DCS Hall of Fame
            </h1>
            <p className="text-lg text-gray-600 leading-relaxed text-center max-w-4xl mx-auto">
              The Duanesburg Central School Hall of Fame honors outstanding alumni and staff members
              who have made significant contributions to their community, profession, or the school.
              These individuals exemplify the values and excellence that DCS strives to instill in all students.
            </p>
          </div>
        </section>

        {/* Navigation Cards */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12">
          {/* Awards */}
          <Link href="/awards" className="group">
            <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 border-l-4 border-yellow-500">
              <h3 className="text-xl font-bold text-gray-800 mb-3 group-hover:text-blue-600 transition-colors">
                Awards
              </h3>
              <p className="text-gray-600">
                View special awards and recognitions given to our Hall of Fame members.
              </p>
            </div>
          </Link>

          {/* Staff Hall of Fame */}
          <Link href="/staff-hall-of-fame" className="group">
            <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 border-l-4 border-green-500">
              <h3 className="text-xl font-bold text-gray-800 mb-3 group-hover:text-blue-600 transition-colors">
                Staff Hall of Fame
              </h3>
              <p className="text-gray-600">
                Honoring outstanding teachers, administrators, and staff members who have served DCS.
              </p>
            </div>
          </Link>

          {/* Alumni Hall of Fame */}
          <Link href="/alumni-hall-of-fame" className="group">
            <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 border-l-4 border-purple-500">
              <h3 className="text-xl font-bold text-gray-800 mb-3 group-hover:text-blue-600 transition-colors">
                Alumni Hall of Fame
              </h3>
              <p className="text-gray-600">
                Celebrating distinguished alumni who have achieved excellence in their careers and communities.
              </p>
            </div>
          </Link>
        </div>

        {/* Quick Stats */}
        <section className="bg-white rounded-lg shadow-md p-8">
          <h3 className="text-2xl font-bold text-gray-800 mb-6 text-center">
            Hall of Fame Statistics
          </h3>

          {loading && (
            <div className="text-center py-8">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="mt-2 text-gray-600">Loading statistics...</p>
            </div>
          )}

          {error && (
            <div className="text-center py-8">
              <p className="text-red-600">Error loading statistics: {error}</p>
              <button
                onClick={() => window.location.reload()}
                className="mt-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors"
              >
                Retry
              </button>
            </div>
          )}

          {statistics && !loading && !error && (
            <div className="grid md:grid-cols-3 gap-8 text-center">
              <div>
                <div className="text-4xl font-bold text-green-600 mb-2">{statistics.staffMembers}</div>
                <div className="text-gray-600">Staff Members</div>
              </div>
              <div>
                <div className="text-4xl font-bold text-purple-600 mb-2">{statistics.alumniMembers}</div>
                <div className="text-gray-600">Alumni Members</div>
              </div>
              <div>
                <div className="text-4xl font-bold text-blue-600 mb-2">{statistics.totalMembers}</div>
                <div className="text-gray-600">Total Inductees</div>
              </div>
            </div>
          )}
        </section>
      </main>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-8 mt-12">
        <div className="container mx-auto px-4 text-center">
          <p className="text-gray-300">
            © 2024 Duanesburg Central School Hall of Fame. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  )
}
